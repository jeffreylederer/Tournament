using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Tournament.Models;

namespace Tournament.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UsersController : Controller
    {
        private TournamentEntities db = new TournamentEntities();

        // GET: Users
        public ActionResult Index()
        {
            return View(db.Users.ToList());
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
        public ActionResult Edit([Bind(Include = "id,username,password,Roles")] User user)
        {
            if (ModelState.IsValid)
            {
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
