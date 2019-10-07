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
   
    public class SchedulesController : Controller
    {
        private TournamentEntities db = new TournamentEntities();

        [Authorize]
        // GET: Schedules
        public ActionResult Index()
        {
            int leagueid = (int) this.HttpContext.Session["leagueid"];
            ViewBag.LeagueName = (string)HttpContext.Session["leaguename"];
            var list = db.Schedules.Where(x => x.Leagueid == leagueid).OrderBy(x=>x.WeekNumber).ToList();
            return View(list);
        }


        [Authorize(Roles = "Admin,LeagueAdmin")]
        // GET: Schedules/Create
        public ActionResult Create()
        {
            var id =1;
            var leagueid = (int)this.HttpContext.Session["leagueid"];
            DateTime date = DateTime.Now;
            var items = db.Schedules.Where(x=>x.Leagueid==leagueid).OrderByDescending(x => x.WeekNumber);
            if (items.Count() > 0)
            {
                date = items.First().GameDate.AddDays(7);
                id = items.First().id + 1;
            }

            var item = new Schedule()
            {
                WeekNumber = id,
                GameDate = date,
                Leagueid = leagueid,
                Cancelled = false
            };
            ViewBag.LeagueName = (string)HttpContext.Session["leaguename"];
            return View(item);
        }

        // POST: Schedules/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "GameDate,WeekNumber,LeagueId,Cancelled")] Schedule schedule)
        {
            if (ModelState.IsValid)
            {
                db.Schedules.Add(schedule);
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
            ViewBag.LeagueName = (string)HttpContext.Session["leaguename"];
            return View(schedule);
        }

        [Authorize(Roles = "Admin,LeagueAdmin")]
        // GET: Schedules/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Schedule schedule = db.Schedules.Find(id);
            if (schedule == null)
            {
                return HttpNotFound();
            }
            ViewBag.LeagueName = (string)HttpContext.Session["leaguename"];
            return View(schedule);
        }

        // POST: Schedules/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int? id, byte[] rowVersion)
        {
            string[] fieldsToBind = new string[] {"GameDate","Cancelled","WeekNumber", "rowversion" };
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var scheduleToUpdate = db.Schedules.Find(id);
            if (scheduleToUpdate == null)
            {
                var scheduleToDelete = new Schedule();
                TryUpdateModel(scheduleToDelete, fieldsToBind);
                ModelState.AddModelError(string.Empty,
                    "Unable to save changes. The schedule item was deleted by another user.");
                return View(scheduleToDelete);
            }

            if (TryUpdateModel(scheduleToUpdate, fieldsToBind))
            {
                try
                {
                    db.Entry(scheduleToUpdate).OriginalValues["rowversion"] = rowVersion;
                    db.SaveChanges();

                    return RedirectToAction("Index");
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    var entry = ex.Entries.Single();
                    var clientValues = (Schedule)entry.Entity;
                    var databaseEntry = entry.GetDatabaseValues();
                    if (databaseEntry == null)
                    {
                        ModelState.AddModelError(string.Empty,
                            "Unable to save changes. The schedule item was deleted by another user.");
                    }
                    else
                    {
                        var databaseValues = (Schedule)databaseEntry.ToObject();

                        if (databaseValues.WeekDate != clientValues.WeekDate)
                            ModelState.AddModelError("Game Date", "Current value: "
                                                                    + databaseValues.GameDate.ToShortDateString());
                        if (databaseValues.Cancelled != clientValues.Cancelled)
                            ModelState.AddModelError("Cancelled", "Current value: "
                                                                  + databaseValues.Cancelled);
                        if (databaseValues.WeekNumber != clientValues.WeekNumber)
                            ModelState.AddModelError("Week Number", "Current value: "
                                                                  + databaseValues.WeekNumber.ToString());
                        ModelState.AddModelError(string.Empty, "The record you attempted to edit "
                                                               + "was modified by another user after you got the original value. The "
                                                               + "edit operation was canceled and the current values in the database "
                                                               + "have been displayed. If you still want to edit this record, click "
                                                               + "the Save button again. Otherwise click the Back to List hyperlink.");
                        scheduleToUpdate.rowversion = databaseValues.rowversion;
                    }
                }
                catch (RetryLimitExceededException dex)
                {
                    //Log the error (uncomment dex variable name and add a line here to write a log.)
                    ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists, see your system administrator.");
                    ErrorSignal.FromCurrentContext().Raise(dex);
                }
            }
            ViewBag.LeagueName = (string)HttpContext.Session["leaguename"];
            return View(scheduleToUpdate); ;
        }

        [Authorize(Roles = "Admin,LeagueAdmin")]
        // GET: Schedules/Delete/5
        public ActionResult Delete(int? id, bool? concurrencyError)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var schedule = db.Schedules.Find(id);
            if (schedule == null)
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
            ViewBag.LeagueName = (string)HttpContext.Session["leaguename"];
            return View(schedule);
        }

        // POST: Schedules/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(Schedule schedule)
        {
            try
            {
                db.Entry(schedule).State = EntityState.Deleted;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            catch (DbUpdateConcurrencyException)
            {
                return RedirectToAction("Delete", new { concurrencyError = true, id = schedule.id });
            }
            catch (DataException dex)
            {
                //Log the error (uncomment dex variable name after DataException and add a line here to write a log.
                ModelState.AddModelError(string.Empty, "Unable to delete. Try again, and if the problem persists contact your system administrator.");
                ErrorSignal.FromCurrentContext().Raise(dex);
                ViewBag.LeagueName = (string)HttpContext.Session["leaguename"];
                return View(schedule);
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
