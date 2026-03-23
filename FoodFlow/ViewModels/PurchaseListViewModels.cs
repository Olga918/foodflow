using FoodFlow.Enums;

namespace FoodFlow.ViewModels
{
    public sealed class PurchaseListSummaryRow
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public PurchaseListStatus Status { get; set; }
        public string Note { get; set; } = string.Empty;
        public int LinesCount { get; set; }
        public decimal TotalSuggested { get; set; }
        public decimal TotalReceived { get; set; }
    }

    public sealed class PurchaseListLineRow
    {
        public int LineId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string Unit { get; set; } = string.Empty;
        public decimal CurrentStock { get; set; }
        public decimal SuggestedQuantity { get; set; }
        public decimal ReceivedQuantity { get; set; }
        public decimal Remaining => Math.Max(0, SuggestedQuantity - ReceivedQuantity);
    }

    public sealed class PurchaseListDetailViewModel
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public PurchaseListStatus Status { get; set; }
        public string Note { get; set; } = string.Empty;
        public List<PurchaseListLineRow> Lines { get; set; } = new();
    }
}
