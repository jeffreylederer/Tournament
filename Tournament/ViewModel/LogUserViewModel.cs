
using System.ComponentModel.DataAnnotations;
namespace Tournament.Models
{
    public class LogUserViewModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "User Id")]
        public string Username { get; set; }

        [Required]
        public string Password { get; set; }
    }
}