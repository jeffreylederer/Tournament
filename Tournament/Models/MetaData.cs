using System.ComponentModel.DataAnnotations;

namespace Tournament.Models
{
    [MetadataType(typeof(PlayerMetaData))]
    public partial class Player
    {
    }
    public class PlayerMetaData
    {

        [Display(Name = "First Name")]
        [StringLength(30)]
        public string FirstName { get; set; }

        [Display(Name = "Last Name")]
        [StringLength(30)]
        public string LastName { get; set; }

        [Display(Name = "Shorten Name")]
        [StringLength(25)]
        public string shortname { get; set; }

        [Display(Name = "In Tuesday League")]
        public bool TuesdayLeague { get; set; }
        [Display(Name = "In Wednesday League")]
        public bool WednesdayLeague { get; set; }

        [Display(Name = "Player")]
        public string FullName { get; set; }

        [Display(Name = "Nickname")]
        public string NickName { get; set; }
    }

    [MetadataType(typeof(TeamMetaData))]
    public partial class Team
    {
    }
    public partial class TeamMetaData
    {
        [Display(Name = "Team Number")]
        [Required]
        public int id { get; set; }

        [Display(Name = "Vice Skip")]
        public string ViceSkip { get; set; }
    }


    [MetadataType(typeof(ScheduleMetaData))]
    public partial class Schedule
    {
    }
    public partial class ScheduleMetaData
    {
        [Required]
        public int id { get; set; }

        [Display(Name = "Round Number")]
        public int RoundNo { get; set; }
    }
}