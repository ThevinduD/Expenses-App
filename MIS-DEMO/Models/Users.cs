using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MIS_DEMO.Models
{
    [Keyless]
    [Table("USERS")]
    public class Users
    {
        [Column("UserName")]
        [Required]
        public string UserName { get; set; } = null!;

        [Column("Password")]
        [Required]
        public string Password { get; set; } = null!;

        [Column("Description")]
        public string Description { get; set; } = null!;
    }
}
