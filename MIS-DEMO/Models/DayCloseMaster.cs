using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MIS_DEMO.Models
{
    [Table("DayCloseMaster")]
    public class DayCloseMaster
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public DateTime CloseDate { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal OfficialMileage { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal PersonalMileage { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal RealMileage { get; set; }

        public int DoctorVisits { get; set; }

        public int PharmacyVisits { get; set; }

        public string Remark { get; set; }

        public string Username { get; set; }

        public string RepCode { get; set; }
    }
}