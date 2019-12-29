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
        public ActionResult Index()
        {
            int leagueid = (int) this.HttpContext.Session["leagueid"];

            var list = db.Schedules.Where(x => x.Leagueid == leagueid).OrderBy(x=>x.WeekNumber).ToList();
            return View(list);
        }


        [Authorize(Roles = "Admin,LeagueAdmin")]
        // GET: Schedules/Create
        public ActionResult Create()
        {
            var leagueid = (int)this.HttpContext.Session["leagueid"];
            DateTime date = DateTime.Now;
            int WeekNumber = 1;
            var list = db.Schedules.Where(x => x.Leagueid == leagueid).OrderBy(x=>x.GameDate).ToList();
            if (list.Any())
            {
                WeekNumber = list.Last().WeekNumber + 1;
                date = list.Last().GameDate.AddDays(7);
            }
            var schedule = new Schedule()
            {
                WeekNumber = WeekNumber,
                GameDate = date,
                Leagueid = leagueid,
                Cancelled = false
            };
            ViewBag.Schedule = db.Schedules.Where(x => x.Leagueid == leagueid).OrderBy(x => x.GameDate);
            return View(schedule);
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
                int WeekNumber = 1;
                try
                {
                    if (ModelState.IsValid)
                    {
                        var list = db.Schedules.Where(x => x.Leagueid == schedule.Leagueid).ToList();
                        if(list.Any(x=>x.GameDate == schedule.GameDate))
                        {
                            ModelState.AddModelError(string.Empty,"Insert failed, Game date already in list");
                        }
                        list.Add(schedule);
                        list.Sort((a, b) => a.GameDate.CompareTo(b.GameDate));
                        foreach (var item in list)
                        {
                            item.WeekNumber = WeekNumber++;
                            if (item.GameDate == schedule.GameDate)
                                db.Schedules.Add(item);
                            else
                            {
                                db.Entry(item).State = EntityState.Modified;
                            }
                        }
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
            return View(schedule);
        }

        // POST: Schedules/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "id,GameDate,WeekNumber,Leagueid,Cancelled,rowversion")] Schedule schedule)
        {
            try
            {

                if (ModelState.IsValid)
                {
                    db.Entry(schedule).State = EntityState.Modified;
                    db.SaveChanges();
                    return RedirectToAction("Index");
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
                    if (databaseValues.WeekNumber != clientValues.WeekNumber)
                        ModelState.AddModelError("Week Number", "Current value: "
                                                                + databaseValues.WeekNumber);
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
           if(db.Matches.Any(x=>x.WeekId == schedule.id))
            {
                ViewBag.Error = "Unable to delete this record, there are matches scheduled to play on this date";
                return View(schedule);
            }
            try
            {
                var list = db.Schedules.Where(x => x.Leagueid == schedule.Leagueid && x.GameDate != schedule.GameDate)
                    .OrderBy(x => x.GameDate);
                db.Entry(schedule).Property("rowversion").OriginalValue = rowversion;
                db.Entry(schedule).State = EntityState.Deleted;
                int WeekNo = 1;
                foreach (var item in list)
                {
                    item.WeekNumber = WeekNo++;
                    db.Entry(item).State = EntityState.Modified;
                }
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
