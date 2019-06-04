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

namespace Tournament.Controllers
{

    public class TeamsController : Controller
    {
        private TournamentEntities db = new TournamentEntities();
       

       
        // GET: Teams
        public ActionResult Index()
        {
            var Teams = db.Teams;
            ViewBag.TeamSize = (int) HttpContext.Session["teamsize"];
            return View(Teams.Where(x=>x.Leagueid == (int)HttpContext.Session["leagueid"]).OrderBy(x => x.TeamNo).ToList());
        }

        public ActionResult RemoveLead(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Team Team = db.Teams.Find(id);
            if (Team == null)
            {
                return HttpNotFound();
            }
            Team.Lead = null;
            db.Entry(Team).State = EntityState.Modified;
            try
            {
                db.SaveChanges();
            }
            catch (Exception e)
            {
                ErrorSignal.FromCurrentContext().Raise(e); ;
            }
            return RedirectToAction("Index");
        }

        public ActionResult RemoveViceSkip(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Team Team = db.Teams.Find(id);
            if (Team == null)
            {
                return HttpNotFound();
            }
            Team.ViceSkip = null;
            db.Entry(Team).State = EntityState.Modified;
            try
            {
                db.SaveChanges();
            }
            catch (Exception e)
            {
                ErrorSignal.FromCurrentContext().Raise(e); ;
            }
            return RedirectToAction("Index");
        }

        public ActionResult TeamReport()
        {
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

            var teams = db.Teams.Where(x => x.Leagueid == (int)HttpContext.Session["leagueid"]);
            var ds = new TournamentDS();
            foreach (var team in teams)
            {
                ds.Team.AddTeamRow(team.id, team.Player.FullName, team.ViceSkip == null? "": team.Player1.FullName, team.Lead== null? "" : team.Player2.FullName);
            }
            reportViewer.LocalReport.DataSources.Add(new ReportDataSource("Team", ds.Team.Rows));

            ViewBag.ReportViewer = reportViewer;
            return View();
        }



        // GET: Teams/Create
        public ActionResult Create()
        {
            var items = db.Teams.Where(x => x.Leagueid == (int)HttpContext.Session["leagueid"]).OrderBy(x=>x.TeamNo).ToList();
            int id = 1;
            if (items.Count > 1)
            {
                items.Sort((a, b) => a.id.CompareTo(b.id));
                id = items[items.Count - 1].id + 1;
            }

            var item = new Team()
            {
                id = id,
                Leagueid = (int)HttpContext.Session["leagueid"]
            };
            var teams = db.Teams.Where(x=>x.Leagueid == (int)HttpContext.Session["leagueid"]).OrderBy(x => x.TeamNo);
            var list = new List<Player>();
            foreach (var player in db.Players.Where(x => x.Active && x.Leagueid == (int)HttpContext.Session["leagueid"]))
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
        public ActionResult Create([Bind(Include = "id,Skip,Lead,ViceSkip,LeagueId, TeamNo")] Team team)
        {
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

            var teams = db.Teams.Where(x=>x.Leagueid == (int)HttpContext.Session["leagueid"]).OrderBy(x => x.TeamNo);
            var list = new List<Player>();
            foreach (var player in db.Players.Where(x => x.Active && x.Leagueid == (int)HttpContext.Session["leagueid"]))
            {
                if (!teams.Any(x => x.Skip == player.id || x.Lead == player.id || x.ViceSkip == player.id))
                    list.Add(player);
            }
            ViewBag.Skip = new SelectList(list.OrderBy(x => x.LastName), "id", "FullName", team.Skip);
            ViewBag.ViceSkip = new SelectList(list.OrderBy(x => x.LastName), "id", "FullName", team.ViceSkip);
            ViewBag.Lead = new SelectList(list.OrderBy(x => x.LastName), "id", "FullName", team.Lead);
            ViewBag.Teams = teams;
            ViewBag.TeamSize = (int)HttpContext.Session["teamsize"]; 
            return View(team);
        }

        // GET: Teams/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Team team = db.Teams.Find(id);
            if (team == null)
            {
                return HttpNotFound();
            }
            var teams = db.Teams.Where(x => x.Leagueid == (int)HttpContext.Session["leagueid"]).OrderBy(x => x.TeamNo);
            var list = new List<Player>();
            foreach (var player in db.Players.Where(x => x.Active && x.Leagueid== (int)HttpContext.Session["leagueid"]))
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
        public ActionResult Edit([Bind(Include = "id,Skip,Lead,ViceSkip,LeagueId,TeamNo")] Team team)
        {
            if (ModelState.IsValid)
            {
                db.Entry(team).State = EntityState.Modified;
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
                    ModelState.AddModelError(string.Empty, e.Message);

                }
                catch (Exception e)
                {
                    ErrorSignal.FromCurrentContext().Raise(e);
                    ModelState.AddModelError(string.Empty, "Edit failed");
                }
            }
            var teams = db.Teams.Where(x => x.Leagueid == (int)HttpContext.Session["leagueid"]).OrderBy(x => x.TeamNo);

            var list = new List<Player>();
            foreach (var player in db.Players.Where(x => x.Active && x.Leagueid == (int)HttpContext.Session["leagueid"]))
            {
                if (!teams.Any(x => x.Skip == player.id || x.Lead == player.id || x.ViceSkip == player.id))
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

        // GET: Teams/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Team team = db.Teams.Find(id);
            if (team == null)
            {
                return HttpNotFound();
            }
            ViewBag.TeamSize = (int)HttpContext.Session["teamsize"];
            return View(team);
        }

        // POST: Teams/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Team team = db.Teams.Find(id);
            if (team == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            db.Teams.Remove(team);
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
                ViewBag.Error = ex.Message;
            }
            catch (Exception e)
            {
                ErrorSignal.FromCurrentContext().Raise(e);
                ViewBag.Error = "Delete failed";
            }
            ViewBag.TeamSize = (int)HttpContext.Session["teamsize"];
            return View(team);
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
