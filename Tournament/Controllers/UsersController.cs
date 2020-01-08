using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using Elmah;
using Tournament.Code;
using Tournament.Models;

namespace Tournament.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UsersController : Controller
    {
        private readonly TournamentEntities _db = new TournamentEntities();

        // GET: Users
        public ActionResult Index()
        {
            return View(_db.Users.Where(x => x.Roles != "Mailer").ToList());
        }

       
        // GET: Users/Create
        public ActionResult Create()
        {
            var dict = new List<Role>();
            dict.Add(new Role("", "No Roles"));
            dict.Add(new Role("Admin", "Admin"));
            ViewBag.Roles = new SelectList(dict, "RoleValue", "RoleText", "");
            return View();
        }

        // POST: Users/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "id,username,password,Roles")] User user)
        {
            if (ModelState.IsValid)
            {
                user.username = user.username.ToLower().Trim();
                _db.Users.Add(user);
                _db.SaveChanges();
                return RedirectToAction("Index");
            }
            var dict = new List<Role>();
            dict.Add(new Role("", "No Roles"));
            dict.Add(new Role("Admin", "Admin"));
            ViewBag.Roles = new SelectList(dict, "RoleValue", "RoleText", user.Roles);
            return View(user);
        }

        // GET: Users/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            User user = _db.Users.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            var sharedSecret = DateTime.Now.TimeOfDay.ToString();
            TempData["Secret"] = sharedSecret;
            user.password = Crypto.EncryptStringAES(user.password, sharedSecret);
            var dict = new List<Role>();
            dict.Add(new Role("", "No Roles"));
            dict.Add(new Role("Admin", "Admin"));
            ViewBag.Roles = new SelectList(dict, "RoleValue", "RoleText", user.Roles);
            return View(user);
        }

        // POST: Users/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "id,username,Roles,password,rowversion")] User user)
        {
            if (ModelState.IsValid)
            {
                var sharedSecret = (string) TempData["Secret"];
                user.password = Crypto.DecryptStringAES(user.password, sharedSecret);
                user.username = user.username.ToLower().Trim();
                _db.Entry(user).State = EntityState.Modified;
                _db.SaveChanges();
                return RedirectToAction("Index");
            }
            var dict = new List<Role>();
            dict.Add(new Role("", "No Roles"));
            dict.Add(new Role("Admin", "Admin"));
            ViewBag.Roles = new SelectList(dict, "RoleValue", "RoleText", user.Roles);
            return View(user);
        }

        // GET: Users/Delete/5
        [Authorize(Roles = "Admin")]
        public ActionResult Delete(int? id, bool? concurrencyError)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var user = _db.Users.Find(id);
            if (user == null)
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

            return View(user);
        }

        // POST: Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id, byte[] rowversion)
        {

            var user = _db.Users.Find(id);
            if (user == null)
            {
                ViewBag.Message = "Record was deleted by another user";
            }

            else
            {
                try
                {
                    _db.Entry(user).Property("rowversion").OriginalValue = rowversion;
                    _db.Entry(user).State = EntityState.Deleted;
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
            return View(user);
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
