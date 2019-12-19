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

namespace Tournament.Controllers
{

    public class TeamsController : Controller
    {
        private TournamentEntities db = new TournamentEntities();
       

       [Authorize]
        // GET: Teams
        public ActionResult Index()
        {
            var leagueid = (int)HttpContext.Session["leagueid"];
           
            ViewBag.TeamSize = (int) HttpContext.Session["teamsize"];
            return View(db.Teams.Where(x=>x.Leagueid == leagueid).OrderBy(x => x.TeamNo).ToList());
        }

        

        [Authorize]
        public ActionResult TeamReport()
        {
            var leagueid = (int)HttpContext.Session["leagueid"];
            var reportViewer = new ReportViewer()
            {
                ProcessingMode = ProcessingMode.Local,
                Width = Unit.Pixel(800),
                Height = Unit.Pixel(1000),
                ShowExportControls =  true
            };
            switch ((int)HttpContext.Session["teamsize"])
            {
                case 1:
                    reportViewer.LocalReport.ReportPath = Server.MapPath("/ReportFiles/SingleTeams.rdlc");
                    break;
                case 2:
                    reportViewer.LocalReport.ReportPath = Server.MapPath("/ReportFiles/PairsTeams.rdlc");
                    break;
                case 3:
                    reportViewer.LocalReport.ReportPath = Server.MapPath("/ReportFiles/TriplesTeams.rdlc");
                    break;
            }

            var p2 = new ReportParameter("Description", (string) HttpContext.Session["leaguename"]);
            reportViewer.LocalReport.SetParameters(new ReportParameter[] { p2 });

            var teams = db.Teams.Where(x => x.Leagueid == leagueid);
            var ds = new TournamentDS();
            foreach (var team in teams)
            {
                ds.Team.AddTeamRow(team.TeamNo, team.Player.Membership.FullName,  team.Lead== null? "" : team.Player2.Membership.FullName, team.ViceSkip == null ? "" : team.Player1.Membership.FullName);
            }
            reportViewer.LocalReport.DataSources.Add(new ReportDataSource("Team", ds.Team.Rows));

            ViewBag.ReportViewer = reportViewer;
            return View();
        }



        // GET: Teams/Create
        public ActionResult Create()
        {
            var leagueid = (int)HttpContext.Session["leagueid"];
            var items = db.Teams.Where(x => x.Leagueid == leagueid).OrderBy(x=>x.TeamNo).ToList();
            int TeamNo = 1;
            if (items.Any())
            {
                items.Sort((a, b) => a.TeamNo.CompareTo(b.TeamNo));
                TeamNo = items[items.Count - 1].TeamNo + 1;
            }

            var team = new Team()
            {
                TeamNo = TeamNo,
                Leagueid = leagueid
            };

            var teams = db.Teams.Where(x=>x.Leagueid == leagueid).OrderBy(x => x.TeamNo);
            var list = RemainingPlayers(team, teams);
            
            ViewBag.List = list;
            ViewBag.Teams = teams;
            ViewBag.TeamSize = (int)HttpContext.Session["teamsize"];
            return View(team);
        }

        // POST: Teams/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "id,Skip,Lead,ViceSkip,TeamId,LeagueId,TeamNo")] Team team)
        {
            var leagueid = (int)HttpContext.Session["leagueid"];
            if (ModelState.IsValid)
            {
                    db.Teams.Add(team);
                    try
                    {
                        if (CheckTeam(team))
                        {
                            ModelState.AddModelError(string.Empty,
                                "Unable to create record, a player cannot be on a team in multiple positions");
                        }
                        else
                        {
                        db.SaveChanges();
                        return RedirectToAction("Index");
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
            var teams = db.Teams.Where(x => x.Leagueid == team.Leagueid).OrderBy(x => x.TeamNo);
            var list = RemainingPlayers(team, teams);
            ViewBag.List = list;
            ViewBag.Teams = teams;
            ViewBag.TeamSize = (int)HttpContext.Session["teamsize"];
           
            return View(team);
        }

        // GET: Teams/Edit/5
        public ActionResult Edit(int? id)
        {
            var leagueid = (int)HttpContext.Session["leagueid"];
           
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Team team = db.Teams.Find(id);
            if (team == null)
            {
                return HttpNotFound();
            }
            var teams = db.Teams.Where(x => x.Leagueid == leagueid).OrderBy(x => x.TeamNo);

            var list = RemainingPlayers(team, teams);
            ViewBag.List = list;
            ViewBag.Teams = teams;
            ViewBag.TeamSize = (int)HttpContext.Session["teamsize"]; 
            return View(team);
        }

        

        // POST: Teams/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "id,Skip,Lead,ViceSkip,TeamId,LeagueId,TeamNo,rowversion")] Team team)
        {
            
            try
            {
                if (ModelState.IsValid)
                {
                    if (CheckTeam(team))
                    {
                        ModelState.AddModelError(string.Empty,
                            "Unable to save record, a player cannot be on a team in multiple positions");
                    }
                    else
                    {

                        db.Entry(team).State = EntityState.Modified;
                        db.SaveChanges();
                        return RedirectToAction("Index");
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


                    ModelState.AddModelError(string.Empty, "The record you attempted to edit "
                                                           + "was modified by another user after you got the original value. The "
                                                           + "edit operation was canceled and the current values in the database "
                                                           + "have been displayed. If you still want to edit this record, click "
                                                           + "the Save button again. Otherwise click the Back to List hyperlink.");
                    team.rowversion = databaseValues.rowversion;
                }
            }

            var teams = db.Teams.Where(x => x.Leagueid == team.Leagueid).OrderBy(x => x.TeamNo);

            var list = RemainingPlayers(team, teams);
            ViewBag.List = list;
            ViewBag.Teams = teams;
            ViewBag.TeamSize = (int)HttpContext.Session["teamsize"];
            return View(team);
        }

        // GET: Teams/Delete/5
        public ActionResult Delete(int? id, bool? concurrencyError)
        {
            ViewBag.TeamSize = (int)HttpContext.Session["teamsize"];
           
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var team = db.Teams.Find(id);
            if (team == null)
            {
                if (concurrencyError.GetValueOrDefault())
                {
                    return RedirectToAction("Index");
                }
                return HttpNotFound();
            }

            if (concurrencyError.GetValueOrDefault())
            {
                ViewBag.Error = "The record you attempted to delete "
                                                  + "was modified by another user after you got the original values. "
                                                  + "The delete operation was canceled and the current values in the "
                                                  + "database have been displayed. If you still want to delete this "
                                                  + "record, click the Delete button again. Otherwise "
                                                  + "click the Back to List hyperlink.";
            }

            return View(team);
        }

        // POST: Teams/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id, byte[] rowversion)
        {

            ViewBag.TeamSize = (int)HttpContext.Session["teamsize"];
            var team = db.Teams.Find(id);
            if (team == null)
            {
                ViewBag.Error = "Unable to delete this record, another user deleted this record";
                return View(new Team());
            }
            if (db.Matches.Any(x => x.TeamNo1 == team.id || x.TeamNo2 == team.id))
            {
                ViewBag.Error = "Unable to delete this record, this team is scheduled to play matches";
                return View(team);
            }
            try
            {
                db.Entry(team).Property("rowversion").OriginalValue = rowversion;
                db.Entry(team).State = EntityState.Deleted;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            catch (DbUpdateConcurrencyException ex)
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

            return View(team);
        }

        /// <summary>
        /// Generate an order list of remaining players including members of the current team
        /// </summary>
        /// <param name="team">current team</param>
        /// <param name="leagueid">league number</param>
        /// <param name="teams">order list of all teas</param>
        /// <returns></returns>
        private List<PlayerViewModel> RemainingPlayers(Team team, IOrderedQueryable<Team> teams)
        {
            var list = new List<PlayerViewModel>();
            foreach (var player in db.Players.Where(x => x.Leagueid == team.Leagueid))
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

            list.Sort((a, b) => String.Compare(a.LastName, b.LastName, StringComparison.Ordinal));
            return list;
        }

        private bool CheckTeam(Team team)
        {
            if (team.Skip.HasValue && (team.Skip == team.ViceSkip || team.Skip == team.Lead))
                    return true;
            if (team.ViceSkip.HasValue && team.ViceSkip == team.Lead)
                return true;
            return false;
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
