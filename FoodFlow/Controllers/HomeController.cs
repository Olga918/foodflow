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

        public IActionResult Index()
        {
            var vm = new HomeIndexViewModel
            {
                RecommendedDishes = _context.MenuItems
                    .AsNoTracking()
                    .Include(x => x.Category)
                    .Where(x => x.IsAvailable)
                    .OrderByDescending(x => x.Price)
                    .ThenBy(x => x.Name)
                    .Take(6)
                    .ToList()
            };

            return View(vm);
        }

        public IActionResult Privacy()
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
