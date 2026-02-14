namespace MIS_DEMO.Models
{
    public class TeamWiseSaleRowVm
    {
        public string LocShort { get; set; } = "";
        public decimal ThisMonth { get; set; }
        public decimal LastMonth { get; set; }

        public decimal RatePercent
        {
            get
            {
                if (LastMonth <= 0)
                    return ThisMonth > 0 ? 100 : 0;

                return Math.Round((ThisMonth / LastMonth) * 100, 1);
            }
        }
    }
}
