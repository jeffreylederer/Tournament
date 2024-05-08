using Elmah;
using Microsoft.Reporting.WebForms;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.UI.WebControls;
using Tournament.Code;
using Tournament.Models;
using Tournament.ReportFiles;
using System.IO;
using DocumentFormat.OpenXml.Office2010.Excel;

namespace Tournament.Controllers
{

    public class MatchesController : Controller
    {
        private readonly TournamentEntities _db = new TournamentEntities();

        [Authorize]
        // GET: Matches
        public ActionResult Index(int id, int? weekid)
        {

            ViewBag.PlayOffs = false;

            var league = _db.Leagues.Find(id);
            if (league == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.NotFound);
            }

            Schedule schedule = null;
            if (!weekid.HasValue || weekid.Value == 0)
            {
                
                if (_db.Schedules.Any(x => x.Leagueid == id))
                {
                    schedule = _db.Schedules.Where(x => x.Leagueid==id).First();
                    weekid = schedule.id;
                }
                else
                {
                    ViewBag.Error = "No dates have been scheduled";
                }
            }
            else if(weekid > 0)
            {
                
                schedule = _db.Schedules.Find(weekid.Value);
                if (schedule == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.NotFound);
                }
            }
            var playoffs = ViewBag.PlayOffs = _db.Schedules.Find(weekid.Value).PlayOffs;

            ViewBag.TeamSize = league.TeamSize;
            var matches = new List<Match>();
            if (schedule != null)
            {
                if(!playoffs)
                    matches = _db.Matches.Where(x => x.WeekId == schedule.id && x.Rink != -1).OrderBy(x => x.Rink).ToList();
                ViewBag.ScheduleID =
                    new SelectList(_db.Schedules.Where(x => x.Leagueid == id).OrderBy(x => x.GameDate).ToList(), "id",
                        "WeekDate", weekid);
                ViewBag.Date = schedule.WeekDate;
            }
            else
            {
                ViewBag.Date = DateTime.Now.ToShortDateString();
                ViewBag.scheduleId =
                    new SelectList(_db.Schedules.Where(x => x.Leagueid == id).OrderBy(x => x.GameDate).ToList(), "id",
                        "WeekDate");
            }

            ViewBag.WeekId = weekid;
            ViewBag.Id = id;
            
            return View(matches);

        }

        [Authorize(Roles = "Admin,LeagueAdmin")]
        public ActionResult MoveUp(int id)
        {
            
            var match = _db.Matches.Find(id);
            if (match == null)
                return HttpNotFound();
            var weekMatches = _db.Matches.Where(x => x.WeekId == match.WeekId);
            var match1 = weekMatches.First(x => x.Rink == match.Rink-1);
            match1.Rink = match.Rink;
            match.Rink = match1.Rink-1;
            _db.Entry(match).State = EntityState.Modified;
            try
            {
                _db.SaveChanges();
            }
            catch (Exception e)
            {
                ErrorSignal.FromCurrentContext().Raise(e);
                throw;
            }
            return RedirectToAction("Index", new { weekid = match.WeekId, id = match.Schedule.Leagueid });
        }

        [Authorize(Roles = "Admin,LeagueAdmin")]
        public ActionResult CreateMatches(int id)
        {
            var missing = Missing(id);
            if (missing.Count > 0)
            {
                ViewBag.Error = "Some players not assigned to a team";
                return View(missing);
            }
            var complete = Complete(id);
            if (!complete)
                ViewBag.Error = "Not all teams are complete";
            var leaguename = "";
            var cookie = Request.Cookies["leaguename"];
            if (cookie != null)
            {
                leaguename = cookie.Value;
            }
            ViewBag.LeagueName = leaguename;
            return View(missing);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateMatches()
        {
            var leaguename = "";
            var cookie = Request.Cookies["leaguename"];
            if (cookie != null)
            {
                leaguename = cookie.Value;
            }
            ViewBag.LeagueName = leaguename;


            var leagueid = 0;
            cookie = Request.Cookies["leagueid"];
            if (cookie != null)
            {
                int.TryParse(cookie.Value, out leagueid);
            }
            var league = _db.Leagues.Find(leagueid);
            

            var numOfWeeks = _db.Schedules.Count(x => x.Leagueid == leagueid);
            var numofTeams = _db.Teams.Count(x => x.Leagueid == leagueid);


            var missing = Missing(leagueid);
            

            var complete = Complete(leagueid);
            if (!complete)
            {
                ViewBag.Error = "Not all teams are complete";
                return View(missing);
            }
             
            if (league.Divisions > 1 )
            {
                if (_db.Teams.Where(x => x.Leagueid == leagueid).Count() % 2 == 1)
                {
                    ViewBag.Error="League with divisions must have an even number of teams";
                    return View(missing);
                }
                var divisionSize = _db.Teams.Where(x => x.Leagueid == leagueid && x.DivisionId == 1).Count();
                for (int j = 2; j <= league.Divisions; j++)
                {
                    if(_db.Teams.Where(x => x.Leagueid == leagueid && x.DivisionId == j).Count() != divisionSize)
                    {
                        ViewBag.Error="Each division in a league must be the same size";
                        return View(missing);
                    }

                }
            }

             var matches = _db.Matches.Where(x => x.Team.Leagueid == leagueid).ToList();
            foreach (var match in matches)
            {
                if (match.Team1Score != 0 || match.Team2Score != 0 || match.ForFeitId != 0)
                {
                    ViewBag.Error = "Matches cannot be delete, some matches have scores";
                    return View(missing);
                }
            }

            _db.Matches.RemoveRange(matches);
            ;
            
            var cs = new CreateSchedule();
            List<CalculatedMatch> newMatches = null;
            if (league.Divisions > 1)
                newMatches = cs.matchesWithDivisions(_db.Schedules.Where(x => !x.PlayOffs).Count(x => x.Leagueid == leagueid), numofTeams);
            else
                newMatches = cs.RoundRobin(numOfWeeks, numofTeams);
                        
            var scheduleList = _db.Schedules.Where(x=>x.Leagueid== leagueid).ToList();
            var lookup = new Dictionary<int, DateTime>();
            int i = 1;
            foreach (var item in scheduleList)
            {
                lookup[i++] = item.GameDate;
            }
            var teamList = _db.Teams.Where(x => x.Leagueid == leagueid).ToList();

            foreach (var match in newMatches)
            {
                var team1 = teamList.Find(x => x.TeamNo == match.Team1 + 1);
                var team2 = teamList.Find(x => x.TeamNo == match.Team2 + 1);
                var date = lookup[match.Week + 1];
                var round = scheduleList.Find(x => x.GameDate == date);
                _db.Matches.Add(new Match()
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
                _db.SaveChanges();
            }
            catch (Exception e)
            {
                ErrorSignal.FromCurrentContext().Raise(e);
                ViewBag.Error = $"No matches were created, Error {e.Message}";
                return View(new List<Player>());
            }
            var rounds = _db.Schedules.Where(x => x.Leagueid == leagueid);
            if (!rounds.Any())
            {
                ViewBag.Error = "No matches created becuse no weeks have been scheduled";
                return View(missing);
            }
            return RedirectToAction("index", new { weekid = rounds.First().id, id= leagueid});
        }

        /// <summary>
        /// Check to make sure every position on a team is filed
        /// </summary>
        /// <param name="leagueid"></param>
        /// <returns>true if every poosition is filled</returns>
        private bool Complete(int leagueid)
        {
            var teamsize = _db.Leagues.Find(leagueid).TeamSize;
            var complete = true;
            foreach (var team in _db.Teams.Where(x => x.Leagueid == leagueid).ToList())
            {
                switch (teamsize)
                {
                    case 1:
                        if (!team.Skip.HasValue)
                        {
                            complete = false;
                        }
                        break;

                    case 2:
                        if (!team.Skip.HasValue || !team.Lead.HasValue)
                        {
                            complete = false;
                        }
                        break;
                    case 3:
                        if (!team.Skip.HasValue || !team.Lead.HasValue || !team.ViceSkip.HasValue)
                        {
                            complete = false;
                        }
                        break;
                }
                if (!complete)
                    break;

            }
            return complete;
        }

        /// <summary>
        /// Make sure every player in the league list of players is assigned to a team
        /// </summary>
        /// <param name="leagueid"></param>
        /// <returns>returns a list of players still unassigned</returns>
        private List<Player> Missing(int leagueid)
        {
            var teams = _db.Teams.Where(x => x.Leagueid == leagueid);
            var playerList = _db.Players.Where(x => x.Leagueid == leagueid).ToList();
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
        public ActionResult ClearSchedule(int id)
        {
            TempData["id"] = id;
            return View();
        }

        [Authorize(Roles = "Admin,LeagueAdmin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ClearSchedule()
        {
           var leagueid =(int) TempData["id"];
            var matches = _db.Matches.Where(x=>x.Team.Leagueid == leagueid).ToList();
            foreach (var match in matches)
            {
                if ((match.Team1Score + match.Team2Score + match.ForFeitId) > 0)
                {
                    ViewBag.Error = "Matches cannot be delete, some matches have scores";
                    return View();
                }
            }
            try
            {
                _db.Matches.RemoveRange(matches);
                _db.SaveChanges();
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
        public ActionResult StandingsReport(int id, int weekid)
        {

            var reportViewer = new ReportViewer()
            {
                ProcessingMode = ProcessingMode.Local,
                Width = Unit.Pixel(800),
                Height = Unit.Pixel(1000),
                ShowExportControls = true
            };
            ViewBag.WeekId = weekid;
            ViewBag.Id = id;

            var league = _db.Leagues.Find(id);
            if (league == null)
                return HttpNotFound();


            var ds = new TournamentDS();
            bool IsBye = false;
            bool isCancelled = false;
  

           
            var week = _db.Schedules.Find(weekid);
            if (week == null)
                return HttpNotFound();
            var weekDate = week.WeekDate;
            if (!week.Cancelled)
            {
                foreach (var item in _db.Matches.Include(x=>x.Team1).Include(x=>x.Team).Include(x=>x.Schedule).Where(x => x.WeekId == weekid && x.Rink != -1)
                    .OrderBy(x => x.Rink))
                {
                    int forfeit = 0;
                    var team1score = item.Team1Score;
                    var team2score = item.Team2Score;
                    if (item.ForFeitId > 0)
                    {
                        if (item.ForFeitId == item.Team.TeamNo)
                        {
                            forfeit = item.Team.TeamNo;
                            team1score = 0;
                            team2score = 14;
                        }
                        else
                        {
                            forfeit = item.Team1.TeamNo;
                            team1score = 14;
                            team2score = 0;
                        }
                    }
                    else if (item.ForFeitId == -1)
                    {
                        forfeit = -1;
                        team1score = 0;
                        team2score = 0;
                    }
                    else
                    {
                        if (item.Team1Score + item.Team2Score == 0)
                        {
                            ViewBag.Error = "Some matches were not scored";
                            reportViewer.LocalReport.ReportPath = Server.MapPath("/ReportFiles/Empty.rdlc");
                            var p11 = new ReportParameter("Message", "Some matches were not scored");
                            reportViewer.LocalReport.SetParameters(new ReportParameter[] { p11 });
                            ViewBag.ReportViewer = reportViewer;
                            return View();
                        }
                    }
                    ds.Game.AddGameRow(item.Team.TeamNo,
                        Players(item.Team, league.TeamSize),
                        item.Team1.TeamNo,
                        Players(item.Team1, league.TeamSize),
                        team1score,
                        team2score, 
                        item.Rink, 
                        forfeit==0?"":(forfeit==-1?"Both": forfeit.ToString()));
                }


                // check for byes
                var matches = _db.Matches.Where(x => x.Rink == -1 && x.WeekId == weekid);
                
                if (matches.Any())
                {
                    var match = matches.First();

                    ds.Byes.AddByesRow(match.Schedule.WeekDate, match.Team.TeamNo,
                        Players(match.Team, league.TeamSize));
                    reportViewer.LocalReport.DataSources.Add(new ReportDataSource("Bye", ds.Byes.Rows));
                    IsBye = true;
                }
                else
                {
                    reportViewer.LocalReport.DataSources.Add(new ReportDataSource("Bye", new System.Data.DataTable()));
                    
                }
            }
            else
            {
                isCancelled = true;
                reportViewer.LocalReport.DataSources.Add(new ReportDataSource("Game", new System.Data.DataTable()));
                reportViewer.LocalReport.DataSources.Add(new ReportDataSource("Bye", new System.Data.DataTable()));
            }
            
            reportViewer.LocalReport.DataSources.Add(new ReportDataSource("Stand", CalculateStandings.Doit(weekid, league).Rows));
            reportViewer.LocalReport.ReportPath = Server.MapPath("/ReportFiles/Standings.rdlc");
            reportViewer.LocalReport.DataSources.Add(new ReportDataSource("Game", ds.Game.Rows));

            var p1 = new ReportParameter("WeekDate", weekDate);
            var p2 = new ReportParameter("Description",league.LeagueName);
            var p3 = new ReportParameter("IsBye", IsBye ? "1" : "0");
            var p4 = new ReportParameter("IsCancelled", isCancelled ? "1" : "0");
            var p5 = new ReportParameter("PointsCount", league.PointsCount ? "1" : "0");
            var p6 = new ReportParameter("TiesAllowed", league.TiesAllowed ? "1" : "0");
            var p7 = new ReportParameter("Divisions", league.Divisions.ToString());

            reportViewer.LocalReport.SetParameters(new ReportParameter[] { p1, p2, p3, p4, p5, p6, p7 });

 
            ViewBag.ReportViewer = reportViewer;
            return View();
        }

        /// <summary>
        /// Returns a string representing the players on a team
        /// </summary>
        /// <param name="team"></param>
        /// <param name="teamsize"></param>
        /// <returns></returns>
        private string Players(Team team, int teamsize)
        {
            switch (teamsize)
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
        public ActionResult ScoreCardReport(int id, int weekid)
        {
            var league = _db.Leagues.Find(id);
            if (league == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.NotFound);
            }
            var reportViewer = new ReportViewer()
            {
                ProcessingMode = ProcessingMode.Local,
                Width = Unit.Pixel(800),
                Height = Unit.Pixel(1000),
                ShowExportControls = true
            };
            ViewBag.WeekId = weekid;

            var matches = _db.GetMatchAll(weekid).ToList();
            reportViewer.LocalReport.DataSources.Add(new ReportDataSource("Match", matches));

            reportViewer.LocalReport.ReportPath = Server.MapPath("/ReportFiles/ScoreCard.rdlc");
            ViewBag.TeamSize = league.TeamSize;
            var p1 = new ReportParameter("teamsize", league.TeamSize.ToString());
            var p2 = new ReportParameter("Description", league.LeagueName);
            reportViewer.LocalReport.SetParameters(new ReportParameter[] { p1, p2 });
            ViewBag.ReportViewer = reportViewer;
            ViewBag.Id = id;
            return View();
        }

        [Authorize]
        public ActionResult ByesReport(int id)
        {
            var reportViewer = new ReportViewer()
            {
                ProcessingMode = ProcessingMode.Local,
                Width = Unit.Pixel(800),
                Height = Unit.Pixel(1000),
                ShowExportControls = true
            };

            var ds = new TournamentDS();
            var league = _db.Leagues.Find(id);
            if (league == null)
                return HttpNotFound();

            foreach (var item in _db.Schedules.Where(x => x.Leagueid == id).OrderBy(x=>x.GameDate))
            {
                var matches = _db.Matches.Where(x => x.Rink == -1 && x.WeekId == item.id);
                if(matches.Any())
                    ds.Byes.AddByesRow(item.WeekDate, matches.First().Team.TeamNo, Players(matches.First().Team, league.TeamSize));

            }
            reportViewer.LocalReport.ReportPath = Server.MapPath("/ReportFiles/Byes.rdlc");

            var p2 = new ReportParameter("Description", league.LeagueName);
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
            Match match = _db.Matches.Find(id);
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
                },
                new SelectListItem
                {
                    Value = "-1",
                    Text = "Both",
                    Selected = match.ForFeitId == -1
                }
            };
            ViewBag.TeamSize = match.Schedule.League.TeamSize;
            ViewBag.ForFeitId = forfeits;
            ViewBag.Id = match.Schedule.Leagueid;
            return View(match);
        }

        // POST: Matches/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Scoring([Bind(Include = "id,WeekId,Rink,TeamNo1,TeamNo2,Team1Score,Team2Score,ForFeitId,rowversion")] Match match)
        {
            League league=null;
            try
            {

                if (ModelState.IsValid)
                {
                    var schedule = _db.Schedules.Find(match.WeekId);
                    if (schedule == null)
                        return HttpNotFound();
                    league = schedule.League;
                    if (!league.TiesAllowed && match.Team1Score == match.Team2Score && match.ForFeitId == 0 && (match.Team1Score + match.Team2Score > 0 ))
                    {
                        ModelState.AddModelError(string.Empty, "Matched not scored, ties not allowed");
                    }
                    else
                    {
                        _db.Entry(match).State = EntityState.Modified;
                        _db.SaveChanges();
                        return RedirectToAction("Index", new { weekid = match.WeekId, id = league.id });
                    }
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

            var newmatch = _db.Matches.Find(match.id);
            if (newmatch == null)
                return HttpNotFound();
            newmatch.Team1Score = match.Team1Score;
            newmatch.Team2Score = match.Team2Score;
            newmatch.ForFeitId = match.ForFeitId;

            var forfeits = new List<SelectListItem>
            {
                new SelectListItem {Value = "0", Text = "No Forfeit", Selected = match.ForFeitId == 0},
                new SelectListItem
                {
                    Value = newmatch.Team.TeamNo.ToString(),
                    Text = $"Team No. {newmatch.Team.TeamNo}",
                    Selected = match.ForFeitId == newmatch.Team.TeamNo
                },
                new SelectListItem
                {
                    Value = newmatch.Team1.TeamNo.ToString(),
                    Text = $"Team No. {newmatch.Team1.TeamNo}",
                    Selected = match.ForFeitId == newmatch.Team1.TeamNo
                },
                new SelectListItem
                {
                    Value = "-1",
                    Text = "Both",
                    Selected = match.ForFeitId == -1
                }
            };
            ViewBag.TeamSize = newmatch.Schedule.League.TeamSize;
            ViewBag.ForFeitId = forfeits;
            ViewBag.Id = newmatch.Schedule.Leagueid;
            return View(newmatch);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
