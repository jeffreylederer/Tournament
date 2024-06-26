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
    
    public partial class Team
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Team()
        {
            this.Matches = new HashSet<Match>();
            this.Matches1 = new HashSet<Match>();
        }
    
        public int id { get; set; }
        public Nullable<int> Skip { get; set; }
        public Nullable<int> ViceSkip { get; set; }
        public Nullable<int> Lead { get; set; }
        public int Leagueid { get; set; }
        public int TeamNo { get; set; }
        public byte[] rowversion { get; set; }
        public short DivisionId { get; set; }
    
        public virtual League League { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Match> Matches { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Match> Matches1 { get; set; }
        public virtual Player Player { get; set; }
        public virtual Player Player1 { get; set; }
        public virtual Player Player2 { get; set; }
    }
}
