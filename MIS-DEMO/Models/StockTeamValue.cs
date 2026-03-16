using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace MIS_DEMO.Models
{
    [Keyless]
    [Table("VW_STOCK_TEAM_VALUE")]
    public class StockTeamValue
    {
        [Column("TeamCode")]
        public string TeamCode { get; set; }

        [Column("TeamName")]
        public string TeamName { get; set; }

        public string SupCode { get; set; }
        public string ItemRefNo { get; set; }
        public string ItemID { get; set; }

        [Column("BatchNo")]
        public string BatchNo { get; set; }

        public decimal StockQty { get; set; }
        public decimal CostPrice { get; set; }
        public decimal? StockValue { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public int? ExpiryDays { get; set; }

        [Column("ShipmentDate")]
        public DateTime? ShipmentDate { get; set; }
        public int? AgingDays { get; set; }
        public string Description { get; set; }
        public string SupName { get; set; }
    }
}
