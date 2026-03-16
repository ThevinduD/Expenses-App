namespace MIS_DEMO.Models.ViewModels
{
    public class DeliveredInvoicesViewModel
    {
        public DateTime MonthStart { get; set; }
        public DateTime MonthEndExclusive { get; set; }

        public List<DeliveredTeamRowVm> Rows { get; set; } = new();

        public int DeliveredCountTotal { get; set; }
        public decimal DeliveredAmountTotal { get; set; }
    }

    public class DeliveredTeamRowVm
    {
        public string Team { get; set; } = "";
        public int DeliveredCount { get; set; }
        public decimal DeliveredAmount { get; set; }
    }
}
