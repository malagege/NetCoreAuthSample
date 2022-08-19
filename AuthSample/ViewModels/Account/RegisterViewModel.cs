using System.ComponentModel.DataAnnotations;

namespace AuthSample.ViewModels.Account
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "信箱為必填")]
        [EmailAddress(ErrorMessage = "信箱格式不正確")]
        [Display(Name = "信箱")]
        public string Email { get; set; }

        [Required(ErrorMessage = "密碼為必填")]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "確任密碼")]
        [Compare("Password", ErrorMessage = "兩次密碼不一致")]
        public string ConfirmPassword { get; set; }

        [Display(Name = "城市")]
        public string City { get; set; }
    }
}