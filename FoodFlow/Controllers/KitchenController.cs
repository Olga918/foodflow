using FoodFlow.Data;
using FoodFlow.Enums;
using FoodFlow.Models;
using FoodFlow.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FoodFlow.Controllers
{
    [Authorize(Roles = "Cook")]
    public class KitchenController : Controller
    {
        private readonly ApplicationDbContext _context;

        public KitchenController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var orders = await _context.Orders
                .Include(x => x.Items)
                    .ThenInclude(x => x.MenuItem)
                .Where(x => x.Status == OrderStatus.New || x.Status == OrderStatus.Cooking || x.Status == OrderStatus.Ready)
                .OrderBy(x => x.CreatedAt)
                .ToListAsync();

            return View(orders);
        }

        /// <summary>Prepare portions on the line: raw stock is written off here (real kitchen), not at client checkout.</summary>
        public async Task<IActionResult> Prepare()
        {
            var items = await _context.MenuItems
                .Include(x => x.Category)
                .OrderBy(x => x.Name)
                .ToListAsync();

            var ids = items.Select(x => x.Id).ToList();
            var recipeCounts = await _context.RecipeIngredients
                .Where(x => ids.Contains(x.MenuItemId))
                .GroupBy(x => x.MenuItemId)
                .Select(g => new { MenuItemId = g.Key, Cnt = g.Count() })
                .ToDictionaryAsync(x => x.MenuItemId, x => x.Cnt);

            var vm = new KitchenPrepareViewModel
            {
                Rows = items.Select(i => new KitchenPrepareRow
                {
                    MenuItemId = i.Id,
                    Name = i.Name,
                    CategoryName = i.Category?.Name,
                    KitchenPortions = i.KitchenPortions,
                    RecipeLineCount = recipeCounts.GetValueOrDefault(i.Id)
                }).ToList()
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Prepare(int menuItemId, int portions)
        {
            if (portions < 1)
            {
                TempData["KitchenMessage"] = "Portions must be at least 1.";
                return RedirectToAction(nameof(Prepare));
            }

            var menuItem = await _context.MenuItems.FirstOrDefaultAsync(x => x.Id == menuItemId);
            if (menuItem is null)
            {
                return NotFound();
            }

            var recipe = await _context.RecipeIngredients
                .Include(x => x.Product)
                .Where(x => x.MenuItemId == menuItemId)
                .ToListAsync();

            if (!recipe.Any())
            {
                TempData["KitchenMessage"] =
                    $"No recipe for \"{menuItem.Name}\". The storekeeper must add ingredients per portion in Stock.";
                return RedirectToAction(nameof(Prepare));
            }

            var requiredByProduct = new Dictionary<int, decimal>();
            foreach (var row in recipe)
            {
                var need = row.AmountPerDish * portions;
                if (requiredByProduct.TryGetValue(row.ProductId, out var existing))
                {
                    requiredByProduct[row.ProductId] = existing + need;
                }
                else
                {
                    requiredByProduct[row.ProductId] = need;
                }
            }

            var productIds = requiredByProduct.Keys.ToList();
            var products = await _context.Products.Where(x => productIds.Contains(x.Id)).ToListAsync();
            var shortages = new List<string>();

            foreach (var (productId, need) in requiredByProduct)
            {
                var product = products.FirstOrDefault(x => x.Id == productId);
                if (product is null)
                {
                    shortages.Add($"Product id {productId} is missing in the warehouse catalog.");
                    continue;
                }

                if (product.QuantityInStock < need)
                {
                    shortages.Add(
                        $"{product.Name}: need {need:0.###} {product.Unit} for {portions} portion(s), warehouse has {product.QuantityInStock:0.###} {product.Unit}.");
                }
            }

            if (shortages.Any())
            {
                TempData["KitchenMessage"] =
                    "Not enough warehouse stock to prepare: " + string.Join(" | ", shortages);
                return RedirectToAction(nameof(Prepare));
            }

            foreach (var product in products)
            {
                var need = requiredByProduct[product.Id];
                if (need <= 0)
                {
                    continue;
                }

                product.QuantityInStock -= need;
                _context.StockTransactions.Add(new StockTransaction
                {
                    ProductId = product.Id,
                    Type = StockTransactionType.WriteOff,
                    Quantity = need,
                    Comment = $"Kitchen prep: {menuItem.Name} ×{portions}",
                    CreatedAt = DateTime.UtcNow
                });
            }

            menuItem.KitchenPortions += portions;
            await _context.SaveChangesAsync();

            TempData["KitchenMessage"] = $"Prepared {portions} portion(s) of \"{menuItem.Name}\" from warehouse stock.";
            return RedirectToAction(nameof(Prepare));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, OrderStatus newStatus)
        {
            var order = await _context.Orders.FirstOrDefaultAsync(x => x.Id == id);
            if (order is null)
            {
                return NotFound();
            }

            if (!IsTransitionAllowed(order.Status, newStatus))
            {
                TempData["KitchenMessage"] = "Invalid status transition.";
                return RedirectToAction(nameof(Index));
            }

            order.Status = newStatus;
            await _context.SaveChangesAsync();
            TempData["KitchenMessage"] = $"Order #{order.Id} moved to {order.Status}.";
            return RedirectToAction(nameof(Index));
        }

        private static bool IsTransitionAllowed(OrderStatus current, OrderStatus next)
        {
            return (current, next) switch
            {
                (OrderStatus.New, OrderStatus.Cooking) => true,
                (OrderStatus.Cooking, OrderStatus.Ready) => true,
                _ => false
            };
        }
    }
}
