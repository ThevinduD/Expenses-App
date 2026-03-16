using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MIS_DEMO.Models
{
    [Table("DIR_SUP_MAP")]
    public class DirSupMap
    {
        [Key]
        public int Id { get; set; }

        public string UserNameDir { get; set; }
        public string SupCode { get; set; }
    }
}
