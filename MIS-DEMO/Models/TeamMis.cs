namespace MIS_DEMO.Models
{
    public class TeamMis
    {
        public string LocCode { get; set; } = "";   // e.g., L010 (same as Pat_Name)
        public string? LocShort { get; set; }       // e.g., GA, GB, etc
        public string? LocName { get; set; }
    }
}
