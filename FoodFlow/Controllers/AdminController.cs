using FoodFlow.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FoodFlow.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.OrdersCount = await _context.Orders.CountAsync();
            ViewBag.OrderItemsCount = await _context.OrderItems.CountAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ClearTestOrders()
        {
            var orderItemsDeleted = await _context.OrderItems.ExecuteDeleteAsync();
            var ordersDeleted = await _context.Orders.ExecuteDeleteAsync();

            TempData["AdminMessage"] = $"Deleted {ordersDeleted} orders and {orderItemsDeleted} order items.";
            return RedirectToAction(nameof(Index));
        }
    }
}
