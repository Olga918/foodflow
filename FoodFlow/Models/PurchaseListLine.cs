namespace FoodFlow.Models
{
    public class PurchaseListLine
    {
        public int Id { get; set; }
        public int PurchaseListId { get; set; }
        public int ProductId { get; set; }

        /// <summary>Planned quantity to buy (from reorder rules).</summary>
        public decimal SuggestedQuantity { get; set; }

        /// <summary>Actually received on warehouse (cumulative).</summary>
        public decimal ReceivedQuantity { get; set; }

        public PurchaseList? PurchaseList { get; set; }
        public Product? Product { get; set; }
    }
}
