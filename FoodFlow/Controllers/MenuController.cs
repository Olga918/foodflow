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

            var items = await query.ToListAsync();
            var itemIds = items.Select(x => x.Id).ToList();
            var recipeCounts = await _context.RecipeIngredients
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

            var vm = new MenuIndexViewModel
            {
                SelectedCategoryId = categoryId,
                Categories = categories,
                Items = rows
            };

            return View(vm);
        }
    }
} 