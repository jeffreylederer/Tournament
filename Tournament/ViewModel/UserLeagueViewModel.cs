using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Tournament.Models
{
    public class UserLeagueViewModel
    {
        [Key]
        public int leagueid { get; set; }
        public string LeagueName { get; set; }
        public IEnumerable<UserLeague> userLeagues { get; set; }
    }
}