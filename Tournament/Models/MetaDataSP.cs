using System;
using System.ComponentModel.DataAnnotations;

namespace Tournament.Models
{
    [MetadataType(typeof(MembershipAllowDelete_Result_MetaData))]
    public partial class MembershipAllowDelete_Result
    {
    }
    public class MembershipAllowDelete_Result_MetaData
    {

        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Display(Name = "Last Name")]
        public string LastName { get; set; }
        public string FullName { get; set; }

        [Display(Name = "Shorten Name")]
        public string shortname { get; set; }

        [Display(Name = "Nickname")]
        public string NickName { get; set; }

    }

    [MetadataType(typeof(TeamAllowDelete_Result_MetaData))]
    public partial class TeamAllowDelete_Result
    {
    }

    public class TeamAllowDelete_Result_MetaData
    {
        [Display(Name = "Skip")]
        public string skip { get; set; }

        [Display(Name = "Vice Skip")]
        public string ViceSkip { get; set; }

        [Display(Name = "Team Number")]
        public int TeamNo { get; set; }
    }

    [MetadataType(typeof(PlayerAllowDelete_Result_MetaData))]
    public partial class PlayerAllowDelete_Result
    {
    }

    public class PlayerAllowDelete_Result_MetaData
    {
        public int id { get; set; }
        public Nullable<int> cnt { get; set; }
        public string FullName { get; set; }

        [Display(Name = "Last Name")]
        public string LastName { get; set; }
    }

    [MetadataType(typeof(LeagueAllowDelete_Resultt_MetaData))]
    public partial class LeagueAllowDelete_Result
    {
    }
    public class LeagueAllowDelete_Resultt_MetaData
    {

        [Display(Name = "League Name")]
        public string LeagueName { get; set; }

        [Display(Name = "Still Playing")]
        public bool Active { get; set; }

        [Display(Name = "Team Size")]
        public int TeamSize { get; set; }

        [Display(Name = "Are ties allowed")]
        public bool TiesAllowed { get; set; }

        [Display(Name = "Do scores count")]
        public bool PointsCount { get; set; }

        [Display(Name = "Multliplier for a win")]
        public short WinPoints { get; set; }

        [Display(Name = "Multliplier for a tie")]
        public short TiePoints { get; set; }

        [Display(Name = "Multliplier for a bye")]
        public short ByePoints { get; set; }

        [Display(Name = "Start Week")]
        public int StartWeek { get; set; }
    }

    [MetadataType(typeof(ScheduleAllowDelete_Result_MetaData))]
    public partial class ScheduleAllowDelete_Result
    {
    }

    public class ScheduleAllowDelete_Result_MetaData
    {
        public int id { get; set; }
        [Display(Name = "Game Date")]
        public System.DateTime GameDate { get; set; }

        public bool Cancelled { get; set; }

        public string WeekDate { get; set; }

    }
}