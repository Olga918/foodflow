namespace FoodFlow.Models
{
    public class MenuItem
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int MenuCategoryId { get; set; }
        public decimal Price { get; set; }
        public bool IsAvailable { get; set; } = true;

        /// <summary>Portions ready on the line (prepared from warehouse stock). Client orders consume this, not raw stock directly.</summary>
        public int KitchenPortions { get; set; }

        public MenuCategory? Category { get; set; }
    }
}
