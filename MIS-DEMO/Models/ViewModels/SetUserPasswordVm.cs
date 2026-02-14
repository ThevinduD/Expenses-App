using System.ComponentModel.DataAnnotations;

namespace MIS_DEMO.Models.ViewModels
{
    public class SetUserPasswordVm
    {
        [Required]
        public string UserName { get; set; } = "";

        [Required, MinLength(4)]
        public string NewPassword { get; set; } = "";

        [Required, Compare(nameof(NewPassword))]
        public string ConfirmPassword { get; set; } = "";
    }
}
