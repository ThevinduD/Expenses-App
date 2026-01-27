namespace MIS_DEMO.Models.ViewModels
{
    public class SalesSummaryViewModel
    {
        //SALES
        public decimal TodaySales { get; set; }
        public decimal YesterdaySales { get; set; }
        public decimal ThisMonthSales { get; set; }
        public decimal LastMonthSales { get; set; }

        // RETURNS
        public decimal TodayReturns { get; set; }
        public decimal YesterdayReturns { get; set; }
        public decimal ThisMonthReturns { get; set; }
        public decimal LastMonthReturns { get; set; }

        // NET
        public decimal TodayNet => TodaySales - TodayReturns;
        public decimal YesterdayNet => YesterdaySales - YesterdayReturns;
        public decimal ThisMonthNet => ThisMonthSales - ThisMonthReturns;
        public decimal LastMonthNet => LastMonthSales - LastMonthReturns;
    }
}
