using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using Elmah;
using Tournament.Models;

namespace Tournament.Controllers
{
    [Authorize(Roles="Admin")]
    public class UserLeaguesController : Controller
    {
        private TournamentEntities db = new TournamentEntities();

        // GET: UserLeagues
        public ActionResult Index(int id)
        {
            var userLeagues = db.UserLeagues.Include(u => u.League).Include(u => u.User).Where(x=>x.LeagueId==id);
            ViewBag.LeagueName = db.Leagues.Find(id).LeagueName;
            var model = new UserLeagueViewModel()
            {
                LeagueName = db.Leagues.Find(id).LeagueName,
                leagueid = id,
                userLeagues = userLeagues
            };
            return View(model);
        }

        
        // GET: UserLeagues/Create
        public ActionResult Create(int id)
        {
            var league = db.Leagues.Find(id);
            ViewBag.LeagueName = league.LeagueName;
            var list = db.Users.Where(x => x.Roles != "Mailer" && x.Roles != "Admin").ToList();
            foreach (var userLeague in db.UserLeagues.Where(x=>x.LeagueId==id))
            {
                if(list.Any(x=>x.id == userLeague.UserId))
                    list.RemoveAll(x => x.id == userLeague.UserId);
            }
            ViewBag.UserId = new SelectList(list, "id", "username");
            var userleague = new UserLeague()
            {
                LeagueId = id
            };
            return View(userleague);
        }

        // POST: UserLeagues/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "id,UserId,LeagueId,Roles,rowversion")] UserLeague userLeague)
        {
            if (ModelState.IsValid)
            {
                db.UserLeagues.Add(userLeague);
                try
                {
                    db.SaveChanges();
                    return RedirectToAction("Index", new {id=userLeague.LeagueId});
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


            var list = db.Users.Where(x => x.Roles != "Mailer" || x.Roles != "Admin").ToList();
            foreach (var item in db.UserLeagues.Where(x => x.LeagueId == userLeague.LeagueId))
            {
                if (list.Any(x => x.id == item.UserId))
                    list.RemoveAll(x => x.id == item.UserId);
            }
            ViewBag.UserId = new SelectList(list, "id", "username");
            var league = db.Leagues.Find(userLeague.LeagueId);
            ViewBag.LeagueName = league.LeagueName;
            return View(userLeague);
        }

        // GET: UserLeagues/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            UserLeague userLeague = db.UserLeagues.Find(id);
            if (userLeague == null)
            {
                return HttpNotFound();
            }
            ViewBag.LeagueName = userLeague.League.LeagueName;
            return View(userLeague);
        }

        // POST: UserLeagues/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "id,UserId,LeagueId,Roles,rowversion")] UserLeague userLeague)
        {
            try
            {

                if (ModelState.IsValid)
                {
                    db.Entry(userLeague).State = EntityState.Modified;
                    db.SaveChanges();
                    return RedirectToAction("Index", new {id=userLeague.LeagueId});
                }
            }
            catch (DbUpdateConcurrencyException ex)
            {
                var entry = ex.Entries.Single();
                var clientValues = (UserLeague)entry.Entity;
                var databaseEntry = entry.GetDatabaseValues();
                if (databaseEntry == null)
                {
                    ModelState.AddModelError(string.Empty,
                        "Unable to save changes. The member was deleted by another user.");
                }
                else
                {
                    var databaseValues = (UserLeague)databaseEntry.ToObject();

                    if (databaseValues.UserId != clientValues.UserId)
                        ModelState.AddModelError("User", "Current value: "
                                                         + databaseValues.User.username);
                    if (databaseValues.LeagueId != clientValues.LeagueId)
                        ModelState.AddModelError("League", "Current value: "
                                                              + databaseValues.League.LeagueName);
                    if (databaseValues.Roles != clientValues.Roles)
                        ModelState.AddModelError("Roles", "Current value: "
                                                              + databaseValues.Roles);


                    ModelState.AddModelError(string.Empty, "The record you attempted to edit "
                                                           + "was modified by another user after you got the original value. The "
                                                           + "edit operation was canceled and the current values in the database "
                                                           + "have been displayed. If you still want to edit this record, click "
                                                           + "the Save button again. Otherwise click the Back to List hyperlink.");
                    userLeague.rowversion = databaseValues.rowversion;
                }
            }
            catch (Exception dex)
            {
                //Log the error (uncomment dex variable name and add a line here to write a log.)
                ModelState.AddModelError("",
                    "Unable to save changes. Try again, and if the problem persists, see your system administrator.");
                ErrorSignal.FromCurrentContext().Raise(dex);
            }
            return View(userLeague);
        }

        // GET: UserLeagues/Delete/5
        public ActionResult Delete(int? id, bool? concurrencyError)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var userLeague = db.UserLeagues.Find(id);
            if (userLeague == null)
            {
                if (concurrencyError.GetValueOrDefault())
                {
                    return RedirectToAction("Index","Leagues");
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
            return View(userLeague);
        }

        // POST: UserLeagues/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id, byte[] rowversion)
        {
            var userLeague = db.UserLeagues.Find(id);
            if (userLeague == null)
            {
                ViewBag.Error = "Record was delete by another user";
            }
            else
            {
                try
                {
                    db.UserLeagues.RemoveRange(db.UserLeagues.Where(x => x.LeagueId == userLeague.LeagueId));
                    db.Entry(userLeague).Property("rowversion").OriginalValue = rowversion;
                    db.Entry(userLeague).State = EntityState.Deleted;
                    db.SaveChanges();
                    return RedirectToAction("Index","Home");
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    return RedirectToAction("Delete", new {concurrencyError = true, id = id});
                }
                catch (Exception dex)
                {
                    //Log the error (uncomment dex variable name after DataException and add a line here to write a log.
                    ViewBag.Error =
                        "Unable to delete. Try again, and if the problem persists contact your system administrator.";
                    ErrorSignal.FromCurrentContext().Raise(dex);

                }
            }
            return View(userLeague);
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
