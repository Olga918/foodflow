namespace FoodFlow.Models
{
    public class RecipeIngredient
    {
        public int Id { get; set; }
        public int MenuItemId { get; set; }
        public int ProductId { get; set; }
        public decimal AmountPerDish { get; set; }

        public MenuItem? MenuItem { get; set; }
        public Product? Product { get; set; }
    }
}
