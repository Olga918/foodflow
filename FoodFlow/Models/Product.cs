namespace FoodFlow.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal QuantityInStock { get; set; }
        public decimal ReorderLevel { get; set; }
        public string Unit { get; set; } = "g";
    }
}
