using FoodFlow.Data;
using FoodFlow.Enums;
using FoodFlow.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;

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

        [HttpGet]
        public async Task<IActionResult> Reports(DateTime? from, DateTime? to)
        {
            var vm = await BuildReportsViewModelAsync(from, to);

            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> ExportReportsCsv(DateTime? from, DateTime? to)
        {
            var vm = await BuildReportsViewModelAsync(from, to);
            var csv = BuildCsv(vm);
            var fileName = $"foodflow-reports-{vm.FromDate:yyyyMMdd}-{vm.ToDate:yyyyMMdd}.csv";
            return File(Encoding.UTF8.GetBytes(csv), "text/csv", fileName);
        }

        private async Task<AdminReportsViewModel> BuildReportsViewModelAsync(DateTime? from, DateTime? to)
        {
            var utcToday = DateTime.UtcNow.Date;
            var fromDate = (from ?? utcToday.AddDays(-30)).Date;
            var toDate = (to ?? utcToday).Date;

            if (toDate < fromDate)
            {
                (fromDate, toDate) = (toDate, fromDate);
            }

            var endExclusive = toDate.AddDays(1);

            var filteredOrdersQuery = _context.Orders
                .AsNoTracking()
                .Where(x =>
                    x.CreatedAt >= fromDate &&
                    x.CreatedAt < endExclusive &&
                    x.Status != OrderStatus.Cancelled);

            var ordersCount = await filteredOrdersQuery.CountAsync();
            var totalRevenue = await filteredOrdersQuery
                .Select(x => (decimal?)x.TotalAmount)
                .SumAsync() ?? 0m;
            var averageCheck = ordersCount == 0 ? 0m : decimal.Round(totalRevenue / ordersCount, 2);

            var topMenuItems = await _context.OrderItems
                .AsNoTracking()
                .Where(x =>
                    x.Order != null &&
                    x.Order.CreatedAt >= fromDate &&
                    x.Order.CreatedAt < endExclusive &&
                    x.Order.Status != OrderStatus.Cancelled)
                .GroupBy(x => new
                {
                    x.MenuItemId,
                    MenuItemName = x.MenuItem != null ? x.MenuItem.Name : "Unknown"
                })
                .Select(x => new TopMenuItemReportRow
                {
                    MenuItemId = x.Key.MenuItemId,
                    MenuItemName = x.Key.MenuItemName,
                    QuantitySold = x.Sum(i => i.Quantity),
                    Revenue = x.Sum(i => i.UnitPrice * i.Quantity)
                })
                .OrderByDescending(x => x.QuantitySold)
                .ThenByDescending(x => x.Revenue)
                .Take(10)
                .ToListAsync();

            var dailySalesRaw = await filteredOrdersQuery
                .GroupBy(x => x.CreatedAt.Date)
                .Select(x => new DailySalesReportRow
                {
                    Date = x.Key,
                    OrdersCount = x.Count(),
                    Revenue = x.Sum(i => i.TotalAmount)
                })
                .OrderBy(x => x.Date)
                .ToListAsync();

            // Все календарные дни в диапазоне — иначе Chart.js рисует только точки без линии при одном дне с данными.
            var dailySales = FillDailySalesGaps(dailySalesRaw, fromDate, toDate);

            var lowStockProducts = await _context.Products
                .AsNoTracking()
                .Where(x => x.QuantityInStock <= x.ReorderLevel)
                .OrderBy(x => x.QuantityInStock)
                .ThenBy(x => x.Name)
                .Select(x => new LowStockReportRow
                {
                    ProductId = x.Id,
                    ProductName = x.Name,
                    QuantityInStock = x.QuantityInStock,
                    ReorderLevel = x.ReorderLevel,
                    Unit = x.Unit
                })
                .ToListAsync();

            return new AdminReportsViewModel
            {
                FromDate = fromDate,
                ToDate = toDate,
                OrdersCount = ordersCount,
                TotalRevenue = totalRevenue,
                AverageCheck = averageCheck,
                TopMenuItems = topMenuItems,
                DailySales = dailySales,
                LowStockProducts = lowStockProducts
            };
        }

        private static List<DailySalesReportRow> FillDailySalesGaps(
            IReadOnlyList<DailySalesReportRow> rows,
            DateTime fromDate,
            DateTime toDate)
        {
            var byDay = rows.ToDictionary(r => r.Date.Date);
            var list = new List<DailySalesReportRow>();
            for (var d = fromDate.Date; d <= toDate.Date; d = d.AddDays(1))
            {
                if (byDay.TryGetValue(d, out var row))
                    list.Add(row);
                else
                    list.Add(new DailySalesReportRow { Date = d, OrdersCount = 0, Revenue = 0m });
            }

            return list;
        }

        private static string BuildCsv(AdminReportsViewModel vm)
        {
            var sb = new StringBuilder();
            sb.AppendLine("FoodFlow Reports");
            sb.AppendLine($"From,{vm.FromDate:yyyy-MM-dd}");
            sb.AppendLine($"To,{vm.ToDate:yyyy-MM-dd}");
            sb.AppendLine($"Completed Orders,{vm.OrdersCount}");
            sb.AppendLine($"Revenue,{vm.TotalRevenue:0.00}");
            sb.AppendLine($"Average Check,{vm.AverageCheck:0.00}");
            sb.AppendLine();

            sb.AppendLine("Daily Sales");
            sb.AppendLine("Date,Orders,Revenue");
            foreach (var row in vm.DailySales)
            {
                sb.AppendLine($"{row.Date:yyyy-MM-dd},{row.OrdersCount},{row.Revenue:0.00}");
            }

            sb.AppendLine();
            sb.AppendLine("Top Dishes");
            sb.AppendLine("Dish,Quantity Sold,Revenue");
            foreach (var row in vm.TopMenuItems)
            {
                sb.AppendLine($"{EscapeCsv(row.MenuItemName)},{row.QuantitySold},{row.Revenue:0.00}");
            }

            sb.AppendLine();
            sb.AppendLine("Low Stock Products");
            sb.AppendLine("Product,In Stock,Reorder Level,Unit");
            foreach (var row in vm.LowStockProducts)
            {
                sb.AppendLine($"{EscapeCsv(row.ProductName)},{row.QuantityInStock:0.###},{row.ReorderLevel:0.###},{EscapeCsv(row.Unit)}");
            }

            return sb.ToString();
        }

        private static string EscapeCsv(string value)
        {
            if (value.Contains(',') || value.Contains('"') || value.Contains('\n'))
            {
                return $"\"{value.Replace("\"", "\"\"")}\"";
            }

            return value;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ClearTestOrders()
        {
            // ExecuteDeleteAsync() генерирует SQL с алиасами, несовместимый с некоторыми версиями MariaDB/MySQL.
            var orderItemsDeleted = await _context.Database.ExecuteSqlRawAsync("DELETE FROM `OrderItems`");
            var ordersDeleted = await _context.Database.ExecuteSqlRawAsync("DELETE FROM `Orders`");

            TempData["AdminMessage"] = $"Deleted {ordersDeleted} orders and {orderItemsDeleted} order items.";
            return RedirectToAction(nameof(Index));
        }
    }
}
