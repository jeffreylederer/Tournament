using Elmah;
using System;
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
            var leagueid = (int)HttpContext.Session["leagueid"];
            var Matches = db.Matches.Where(x => x.RoundId == RoundId.Value && x.Rink != -1);
            ViewBag.ScheduleID = new SelectList(db.Schedules.Where(x=>x.Leagueid == leagueid), "id", "RoundName", RoundId == null?"0": RoundId.ToString());
            ViewBag.RoundId = db.Schedules.Find(RoundId.Value).id;
            ViewBag.WeekID = RoundId;
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
            

            foreach (var match in matches)
            {
                db.Matches.Add(new Match()
                {
                    id=0,
                    RoundId = match.Week + 1,
                    Rink = match.Rink == -1 ? -1 : match.Rink + 1,
                    TeamNo1 = match.Team1 + 1,
                    TeamNo2 = match.Team2 + 1,
                    Team1Score = 0,
                    Team2Score = 0
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

        public ActionResult ScoringReport(int RoundId)
        {

            var reportViewer = new ReportViewer()
            {
                ProcessingMode = ProcessingMode.Local,
                Width = Unit.Pixel(800),
                Height = Unit.Pixel(1000),
                ShowExportControls = true
            };
            ViewBag.RoundId = RoundId;

            
            var p2 = new ReportParameter("Description", (string)HttpContext.Session["leaguename"]);
            reportViewer.LocalReport.SetParameters(new ReportParameter[] { p2 });
            var RoundName = db.Schedules.Find(RoundId).RoundName;
            var ds = new TournamentDS();
            reportViewer.LocalReport.ReportPath = Server.MapPath("/ReportFiles/Byes.rdlc");

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
            using (var db = new TournamentEntities())
            {
                foreach (var item in db.Schedules.Where(x => x.Leagueid == leagueid).OrderBy(x=>x.SortOrder))
                {
                    var schedule = item.RoundName;
                    foreach (var match in db.Matches.Where(x => x.RoundId == item.id && x.Rink == -1).ToList())
                    {
                        var team = db.Teams.Find(match.TeamNo1);
                        ds.Byes.AddByesRow(item.RoundName, match.TeamNo1,
                            team.Player.NickName +
                            (!team.ViceSkip.HasValue? "": $",{team.Player1.NickName}") +
                            (!team.Lead.HasValue? "": $",{team.Player2.NickName}"));
                    }
                }
            }

            var p2 = new ReportParameter("Description", "Description");
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
            ViewBag.Forfeit = new SelectList(db.Forfeits, "value", "text", match.ForFeitId);
            return View(match);
        }

        // POST: Matches/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Scoring(int? id, byte[] rowVersion)
        {
            string[] fieldsToBind = new string[] {"RoundId","Rink","TeamNo1","TeamNo2","Team1Score","Team2Score","ForfeitId", "rowversion" };
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

                    return RedirectToAction("Index");
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
            ViewBag.Forfeit = new SelectList(db.Forfeits, "value", "text", update.ForFeitId);
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
