namespace MIS_DEMO.Models.ViewModels
{
    public class TeamNonDeliveredInvoicesViewModel
    {
        public string Team { get; set; } = "";
        public string Bucket { get; set; } = "under"; // under | over
        public DateTime CutoffDate { get; set; }

        // filters
        public string? Invoice { get; set; }
        public string? Rep { get; set; }
        public string? Customer { get; set; }

        // data
        public int TotalRows { get; set; }
        public decimal TotalAmount { get; set; }
        public List<NonDeliveredInvoiceLineVm> Rows { get; set; } = new();
    }
}
