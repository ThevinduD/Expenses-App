namespace MIS_DEMO.Models.ViewModels
{
    public class NonDeliveredInvoiceLineVm
    {
        public string? InvoiceNo { get; set; }
        public DateTime RefDate { get; set; }
        public decimal Amount { get; set; }
        public string? Customer { get; set; }
        public string? SalesRep { get; set; }
        public string? SalesRepCode { get; set; }
    }
}
