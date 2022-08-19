using System.ComponentModel.DataAnnotations;

namespace AuthSample.ViewModels.Admin
{
    public class CreateRoleViewModel
    {
        [Required]
        [Display(Name = "角色")]
        public string RoleName { get; set; }
    }
}