using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Core;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Elmah;
using Tournament.Models;

namespace Tournament.Controllers
{

    public class MembershipController : Controller
    {
        private readonly TournamentEntities _db = new TournamentEntities();


        // GET: Memberships
        [Authorize]
        public ActionResult Index(string sortOrder)
        {
            
            ViewData["FullNameSortParm"] = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewData["FirstNameSortParm"] = sortOrder == "firstname" ? "firstname_desc" : "firstname";
            var list = _db.MembershipAllowDelete().ToList();
            switch (sortOrder)
            {
                case "name_desc":
                    list.Sort((a, b) => String.Compare(b.LastName + " " + b.FirstName, a.LastName + " " + a.FirstName, StringComparison.CurrentCulture));
                    break;
                
                case "firstname_desc":
                    list.Sort((a, b) => String.Compare(b.FirstName, a.FirstName, StringComparison.CurrentCulture));
                    break;
                case "firstname":
                    list.Sort((a, b) => String.Compare(a.FirstName, b.FirstName, StringComparison.CurrentCulture));
                    break;
                default:
                    list.Sort((a, b) => String.Compare(a.LastName + " " + a.FirstName, b.LastName + " " + b.FirstName, StringComparison.CurrentCulture));
                    break;

            }
            ViewBag.Count = list.Count;
            

            return View(list);
        }


        // GET: Memberships/Create
        [Authorize(Roles = "Admin,LeagueAdmin")]
        public ActionResult Create()
        {
            
            return View();
        }

        // POST: Memberships/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "id,FirstName,LastName,shortname,NickName,Wheelchair")] Membership membership)
        {
            if (ModelState.IsValid)
            {
                _db.Memberships.Add(membership);
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
            return View(membership);
        }

        // GET: Memberships/Edit/5
        [Authorize(Roles = "Admin,LeagueAdmin")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Membership membership = _db.Memberships.Find(id);
            if (membership == null)
            {
                return HttpNotFound();
            }
            return View(membership);
        }

        // POST: Memberships/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "id,FirstName,LastName,shortname,NickName,Wheelchair,rowversion")] Membership membership)
        {

            try
            {

                if (ModelState.IsValid)
                {
                    _db.Entry(membership).State = EntityState.Modified;
                    _db.SaveChanges();
                    return RedirectToAction("Index");
                }
            }
            catch (DbUpdateConcurrencyException ex)
            {
                var entry = ex.Entries.Single();
                var clientValues = (Membership) entry.Entity;
                var databaseEntry = entry.GetDatabaseValues();
                if (databaseEntry == null)
                {
                    ModelState.AddModelError(string.Empty,
                        "Unable to save changes. The member was deleted by another user.");
                }
                else
                {
                    var databaseValues = (Membership) databaseEntry.ToObject();

                    if (databaseValues.FirstName != clientValues.FirstName)
                        ModelState.AddModelError("First Name", "Current value: "
                                                               + databaseValues.FirstName);
                    if (databaseValues.LastName != clientValues.LastName)
                        ModelState.AddModelError("Last Name", "Current value: "
                                                              + databaseValues.LastName);
                    if (databaseValues.shortname != clientValues.shortname)
                        ModelState.AddModelError("Short Name", "Current value: "
                                                               + databaseValues.shortname);
                    if (databaseValues.Wheelchair != clientValues.Wheelchair)
                        ModelState.AddModelError("Wheelchair", "Current value: "
                                                               + databaseValues.Wheelchair);

                    ModelState.AddModelError(string.Empty, "The record you attempted to edit "
                                                           + "was modified by another user after you got the original value. The "
                                                           + "edit operation was canceled and the current values in the database "
                                                           + "have been displayed. If you still want to edit this record, click "
                                                           + "the Save button again. Otherwise click the Back to List hyperlink.");
                    membership.rowversion = databaseValues.rowversion;
                }
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError("",
                    "Unable to save changes. Cannot have duplicate names.");
            }
            catch (Exception dex)
            {
                //Log the error (uncomment dex variable name and add a line here to write a log.)
                ModelState.AddModelError("",
                    "Unable to save changes. Try again, and if the problem persists, see your system administrator.");
                ErrorSignal.FromCurrentContext().Raise(dex);
            }
            return View(membership);
        }

       // GET: Memberships/Delete/5
        [Authorize(Roles = "Admin,LeagueAdmin")]
        public ActionResult Delete(int? id, bool? concurrencyError)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var membership = _db.Memberships.Find(id);
            if (membership == null)
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

            return View(membership);
        }

        // POST: Memberships/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id, byte[] rowversion)
        {
            
            var membership = _db.Memberships.Find(id);
            if (membership == null)
            {
                ViewBag.Message = "Record was deleted by another user";
            }
            
            else
            {
                try
                {
                    _db.Entry(membership).Property("rowversion").OriginalValue = rowversion;
                    _db.Entry(membership).State = EntityState.Deleted;
                    _db.SaveChanges();
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
            return View(membership);
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
