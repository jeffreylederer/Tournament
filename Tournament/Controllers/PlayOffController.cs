using DocumentFormat.OpenXml.InkML;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;
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
using Tournament.Code;
using Tournament.Models;
using Tournament.ReportFiles;

namespace Tournament.Controllers
{
    public class PlayOffController : Controller
    {
        private readonly TournamentEntities _db = new TournamentEntities();
        // GET: PlayOff
        public ActionResult Index(int id, int? weekid)
        {
            ViewBag.Error = string.Empty;

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
                    schedule = _db.Schedules.Where(x => x.Leagueid == id && x.PlayOffs).First();
                    weekid = schedule.id;
                }
                else
                {
                    ViewBag.Error = "No dates have been scheduled";
                }
            }
            else if (weekid > 0)
            {

                schedule = _db.Schedules.Find(weekid.Value);
                if (schedule == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.NotFound);
                }
            }
            
            ViewBag.TeamSize = league.TeamSize;
            var matches = _db.Matches.Where(x => x.WeekId == weekid.Value).OrderBy(x => x.Rink).ToList();
            if (schedule != null)
            {
                
                ViewBag.ScheduleID =
                    new SelectList(_db.Schedules.Where(x => x.Leagueid == id && x.PlayOffs).OrderBy(x => x.GameDate).ToList(), "id",
                        "WeekDate", weekid);
                ViewBag.Date = schedule.WeekDate;
            }
            else
            {
                ViewBag.Date = DateTime.Now.ToShortDateString();
                ViewBag.scheduleId =
                    new SelectList(_db.Schedules.Where(x => x.Leagueid == id && x.PlayOffs).OrderBy(x => x.GameDate).ToList(), "id",
                        "WeekDate");
            }

            ViewBag.WeekId = weekid;
            ViewBag.Id = id;

            foreach (Team item in _db.Teams.Where(x => x.Leagueid == league.id))
            {
                switch (league.TeamSize)
                {
                    case 1:
                        if (item.Skip == null)
                        {
                            ViewBag.Error = $"No player on team number  {item.TeamNo}";
                            return View();
                        }

                        break;
                    case 2:
                        if (item.Skip == null || item.Lead == null)
                        {
                            ViewBag.Error = $"Player is missing from team number {item.TeamNo}";
                            return View();
                        }

                        break;
                    case 3:
                        if (item.Skip == null || item.Lead == null || item.ViceSkip == null)
                        {
                            ViewBag.Error = $"Player is missing from team number {item.TeamNo}";
                            return View();
                        }

                        break;
                }
            }


            ViewBag.Visible = (matches.Count() == _db.Teams.Where(x => x.Leagueid == league.id).Count() / 2) ? "0": "1";
            return View(matches);
        }

      

        public ActionResult ScheduleReport(int id, int weekid)
        {
            ViewBag.Id = id;
            ViewBag.WeekId = weekid;

            ViewBag.Id = id;
            var league = _db.Leagues.Find(id);
            if (league == null)
                return HttpNotFound();
            var week = _db.Schedules.Find(weekid);
            if(week == null)
                return HttpNotFound();
            ViewBag.Date = week.GameDate.ToShortDateString();
            var table = new TournamentDS.ScheduleDataTable();
            int weekNumber = 1;
            int rinks = 0;

            var matches = _db.Matches.Where(x => x.WeekId == week.id).OrderBy(x => x.Rink).ToList();
            var matchesByes = _db.Matches.Where(x => x.Rink == -1 && x.WeekId == week.id).ToList();
            object[] row = new object[13];
            row[0] = weekNumber++;
            row[1] = $"{week.GameDate.Month}/{week.GameDate.Day}";
            if (matchesByes.Any())
                row[2] = matchesByes.First().Team.TeamNo.ToString();
            int rinkNumber = 3;
            foreach (var match in matches)
            {
                if (match.Rink != -1)
                {
                    row[rinkNumber++] = $"{match.Team.TeamNo}-{match.Team1.TeamNo}";
                    rinks = Math.Max(rinks, match.Rink);
                }

            }
            table.Rows.Add(row);
         
            var reportViewer = new ReportViewer()
            {
                ProcessingMode = ProcessingMode.Local,
                Width = Unit.Pixel(800),
                Height = Unit.Pixel(1000),
                ShowExportControls = true
            };


            reportViewer.LocalReport.ReportPath = Server.MapPath("/ReportFiles/Schedule.rdlc");


            var teams = _db.Teams.Where(x => x.Leagueid == id).OrderBy(x => x.TeamNo);
            var ds = new TournamentDS();
            foreach (var team in teams)
            {
                ds.Team.AddTeamRow(team.TeamNo, team.Player.Membership.FullName, team.Lead == null ? "" : team.Player2.Membership.FullName, team.ViceSkip == null ? "" : team.Player1.Membership.FullName, team.DivisionId);
            }
            reportViewer.LocalReport.DataSources.Add(new ReportDataSource("Team", ds.Team.Rows));
            reportViewer.LocalReport.DataSources.Add(new ReportDataSource("Schedule", table.Rows));
            var p1 = new ReportParameter("TeamSize", league.TeamSize.ToString());
            var p2 = new ReportParameter("Description", $"{league.LeagueName} Playoff");
            var p3 = new ReportParameter("Rinks", (teams.Count() / 2).ToString());
            var p4 = new ReportParameter("Divisions", league.Divisions.ToString());
            reportViewer.LocalReport.SetParameters(new ReportParameter[] { p1, p2, p3, p4 });
            ViewBag.ReportViewer = reportViewer;
            return View();
        }

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
           
            foreach (var item in _db.Matches.Include(x => x.Team1).Include(x => x.Team).Include(x => x.Schedule).Where(x => x.WeekId == weekid && x.Rink != -1)
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
                    forfeit == 0 ? "" : (forfeit == -1 ? "Both" : forfeit.ToString()));
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
          
           

            reportViewer.LocalReport.DataSources.Add(new ReportDataSource("Stand", CalculateStandings.DoitPlayoffs(weekid, league).Rows));
            reportViewer.LocalReport.ReportPath = Server.MapPath("/ReportFiles/Standings.rdlc");
            reportViewer.LocalReport.DataSources.Add(new ReportDataSource("Game", ds.Game.Rows));

            var p1 = new ReportParameter("WeekDate", weekDate);
            var p2 = new ReportParameter("Description", $"{league.LeagueName} Playoff");
            var p3 = new ReportParameter("IsBye", IsBye ? "1" : "0");
            var p4 = new ReportParameter("IsCancelled", isCancelled ? "1" : "0");
            var p5 = new ReportParameter("PointsCount", league.PointsCount ? "1" : "0");
            var p6 = new ReportParameter("TiesAllowed", league.TiesAllowed ? "1" : "0");
            var p7 = new ReportParameter("Divisions", league.Divisions.ToString());
            var p8 = new ReportParameter("PlayOff", week.PlayOffs ? "1" : "0");

            reportViewer.LocalReport.SetParameters(new ReportParameter[] { p1, p2, p3, p4, p5, p6, p7, p8 });


            ViewBag.ReportViewer = reportViewer;
            return View();
        }

        [Authorize(Roles = "Admin,LeagueAdmin")]
        public ActionResult CreateMatch(int weekid)
        {

            var schedule = _db.Schedules.Find(weekid);
            if (schedule == null)
                return HttpNotFound();
            var league = _db.Leagues.Find(schedule.Leagueid);
            if (league == null)
                return HttpNotFound();
               
            var list = ViewBag.List = GetList(weekid, schedule.Leagueid);
            if (list.Count == 0)
            {
                ViewBag.Error = "No teams left";
                return View();
            }
            if (list.Count < 2)
            {
                return RedirectToAction($"Index", new { id = schedule.Leagueid, weekid = weekid });
            }
            var playoff = new PlayoffMatch()
            {
                WeekId = weekid,
                LeagueId = schedule.Leagueid,
                GameDate = schedule.GameDate.ToShortDateString(),
                TeamNo1 = 0,
                TeamNo2 = 0
            };
            return View(playoff);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateMatch([Bind(Include = "TeamNo1,TeamNo2,WeekId,LeagueId,GameDate")] PlayoffMatch playoff)
        {
           
            ViewBag.List = GetList(playoff.WeekId, playoff.LeagueId);
           
            if (!ModelState.IsValid)
            {
                ViewBag.Error = "try again";
                return View(playoff);
            }
            if (playoff.TeamNo1 == 0 || playoff.TeamNo2==0)
            {
                ViewBag.Error = "Select a team";
                return View(playoff);
            }
            ViewBag.Error = "";
            if(playoff.TeamNo1 == playoff.TeamNo2)
            {
                ViewBag.Error = "Teams must be different";
                return View(playoff);
            }
            int Rink = 1;
            var matches = _db.Matches.Where(x=>x.WeekId == playoff.WeekId).ToList();
            if(matches .Count > 0)
            {
                Rink = matches.Max(x => x.Rink) + 1;
            }
            var newMatch = new Match()
            {
                WeekId = playoff.WeekId,
                Rink = Rink,
                TeamNo1 = playoff.TeamNo1,
                TeamNo2 = playoff.TeamNo2,
                Team1Score = 0,
                Team2Score = 0,
                ForFeitId = 0
            };
            try
            {
                _db.Matches.Add(newMatch);
                _db.SaveChanges();
                return RedirectToAction($"Index", new {id= playoff.LeagueId, weekid= playoff.WeekId });
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
            return View(playoff);
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
            League league = null;
            try
            {

                if (ModelState.IsValid)
                {
                    var schedule = _db.Schedules.Find(match.WeekId);
                    if (schedule == null)
                        return HttpNotFound();
                    league = schedule.League;
                    if (!league.TiesAllowed && match.Team1Score == match.Team2Score && match.ForFeitId == 0 && (match.Team1Score + match.Team2Score > 0))
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
                    if (databaseValues.Team2Score != clientValues.Team2Score)
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

        [Authorize(Roles = "Admin,LeagueAdmin")]
        public ActionResult MoveUp(int id)
        {

            var match = _db.Matches.Find(id);
            if (match == null)
                return HttpNotFound();
            var weekMatches = _db.Matches.Where(x => x.WeekId == match.WeekId);
            var match1 = weekMatches.First(x => x.Rink == match.Rink - 1);
            match1.Rink = match.Rink;
            match.Rink = match1.Rink - 1;
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

        private List<SelectListItem> GetList(int weekid, int leagueid)
        {
            var league = _db.Leagues.Find(leagueid);
            var list = new List<SelectListItem>();
            list.Add(new SelectListItem()
            {
                Text = " -- Select Team --",
                Value = "0",
                Selected = true
            });
            List<Match> matches = _db.Matches.Where(x => x.WeekId == weekid).ToList(); ;
            foreach (var team in _db.Teams.OrderBy(x => x.DivisionId).Where(x => x.Leagueid == leagueid))
            {
                if (matches.Any(x => x.Team1.id == team.id) || matches.Any(x => x.Team.id == team.id) )
                    continue;

                switch (league.TeamSize)
                {
                    case 1:
                        list.Add(new SelectListItem()
                        {
                            Text = $"{team.Player1.Membership.NickName} -D:{team.DivisionId}",
                            Value = team.id.ToString()
                        });
                        break;
                    case 2:
                        list.Add(new SelectListItem()
                        {
                            Text = $"{team.Player.Membership.NickName}, {team.Player2.Membership.NickName} -D:{team.DivisionId}",
                            Value = team.id.ToString()
                        });
                        break;
                    case 3:
                        list.Add(new SelectListItem()
                        {
                            Text = $"{team.Player.Membership.NickName}, {team.Player1.Membership.NickName}, {team.Player2.Membership.NickName} -D:{team.DivisionId}",
                            Value = team.id.ToString()
                        });
                        break;
                }
               
            }
            return list;
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