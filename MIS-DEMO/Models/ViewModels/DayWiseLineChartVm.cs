namespace MIS_DEMO.Models.ViewModels
{
    public class DayWiseLineChartVm
    {
        public List<string> Labels { get; set; } = new();
        public List<decimal> Today { get; set; } = new();
        public List<decimal> Yesterday { get; set; } = new();
    }
}
