using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Tournament.Models
{
    public class ChangePasswordViewModel
    {
        [EmailAddress]
        [Required]
        [Display(Name = "User ID")]
        public string EmailAddress { get; set; }

        [Required]
        [Display(Name = "Current Password")]
        public string OldPassword { get; set; }

        [Required]
        [Display(Name = "New Password")]
        public string Password { get; set; }

        [Required]
        [Display(Name = "Confirm New Password")]
        public string Confirm { get; set; }
    }
}