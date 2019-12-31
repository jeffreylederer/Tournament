using System.Collections.Generic;

namespace Tournament.Models
{
    public class UserLeagueViewModel
    {
        public int leagueid { get; set; }
        public string LeagueName { get; set; }
        public IEnumerable<UserLeague> userLeagues { get; set; }
    }
}