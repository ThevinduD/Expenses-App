namespace MIS_DEMO.Models.ViewModels
{
    public class DashboardViewModel
    {
        public decimal TodayTotalSales { get; set; }
        public decimal TodayTotalReturns { get; set; } 
        public decimal TodayNetSales => TodayTotalSales - TodayTotalReturns;
        public decimal NonDeliveredUnder45 { get; set; }
        public decimal NonDeliveredOver45 { get; set; }
        public List<TeamWiseSaleRowVm> TeamWiseSales { get; set; } = new();
        public decimal TeamWiseThisMonthTotal { get; set; }
        public decimal TeamWiseLastMonthTotal { get; set; }


    }
}
