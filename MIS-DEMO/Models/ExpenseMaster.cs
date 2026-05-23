using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MIS_DEMO.Models
{
    [Table("ExpenseMaster")]
    public class ExpenseMaster
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public DateTime ExpenseDate { get; set; }

        public string Username { get; set; }
        public string RepCode { get; set; }

        public string TranNo { get; set; }

        public string ExpenseType { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        public string Remark { get; set; }

        public string DocumentPath { get; set; } 
        public string Status { get; set; } = "Pending";
    }
}