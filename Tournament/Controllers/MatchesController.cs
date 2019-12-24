using Elmah;
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
   
    public class MatchesController : Controller
    {
        private TournamentEntities db = new TournamentEntities();

        [Authorize]
        // GET: Matches
        public ActionResult Index(int? scheduleId)
        {
            var leagueid = (int)HttpContext.Session["leagueid"];
            var league = db.Leagues.Find(leagueid);
            ViewBag.TeamSize = league.TeamSize;
            if (!scheduleId.HasValue)
                scheduleId = db.Schedules.Where(x => x.Leagueid == leagueid).First().id;
            var matches = db.Matches.Where(x => x.WeekId == scheduleId.Value && x.Rink != -1).OrderBy(x=>x.Rink);
            ViewBag.scheduleId = new SelectList(db.Schedules.Where(x=>x.Leagueid == leagueid).OrderBy(x=>x.WeekNumber).ToList(), "id", "WeekDate", scheduleId);
            ViewBag.Date = db.Schedules.Find(scheduleId.Value).WeekDate;
            ViewBag.WeekID = scheduleId;
            return View(matches);

        }

        [Authorize(Roles = "Admin,LeagueAdmin")]
        public ActionResult MoveUp(int id)
        {
            
            var match = db.Matches.Find(id);
            var weekMatches = db.Matches.Where(x => x.WeekId == match.WeekId);
            var match1 = weekMatches.First(x => x.Rink == match.Rink-1);
            match1.Rink = match.Rink;
            match.Rink = match1.Rink-1;
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
            return RedirectToAction("Index", new { scheduleId = match.WeekId });
        }

        [Authorize(Roles = "Admin,LeagueAdmin")]
        public ActionResult CreateMatches()
        {
            var leagueid = (int)HttpContext.Session["leagueid"];
            var missing = Missing(leagueid);
            if (missing.Count > 0)
            {
                ViewBag.Error = "Some players not assigned to a team";
                return View(missing);
            }
            var complete = Complete(leagueid);
            if (!complete)
                ViewBag.Error = "Not all teams are complete";
            return View(missing);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateMatches(string DeleteIT)
        {
            var leagueid = (int)HttpContext.Session["leagueid"];
            var numOfWeeks = db.Schedules.Count(x => x.Leagueid == leagueid);
            var numofTeams = db.Teams.Count(x => x.Leagueid == leagueid);

            var missing = Missing(leagueid);
            if (missing.Count > 0)
            {
                ViewBag.Error = "Some players not assigned to a team";
                return View(missing);
            }

            var complete = Complete(leagueid);
            if (!complete)
            {
                ViewBag.Error = "Not all teams are complete";
                return View(missing);
            }

            var matches = db.Matches.Where(x => x.Team.Leagueid == leagueid).ToList();
            foreach (var match in matches)
            {
                if (match.Team1Score != 0 || match.Team2Score != 0 || match.ForFeitId != 0)
                {
                    ViewBag.Error = "Matches cannot be delete, some matches have scores";
                    return View(missing);
                }
            }

            db.Matches.RemoveRange(matches);

            var cs = new CreateSchedule();

            var newMatches = numofTeams % 2 == 0 ? cs.NoByes(numOfWeeks, numofTeams) : cs.Byes(numOfWeeks, numofTeams);
            
            var scheduleList = db.Schedules.Where(x=>x.Leagueid== leagueid).ToList();
            var teamList = db.Teams.Where(x => x.Leagueid == leagueid).ToList();

            foreach (var match in newMatches)
            {
                var team1 = teamList.Find(x => x.TeamNo == match.Team1 + 1);
                var team2 = teamList.Find(x => x.TeamNo == match.Team2 + 1);
                var round = scheduleList.Find(x => x.WeekNumber == match.Week + 1);
                db.Matches.Add(new Match()
                {
                    id=0,
                    WeekId = round.id,
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
                ViewBag.Error = $"No matches were created, Error {e.Message}";
                return View(new List<Player>());
            }
            var round1 = db.Schedules.Where(x => x.Leagueid == leagueid).First();
            return RedirectToAction("index", new { scheduleid = round1.id });
        }

        private bool Complete(int leagueid)
        {
            var teamsize = db.Leagues.Find(leagueid).TeamSize;
            var complete = true;
            foreach (var team in db.Teams.Where(x => x.Leagueid == leagueid).ToList())
            {
                switch (teamsize)
                {
                    case 1:
                        if (!team.Skip.HasValue)
                        {
                            ViewBag.Error = "Not all teams are complete";
                            complete = false;
                        }
                        break;

                    case 2:
                        if (!team.Skip.HasValue || !team.Lead.HasValue)
                        {
                            ViewBag.Error = "Not all teams are complete";
                            complete = false;
                        }
                        break;
                    case 3:
                        if (!team.Skip.HasValue || !team.Lead.HasValue || !team.ViceSkip.HasValue)
                        {
                            ViewBag.Error = "Not all teams are complete";
                            complete = false;
                        }
                        break;
                }
                if (!complete)
                    break;

            }
            return complete;
        }

        private List<Player> Missing(int leagueid)
        {
            var teams = db.Teams.Where(x => x.Leagueid == leagueid);
            var playerList = db.Players.Where(x => x.Leagueid == leagueid).ToList();
            foreach (var team in teams)
            {
                if (team.Skip.HasValue)
                {
                    playerList.Remove(team.Player);
                }
                if (team.Lead.HasValue )
                {
                    playerList.Remove(team.Player2);
                }
                if (team.ViceSkip.HasValue)
                {
                    playerList.Remove(team.Player1);
                }

            }
            return playerList;
        }

        [Authorize(Roles = "Admin,LeagueAdmin")]
        public ActionResult ClearSchedule()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ClearSchedule(string DeleteIT)
        {
            var leagueid = (int)HttpContext.Session["leagueid"];
            var matches = db.Matches.Where(x=>x.Team.Leagueid == leagueid).ToList();
            foreach (var match in matches)
            {
                if (match.Team1Score != 0 || match.Team2Score != 0 || match.ForFeitId != 0)
                {
                    ViewBag.Error = "Matches cannot be delete, some matches have scores";
                    return View();
                }
            }
            try
            {
                db.Matches.RemoveRange(matches);
                db.SaveChanges();
            }
            catch (Exception e)
            {
                ErrorSignal.FromCurrentContext().Raise(e);
                ViewBag.Error = $"Matches were not removed, Error: {e.Message}";
                return View();
            }
            
            return RedirectToAction("Welcome", "Home");
        }

        [Authorize]
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
            var WeekDate = week.WeekDate;
            if (!week.Cancelled)
            {
                foreach (var item in db.Matches.Include(x=>x.Team1).Include(x=>x.Team).Include(x=>x.Schedule).Where(x => x.WeekId == id && x.Rink != -1)
                    .OrderBy(x => x.Rink))
                {
                    var forfeit = "";
                    if(item.ForFeitId != 0)
                    {
                        if(item.ForFeitId == item.Team.TeamNo)
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

                var matches = db.Matches.Where(x => x.Rink == -1 && x.WeekId == id);
                if (matches.Any())
                {
                    var match = matches.First();

                    ds.Byes.AddByesRow(match.Schedule.WeekDate, match.Team.TeamNo,
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

            reportViewer.LocalReport.DataSources.Add(new ReportDataSource("Stand", CalculateStandings.Doit(id, (int)HttpContext.Session["teamsize"], (int)HttpContext.Session["leagueid"]).Rows));
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
                    return $"{team.Player.Membership.NickName}";
                case 2:
                    return $"{team.Player.Membership.NickName}, {team.Player2.Membership.NickName}";
                case 3:
                    return $"{team.Player.Membership.NickName}, {team.Player1.Membership.NickName}, {team.Player2.Membership.NickName}";

            }
            return "";
        }

        [Authorize]
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
            var matches = db.GetMatchAll(id).ToList();
            reportViewer.LocalReport.DataSources.Add(new ReportDataSource("Match", matches));

            reportViewer.LocalReport.ReportPath = Server.MapPath("/ReportFiles/ScoreCard.rdlc");

            var p1 = new ReportParameter("teamsize", HttpContext.Session["teamsize"].ToString());
            var p2 = new ReportParameter("Description", (string) HttpContext.Session["leaguename"]);
            reportViewer.LocalReport.SetParameters(new ReportParameter[] { p1, p2 });
            ViewBag.ReportViewer = reportViewer;
            return View();
        }

        [Authorize]
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


            foreach (var item in db.Schedules.Where(x => x.Leagueid == leagueid).OrderBy(x=>x.WeekNumber))
            {
                var matches = db.Matches.Where(x => x.Rink == -1 && x.WeekId == item.id);
                if(matches.Any())
                    ds.Byes.AddByesRow(item.WeekDate, matches.First().Team.TeamNo, Players(matches.First().Team));

            }
            reportViewer.LocalReport.ReportPath = Server.MapPath("/ReportFiles/Byes.rdlc");

            var p2 = new ReportParameter("Description", (string)HttpContext.Session["leaguename"]);
            reportViewer.LocalReport.SetParameters(new ReportParameter[] { p2 });

            reportViewer.LocalReport.DataSources.Add(new ReportDataSource("Byes", ds.Byes.Rows));

            ViewBag.ReportViewer = reportViewer;
            return View();
        }

        [Authorize(Roles = "Admin,LeagueAdmin,Scorer")]
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
            var forfeits = new List<SelectListItem>
            {
                new SelectListItem {Value = "0", Text = "No Forfeit", Selected = match.ForFeitId == 0},
                new SelectListItem
                {
                    Value = match.Team.TeamNo.ToString(),
                    Text = $"Team No. {match.Team.TeamNo}",
                    Selected = match.ForFeitId == match.Team.TeamNo
                },
                new SelectListItem
                {
                    Value = match.Team1.TeamNo.ToString(),
                    Text = $"Team No. {match.Team1.TeamNo}",
                    Selected = match.ForFeitId == match.Team1.TeamNo
                }
            };
            ViewBag.TeamSize = (int)HttpContext.Session["teamsize"];
            ViewBag.ForFeitId = forfeits;
            return View(match);
        }

        // POST: Matches/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Scoring([Bind(Include = "id,WeekId,Rink,TeamNo1,TeamNo2,Team1Score,Team2Score,ForFeitId,rowversion")] Match match)
        {
            try
            {

                if (ModelState.IsValid)
                {
                    db.Entry(match).State = EntityState.Modified;
                    db.SaveChanges();
                    return RedirectToAction("Index", new { scheduleId = match.WeekId });
                }
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
                        match.rowversion = databaseValues.rowversion;
                    }
                }
            catch (Exception dex)
            {
                //Log the error (uncomment dex variable name and add a line here to write a log.)
                ModelState.AddModelError("",
                    "Unable to save changes. Try again, and if the problem persists, see your system administrator.");
                ErrorSignal.FromCurrentContext().Raise(dex);

            }
            var forfeits = new List<SelectListItem>
            {
                new SelectListItem {Value = "0", Text = "No Forfeit", Selected = match.ForFeitId == 0},
                new SelectListItem
                {
                    Value = match.Team.TeamNo.ToString(),
                    Text = $"Team No. {match.Team.TeamNo}",
                    Selected = match.ForFeitId == match.Team.TeamNo
                },
                new SelectListItem
                {
                    Value = match.Team1.TeamNo.ToString(),
                    Text = $"Team No. {match.Team1.TeamNo}",
                    Selected = match.ForFeitId == match.Team1.TeamNo
                }
            };
            ViewBag.TeamSize = (int)HttpContext.Session["teamsize"];
            ViewBag.ForFeitId = forfeits;
            return View(match);
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
