namespace MIS_DEMO.Models.ViewModels
{
    public class RepPerformanceDetailsViewModel
    {
        public string MonthLabel { get; set; }
        public int MonthOffset { get; set; }
        public List<RepPerformanceRow> Rows { get; set; } = new();

        // Top Summary KPIs
        public decimal TotalGross { get; set; }
        public decimal TotalReturn { get; set; }
        public decimal TotalNet { get; set; }
        public decimal TotalTarget { get; set; }
    }

    public class RepPerformanceRow
    {
        public string Team { get; set; }
        public string RepCode { get; set; }
        public string RepName { get; set; }
        public decimal GrossSale { get; set; }
        public decimal ReturnSale { get; set; }
        public decimal NetSale { get; set; }
        public decimal Target { get; set; }

        // Calculate Run Rate dynamically. Protect against divide-by-zero!
        public decimal RunRatePercent => Target > 0
            ? Math.Round((NetSale / Target) * 100, 1)
            : (NetSale > 0 ? 100 : 0);
    }
}