using FoodFlow.Data;
using FoodFlow.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace FoodFlow.Controllers
{
    public class MenuController : Controller
    {
        private readonly ApplicationDbContext _context;
        public MenuController(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index(int? categoryId)
        {
            var categories = await _context.MenuCategories
                .OrderBy(x => x.SortOrder)
                .ToListAsync();

            var query = _context.MenuItems
                .Include(x => x.Category)
                .OrderBy(x => x.MenuCategoryId)
                .ThenBy(x => x.Name)
                .AsQueryable();

            if (categoryId.HasValue)
            {
                query = query.Where(x => x.MenuCategoryId == categoryId.Value);
            }

            var vm = new MenuIndexViewModel
            {
                SelectedCategoryId = categoryId,
                Categories = categories,
                Items = await query.ToListAsync()
            };

            return View(vm);
        }
    }
} 