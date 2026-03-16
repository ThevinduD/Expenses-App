using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MIS_DEMO.Models
{
    [Table("SUPPLIER_ASM")]
    public class SupplierAsm
    {
        [Key]
        public int Id { get; set; }   // if table has no PK, keep this anyway

        public string ASMCODE { get; set; }
        public string SUPCODE { get; set; }
    }
}
