//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Tournament.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class League
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public League()
        {
            this.Players = new HashSet<Player>();
            this.Schedules = new HashSet<Schedule>();
            this.Teams = new HashSet<Team>();
            this.UserLeagues = new HashSet<UserLeague>();
        }
    
        public int id { get; set; }
        public string LeagueName { get; set; }
        public bool Active { get; set; }
        public int TeamSize { get; set; }
        public byte[] rowversion { get; set; }
        public bool TiesAllowed { get; set; }
        public bool PointsCount { get; set; }
        public short WinPoints { get; set; }
        public short TiePoints { get; set; }
        public short ByePoints { get; set; }
        public int StartWeek { get; set; }
        public short Divisions { get; set; }
        public bool PointsLimit { get; set; }
        public bool PlayOffs { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Player> Players { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Schedule> Schedules { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Team> Teams { get; set; }
        public virtual RinkOrder RinkOrder { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<UserLeague> UserLeagues { get; set; }
    }
}
