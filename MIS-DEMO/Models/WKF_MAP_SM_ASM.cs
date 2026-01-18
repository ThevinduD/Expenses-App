using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MIS_DEMO.Models
{
    [Keyless] // Because this table probably doesn't have a PK
    [Table("WKF_MAP_SM_ASM")]
    public class WKF_MAP_SM_ASM
    {
        public string UserNameSM { get; set; }
        public string UserNameASM { get; set; }
    }
}
