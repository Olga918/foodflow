using FoodFlow.Data;
using FoodFlow.Enums;
using FoodFlow.Models;
using FoodFlow.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FoodFlow.Controllers
{
    [Authorize(Roles = "Storekeeper")]
    public class StockController : Controller
    {
        private readonly ApplicationDbContext _context;

        public StockController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var vm = new StockIndexViewModel
            {
                Products = await _context.Products
                    .OrderBy(x => x.Name)
                    .ToListAsync(),
                MenuItems = await _context.MenuItems
                    .Include(x => x.Category)
                    .OrderBy(x => x.Category != null ? x.Category.SortOrder : 999)
                    .ThenBy(x => x.Name)
                    .ToListAsync(),
                RecipeIngredients = await _context.RecipeIngredients
                    .Include(x => x.MenuItem)
                    .Include(x => x.Product)
                    .OrderBy(x => x.MenuItem!.Name)
                    .ThenBy(x => x.Product!.Name)
                    .ToListAsync(),
                Transactions = await _context.StockTransactions
                    .Include(x => x.Product)
                    .OrderByDescending(x => x.CreatedAt)
                    .Take(20)
                    .ToListAsync()
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddProduct(string name, string unit, decimal reorderLevel)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                TempData["StockError"] = "Product name is required.";
                return RedirectToAction(nameof(Index));
            }

            if (reorderLevel < 0)
            {
                TempData["StockError"] = "Reorder level cannot be negative.";
                return RedirectToAction(nameof(Index));
            }

            var normalizedName = name.Trim();
            var exists = await _context.Products
                .AnyAsync(x => x.Name.ToLower() == normalizedName.ToLower());
            if (exists)
            {
                TempData["StockError"] = "Product with this name already exists.";
                return RedirectToAction(nameof(Index));
            }

            var product = new Product
            {
                Name = normalizedName,
                Unit = string.IsNullOrWhiteSpace(unit) ? "g" : unit.Trim(),
                ReorderLevel = reorderLevel,
                QuantityInStock = 0
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            TempData["StockMessage"] = $"Product '{product.Name}' added.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApplyOperation(int productId, StockTransactionType type, decimal quantity, string? comment)
        {
            var product = await _context.Products.FirstOrDefaultAsync(x => x.Id == productId);
            if (product is null)
            {
                TempData["StockError"] = "Product not found.";
                return RedirectToAction(nameof(Index));
            }

            if (type == StockTransactionType.Adjustment && quantity < 0)
            {
                TempData["StockError"] = "Adjustment value cannot be negative.";
                return RedirectToAction(nameof(Index));
            }

            if (type != StockTransactionType.Adjustment && quantity <= 0)
            {
                TempData["StockError"] = "Quantity must be greater than zero.";
                return RedirectToAction(nameof(Index));
            }

            decimal transactionQuantity;
            switch (type)
            {
                case StockTransactionType.Incoming:
                    product.QuantityInStock += quantity;
                    transactionQuantity = quantity;
                    break;

                case StockTransactionType.WriteOff:
                    if (product.QuantityInStock < quantity)
                    {
                        TempData["StockError"] = $"Not enough stock for '{product.Name}'.";
                        return RedirectToAction(nameof(Index));
                    }

                    product.QuantityInStock -= quantity;
                    transactionQuantity = quantity;
                    break;

                case StockTransactionType.Adjustment:
                    var delta = quantity - product.QuantityInStock;
                    if (delta == 0)
                    {
                        TempData["StockMessage"] = $"No changes for '{product.Name}'.";
                        return RedirectToAction(nameof(Index));
                    }

                    product.QuantityInStock = quantity;
                    transactionQuantity = delta;
                    break;

                default:
                    TempData["StockError"] = "Unsupported stock operation.";
                    return RedirectToAction(nameof(Index));
            }

            _context.StockTransactions.Add(new StockTransaction
            {
                ProductId = product.Id,
                Type = type,
                Quantity = transactionQuantity,
                Comment = string.IsNullOrWhiteSpace(comment) ? null : comment.Trim(),
                CreatedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();
            TempData["StockMessage"] = $"Stock updated for '{product.Name}'.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddRecipeIngredient(int menuItemId, int productId, decimal amountPerDish)
        {
            if (amountPerDish <= 0)
            {
                TempData["StockError"] = "Amount per dish must be greater than zero.";
                return RedirectToAction(nameof(Index));
            }

            var menuItemExists = await _context.MenuItems.AnyAsync(x => x.Id == menuItemId);
            var productExists = await _context.Products.AnyAsync(x => x.Id == productId);
            if (!menuItemExists || !productExists)
            {
                TempData["StockError"] = "Invalid menu item or product.";
                return RedirectToAction(nameof(Index));
            }

            var existing = await _context.RecipeIngredients
                .FirstOrDefaultAsync(x => x.MenuItemId == menuItemId && x.ProductId == productId);

            if (existing is null)
            {
                _context.RecipeIngredients.Add(new RecipeIngredient
                {
                    MenuItemId = menuItemId,
                    ProductId = productId,
                    AmountPerDish = amountPerDish
                });
            }
            else
            {
                existing.AmountPerDish = amountPerDish;
            }

            await _context.SaveChangesAsync();
            TempData["StockMessage"] = "Recipe ingredient saved.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveRecipeIngredient(int id)
        {
            var recipeIngredient = await _context.RecipeIngredients.FirstOrDefaultAsync(x => x.Id == id);
            if (recipeIngredient is null)
            {
                TempData["StockError"] = "Recipe ingredient not found.";
                return RedirectToAction(nameof(Index));
            }

            _context.RecipeIngredients.Remove(recipeIngredient);
            await _context.SaveChangesAsync();
            TempData["StockMessage"] = "Recipe ingredient removed.";
            return RedirectToAction(nameof(Index));
        }
    }
}
