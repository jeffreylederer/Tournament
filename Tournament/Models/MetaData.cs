using System;
using System.CodeDom;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Security.Policy;

namespace Tournament.Models
{

    [MetadataType(typeof(MembershipMetaData))]
    public partial class Membership
    {
    }

    public class MembershipMetaData
    {

        [Display(Name = "First Name")]
        [Required]
        [StringLength(30)]
        public string FirstName { get; set; }

        [Display(Name = "Last Name")]
        [Required]
        [StringLength(30)]
        public string LastName { get; set; }

        [Display(Name = "Shorten Name")]
        [StringLength(25)]
        public string shortname { get; set; }
        
        [Display(Name = "Nickname")]
        public string NickName { get; set; }

        [Timestamp]
        public byte[] rowversion { get; set; }
    }

    [MetadataType(typeof(PlayerMetaData))]
    public partial class Player
    {
    }
    public class PlayerMetaData
    {
        [Required]
        [Display(Name="Player")]
        public int MembershipId { get; set; }
        [Timestamp]
        public byte[] rowversion { get; set; }

    }

    [MetadataType(typeof(TeamMetaData))]
    public partial class Team
    {
    }
    public class TeamMetaData
    {
        [Display(Name = "Team Number")]
        [Required]
        public int TeamNo { get; set; }

        [Display(Name = "Vice Skip")]
        public string ViceSkip { get; set; }

        [Display(Name = "League Name")]
        public string Leagueid { get; set; }

        [Timestamp]
        public byte[] rowversion { get; set; }
    }


    [MetadataType(typeof(ScheduleMetaData))]
    public partial class Schedule
    {
    }
    public class ScheduleMetaData
    {
        [Required]
        public int id { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Game Date")]
        [DisplayFormat(DataFormatString = "{0:d}")]
        [Required]
        public DateTime GameDate { get; set; }

        [Display(Name = "Week Number")]
        [Required]
        public int WeekNumber { get; set; }

        [Timestamp]
        public byte[] rowversion { get; set; }
    }

    [MetadataType(typeof(MatchMetaData))]
    public partial class Match
    {
    }

    public class MatchMetaData
    {

        [Display(Name = "Game Number")]
        public int id { get; set; }

        [Display(Name = "Date")]
        [Required]
        public int WeekId { get; set; }

        [Required]
        public int Rink { get; set; }

        [Display(Name = "Team 1")]
        [Required]
        public int TeamNo1 { get; set; }

        [Display(Name = "Team 2")]
        public int TeamNo2 { get; set; }

        [Display(Name = "Team 1 Score")]
        public int Team1Score { get; set; }

        [Display(Name = "Team 2 Score")]
        public int Team2Score { get; set; }

        [Display(Name = "Team Forfeiting")]
        public int ForFeitId { get; set; }

        [Timestamp]
        public byte[] rowversion { get; set; }
    }

    [MetadataType(typeof(LeagueMetaData))]
    public partial class League
    {
    }

    public class LeagueMetaData
    {
        [Display(Name = "League Name")]
        [Required]
        public string LeagueName { get; set; }

        [Display(Name = "Still Playing")]
        public bool Active { get; set; }

        [Display(Name = "Team Size")]
        [Range(1,3,ErrorMessage = "Teams may have 1,2, or 3 players")]
        [Required]
        public int TeamSize { get; set; }

        [Required]
        [Display(Name = "Are ties allowed")]
        public bool TiesAllowed { get; set; }

        [Required]
        [Display(Name = "Do scores count")]
        public bool PointsCount { get; set; }

        [Range(1,3)]
        [Required]
        [Display(Name = "Multliplier for a win")]
        public short WinPoints { get; set; }

        [Range(1, 3)]
        [Required]
        [Display(Name = "Multliplier for a tie")]
        public short TiePoints { get; set; }

        [Range(1, 3)]
        [Required]
        [Display(Name = "Multliplier for a bye")]
        public short ByePoints { get; set; }

        [Required]
        [Range(minimum: 1, maximum:99, ErrorMessage = "Numbering starts at 1")]
        [Display(Name = "Start Week")]
        public short StartWeek { get; set; }



        [Timestamp]
        public byte[] rowversion { get; set; }
    }

    [MetadataType(typeof(UserLeagueMetaData))]
    public partial class UserLeague
    {
    }

    public class UserLeagueMetaData
    {
        [Display(Name = "User")]
        public int UserId { get; set; }

        [Required]
        [Display(Name = "League Name")]
        public int LeagueId { get; set; }

        [Required]
        [Display(Name = "Role")]
        public string Roles { get; set; }

        [Timestamp]
        public byte[] rowversion { get; set; }
    }

    [MetadataType(typeof(UserMetaData))]
    public partial class User
    {
    }

    public class UserMetaData
    {
        [Display(Name = "User EmailAddress")]
        [EmailAddress]
        public int username { get; set; }

        [Display(Name = "Role")]
        public string Roles { get; set; }

        [Timestamp]
        public byte[] rowversion { get; set; }
    }

    [MetadataType(typeof(RinkOrderMetaData))]
    public partial class RinkOrder
    {
    }
    public class RinkOrderMetaData
    {
        [Required]
        [Display(Name = "Week")]
        public int id { get; set; }

        [Required]
        public string Green { get; set; }

        [Required]
        public string Direction { get; set; }

        [Required]
        public string Boundary { get; set; }

        [Timestamp]
        public byte[] rowversion { get; set; }
    }
}