namespace FoodFlow.ViewModels
{
    public sealed class KitchenPrepareRow
    {
        public int MenuItemId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? CategoryName { get; set; }
        public int KitchenPortions { get; set; }
        public int RecipeLineCount { get; set; }

        public bool CanPrepare => RecipeLineCount > 0;
    }

    public sealed class KitchenPrepareViewModel
    {
        public List<KitchenPrepareRow> Rows { get; set; } = new();
    }
}
