﻿using System;
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
        private TournamentEntities db = new TournamentEntities();

        // GET: Leagues
        public ActionResult Index()
        {
            return View(db.Leagues.ToList());
        }

        
        // GET: Leagues/Create
        public ActionResult Create()
        {
            var item = new League()
            {
                Active = true
            };
            
            return View(item);
        }

        // POST: Leagues/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "id,LeagueName,TeamSize,Active,TiesAllowed,PointsCount,WinPoints,TiePoints,ByePoints")] League league)
        {
            if (ModelState.IsValid)
            {
                db.Leagues.Add(league);
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
            

            return View(league);
        }

        // GET: Leagues/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            League league = db.Leagues.Find(id);
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
        public ActionResult Edit([Bind(Include = "id,LeagueName,TeamSize,Active,rowversion,TiesAllowed,PointsCount,WinPoints,TiePoints,ByePoints")] League league)
        {
            try
            {

                if (ModelState.IsValid)
                {
                    db.Entry(league).State = EntityState.Modified;
                    db.SaveChanges();
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
                    if (databaseValues.Active != clientValues.Active)
                        ModelState.AddModelError("Active", "Current value: "
                                                              + databaseValues.Active);
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
            League league = db.Leagues.Find(id);
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
            var league = db.Leagues.Find(id);
            if (league == null)
            {
                ViewBag.Error = "Unable to delete this record, another user deleted this record";
            }
            else if(db.Players.Where(x=>x.Leagueid == id).Count() != 0 || db.Schedules.Where(x=>x.Leagueid==id).Count() != 0)
            {
                ViewBag.Error = "Unable to delete this record, there are players or scheduled weeks assigned to this league";
            }
            else
            {
                try
                {
                    
                    db.Matches.RemoveRange(db.Matches.Where(x => x.Team.Leagueid == league.id).ToList());
                    db.Teams.RemoveRange(db.Teams.Where(x => x.Leagueid == league.id).ToList());
                    db.Players.RemoveRange(db.Players.Where(x => x.Leagueid == league.id).ToList());
                    db.Schedules.RemoveRange(db.Schedules.Where(x => x.Leagueid == league.id).ToList());
                    db.Entry(league).Property("rowversion").OriginalValue = rowversion;
                    db.Entry(league).State = EntityState.Deleted;
                    db.SaveChanges();
                    return RedirectToAction("Index");
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
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
