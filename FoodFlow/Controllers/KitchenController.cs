using FoodFlow.Data;
using FoodFlow.Enums;
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
