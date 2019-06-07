﻿using Elmah;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using System.Web.UI.WebControls;
using Microsoft.Reporting.WebForms;
using Tournament.Code;
using Tournament.Models;
using Tournament.ReportFiles;
using System.Data.Entity.Infrastructure;

namespace Tournament.Controllers
{
    [Authorize]
    public class MatchesController : Controller
    {
        private TournamentEntities db = new TournamentEntities();

        // GET: Matches
        public ActionResult Index(int? RoundId)
        {
            int id;
            var leagueid = (int)HttpContext.Session["leagueid"];
            if (!RoundId.HasValue)
            {
                var weeks = db.Schedules.Where(x => x.Leagueid == leagueid).OrderBy(x => x.SortOrder);
                if (weeks.Any())
                {
                    id = weeks.First().id;
                }
                else
                {
                    return View(new List<Match>());
                }
            }
            else
            {
                id = RoundId.Value;
            }
            
            ViewBag.TeamSize = (int)HttpContext.Session["teamsize"];
            var Matches = db.Matches.Where(x => x.RoundId == id && x.Rink != -1);
            ViewBag.ScheduleID = new SelectList(db.Schedules.Where(x=>x.Leagueid == leagueid), "id", "RoundName", id.ToString());
            ViewBag.RoundId = db.Schedules.Find(id).id;
            ViewBag.WeekID = id;
            return View(Matches.OrderBy(x => x.Rink).ToList());
        }

        public ActionResult MoveUp(int id, int weekid)
        {
            var Matches = db.Matches.Where(x => x.RoundId == weekid).OrderBy(x => x.Rink);
            var match = Matches.First(x => x.Rink == id);
            var match1 = Matches.First(x => x.Rink == id + 1);
            match1.Rink = id;
            match.Rink = id + 1;
            db.Entry(match).State = EntityState.Modified;
            try
            {
                db.SaveChanges();
            }
            catch (Exception e)
            {
                ErrorSignal.FromCurrentContext().Raise(e);
                throw;
            }
            
            return RedirectToAction("Index", new { ScheduleID = weekid });
        }

       public ActionResult CreateMatches()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateMatches(string DeleteIT)
        {
            var leagueid = (int)HttpContext.Session["leagueid"];
            var numOfWeeks = db.Schedules.Count(x => x.Leagueid == leagueid);
            var numofTeams = db.Teams.Count(x => x.Leagueid == leagueid);
            foreach (var item in db.Schedules.Where(x => x.Leagueid == leagueid))
            {
                foreach(var match in db.Matches.Where(x => x.RoundId == item.id).ToList())
                    db.Matches.Remove(match);
            }
            db.SaveChanges();
            var cs = new CreateSchedule();

            var matches = numofTeams % 2 == 0 ? cs.NoByes(numOfWeeks, numofTeams) : cs.Byes(numOfWeeks, numofTeams);
            
            var scheduleList = db.Schedules.Where(x=>x.Leagueid== leagueid).ToList();
            var teamList = db.Teams.Where(x => x.Leagueid == leagueid).ToList();

            foreach (var match in matches)
            {
                var team1 = teamList.Find(x => x.TeamNo == match.Team1 + 1);
                var team2 = teamList.Find(x => x.TeamNo == match.Team2 + 1);
                var round = scheduleList.Find(x => x.SortOrder == match.Week + 1);
                db.Matches.Add(new Match()
                {
                    id=0,
                    RoundId = round.id,
                    Rink = match.Rink == -1 ? -1 : match.Rink + 1,
                    TeamNo1 = team1.id,
                    TeamNo2 = match.Rink == -1? (int?) null : team2.id,
                    Team1Score = 0,
                    Team2Score = 0,
                    ForFeitId = 0
                });
               
            }
            try
            {
                db.SaveChanges();
            }
            catch (Exception e)
            {
                ErrorSignal.FromCurrentContext().Raise(e);
            }
            var round1 = db.Schedules.Where(x => x.Leagueid == leagueid).First();
            return RedirectToAction("index", new { RoundId = round1.id });
        }

        
        public ActionResult ClearSchedule()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ClearSchedule(string DeleteIT)
        {
            var leagueid = (int)HttpContext.Session["leagueid"];
            foreach (var item in db.Schedules.Where(x => x.Leagueid == leagueid))
            {
                foreach (var match in db.Matches.Where(x => x.RoundId == item.id).ToList())
                    db.Matches.Remove(match);
            }
            db.Schedules.RemoveRange(db.Schedules.Where(x => x.Leagueid == leagueid));
            db.Teams.RemoveRange(db.Teams.Where(x => x.Leagueid == leagueid));
            try
            {
                db.SaveChanges();
            }
            catch (Exception e)
            {
                ErrorSignal.FromCurrentContext().Raise(e);
            }
            
            return RedirectToAction("index", "Home");
        }

        public ActionResult StandingsReport(int id)
        {

            var reportViewer = new ReportViewer()
            {
                ProcessingMode = ProcessingMode.Local,
                Width = Unit.Pixel(800),
                Height = Unit.Pixel(1000),
                ShowExportControls = true
            };
            ViewBag.RoundId = id;


            var ds = new TournamentDS();
            bool IsBye = false;
            bool isCancelled = false;

           
            var week = db.Schedules.Find(id);
            var WeekDate = week.RoundName;
            if (!week.Cancelled)
            {
                foreach (var item in db.Matches.Include(x=>x.Team1).Include(x=>x.Team).Include(x=>x.Schedule).Where(x => x.RoundId == id && x.Rink != -1)
                    .OrderBy(x => x.Rink))
                {
                    var forfeit = "";
                    if(item.ForFeitId != 0)
                    {
                        if(item.ForFeitId == item.Team.id)
                            forfeit = item.Team.TeamNo.ToString();
                        else
                        {
                            forfeit = item.Team1.TeamNo.ToString();
                        }
                    }
                    ds.Game.AddGameRow(item.Team.TeamNo,
                        Players(item.Team),
                        item.Team1.TeamNo,
                        Players(item.Team1),
                        item.Team1Score,
                        item.Team2Score, item.Rink, forfeit);
                }

                var matches = db.Matches.Where(x => x.Rink == -1 && x.RoundId == id);
                if (matches.Any())
                {
                    var match = matches.First();

                    ds.Byes.AddByesRow(match.Schedule.RoundName, match.Team.TeamNo,
                        Players(match.Team));
                    reportViewer.LocalReport.DataSources.Add(new ReportDataSource("Bye", ds.Byes.Rows));
                    IsBye = true;
                }
                else
                {
                    reportViewer.LocalReport.DataSources.Add(new ReportDataSource("Bye", new System.Data.DataTable()));
                }
                reportViewer.LocalReport.DataSources.Add(new ReportDataSource("Game", ds.Game.Rows));
            }
            else
            {
                isCancelled = true;
                reportViewer.LocalReport.DataSources.Add(new ReportDataSource("Game", new System.Data.DataTable()));
                reportViewer.LocalReport.DataSources.Add(new ReportDataSource("Bye", new System.Data.DataTable()));
            }

            reportViewer.LocalReport.DataSources.Add(new ReportDataSource("Stand", CalculateStandings.Doit(id, (int)HttpContext.Session["teamsize"]).Rows));
            reportViewer.LocalReport.ReportPath = Server.MapPath("/ReportFiles/Standings.rdlc");

            var p1 = new ReportParameter("WeekDate", WeekDate);
            var p2 = new ReportParameter("Description", (string)HttpContext.Session["leaguename"]);
            var p3 = new ReportParameter("IsBye", IsBye ? "1" : "0");
            var p4 = new ReportParameter("IsCancelled", isCancelled ? "1" : "0");
            reportViewer.LocalReport.SetParameters(new ReportParameter[] { p1, p2, p3, p4 });
            

            ViewBag.ReportViewer = reportViewer;
            return View();
        }

        private string Players(Team team)
        {
            var TeamSize = (int)HttpContext.Session["teamsize"];
            switch (TeamSize)
            {
                case 1:
                    return $"{team.Player.NickName}";
                case 2:
                    return $"{team.Player.NickName}, {team.Player2.NickName}";
                case 3:
                    return $"{team.Player.NickName}, {team.Player1.NickName}, {team.Player2.NickName}";

            }
            return "";
        }

        public ActionResult ScoreCardReport(int id)
        {

            var reportViewer = new ReportViewer()
            {
                ProcessingMode = ProcessingMode.Local,
                Width = Unit.Pixel(800),
                Height = Unit.Pixel(1000),
                ShowExportControls = true
            };
            ViewBag.RoundId = id;

            var ds = new TournamentDS();
            foreach (var match in db.Matches.Where(x => x.RoundId == id && x.Rink != -1).Include(x=>x.Schedule).Include(x=>x.Team).Include(x=>x.Team1).OrderBy(x => x.Rink))
            {
                ds.ScoreCard.AddScoreCardRow(match.Rink, match.Schedule.RoundName,
                    match.Team.Player.NickName, 
                    match.Team.ViceSkip.HasValue? match.Team.Player1.NickName:"",
                    match.Team.Lead.HasValue ? match.Team.Player2.NickName:"",
                    match.Team1.Player.NickName,
                    match.Team1.ViceSkip.HasValue ? match.Team1.Player1.NickName : "",
                    match.Team1.Lead.HasValue ? match.Team1.Player2.NickName : "",
                    match.Team.TeamNo, match.Team1.TeamNo);
            }
            reportViewer.LocalReport.DataSources.Add(new ReportDataSource("Match", ds.ScoreCard.Rows));

            reportViewer.LocalReport.ReportPath = Server.MapPath("/ReportFiles/ScoreCard.rdlc");

            var p1 = new ReportParameter("teamsize", HttpContext.Session["teamsize"].ToString());
            var p2 = new ReportParameter("Description", (string) HttpContext.Session["leaguename"]);
            reportViewer.LocalReport.SetParameters(new ReportParameter[] { p1, p2 });
            ViewBag.ReportViewer = reportViewer;
            return View();
        }

        public ActionResult ByesReport()
        {
            var leagueid = (int)HttpContext.Session["leagueid"];
            var reportViewer = new ReportViewer()
            {
                ProcessingMode = ProcessingMode.Local,
                Width = Unit.Pixel(800),
                Height = Unit.Pixel(1000),
                ShowExportControls = true
            };

            var ds = new TournamentDS();


            foreach (var item in db.Schedules.Where(x => x.Leagueid == leagueid).OrderBy(x=>x.SortOrder))
            {
                var matches = db.Matches.Where(x => x.Rink == -1 && x.RoundId == item.id);
                if(matches.Any())
                    ds.Byes.AddByesRow(item.RoundName, matches.First().Team.TeamNo, Players(matches.First().Team));

            }
            reportViewer.LocalReport.ReportPath = Server.MapPath("/ReportFiles/Byes.rdlc");

            var p2 = new ReportParameter("Description", (string)HttpContext.Session["leaguename"]);
            reportViewer.LocalReport.SetParameters(new ReportParameter[] { p2 });

            reportViewer.LocalReport.DataSources.Add(new ReportDataSource("Byes", ds.Byes.Rows));

            ViewBag.ReportViewer = reportViewer;
            return View();
        }


        public ActionResult Scoring(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Match match = db.Matches.Find(id);
            if (match == null)
            {
                return HttpNotFound();
            }
            var forfeits = new List<Forfeit>();
            forfeits.Add(new Forfeit() {Id = 0, Term = "No Forfeit"});
            forfeits.Add(new Forfeit() {Id=match.TeamNo1, Term = $"Team No. {match.Team.TeamNo}"});
            forfeits.Add(new Forfeit() {Id = match.TeamNo2.Value, Term = $"Team No. {match.Team1.TeamNo}"});
            ViewBag.TeamSize = (int)HttpContext.Session["teamsize"];
            ViewBag.ForFeitId = new SelectList(forfeits, "id", "term", match.ForFeitId);
            return View(match);
        }

        // POST: Matches/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Scoring(int? id, byte[] rowVersion)
        {
            string[] fieldsToBind = new string[] {"RoundId","Rink","TeamNo1","TeamNo2","Team1Score","Team2Score", "ForFeitId", "rowversion" };
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var update = db.Matches.Find(id);
            if (update == null)
            {
                var delete = new Match();
                TryUpdateModel(delete, fieldsToBind);
                ModelState.AddModelError(string.Empty,
                    "Unable to save changes. The match was deleted by another user.");
                return View(delete);
            }

            if (TryUpdateModel(update, fieldsToBind))
            {
                try
                {
                    db.Entry(update).OriginalValues["rowversion"] = rowVersion;
                    db.SaveChanges();

                    return RedirectToAction("Index", new { RoundId = update.RoundId});
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    var entry = ex.Entries.Single();
                    var clientValues = (Match)entry.Entity;
                    var databaseEntry = entry.GetDatabaseValues();
                    if (databaseEntry == null)
                    {
                        ModelState.AddModelError(string.Empty,
                            "Unable to save changes. The match was deleted by another user.");
                    }
                    else
                    {
                        var databaseValues = (Match)databaseEntry.ToObject();

                        if (databaseValues.Team1Score != clientValues.Team1Score)
                            ModelState.AddModelError("Team 1 Score", "Current value: "
                                                                    + databaseValues.Team1Score);
                        if(databaseValues.Team2Score != clientValues.Team2Score)
                        ModelState.AddModelError("Team 1 Score", "Current value: "
                                                                 + databaseValues.Team2Score);
                        if (databaseValues.ForFeitId != clientValues.ForFeitId)
                            ModelState.AddModelError("Forfeit", "Current value: "
                                                                     + databaseValues.ForFeitId);
                        ModelState.AddModelError(string.Empty, "The record you attempted to edit "
                                                               + "was modified by another user after you got the original value. The "
                                                               + "edit operation was canceled and the current values in the database "
                                                               + "have been displayed. If you still want to edit this record, click "
                                                               + "the Save button again. Otherwise click the Back to List hyperlink.");
                        update.rowversion = databaseValues.rowversion;
                    }
                }
                catch (RetryLimitExceededException dex)
                {
                    //Log the error (uncomment dex variable name and add a line here to write a log.)
                    ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists, see your system administrator.");
                    ErrorSignal.FromCurrentContext().Raise(dex);
                }
            }
            var forfeits = new List<Forfeit>();
            forfeits.Add(new Forfeit() { Id = 0, Term = "No Forfeit" });
            forfeits.Add(new Forfeit() { Id = update.TeamNo1, Term = $"Team No. {update.Team.TeamNo}" });
            forfeits.Add(new Forfeit() { Id = update.TeamNo2.Value, Term = $"Team No. {update.Team1.TeamNo}" });
            ViewBag.TeamSize = (int)HttpContext.Session["teamsize"];
            ViewBag.ForFeitId = new SelectList(forfeits, "id", "term", update.ForFeitId);
            return View(update);
        }


        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
