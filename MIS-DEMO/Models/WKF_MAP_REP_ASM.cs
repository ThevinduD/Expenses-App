using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MIS_DEMO.Models
{
    [Keyless]
    public class WKF_MAP_REP_ASM
    {
        public string UserName { get; set; }
        public string SalesRepCode { get; set; }
    }
}
