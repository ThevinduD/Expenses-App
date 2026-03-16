using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace MIS_DEMO.Models
{
    [Keyless] // CRITICAL: Tells EF Core this table has no primary key
    [Table("TARGET_MONTHS_REP_SPECIAL")]
    public class TargetMonthsRepSpecial
    {
        public string? TranNo { get; set; }
        public string? UserNameAsm { get; set; }
        public string? SalesRepCode { get; set; }
        public int? MonthNo { get; set; }
        public decimal? TargetActual { get; set; }
        public string? UserName { get; set; }
        public DateTime? SysDate { get; set; }
        public string? MonthNameTarget { get; set; }
        public long? TargetRefNo { get; set; } // bigint maps to long
    }
}