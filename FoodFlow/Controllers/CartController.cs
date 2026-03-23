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

            var qtyByMenuItem = cart
                .GroupBy(x => x.MenuItemId)
                .ToDictionary(g => g.Key, g => g.Sum(x => x.Quantity));

            var menuItemIds = qtyByMenuItem.Keys.ToList();
            var menuItems = await _context.MenuItems
                .Where(x => menuItemIds.Contains(x.Id))
                .ToListAsync();
            var menuById = menuItems.ToDictionary(x => x.Id);

            var recipeIngredients = await _context.RecipeIngredients
                .Where(x => menuItemIds.Contains(x.MenuItemId))
                .ToListAsync();

            var missingRecipeNames = new List<string>();
            var portionShortages = new List<string>();

            foreach (var (menuItemId, qty) in qtyByMenuItem)
            {
                var recipeRows = recipeIngredients.Where(x => x.MenuItemId == menuItemId).ToList();
                if (!recipeRows.Any())
                {
                    var name = cart.First(x => x.MenuItemId == menuItemId).Name;
                    missingRecipeNames.Add(name);
                    continue;
                }

                if (!menuById.TryGetValue(menuItemId, out var mi))
                {
                    portionShortages.Add($"Dish id {menuItemId} is no longer on the menu.");
                    continue;
                }

                if (mi.KitchenPortions < qty)
                {
                    portionShortages.Add(
                        $"{mi.Name}: need {qty} portion(s) ready on the kitchen line, have {mi.KitchenPortions}. Ask the cook to prepare more from Stock ingredients.");
                }
            }

            if (missingRecipeNames.Any())
            {
                ModelState.AddModelError(string.Empty,
                    "These dishes have no recipe in the system — ordering is blocked until the storekeeper adds ingredients per portion (Stock):");
                foreach (var n in missingRecipeNames.Distinct().OrderBy(x => x))
                {
                    ModelState.AddModelError(string.Empty, n);
                }
            }

            if (portionShortages.Any())
            {
                ModelState.AddModelError(string.Empty, "Not enough prepared portions on the kitchen line:");
                foreach (var line in portionShortages)
                {
                    ModelState.AddModelError(string.Empty, line);
                }
            }

            if (!ModelState.IsValid)
            {
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

            foreach (var (menuItemId, qty) in qtyByMenuItem)
            {
                menuById[menuItemId].KitchenPortions -= qty;
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
                .FirstOrDefaultAsync(x => x.Id == menuItemId && x.IsAvailable);

            if (menuItem is null)
            {
                return NotFound();
            }

            var recipeLines = await _context.RecipeIngredients.CountAsync(x => x.MenuItemId == menuItemId);
            if (recipeLines == 0)
            {
                TempData["CartMessage"] =
                    $"\"{menuItem.Name}\" cannot be ordered yet: no recipe (ingredients per portion). The storekeeper must configure it in Stock.";
                return RedirectToAction("Index", "Menu");
            }

            var cart = GetCart();
            var existing = cart.FirstOrDefault(x => x.MenuItemId == menuItem.Id);
            var newTotal = (existing?.Quantity ?? 0) + quantity;
            if (newTotal > menuItem.KitchenPortions)
            {
                TempData["CartMessage"] =
                    $"Only {menuItem.KitchenPortions} portion(s) of \"{menuItem.Name}\" are ready on the kitchen line. Reduce quantity or wait for the cook to prepare more.";
                return RedirectToAction("Index", "Menu");
            }

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
