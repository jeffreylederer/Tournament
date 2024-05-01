using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Elmah;
using Tournament.Models;

namespace Tournament.Controllers
{
    [Authorize(Roles = "Admin")]
    public class LeaguesController : Controller
    {
        private readonly TournamentEntities _db = new TournamentEntities();

        // GET: Leagues
        public ActionResult Index()
        {
            return View(_db.LeagueAllowDelete().ToList());
        }

        
        // GET: Leagues/Create
        public ActionResult Create()
        {
            var item = new League()
            {
                Active = true,
                TiesAllowed = false,
                PointsCount =  true,
                WinPoints = 1,
                TiePoints = 1,
                ByePoints = 1,
                TeamSize=2,
                StartWeek = 1,
                PointsLimit = true,
                Divisions=1
            };
            
            return View(item);
        }

        // POST: Leagues/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "id,LeagueName,TeamSize,Divisions,Active,TiesAllowed,PointsCount,WinPoints,TiePoints,ByePoints,StartWeek, PointsLimit")] League league)
        {
            if (ModelState.IsValid)
            {
                _db.Leagues.Add(league);
                try
                {
                    _db.SaveChanges();
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
            

            return View(league);
        }

        // GET: Leagues/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var league = _db.Leagues.Find(id);
            if (league == null)
            {
                return HttpNotFound();
            }
            return View(league);
        }

        // POST: Leagues/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "id,LeagueName,TeamSize,Divisions,Active,rowversion,TiesAllowed,PointsCount,WinPoints,TiePoints,ByePoints,StartWeek,PointsLimit")] League league)
        {
            try
            {

                if (ModelState.IsValid)
                {
                    _db.Entry(league).State = EntityState.Modified;
                    _db.SaveChanges();
                    return RedirectToAction("Index");
                }
            }
            catch (DbUpdateConcurrencyException ex)
            {
                var entry = ex.Entries.Single();
                var clientValues = (League)entry.Entity;
                var databaseEntry = entry.GetDatabaseValues();
                if (databaseEntry == null)
                {
                    ModelState.AddModelError(string.Empty,
                        "Unable to save changes. The league was deleted by another user.");
                }
                else
                {
                    var databaseValues = (League)databaseEntry.ToObject();

                    if (databaseValues.LeagueName != clientValues.LeagueName)
                        ModelState.AddModelError("League Name", "Current value: "
                                                                + databaseValues.LeagueName);
                    if (databaseValues.TeamSize != clientValues.TeamSize)
                        ModelState.AddModelError("Team Size", "Current value: "
                                                              + databaseValues.TeamSize);
                    if (databaseValues.Divisions != clientValues.Divisions)
                        ModelState.AddModelError("Number of Divisions", "Current value: "
                                                              + databaseValues.Divisions);
                    if (databaseValues.Active != clientValues.Active)
                        ModelState.AddModelError("Active", "Current value: "
                                                              + databaseValues.Active);
                    if (databaseValues.TiesAllowed != clientValues.TiesAllowed)
                        ModelState.AddModelError("Ties Allowed", "Current value: "
                                                           + databaseValues.TiesAllowed);
                    if (databaseValues.PointsCount != clientValues.PointsCount)
                        ModelState.AddModelError("Do Points Count", "Current value: "
                                                           + databaseValues.PointsCount);
                    if (databaseValues.WinPoints != clientValues.WinPoints)
                        ModelState.AddModelError("Win Points", "Current value: "
                                                           + databaseValues.WinPoints);
                    if (databaseValues.TiePoints != clientValues.TiePoints)
                        ModelState.AddModelError("Tie Points", "Current value: "
                                                           + databaseValues.TiePoints);
                    if (databaseValues.ByePoints != clientValues.ByePoints)
                        ModelState.AddModelError("Bye Points", "Current value: "
                                                               + databaseValues.ByePoints);
                    if (databaseValues.StartWeek != clientValues.StartWeek)
                        ModelState.AddModelError("Start Week", "Current value: "
                                                               + databaseValues.StartWeek);
                    if (databaseValues.PointsLimit != clientValues.PointsLimit)
                        ModelState.AddModelError("PointLimit", "Current value: "
                                                               + databaseValues.StartWeek);

                    ModelState.AddModelError(string.Empty, "The record you attempted to edit "
                                                           + "was modified by another user after you got the original value. The "
                                                           + "edit operation was canceled and the current values in the database "
                                                           + "have been displayed. If you still want to edit this record, click "
                                                           + "the Save button again. Otherwise click the Back to List hyperlink.");
                    league.rowversion = databaseValues.rowversion;
                }
            }
            catch (Exception dex)
            {
                //Log the error (uncomment dex variable name and add a line here to write a log.)
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists, see your system administrator.");
                ErrorSignal.FromCurrentContext().Raise(dex);
            }
            return View(league);
        }

        // GET: Leagues/Delete/5
        public ActionResult Delete(int? id, bool? concurrencyError)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var league = _db.Leagues.Find(id);
            if (league == null)
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

            return View(league);
        }

        // POST: Leagues/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id, byte[] rowversion)
        {
            var league = _db.Leagues.Find(id);
            if (league == null)
            {
                ViewBag.Error = "Unable to delete this record, another user deleted this record";
            }
           else
            {
                try
                {
                    
                    _db.Matches.RemoveRange(_db.Matches.Where(x => x.Team.Leagueid == league.id).ToList());
                    _db.Teams.RemoveRange(_db.Teams.Where(x => x.Leagueid == league.id).ToList());
                    _db.Players.RemoveRange(_db.Players.Where(x => x.Leagueid == league.id).ToList());
                    _db.Schedules.RemoveRange(_db.Schedules.Where(x => x.Leagueid == league.id).ToList());
                    _db.UserLeagues.RemoveRange(_db.UserLeagues.Where(x => x.LeagueId == id));
                    _db.Entry(league).Property("rowversion").OriginalValue = rowversion;
                    _db.Entry(league).State = EntityState.Deleted;
                    _db.SaveChanges();
                    return RedirectToAction("Index","Home");
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

            }
            return View(league);
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
