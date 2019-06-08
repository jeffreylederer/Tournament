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
using Tournament.Models;
using Tournament.ReportFiles;
using System.Data.Entity.Infrastructure;
using System.Text;

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
            ViewBag.LeagueName = (string)HttpContext.Session["leaguename"];
            ViewBag.TeamSize = (int) HttpContext.Session["teamsize"];
            return View(db.Teams.Where(x=>x.Leagueid == leagueid).OrderBy(x => x.TeamNo).ToList());
        }

        [Authorize(Roles = "Admin,LeagueAdmin")]
        public ActionResult RemoveLead(int? id, string rowversion)
        {
            byte[] bytes = Encoding.ASCII.GetBytes(rowversion);
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Team team = db.Teams.Find(id);
            if (team == null)
            {
                return HttpNotFound();
            }
            team.Lead = null;
            db.Entry(team).State = EntityState.Modified;
            try
            {
                db.Entry(team).OriginalValues["rowversion"] = bytes;
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                ErrorSignal.FromCurrentContext().Raise(ex); ;
            }
            catch (RetryLimitExceededException dex)
            {
                //Log the error (uncomment dex variable name and add a line here to write a log.)
                //ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists, see your system administrator.");
                ErrorSignal.FromCurrentContext().Raise(dex);
            }
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Admin,LeagueAdmin")]
        public ActionResult RemoveViceSkip(int? id, string rowversion)
        {
            byte[] bytes = Encoding.ASCII.GetBytes(rowversion);
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Team team = db.Teams.Find(id);
            if (team == null)
            {
                return HttpNotFound();
            }
            team.ViceSkip = null;
            db.Entry(team).State = EntityState.Modified;
            try
            {
                db.Entry(team).OriginalValues["rowversion"] = bytes;
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                ErrorSignal.FromCurrentContext().Raise(ex); ;
            }
            catch (RetryLimitExceededException dex)
            {
                //Log the error (uncomment dex variable name and add a line here to write a log.)
                //ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists, see your system administrator.");
                ErrorSignal.FromCurrentContext().Raise(dex);
            }
            return RedirectToAction("Index");
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
                ds.Team.AddTeamRow(team.id, team.Player.FullName,  team.Lead== null? "" : team.Player2.FullName, team.ViceSkip == null ? "" : team.Player1.FullName);
            }
            reportViewer.LocalReport.DataSources.Add(new ReportDataSource("Team", ds.Team.Rows));

            ViewBag.ReportViewer = reportViewer;
            return View();
        }



        // GET: Teams/Create
        public ActionResult Create()
        {
            var leagueid = (int)HttpContext.Session["leagueid"];
            ViewBag.LeagueName = (string)HttpContext.Session["leaguename"];
            var items = db.Teams.Where(x => x.Leagueid == leagueid).OrderBy(x=>x.TeamNo).ToList();
            int TeamNo = 1;
            if (items.Any())
            {
                items.Sort((a, b) => a.TeamNo.CompareTo(b.TeamNo));
                TeamNo = items[items.Count - 1].TeamNo + 1;
            }

            var item = new Team()
            {
                TeamNo = TeamNo,
                Leagueid = leagueid
            };
            var teams = db.Teams.Where(x=>x.Leagueid == leagueid).OrderBy(x => x.TeamNo);
            var list = new List<Player>();
            foreach (var player in db.Players.Where(x => x.Active && x.Leagueid == leagueid))
            {
                if (!teams.Any(x => x.Skip == player.id || x.Lead == player.id || x.ViceSkip == player.id))
                    list.Add(player);
            }
            ViewBag.Skip = new SelectList(list.OrderBy(x => x.LastName), "id", "FullName", " ");
            ViewBag.ViceSkip = new SelectList(list.OrderBy(x => x.LastName), "id", "FullName", " ");
            ViewBag.Lead = new SelectList(list.OrderBy(x => x.LastName), "id", "FullName", " ");
            ViewBag.Teams = teams;
            ViewBag.TeamSize = (int)HttpContext.Session["teamsize"];
            return View(item);
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
                    db.SaveChanges();
                    return RedirectToAction("Index");
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

            var teams = db.Teams.Where(x=>x.Leagueid == leagueid).OrderBy(x => x.TeamNo);
            var list = new List<Player>();
            foreach (var player in db.Players.Where(x => x.Active && x.Leagueid == leagueid))
            {
                if (!teams.Any(x => x.Skip == player.id || x.Lead == player.id || x.ViceSkip == player.id))
                    list.Add(player);
            }
            ViewBag.Skip = new SelectList(list.OrderBy(x => x.LastName), "id", "FullName", team.Skip);
            ViewBag.ViceSkip = new SelectList(list.OrderBy(x => x.LastName), "id", "FullName", team.ViceSkip);
            ViewBag.Lead = new SelectList(list.OrderBy(x => x.LastName), "id", "FullName", team.Lead);
            ViewBag.Teams = teams;
            ViewBag.TeamSize = (int)HttpContext.Session["teamsize"];
            ViewBag.LeagueName = (string)HttpContext.Session["leaguename"];
            return View(team);
        }

        // GET: Teams/Edit/5
        public ActionResult Edit(int? id)
        {
            var leagueid = (int)HttpContext.Session["leagueid"];
            ViewBag.LeagueName = (string)HttpContext.Session["leaguename"];
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
            var list = new List<Player>();
            foreach (var player in db.Players.Where(x => x.Active && x.Leagueid == leagueid))
            {
                if(!teams.Any(x => x.Skip == player.id || x.Lead == player.id || x.ViceSkip == player.id))
                    list.Add(player);
            }
            if (team.ViceSkip != null)
                list.Add(team.Player1);
            if (team.Lead != null)
                list.Add(team.Player2);
            ViewBag.Lead = new SelectList(list.OrderBy(x => x.LastName), "id", "FullName", team.Lead);
            ViewBag.ViceSkip = new SelectList(list.OrderBy(x => x.LastName), "id", "FullName", team.ViceSkip);
            ViewBag.Teams = teams;
            ViewBag.TeamSize = (int)HttpContext.Session["teamsize"]; 
            return View(team);
        }

        // POST: Teams/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int? id, byte[] rowVersion)
        {
            string[] fieldsToBind = new string[] {"Skip", "Lead", "ViceSkip", "LeagueId", "TeamNo", "rowversion"};
            var leagueid = (int)HttpContext.Session["leagueid"];
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var update = db.Teams.Find(id);
            if (update == null)
            {
                var delete = new Team();
                TryUpdateModel(delete, fieldsToBind);
                ModelState.AddModelError(string.Empty,
                    "Unable to save changes. The team was deleted by another user.");
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
                    var clientValues = (Team)entry.Entity;
                    var databaseEntry = entry.GetDatabaseValues();
                    if (databaseEntry == null)
                    {
                        ModelState.AddModelError(string.Empty,
                            "Unable to save changes. The team was deleted by another user.");
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
            var teams = db.Teams.Where(x => x.Leagueid == leagueid).OrderBy(x => x.TeamNo);
            var list = new List<Player>();
            foreach (var player in db.Players.Where(x => x.Active && x.Leagueid == leagueid))
            {
                if (!teams.Any(x => x.Skip == player.id || x.Lead == player.id || x.ViceSkip == player.id))
                    list.Add(player);
            }
            if (update.ViceSkip != null)
                list.Add(update.Player1);
            if (update.Lead != null)
                list.Add(update.Player2);
            ViewBag.Lead = new SelectList(list.OrderBy(x => x.LastName), "id", "FullName", update.Lead);
            ViewBag.ViceSkip = new SelectList(list.OrderBy(x => x.LastName), "id", "FullName", update.ViceSkip);
            ViewBag.Teams = teams;
            ViewBag.TeamSize = (int)HttpContext.Session["teamsize"];
            ViewBag.LeagueName = (string)HttpContext.Session["leaguename"];
            return View(update);
        }

        // GET: Teams/Delete/5
        public ActionResult Delete(int? id, bool? concurrencyError)
        {
            ViewBag.TeamSize = (int)HttpContext.Session["teamsize"];
            ViewBag.LeagueName = (string)HttpContext.Session["leaguename"];
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
                ViewBag.ConcurrencyErrorMessage = "The record you attempted to delete "
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
        public ActionResult DeleteConfirmed(Team team)
        {
            ViewBag.TeamSize = (int)HttpContext.Session["teamsize"];
            ViewBag.LeagueName = (string)HttpContext.Session["leaguename"];
            try
            {
                db.Entry(team).State = EntityState.Deleted;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            catch (DbUpdateConcurrencyException)
            {
                return RedirectToAction("Delete", new { concurrencyError = true, id = team.id });
            }
            catch (DataException dex)
            {
                //Log the error (uncomment dex variable name after DataException and add a line here to write a log.
                ModelState.AddModelError(string.Empty, "Unable to delete. Try again, and if the problem persists contact your system administrator.");
                ErrorSignal.FromCurrentContext().Raise(dex);
                return View(team);
            }
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
