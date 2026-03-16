namespace MIS_DEMO.Models.ViewModels
{
    public class TeamStockDetailsViewModel
    {
        public string TeamCode { get; set; }
        public string TeamName { get; set; }
        public decimal TotalStockQty { get; set; }
        public decimal TotalStockValue { get; set; }
        public List<StockDetailLine> StockLines { get; set; } = new();
    }

    public class StockDetailLine
    {
        public string ItemID { get; set; }
        public string BatchNo { get; set; }
        public string Description { get; set; }
        public string SupName { get; set; }
        public decimal StockQty { get; set; }
        public decimal CostPrice { get; set; }
        public decimal StockValue { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public DateTime? ShipmentDate { get; set; }
    }
}
