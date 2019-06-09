using System;
using System.ComponentModel.DataAnnotations;

namespace Tournament.Models
{
    public class ForgotPasswordViewModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "User Id")]
        public string EmailId { get; set; }
    }
}