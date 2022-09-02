using System.ComponentModel.DataAnnotations;

namespace AuthSample.ViewModels.Account
{
    public class ResetPasswordViewModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "密碼")]
        public string Password { get; set; }
        [DataType(DataType.Password)]
        [Display(Name = "確認密碼")]
        [Compare(nameof(Password),ErrorMessage = "密碼不一置，請重新確認")]
        public string ConfirmPassword { get; set; }
        public string Token { get; set; }
    }
}
