using System.ComponentModel.DataAnnotations;

namespace AuthSample.ViewModels.Account
{
    public class EmailAddressViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}