namespace MIS_DEMO.Models.ViewModels
{
    public class TodaySalesDetailsViewModel
    {
        public DateTime Date { get; set; }

        public List<SalesLineViewModel> SalesLines { get; set; } = new();
        public decimal SalesTotal { get; set; }

        public List<ReturnLineViewModel> ReturnLines { get; set; } = new();
        public decimal ReturnTotal { get; set; }

        public decimal NetTotal => SalesTotal - ReturnTotal;
    }
}
