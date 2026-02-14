namespace MIS_DEMO.Models.ViewModels
{
    public class NonDeliveredInvoicesViewModel
    {
        public DateTime CutoffDate { get; set; } // today-45
        public List<NonDeliveredTeamRowVm> Rows { get; set; } = new();

        public int Under45CountTotal { get; set; }
        public decimal Under45AmountTotal { get; set; }

        public int Over45CountTotal { get; set; }
        public decimal Over45AmountTotal { get; set; }

        // Convenience (NET optional)
        public decimal GrandTotalAmount => Under45AmountTotal + Over45AmountTotal;
        public int GrandTotalCount => Under45CountTotal + Over45CountTotal;
    }

    public class NonDeliveredTeamRowVm
    {
        public string Team { get; set; } = "";

        public int Under45Count { get; set; }
        public decimal Under45Amount { get; set; }

        public int Over45Count { get; set; }
        public decimal Over45Amount { get; set; }
    }
}
