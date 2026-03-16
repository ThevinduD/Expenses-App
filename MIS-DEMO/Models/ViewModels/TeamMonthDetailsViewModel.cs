namespace MIS_DEMO.Models.ViewModels
{
    public class TeamMonthDetailsViewModel
    {
        public string LocShort { get; set; } = "";

        // UI mode: "item" | "rep" | "customer"
        public string Mode { get; set; } = "item";

        // Single search box (meaning depends on mode)
        public string? Search { get; set; }

        public decimal NetTotal { get; set; }
        public int MonthOffset { get; set; }
        public string MonthLabel { get; set; }

        // Columns for table
        public List<string> Columns { get; set; } = new();

        // Generic rows (each row = list of cell strings)
        public List<List<string>> TableRows { get; set; } = new();

        // Footer totals (optional)
        public string FooterLabel { get; set; } = "TOTAL";
        public decimal FooterQty { get; set; }
        public decimal FooterAmount { get; set; }

        public List<string> Suggestions { get; set; } = new();
    }
}