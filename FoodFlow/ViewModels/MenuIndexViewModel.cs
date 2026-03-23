using FoodFlow.Models;

namespace FoodFlow.ViewModels
{
    public class MenuIndexViewModel
    {
        public int? SelectedCategoryId { get; set; }
        public List<MenuCategory> Categories { get; set; } = new();
        public List<MenuItemListRow> Items { get; set; } = new();
    }
}
