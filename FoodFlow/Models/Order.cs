using FoodFlow.Enums;

namespace FoodFlow.Models
{
    public class Order
    {
        public int Id { get; set; }
        public string CustomerId { get; set; } = string.Empty;
        public OrderType OrderType { get; set; } = OrderType.Pickup;
        public string DeliveryAddress { get; set; } = string.Empty;
        public string ContactPhone { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public OrderStatus Status { get; set; } = OrderStatus.New;
        public decimal TotalAmount { get; set; }

        public ApplicationUser? Customer { get; set; }
        public List<OrderItem> Items { get; set; } = new();
    }
}
