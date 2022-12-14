using Microsoft.AspNetCore.Authentication;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AuthSample.ViewModels.Account
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "請輸入帳號")]
        [EmailAddress]
        public string Email { get; set; }

        [Required(ErrorMessage = "請輸入密碼")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Display(Name = "記住密碼")]
        public bool RememberMe { get; set; }

        // OAuth 需要參數 ReturnUrl、ExternalLogins
        public string  ReturnUrl { get; set; }
        public IList<AuthenticationScheme> ExternalLogins { get; set; }
    }
}
