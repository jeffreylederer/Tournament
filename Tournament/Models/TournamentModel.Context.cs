﻿//------------------------------------------------------------------------------
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
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Core.Objects;
    using System.Linq;
    
    public partial class TournamentEntities : DbContext
    {
        public TournamentEntities()
            : base("name=TournamentEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<League> Leagues { get; set; }
        public virtual DbSet<Match> Matches { get; set; }
        public virtual DbSet<Membership> Memberships { get; set; }
        public virtual DbSet<Player> Players { get; set; }
        public virtual DbSet<Schedule> Schedules { get; set; }
        public virtual DbSet<Team> Teams { get; set; }
        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<UserLeague> UserLeagues { get; set; }
        public virtual DbSet<RinkOrder> RinkOrders { get; set; }
    
        public virtual ObjectResult<GetMatchAll_Result> GetMatchAll(Nullable<int> weekId)
        {
            var weekIdParameter = weekId.HasValue ?
                new ObjectParameter("WeekId", weekId) :
                new ObjectParameter("WeekId", typeof(int));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<GetMatchAll_Result>("GetMatchAll", weekIdParameter);
        }
    }
}
