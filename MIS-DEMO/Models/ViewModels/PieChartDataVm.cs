namespace MIS_DEMO.Models.ViewModels
{
    public class PieChartDataVm
    {
        public List<string> Labels { get; set; } = new();
        public List<decimal> Values { get; set; } = new();
    }
}
