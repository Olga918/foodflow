using FoodFlow.Data;
using FoodFlow.Enums;
using FoodFlow.Models;
using FoodFlow.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace FoodFlow.Controllers
{
    [Authorize(Roles = "Admin")]
    public class PurchasesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PurchasesController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var lists = await _context.PurchaseLists
                .AsNoTracking()
                .Include(x => x.Lines)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();

            var rows = lists.Select(x => new PurchaseListSummaryRow
            {
                Id = x.Id,
                CreatedAt = x.CreatedAt,
                Status = x.Status,
                Note = x.Note,
                LinesCount = x.Lines.Count,
                TotalSuggested = x.Lines.Sum(l => l.SuggestedQuantity),
                TotalReceived = x.Lines.Sum(l => l.ReceivedQuantity)
            }).ToList();

            return View(rows);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateFromLowStock()
        {
            var lows = await _context.Products
                .Where(p => p.QuantityInStock <= p.ReorderLevel)
                .OrderBy(p => p.Name)
                .ToListAsync();

            if (!lows.Any())
            {
                TempData["PurchaseMessage"] = "No products are at or below reorder level — nothing to add to a purchase list.";
                return RedirectToAction(nameof(Index));
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var list = new PurchaseList
            {
                CreatedAt = DateTime.UtcNow,
                CreatedByUserId = userId,
                Status = PurchaseListStatus.Open,
                Note = "Auto: products at/below reorder level"
            };

            foreach (var p in lows)
            {
                var suggested = p.ReorderLevel * 2m - p.QuantityInStock;
                if (suggested < p.ReorderLevel)
                {
                    suggested = p.ReorderLevel;
                }

                if (suggested <= 0)
                {
                    suggested = p.ReorderLevel;
                }

                list.Lines.Add(new PurchaseListLine
                {
                    ProductId = p.Id,
                    SuggestedQuantity = decimal.Round(suggested, 3),
                    ReceivedQuantity = 0
                });
            }

            _context.PurchaseLists.Add(list);
            await _context.SaveChangesAsync();

            TempData["PurchaseMessage"] =
                $"Purchase list #{list.Id} created with {list.Lines.Count} line(s). Storekeeper can post goods to Stock; you record receipts here.";
            return RedirectToAction(nameof(Details), new { id = list.Id });
        }

        public async Task<IActionResult> Details(int id)
        {
            var list = await _context.PurchaseLists
                .AsNoTracking()
                .Include(x => x.Lines)
                .ThenInclude(x => x.Product)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (list is null)
            {
                return NotFound();
            }

            var vm = new PurchaseListDetailViewModel
            {
                Id = list.Id,
                CreatedAt = list.CreatedAt,
                Status = list.Status,
                Note = list.Note,
                Lines = list.Lines
                    .OrderBy(x => x.Product!.Name)
                    .Select(x => new PurchaseListLineRow
                    {
                        LineId = x.Id,
                        ProductId = x.ProductId,
                        ProductName = x.Product?.Name ?? "?",
                        Unit = x.Product?.Unit ?? "",
                        CurrentStock = x.Product?.QuantityInStock ?? 0,
                        SuggestedQuantity = x.SuggestedQuantity,
                        ReceivedQuantity = x.ReceivedQuantity
                    })
                    .ToList()
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Receive(int listId, int lineId, decimal quantity)
        {
            if (quantity <= 0)
            {
                TempData["PurchaseError"] = "Quantity must be greater than zero.";
                return RedirectToAction(nameof(Details), new { id = listId });
            }

            var line = await _context.PurchaseListLines
                .Include(x => x.Product)
                .FirstOrDefaultAsync(x => x.Id == lineId && x.PurchaseListId == listId);

            if (line is null)
            {
                return NotFound();
            }

            var listHeader = await _context.PurchaseLists.AsNoTracking().FirstOrDefaultAsync(x => x.Id == listId);
            if (listHeader is null)
            {
                return NotFound();
            }

            if (listHeader.Status != PurchaseListStatus.Open)
            {
                TempData["PurchaseError"] = "This purchase list is already completed.";
                return RedirectToAction(nameof(Details), new { id = listId });
            }

            var remaining = line.SuggestedQuantity - line.ReceivedQuantity;
            if (quantity > remaining)
            {
                TempData["PurchaseError"] =
                    $"Cannot receive more than remaining ({remaining:0.###} {line.Product?.Unit}).";
                return RedirectToAction(nameof(Details), new { id = listId });
            }

            var product = line.Product;
            if (product is null)
            {
                return NotFound();
            }

            product.QuantityInStock += quantity;
            line.ReceivedQuantity += quantity;

            _context.StockTransactions.Add(new StockTransaction
            {
                ProductId = product.Id,
                Type = StockTransactionType.Incoming,
                Quantity = quantity,
                Comment = $"Purchase list #{listId} receipt",
                CreatedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();

            var stillPending = await _context.PurchaseListLines
                .AnyAsync(x => x.PurchaseListId == listId && x.ReceivedQuantity < x.SuggestedQuantity);

            if (!stillPending)
            {
                var pl = await _context.PurchaseLists.FindAsync(listId);
                if (pl is not null)
                {
                    pl.Status = PurchaseListStatus.Completed;
                    await _context.SaveChangesAsync();
                }
            }

            TempData["PurchaseMessage"] = $"Received {quantity:0.###} {product.Unit} of {product.Name}.";
            return RedirectToAction(nameof(Details), new { id = listId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Complete(int id)
        {
            var list = await _context.PurchaseLists
                .Include(x => x.Lines)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (list is null)
            {
                return NotFound();
            }

            if (list.Lines.Any(x => x.ReceivedQuantity < x.SuggestedQuantity))
            {
                TempData["PurchaseError"] =
                    "Not all lines are fully received. Receive remaining quantities or adjust outside this flow.";
                return RedirectToAction(nameof(Details), new { id });
            }

            list.Status = PurchaseListStatus.Completed;
            await _context.SaveChangesAsync();

            TempData["PurchaseMessage"] = $"Purchase list #{id} marked completed.";
            return RedirectToAction(nameof(Index));
        }
    }
}
