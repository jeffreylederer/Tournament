using System.ComponentModel.DataAnnotations;

namespace Tournament.Models
{
    public class ResetPasswordViewModel
    {
        [Required]
        [Display(Name = "Activation Code")]
        public string ActivationCode { get; set; }

        
        [Required]
        [Display(Name = "New Password")]
        public string Password { get; set; }

        [Required]
        [Display(Name = "Confirm New Password")]
        public string Confirm { get; set; }
    }
}