using FoodFlow.Models;

namespace FoodFlow.ViewModels
{
    public class HomeIndexViewModel
    {
        public List<MenuItem> RecommendedDishes { get; set; } = new();
    }
}
