namespace FoodFlow.ViewModels
{
    public class AdminReportsViewModel
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public int OrdersCount { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal AverageCheck { get; set; }
        public List<TopMenuItemReportRow> TopMenuItems { get; set; } = new();
        public List<DailySalesReportRow> DailySales { get; set; } = new();
        public List<LowStockReportRow> LowStockProducts { get; set; } = new();
    }

    public class TopMenuItemReportRow
    {
        public int MenuItemId { get; set; }
        public string MenuItemName { get; set; } = string.Empty;
        public int QuantitySold { get; set; }
        public decimal Revenue { get; set; }
    }

    public class LowStockReportRow
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public decimal QuantityInStock { get; set; }
        public decimal ReorderLevel { get; set; }
        public string Unit { get; set; } = "g";
    }

    public class DailySalesReportRow
    {
        public DateTime Date { get; set; }
        public int OrdersCount { get; set; }
        public decimal Revenue { get; set; }
    }
}
