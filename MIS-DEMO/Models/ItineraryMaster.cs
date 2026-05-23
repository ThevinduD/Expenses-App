using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MIS_DEMO.Models
{
    [Table("ItineraryMaster")]
    public class ItineraryMaster
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public DateTime ItineraryDate { get; set; }

        [Required]
        public string ItineraryType { get; set; }

        public string Remark { get; set; }

        public string Username { get; set; }

        public string RepCode { get; set; }
    }
}