using FoodFlow.Data;
using FoodFlow.Models;
using FoodFlow.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace FoodFlow.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var items = await _context.MenuItems
                .AsNoTracking()
                .Include(x => x.Category)
                .Where(x => x.IsAvailable)
                .OrderByDescending(x => x.Price)
                .ThenBy(x => x.Name)
                .Take(6)
                .ToListAsync();

            var itemIds = items.Select(x => x.Id).ToList();
            var recipeCounts = await _context.RecipeIngredients
                .AsNoTracking()
                .Where(x => itemIds.Contains(x.MenuItemId))
                .GroupBy(x => x.MenuItemId)
                .Select(g => new { MenuItemId = g.Key, Cnt = g.Count() })
                .ToDictionaryAsync(x => x.MenuItemId, x => x.Cnt);

            var rows = items.Select(i => new MenuItemListRow
            {
                Id = i.Id,
                Name = i.Name,
                Description = i.Description,
                Price = i.Price,
                IsAvailable = i.IsAvailable,
                KitchenPortions = i.KitchenPortions,
                RecipeLineCount = recipeCounts.GetValueOrDefault(i.Id),
                CategoryName = i.Category?.Name
            }).ToList();

            var vm = new HomeIndexViewModel { RecommendedDishes = rows };
            return View(vm);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult Contact()
        {
            return View();
        }

        public IActionResult AccessDenied()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
