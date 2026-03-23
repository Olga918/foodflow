using FoodFlow.Enums;

namespace FoodFlow.Models
{
    /// <summary>Admin procurement list (what to buy / what was received).</summary>
    public class PurchaseList
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? CreatedByUserId { get; set; }
        public PurchaseListStatus Status { get; set; }
        public string Note { get; set; } = string.Empty;

        public List<PurchaseListLine> Lines { get; set; } = new();
    }
}
