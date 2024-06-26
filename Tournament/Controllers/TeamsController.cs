﻿using DocumentFormat.OpenXml.Spreadsheet;
using Elmah;
using Microsoft.Reporting.WebForms;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using System.Web.UI.WebControls;
using Tournament.Model;
using Tournament.Models;
using Tournament.ReportFiles;
using Tournament.ViewModel;
using static Tournament.ReportFiles.TournamentDS;

namespace Tournament.Controllers
{

    public class TeamsController : Controller
    {
        private readonly TournamentEntities _db = new TournamentEntities();
       

       [Authorize]
        // GET: Teams
        public ActionResult Index(int id)
       {
        ViewBag.Id = id;
        var league = _db.Leagues.Find(id);
        if (league == null)
            return HttpNotFound();
        ViewBag.TeamSize = league.TeamSize;
        string message = string.Empty;
        var list = _db.TeamAllowDelete(id).OrderBy(x => x.TeamNo).OrderBy(x => x.Division).ToList();
        if (league.Divisions > 1)
        {
            message = "Number of teams, Division 1=" + list.Count(x => x.Division == 1);
            for (int i = 1; i < league.Divisions; i++)
                message += $", Division {i + 1}={list.Count(x => x.Division == i + 1)}";
        }
        ViewBag.Message = message;
            ViewBag.NumberDivisions = league.Divisions;
        return View(list);
     }

        

        [Authorize]
        public ActionResult TeamReport(int id)
        {
            ViewBag.Id = id;
            var reportViewer = new ReportViewer()
            {
                ProcessingMode = ProcessingMode.Local,
                Width = Unit.Pixel(800),
                Height = Unit.Pixel(1000),
                ShowExportControls =  true
            };
            var league = _db.Leagues.Find(id);
            if (league == null)
                return HttpNotFound();
            switch (league.TeamSize)
            {
                case 1:
                    reportViewer.LocalReport.ReportPath = Server.MapPath(" /ReportFiles/SingleTeams.rdlc");
                    break;
                case 2:
                    reportViewer.LocalReport.ReportPath = Server.MapPath("/ReportFiles/PairsTeams.rdlc");
                    break;
                case 3:
                    reportViewer.LocalReport.ReportPath = Server.MapPath("/ReportFiles/TriplesTeams.rdlc");
                    break;
            }

            var p2 = new ReportParameter("Description", league.LeagueName);
            var p1 = new ReportParameter("Divisions", league.Divisions.ToString());
            reportViewer.LocalReport.SetParameters(new ReportParameter[] { p2, p1 });

            var teams = _db.Teams.Where(x => x.Leagueid == id).ToList().OrderBy(x => x.TeamNo).OrderBy(x => x.DivisionId);
            var ds = new TournamentDS();
            foreach (var team in teams)
            {
                ds.Team.AddTeamRow(team.TeamNo, team.Player.Membership.FullName,  team.Lead== null? "" : team.Player2.Membership.FullName, team.ViceSkip == null ? "" : team.Player1.Membership.FullName, team.DivisionId);
            }
             reportViewer.LocalReport.DataSources.Add(new ReportDataSource("Team", ds.Team.Rows));

            ViewBag.ReportViewer = reportViewer;
            return View();
        }


        [Authorize(Roles = "Admin,LeagueAdmin")]
        // GET: Teams/Create
        public ActionResult Create(int id)
        {
            var league = _db.Leagues.Find(id);
            if (league == null)
                return HttpNotFound();
            int teamNo = 1;
            var list = _db.Teams.Where(x => x.Leagueid == id).ToList().OrderBy(x=>x.TeamNo).ToList();
            
            if (list.Any())
            {
                teamNo = list.Last().TeamNo + 1;
            }
            var team = new Team()
            {
                DivisionId=1,
                TeamNo = teamNo,
                Leagueid = id,
                Skip=0,
                ViceSkip = 0,
                Lead=0
            };
            var teams = _db.Teams.Where(x => x.Leagueid == team.Leagueid).OrderBy(x => x.TeamNo);
            ViewBag.Teams = teams;
            ViewBag.List = RemainingPlayers(team, teams.ToList());
            var divisions = new List<DivisionViewModel>();
            for (short i = 1; i <= league.Divisions; i++)
                divisions.Add(new DivisionViewModel() { DivisionId = i, DivisionNumber = i });

            ViewBag.Divisions = divisions;
            ViewBag.NumberDivisions = league.Divisions;
            ViewBag.TeamSize = league.TeamSize;
            return View(team);
        }

        // POST: Teams/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "id,DivisionId,Skip,Lead,ViceSkip,TeamId,LeagueId,TeamNo")] Team team)
        {
            team.Skip = team.Skip == 0 ? (int ?) null : team.Skip;
            team.ViceSkip = team.ViceSkip == 0 ? (int?)null : team.ViceSkip;
            team.Lead = team.Lead == 0 ? (int?)null : team.Lead;
            if (ModelState.IsValid)
            {
                

                    _db.Teams.Add(team);
                    try
                    {
                    if (CheckTeam(team))
                    {
                        ModelState.AddModelError(string.Empty,
                            "Unable to create record, a player cannot be on a team in multiple positions");
                    }
                    else
                    {
                        _db.SaveChanges();
                        OrderTeam(team.Leagueid);
                        return RedirectToAction("Index", new {id=team.Leagueid});
                    }
                }
                catch (System.Data.Entity.Infrastructure.DbUpdateException e)
                {
                    ErrorSignal.FromCurrentContext().Raise(e);
                    Exception ex = e;
                    while (ex.InnerException != null)
                        ex = ex.InnerException;
                    ModelState.AddModelError(string.Empty, ex.Message);

                }
                catch (Exception e)
                {
                    ErrorSignal.FromCurrentContext().Raise(e);
                    ModelState.AddModelError(string.Empty, "Insert failed");
                }

            }
            var league = _db.Leagues.Find(team.Leagueid);
            if (league == null)
                return HttpNotFound();
            var teams = _db.Teams.Where(x => x.Leagueid == team.Leagueid).OrderBy(x => x.TeamNo);
            var list = RemainingPlayers(team, teams.ToList());
            ViewBag.List = list;
            ViewBag.Teams = teams;
            ViewBag.TeamSize = league.TeamSize;
            ViewBag.NumberDivisions = league.Divisions;

            return View(team);
        }

        // GET: Teams/Edit/5
        [Authorize(Roles = "Admin,LeagueAdmin")]
        public ActionResult Edit(int? id)
        {
          
           
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Team team = _db.Teams.Find(id);
            if (team == null)
            {
                return HttpNotFound();
            }
            var teams = _db.Teams.Where(x => x.Leagueid == team.Leagueid).OrderBy(x => x.TeamNo);
            var league = _db.Leagues.Find(team.Leagueid);
            if (league == null)
                return HttpNotFound();

            var list = RemainingPlayers(team, teams.ToList());
            ViewBag.List = list;
            var divisions = new List<DivisionViewModel>();
            for (short i = 1; i <= league.Divisions; i++)
                divisions.Add(new DivisionViewModel() { DivisionId = i, DivisionNumber = i });
            ViewBag.Divisions = divisions;
            ViewBag.NumberDivisions = league.Divisions;
            ViewBag.Teams = teams.OrderBy(x=>x.TeamNo).OrderBy(x=>x.DivisionId);
            ViewBag.TeamSize = team.League.TeamSize;
            return View(team);
        }

        

        // POST: Teams/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "id,Skip,Lead,ViceSkip,TeamId,DivisionId,LeagueId,TeamNo,rowversion")] Team team)
        {
            
            try
            {
                if (ModelState.IsValid)
                {
                    team.Skip = team.Skip == 0 ? (int?)null : team.Skip;
                    team.ViceSkip = team.ViceSkip == 0 ? (int?)null : team.ViceSkip;
                    team.Lead = team.Lead == 0 ? (int?)null : team.Lead;
                    team.DivisionId = team.DivisionId;
                    if (CheckTeam(team))
                    {
                        ModelState.AddModelError(string.Empty,
                            "Unable to save record, a player cannot be on a team in multiple positions");
                    }
                    else
                    {

                        _db.Entry(team).State = EntityState.Modified;
                        _db.SaveChanges();
                        OrderTeam(team.Leagueid);
                        return RedirectToAction("Index", new {id = team.Leagueid});
                    }
                }
            }
            catch (DbUpdateConcurrencyException ex)
            {
                var entry = ex.Entries.Single();
                var clientValues = (Team)entry.Entity;
                var databaseEntry = entry.GetDatabaseValues();
                if (databaseEntry == null)
                {
                    ModelState.AddModelError(string.Empty,
                        "Unable to save changes. The member was deleted by another user.");
                }
                else
                {
                    var databaseValues = (Team)databaseEntry.ToObject();

                    if (databaseValues.Skip != clientValues.Skip)
                        ModelState.AddModelError("Skip", "Current value: "
                                                         + databaseValues.Skip);
                    if (databaseValues.ViceSkip != clientValues.ViceSkip)
                        ModelState.AddModelError("Vice Skip", "Current value: "
                                                              + databaseValues.ViceSkip);
                    if (databaseValues.Lead != clientValues.Lead)
                        ModelState.AddModelError("Lead", "Current value: "
                                                         + databaseValues.Lead);
                    if (databaseValues.Leagueid != clientValues.Leagueid)
                        ModelState.AddModelError("League", "Current value: "
                                                           + databaseValues.Leagueid);
                    if (databaseValues.TeamNo != clientValues.TeamNo)
                        ModelState.AddModelError("Team Number", "Current value: "
                                                                + databaseValues.TeamNo);
                    if (databaseValues.DivisionId != clientValues.DivisionId)
                        ModelState.AddModelError("Division", "Current value: "
                                                                + databaseValues.TeamNo);


                    ModelState.AddModelError(string.Empty, "The record you attempted to edit "
                                                           + "was modified by another user after you got the original value. The "
                                                           + "edit operation was canceled and the current values in the database "
                                                           + "have been displayed. If you still want to edit this record, click "
                                                           + "the Save button again. Otherwise click the Back to List hyperlink.");
                    team.rowversion = databaseValues.rowversion;
                }
            }

            var league = _db.Leagues.Find(team.Leagueid);
            if (league == null)
                return HttpNotFound();
            var teams = _db.Teams.Where(x => x.Leagueid == team.Leagueid).OrderBy(x => x.TeamNo);

            var list = RemainingPlayers(team, teams.ToList());
            ViewBag.List = list;
            ViewBag.Teams = teams;
            ViewBag.TeamSize = league.TeamSize;
            return View(team);
        }

        // GET: Teams/Delete/5
        [Authorize(Roles = "Admin,LeagueAdmin")]
        public ActionResult Delete(int? id, bool? concurrencyError)
        {
           
           
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var team = _db.Teams.Find(id);
            if (team == null)
            {
                if (concurrencyError.GetValueOrDefault())
                {
                    return RedirectToAction("Welcome","Home");
                }
                return HttpNotFound();
            }
            var league = _db.Leagues.Find(team.Leagueid);
            if (league == null)
                return HttpNotFound();
            ViewBag.NumberDivisions = league.Divisions;

            if (concurrencyError.GetValueOrDefault())
            {
                ViewBag.Error = "The record you attempted to delete "
                                                  + "was modified by another user after you got the original values. "
                                                  + "The delete operation was canceled and the current values in the "
                                                  + "database have been displayed. If you still want to delete this "
                                                  + "record, click the Delete button again. Otherwise "
                                                  + "click the Back to List hyperlink.";
            }
            ViewBag.TeamSize = team.League.TeamSize;
            return View(team);
        }

        // POST: Teams/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id, byte[] rowversion)
        {

            
            var team = _db.Teams.Find(id);
           
            if (team == null)
            {
                ViewBag.Error = "Unable to delete this record, another user deleted this record";
                return View(new Team());
            }
            try
            {
                _db.Entry(team).Property("rowversion").OriginalValue = rowversion;
                _db.Entry(team).State = EntityState.Deleted;
                int teamno = 1;
                foreach(var item in _db.Teams.Where(x=>x.Leagueid==team.Leagueid && x.id != team.id).OrderBy(x=>x.TeamNo))
                {
                    item.TeamNo = teamno++;
                    _db.Entry(item).State = EntityState.Modified;
                }
                _db.SaveChanges();
                OrderTeam(team.Leagueid);
                return RedirectToAction("Index", new {id=team.Leagueid});
            }
            catch (DbUpdateConcurrencyException)
            {
                return RedirectToAction("Delete", new { concurrencyError = true, id = id });
            }
            catch (Exception dex)
            {
                //Log the error (uncomment dex variable name after DataException and add a line here to write a log.
                ViewBag.Error =
                    "Unable to delete. Try again, and if the problem persists contact your system administrator.";
                ErrorSignal.FromCurrentContext().Raise(dex);

            }
            ViewBag.TeamSize = team.League.TeamSize;
            return View(team);
        }

        /// <summary>
        /// Generate an order list of remaining players including members of the current team
        /// </summary>
        /// <param name="team">current team</param>
        /// <param name="leagueid">league number</param>
        /// <param name="teams">order list of all teas</param>
        /// <returns></returns>
        private List<PlayerViewModel> RemainingPlayers(Team team, List<Team> teams)
        {
            var list = new List<PlayerViewModel>();
            foreach (var player in _db.Players.Where(x => x.Leagueid == team.Leagueid))
            {
                if (!teams.Any(x => x.Skip == player.id || x.Lead == player.id || x.ViceSkip == player.id))
                {
                    list.Add(new PlayerViewModel()
                    {
                        id = player.id,
                        FullName = player.Membership.FullName,
                        LastName = player.Membership.LastName
                    });
                }
            }

            if (team.id != 0)
            {
                if (team.Player != null)
                {
                    list.Add(new PlayerViewModel()
                    {
                        id = team.Player.id,
                        FullName = team.Player.Membership.FullName,
                        LastName = team.Player.Membership.LastName
                    });
                }

                if (team.Player2 != null)
                {
                    list.Add(new PlayerViewModel()
                    {
                        id = team.Player2.id,
                        FullName = team.Player2.Membership.FullName,
                        LastName = team.Player2.Membership.LastName
                    });
                }

                if (team.Player1 != null)
                {
                    list.Add(new PlayerViewModel()
                    {
                        id = team.Player1.id,
                        FullName = team.Player1.Membership.FullName,
                        LastName = team.Player1.Membership.LastName
                    });
                }
            }
            list.Add(new PlayerViewModel()
            {
                id = 0,
                LastName = ""
            });
            list.Sort((a, b) => String.Compare(a.LastName, b.LastName, StringComparison.Ordinal));
            return list;
        }

        /// <summary>
        /// Make sure a player cannot be in multiple positions
        /// </summary>
        /// <param name="team">team record</param>
        /// <returns>true if multiple positions</returns>
        private bool CheckTeam(Team team)
        {
            if (team.Skip.HasValue && (team.Skip == team.ViceSkip || team.Skip == team.Lead))
                    return true;
            if (team.ViceSkip.HasValue && team.ViceSkip == team.Lead)
                return true;
            return false;
        }

        private void OrderTeam(int leagueid)
        {
            bool changed = false;
            var teams = _db.Teams.Where(x => x.Leagueid == leagueid).OrderBy(x => x.DivisionId).ToList();
            for(int i=0;i<teams.Count();i++)
            {
                var item = teams[i];
                if (item.TeamNo != i + 1)
                {
                    item.TeamNo = i + 1;
                    _db.Entry(item).State = EntityState.Modified;
                    changed = true;
                }
            }
            if(changed)
                _db.SaveChanges();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _db.Dispose();
            }
            base.Dispose(disposing);
        }

        /// <summary>
        /// Determine teamsize from league record
        /// </summary>
        /// <param name="leagueid">league key</param>
        /// <returns>teamsize</returns>
        private int TeamSize(int leagueid)
        {
            var league = _db.Leagues.Find(leagueid);
            if (league != null)
                return league.TeamSize;
            return 0;
        }
    }
}
