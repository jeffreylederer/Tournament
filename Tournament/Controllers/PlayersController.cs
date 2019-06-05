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
    [Authorize]
    public class PlayersController : Controller
    {
        private TournamentEntities db = new TournamentEntities();

        // GET: Players
        public ActionResult Index(string sortOrder)
        {
            var leagueid = (int) HttpContext.Session["leagueid"];
            ViewBag.LeagueName = (string)HttpContext.Session["leaguename"];
            ViewData["FullNameSortParm"] = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewData["FirstNameSortParm"] = sortOrder == "firstname" ? "firstname_desc" : "firstname";
            var list = from s in db.Players where s.Leagueid == leagueid
                       select s;
            switch (sortOrder)
            {
                case "name_desc":
                    list = list.OrderByDescending(s => s.LastName + " " + s.FirstName);
                    break;
                case "firstname_desc":
                    list = list.OrderBy(s => s.FirstName);
                    break;
                case "firstname":
                    list = list.OrderByDescending(s => s.FirstName);
                    break;
                default:
                    list = list.OrderBy(s => s.LastName + " " + s.FirstName);
                    break;
            }
            ViewBag.Count = list.Count();
            ViewBag.Active = db.Players.Where(x=>x.Active && x.Leagueid == leagueid).Count();

            return View(list);
        }

        
        // GET: Players/Create
        public ActionResult Create()
        {
            var leagueid = (int)HttpContext.Session["leagueid"];
            ViewBag.LeagueName = (string)HttpContext.Session["leaguename"];
            var item = new Player()
            {
                Active = true,
                Leagueid = leagueid

            };
            return View(item);
        }

        // POST: Players/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "id,FirstName,LastName,Active,FullName,shortname,NickName, LeagueId")] Player player)
        {
            if (ModelState.IsValid)
            {
                db.Players.Add(player);
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
            return View(player);
        }

        // GET: Players/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Player player = db.Players.Find(id);
            if (player == null)
            {
                return HttpNotFound();
            }
            ViewBag.LeagueName = (string)HttpContext.Session["leaguename"];
            return View(player);
        }

        // POST: Players/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int? id, byte[] rowVersion)
        {
            string[] fieldsToBind = new string[] {"FirstName","LastName","Active","shortname","LeagueId", "rowversion" };
            if(id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var playerToUpdate = db.Players.Find(id);
            if (playerToUpdate == null)
            {
                var deletePlayer = new Player();
                TryUpdateModel(deletePlayer, fieldsToBind);
                ModelState.AddModelError(string.Empty,
                    "Unable to save changes. The player was deleted by another user.");
                return View(deletePlayer);
            }

            if (TryUpdateModel(playerToUpdate, fieldsToBind))
            {
                try
                {
                    db.Entry(playerToUpdate).OriginalValues["rowversion"] = rowVersion;
                    db.SaveChanges();

                    return RedirectToAction("Index");
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    var entry = ex.Entries.Single();
                    var clientValues = (Player)entry.Entity;
                    var databaseEntry = entry.GetDatabaseValues();
                    if (databaseEntry == null)
                    {
                        ModelState.AddModelError(string.Empty,
                            "Unable to save changes. The player was deleted by another user.");
                    }
                    else
                    {
                        var databaseValues = (Player)databaseEntry.ToObject();

                        if (databaseValues.FirstName != clientValues.FirstName)
                            ModelState.AddModelError("First Name", "Current value: "
                                                                    + databaseValues.FirstName);
                        if (databaseValues.LastName != clientValues.LastName)
                            ModelState.AddModelError("Last Name", "Current value: "
                                                                    + databaseValues.LastName);
                        if (databaseValues.shortname != clientValues.shortname)
                            ModelState.AddModelError("Short Name", "Current value: "
                                                                  + databaseValues.shortname);
                        if (databaseValues.Active != clientValues.Active)
                            ModelState.AddModelError("Active", "Current value: "
                                                                  + databaseValues.Active);
                        if (databaseValues.Leagueid != clientValues.Leagueid)
                            ModelState.AddModelError("League", "Current value: "
                                                               + databaseValues.Leagueid.ToString());
                        ModelState.AddModelError(string.Empty, "The record you attempted to edit "
                                                               + "was modified by another user after you got the original value. The "
                                                               + "edit operation was canceled and the current values in the database "
                                                               + "have been displayed. If you still want to edit this record, click "
                                                               + "the Save button again. Otherwise click the Back to List hyperlink.");
                        playerToUpdate.rowversion = databaseValues.rowversion;
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
            return View(playerToUpdate);
        }

        // GET: Players/Delete/5
        public ActionResult Delete(int? id, bool? concurrencyError)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var player = db.Players.Find(id);
            if (player == null)
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
            return View(player);
        }

        // POST: Players/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(Player player)
        {
            try
            {
                db.Entry(player).State = EntityState.Deleted;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            catch (DbUpdateConcurrencyException)
            {
                return RedirectToAction("Delete", new { concurrencyError = true, id = player.id });
            }
            catch (DataException dex)
            {
                //Log the error (uncomment dex variable name after DataException and add a line here to write a log.
                ModelState.AddModelError(string.Empty, "Unable to delete. Try again, and if the problem persists contact your system administrator.");
                ErrorSignal.FromCurrentContext().Raise(dex);
                ViewBag.LeagueName = (string)HttpContext.Session["leaguename"];
                return View(player);
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
