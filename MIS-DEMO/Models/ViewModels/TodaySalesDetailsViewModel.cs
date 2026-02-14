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
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }

        //ROW COUNTS 
        public int MaxRows { get; set; } = 1000;

        public int SalesTotalRows { get; set; }
        public int ReturnTotalRows { get; set; }

        public bool SalesIsTruncated { get; set; }
        public bool ReturnIsTruncated { get; set; }
        public string? TruncateNote { get; set; }



    }
}
