namespace MIS_DEMO.Models.ViewModels
{
    public class DashboardViewModel
    {
        public decimal TodayTotalSales { get; set; }
        public decimal TodayTotalReturns { get; set; } 
        public decimal TodayNetSales => TodayTotalSales - TodayTotalReturns;
        public decimal NonDeliveredUnder45 { get; set; }
        public decimal NonDeliveredOver45 { get; set; }
        public decimal ThisMonthDeliveredTotal { get; set; }
        public List<TeamWiseSaleRowVm> TeamWiseSales { get; set; } = new();
        public decimal TeamWiseThisMonthTotal { get; set; }
        public decimal TeamWiseLastMonthTotal { get; set; }

        public decimal TotalStockValue { get; set; } = 0;
        public decimal TotalExpiringSoonValue { get; set; }
        public List<TeamStockRowVm> TeamStockValues { get; set; } = new();
        public List<TopRepSaleRow> TopReps { get; set; } = new();
    }

    public class TeamStockRowVm
    {
        public string TeamCode { get; set; }
        public string TeamName { get; set;  }
        public decimal StockValue { get; set; }
        public decimal ExpiringSoonValue { get; set; }
    }

    public class TopRepSaleRow
    {
        public string Team { get; set; }
        public string RepName { get; set; }
        public decimal NetSale { get; set; }
    }

    public class TopPerformersKpiViewModel
    {
        public List<TopRepSaleRow> TopReps { get; set; } = new();
        public int CurrentMonthOffset { get; set; }
        public string MonthLabel { get; set; }
        public bool IsTop { get; set; } = true; // NEW: Tracks the toggle state
    }
}
