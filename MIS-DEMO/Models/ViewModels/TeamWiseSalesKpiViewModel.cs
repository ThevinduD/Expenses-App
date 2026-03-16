namespace MIS_DEMO.Models.ViewModels
{
    public class TeamWiseSalesKpiViewModel
    {
        public List<TeamWiseSaleRowVm> TeamWiseSales { get; set; } = new();
        public decimal TeamWiseThisMonthTotal { get; set; }
        public decimal TeamWiseLastMonthTotal { get; set; }

        // NEW: Properties for the time-travel feature
        public int CurrentMonthOffset { get; set; }
        public string ThisMonthLabel { get; set; }
        public string LastMonthLabel { get; set; }
    }
}
