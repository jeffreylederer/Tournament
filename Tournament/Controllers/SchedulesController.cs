using Elmah;
using System;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using Tournament.Models;

namespace Tournament.Controllers
{

    public class SchedulesController : Controller
    {
        private TournamentEntities db = new TournamentEntities();

        [Authorize]
        // GET: Schedules
        public ActionResult Index(int id)
        {
            ViewBag.Id = id;
            ViewBag.PlayOffs = db.Leagues.Find(id).PlayOffs;
            return View(db.ScheduleAllowDelete(id).ToList());
        }


        [Authorize(Roles = "Admin,LeagueAdmin")]
        // GET: Schedules/Create
        public ActionResult Create(int id)
        {
            DateTime date = DateTime.Now;
            var list = db.Schedules.OrderBy(x => x.GameDate).ToList();
            if (list.Any())
                date = list.Last().GameDate.AddDays(7);
            var schedule = new Schedule()
            {
                GameDate = date,
                Leagueid = id,
                Cancelled = false
            };
            ViewBag.Schedule = db.Schedules.Where(x => x.Leagueid == id).OrderBy(x => x.GameDate);
            ViewBag.PlayOffs = db.Leagues.Find(id).PlayOffs;
            return View(schedule);
        }

        // POST: Schedules/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "GameDate,PlayOffs,LeagueId,Cancelled")] Schedule schedule)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    if (ModelState.IsValid)
                    {
                        if (db.Schedules.Any(x => x.GameDate == schedule.GameDate))
                        {
                            ModelState.AddModelError(string.Empty, "Duplicate date is not allowed");
                        }
                        else
                        {

                            db.Schedules.Add(schedule);
                            db.SaveChanges();
                            return RedirectToAction("Index", new {id = schedule.Leagueid});
                        }
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
            ViewBag.Schedule = db.Schedules.Where(x => x.Leagueid == schedule.Leagueid).OrderBy(x => x.GameDate);
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
            ViewBag.Schedule = db.Schedules.Where(x => x.Leagueid == schedule.Leagueid).OrderBy(x => x.GameDate);

            ViewBag.PlayOffs = db.Leagues.Find(schedule.Leagueid).PlayOffs;
            return View(schedule);
        }

        // POST: Schedules/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "id,GameDate,PlayOffs,Leagueid,Cancelled,rowversion")] Schedule schedule)
        {
            try
            {

                if (ModelState.IsValid)
                {
                    if (db.Schedules.Any(x => x.GameDate == schedule.GameDate && x.id != schedule.id))
                    {
                        ModelState.AddModelError(string.Empty, "Duplicate date is not allowed");
                    }
                    else
                    {
                        db.Entry(schedule).State = EntityState.Modified;
                        db.SaveChanges();
                        return RedirectToAction("Index", new {id = schedule.Leagueid});
                    }
                }
            }
            catch (DbUpdateConcurrencyException ex)
            {
                var entry = ex.Entries.Single();
                var clientValues = (Schedule)entry.Entity;
                var databaseEntry = entry.GetDatabaseValues();
                if (databaseEntry == null)
                {
                    ModelState.AddModelError(string.Empty,
                        "Unable to save changes. The schedule record was deleted by another user.");
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
                    if (databaseValues.Leagueid != clientValues.Leagueid)
                        ModelState.AddModelError("League", "Current value: "
                                                                + databaseValues.Leagueid);

                    ModelState.AddModelError(string.Empty, "The record you attempted to edit "
                                                           + "was modified by another user after you got the original value. The "
                                                           + "edit operation was canceled and the current values in the database "
                                                           + "have been displayed. If you still want to edit this record, click "
                                                           + "the Save button again. Otherwise click the Back to List hyperlink.");
                    schedule.rowversion = databaseValues.rowversion;
                }
            }
            catch (Exception dex)
            {
                while (dex.InnerException != null)
                    dex = dex.InnerException;
                //Log the error (uncomment dex variable name and add a line here to write a log.)
                ModelState.AddModelError("",
                    $"Unable to save changes. {dex.Message}");
                ErrorSignal.FromCurrentContext().Raise(dex);
            }
            ViewBag.Schedule = db.Schedules.Where(x => x.Leagueid == schedule.Leagueid).OrderBy(x => x.GameDate);
            return View(schedule);
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
                    return RedirectToAction("Welcome", "Home");
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
            ViewBag.PlayOffs = db.Leagues.Find(schedule.Leagueid).PlayOffs;
            return View(schedule);
        }

        // POST: Schedules/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id, byte[] rowversion)
        {

            var schedule = db.Schedules.Find(id);
            if (schedule == null)
            {
                ViewBag.Error = "Unable to delete this record, another user deleted this record";
                return View(new Schedule());
            }
            try
            {
                db.Entry(schedule).Property("rowversion").OriginalValue = rowversion;
                db.Entry(schedule).State = EntityState.Deleted;
                db.SaveChanges();
                return RedirectToAction("Index", new {id = schedule.Leagueid});
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
            return View(schedule);
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
