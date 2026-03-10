using FoodFlow.Data;
using FoodFlow.Enums;
using FoodFlow.Extensions;
using FoodFlow.Models;
using FoodFlow.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace FoodFlow.Controllers
{
    public class CartController : Controller
    {
        private const string CartSessionKey = "FoodFlow.Cart";
        private readonly ApplicationDbContext _context;

        public CartController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var cart = GetCart();
            return View(cart);
        }

        [Authorize]
        [HttpGet]
        public IActionResult Checkout()
        {
            var cart = GetCart();
            if (!cart.Any())
            {
                TempData["CartMessage"] = "Cart is empty. Add items before checkout.";
                return RedirectToAction(nameof(Index));
            }

            var vm = new CheckoutViewModel();
            return View(vm);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Checkout(CheckoutViewModel model)
        {
            var cart = GetCart();
            if (!cart.Any())
            {
                TempData["CartMessage"] = "Cart is empty. Add items before checkout.";
                return RedirectToAction(nameof(Index));
            }

            if (model.OrderType == OrderType.Delivery && string.IsNullOrWhiteSpace(model.DeliveryAddress))
            {
                ModelState.AddModelError(nameof(model.DeliveryAddress), "Delivery address is required for delivery orders.");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var customerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(customerId))
            {
                return Challenge();
            }

            var cartItemIds = cart.Select(x => x.MenuItemId).Distinct().ToList();
            var recipeIngredients = await _context.RecipeIngredients
                .Include(x => x.Product)
                .Where(x => cartItemIds.Contains(x.MenuItemId))
                .ToListAsync();

            var requiredByProduct = new Dictionary<int, decimal>();
            foreach (var cartItem in cart)
            {
                var recipeRows = recipeIngredients.Where(x => x.MenuItemId == cartItem.MenuItemId).ToList();
                foreach (var row in recipeRows)
                {
                    var required = row.AmountPerDish * cartItem.Quantity;
                    if (requiredByProduct.ContainsKey(row.ProductId))
                    {
                        requiredByProduct[row.ProductId] += required;
                    }
                    else
                    {
                        requiredByProduct[row.ProductId] = required;
                    }
                }
            }

            var productIds = requiredByProduct.Keys.ToList();
            var products = await _context.Products
                .Where(x => productIds.Contains(x.Id))
                .ToListAsync();

            var shortages = new List<string>();
            var missingProductIds = productIds.Except(products.Select(x => x.Id)).ToList();
            if (missingProductIds.Any())
            {
                shortages.Add("Some recipe ingredients are missing in stock catalog.");
            }

            foreach (var product in products)
            {
                var required = requiredByProduct[product.Id];
                if (product.QuantityInStock < required)
                {
                    shortages.Add($"{product.Name}: required {required:0.###} {product.Unit}, available {product.QuantityInStock:0.###} {product.Unit}");
                }
            }

            if (shortages.Any())
            {
                ModelState.AddModelError(string.Empty, "Not enough ingredients for this order:");
                foreach (var shortage in shortages)
                {
                    ModelState.AddModelError(string.Empty, shortage);
                }
                return View(model);
            }

            var order = new Order
            {
                CustomerId = customerId,
                OrderType = model.OrderType,
                ContactPhone = model.ContactPhone.Trim(),
                DeliveryAddress = model.OrderType == OrderType.Delivery
                    ? model.DeliveryAddress!.Trim()
                    : string.Empty,
                Status = OrderStatus.New,
                TotalAmount = cart.Sum(x => x.LineTotal),
                Items = cart.Select(x => new OrderItem
                {
                    MenuItemId = x.MenuItemId,
                    Quantity = x.Quantity,
                    UnitPrice = x.UnitPrice
                }).ToList()
            };

            foreach (var product in products)
            {
                var required = requiredByProduct[product.Id];
                if (required <= 0)
                {
                    continue;
                }

                product.QuantityInStock -= required;
                _context.StockTransactions.Add(new StockTransaction
                {
                    ProductId = product.Id,
                    Type = StockTransactionType.WriteOff,
                    Quantity = required,
                    Comment = "Auto write-off from checkout",
                    CreatedAt = DateTime.UtcNow
                });
            }

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            SaveCart(new List<CartItemViewModel>());
            return RedirectToAction(nameof(Success), new { id = order.Id });
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Success(int id)
        {
            var customerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(customerId))
            {
                return Challenge();
            }

            var order = await _context.Orders
                .Where(x => x.Id == id && x.CustomerId == customerId)
                .Select(x => new { x.Id, x.CreatedAt, x.TotalAmount, x.OrderType })
                .FirstOrDefaultAsync();

            if (order is null)
            {
                return NotFound();
            }

            ViewBag.OrderId = order.Id;
            ViewBag.CreatedAt = order.CreatedAt;
            ViewBag.TotalAmount = order.TotalAmount;
            ViewBag.OrderType = order.OrderType;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(int menuItemId, int quantity = 1)
        {
            if (quantity < 1)
            {
                quantity = 1;
            }

            var menuItem = await _context.MenuItems
                .Where(x => x.Id == menuItemId && x.IsAvailable)
                .Select(x => new { x.Id, x.Name, x.Price })
                .FirstOrDefaultAsync();

            if (menuItem is null)
            {
                return NotFound();
            }

            var cart = GetCart();
            var existing = cart.FirstOrDefault(x => x.MenuItemId == menuItem.Id);
            if (existing is null)
            {
                cart.Add(new CartItemViewModel
                {
                    MenuItemId = menuItem.Id,
                    Name = menuItem.Name,
                    UnitPrice = menuItem.Price,
                    Quantity = quantity
                });
            }
            else
            {
                existing.Quantity += quantity;
            }

            SaveCart(cart);
            return RedirectToAction("Index", "Menu");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Remove(int menuItemId)
        {
            var cart = GetCart();
            var item = cart.FirstOrDefault(x => x.MenuItemId == menuItemId);
            if (item is not null)
            {
                cart.Remove(item);
                SaveCart(cart);
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Clear()
        {
            SaveCart(new List<CartItemViewModel>());
            return RedirectToAction(nameof(Index));
        }

        private List<CartItemViewModel> GetCart()
        {
            return HttpContext.Session.GetObject<List<CartItemViewModel>>(CartSessionKey)
                   ?? new List<CartItemViewModel>();
        }

        private void SaveCart(List<CartItemViewModel> cart)
        {
            HttpContext.Session.SetObject(CartSessionKey, cart);
        }
    }
}
