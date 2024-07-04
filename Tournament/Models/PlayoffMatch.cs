using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Tournament.Models
{
    public class PlayoffMatch
    {
        
        [Required]
        
        [Display(Description = "Team No 1")]
        public int TeamNo1 { get; set; }

        [Required]
        [Display(Description = "Team No 2")]
       
        [CustomCompare("TeamNo1", ErrorMessage="Teams nust be different")]
        public int TeamNo2 { get; set; }
        
        public int WeekId { get; set; }

        public int LeagueId { get; set; }

        [Display(Description ="Date")]
        public string GameDate { get; set; }

    }

    
}