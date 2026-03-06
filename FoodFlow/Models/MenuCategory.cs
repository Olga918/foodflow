namespace FoodFlow.Models
{
    public class MenuCategory
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int SortOrder { get; set; }

        public List<MenuItem> Items { get; set; } = new();
    }
}
