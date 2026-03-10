using FoodFlow.Models;

namespace FoodFlow.ViewModels
{
    public class StockIndexViewModel
    {
        public List<Product> Products { get; set; } = new();
        public List<MenuItem> MenuItems { get; set; } = new();
        public List<RecipeIngredient> RecipeIngredients { get; set; } = new();
        public List<StockTransaction> Transactions { get; set; } = new();
    }
}
