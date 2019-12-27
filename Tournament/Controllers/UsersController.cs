using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Tournament.Models;
using Tournament.Code;

namespace Tournament.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UsersController : Controller
    {
        private TournamentEntities db = new TournamentEntities();

        // GET: Users
        public ActionResult Index()
        {
            return View(db.Users.Where(x => x.Roles != "Mailer").ToList());
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
                db.Users.Add(user);
                db.SaveChanges();
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
            User user = db.Users.Find(id);
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
                db.Entry(user).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            var dict = new List<Role>();
            dict.Add(new Role("", "No Roles"));
            dict.Add(new Role("Admin", "Admin"));
            ViewBag.Roles = new SelectList(dict, "RoleValue", "RoleText", user.Roles);
            return View(user);
        }

        // GET: Users/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            User user = db.Users.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }

        // POST: Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            User user = db.Users.Find(id);
            db.UserLeagues.RemoveRange(db.UserLeagues.Where(x => x.UserId == id));
            db.Users.Remove(user);
            db.SaveChanges();
            return RedirectToAction("Index");
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
