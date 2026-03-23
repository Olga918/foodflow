namespace FoodFlow.ViewModels
{
    /// <summary>Menu row for catalog: shows kitchen line stock and whether ordering is allowed.</summary>
    public sealed class MenuItemListRow
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public bool IsAvailable { get; set; }
        public int KitchenPortions { get; set; }
        public int RecipeLineCount { get; set; }
        public string? CategoryName { get; set; }

        public bool CanAddToCart => IsAvailable && RecipeLineCount > 0 && KitchenPortions > 0;
    }
}
