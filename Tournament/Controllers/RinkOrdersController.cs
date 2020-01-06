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
    public class RinkOrdersController : Controller
    {
        private TournamentEntities db = new TournamentEntities();

        // GET: RinkOrders
        public ActionResult Index()
        {
            return View(db.RinkOrders.ToList());
        }

      // GET: RinkOrders/Create
        public ActionResult Create()
        {
            var list = db.RinkOrders.OrderBy(x => x.id).ToList();
            int id = 1;
            if (list.Any())
            {
                id = list.Last().id + 1;
            }
            var item = new RinkOrder()
            {
                id = id
            };
            return View(item);
        }

        // POST: RinkOrders/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "id,Green,Direction,Boundary,rowversion")] RinkOrder rinkOrder)
        {
            if (ModelState.IsValid)
            {
                db.RinkOrders.Add(rinkOrder);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(rinkOrder);
        }

        // GET: RinkOrders/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            RinkOrder rinkOrder = db.RinkOrders.Find(id);
            if (rinkOrder == null)
            {
                return HttpNotFound();
            }
            return View(rinkOrder);
        }

        // POST: RinkOrders/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "id,Green,Direction,Boundary,rowversion")] RinkOrder rinkOrder)
        {
            try
            {

                if (ModelState.IsValid)
                {
                    db.Entry(rinkOrder).State = EntityState.Modified;
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
            }
            catch (DbUpdateConcurrencyException ex)
            {
                var entry = ex.Entries.Single();
                var clientValues = (RinkOrder)entry.Entity;
                var databaseEntry = entry.GetDatabaseValues();
                if (databaseEntry == null)
                {
                    ModelState.AddModelError(string.Empty,
                        "Unable to save changes. The rink was deleted by another user.");
                }
                else
                {
                    var databaseValues = (RinkOrder)databaseEntry.ToObject();

                    if (databaseValues.id != clientValues.id)
                        ModelState.AddModelError("id", "Current value: "
                                                                + databaseValues.id);
                    if (databaseValues.Direction != clientValues.Direction)
                        ModelState.AddModelError("Direction", "Current value: "
                                                              + databaseValues.Direction);
                    if (databaseValues.Green != clientValues.Green)
                        ModelState.AddModelError("Green", "Current value: "
                                                           + databaseValues.Green);
                    if (databaseValues.Boundary != clientValues.Boundary)
                        ModelState.AddModelError("Boundary", "Current value: "
                                                          + databaseValues.Boundary);
                    ModelState.AddModelError(string.Empty, "The record you attempted to edit "
                                                           + "was modified by another user after you got the original value. The "
                                                           + "edit operation was canceled and the current values in the database "
                                                           + "have been displayed. If you still want to edit this record, click "
                                                           + "the Save button again. Otherwise click the Back to List hyperlink.");
                    rinkOrder.rowversion = databaseValues.rowversion;
                }
            }
            catch (Exception dex)
            {
                //Log the error (uncomment dex variable name and add a line here to write a log.)
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists, see your system administrator.");
                ErrorSignal.FromCurrentContext().Raise(dex);
            }
            return View(rinkOrder);
        }

        // GET: RinkOrders/Delete/5
        public ActionResult Delete(int? id, bool? concurrencyError)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            RinkOrder rinkOrder = db.RinkOrders.Find(id);
            if (rinkOrder == null)
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

            return View(rinkOrder);
        }

        // POST: RinkOrders/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id, byte[] rowversion)
        {

            var rinkOrder = db.RinkOrders.Find(id);
            if (rinkOrder == null)
            {
                ViewBag.Message = "Record was deleted by another user";
            }
            
            else
            {
                try
                {
                    var list = db.RinkOrders.OrderBy(x => x.id).ToList();
                    int id1 = 1;
                    foreach (var item in list)
                    {
                        if (item.id != rinkOrder.id)
                        {
                            item.id = id1++;
                            db.Entry(item).State = EntityState.Modified;
                        }
                    }
                    db.Entry(rinkOrder).Property("rowversion").OriginalValue = rowversion;
                    db.Entry(rinkOrder).State = EntityState.Deleted;
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
            return View(rinkOrder);
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
