﻿using System;
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
    
    public class PlayersController : Controller
    {
        private TournamentEntities db = new TournamentEntities();

        [Authorize]
        // GET: Players
        public ActionResult Index(string sortOrder)
        {
            var leagueid = (int) HttpContext.Session["leagueid"];

            ViewData["FullNameSortParm"] = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewData["FirstNameSortParm"] = sortOrder == "firstname" ? "firstname_desc" : "firstname";
            var list = from s in db.Players where s.Leagueid == leagueid
                       select s;
            switch (sortOrder)
            {
                case "name_desc":
                    list = list.OrderByDescending(s => s.Membership.LastName + " " + s.Membership.FirstName);
                    break;
                default:
                    list = list.OrderBy(s => s.Membership.LastName + " " + s.Membership.FirstName);
                    break;
            }
            ViewBag.Count = list.Count();

            return View(list);
        }

        [Authorize(Roles = "Admin,LeagueAdmin")]
        // GET: Players/Create
        public ActionResult Create()
        {
            var leagueid = (int)HttpContext.Session["leagueid"];
            var item = new Player()
            {
                Leagueid = leagueid
            };

            ViewBag.List = GetRemainingMembers.OrderBy(x => x.LastName);
            return View(item);
        }


        // POST: Players/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "id,LeagueId,MembershipId")] Player player)
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

            ViewBag.List = GetRemainingMembers.OrderBy(x => x.LastName);
            return View(player);
        }

        [Authorize(Roles = "Admin,LeagueAdmin")]
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

            ViewBag.List = GetRemainingMembers.OrderBy(x => x.LastName);
            return View(player);
        }

        [Authorize(Roles = "Admin,LeagueAdmin")]
        // POST: Players/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "id,LeagueId,MembershipId,rowversion")] Player player)
        {
            try
            {

                if (ModelState.IsValid)
                {
                    db.Entry(player).State = EntityState.Modified;
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
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

                    if (databaseValues.Leagueid != clientValues.Leagueid)
                        ModelState.AddModelError("League", "Current value: "
                                                               + databaseValues.Leagueid);
                    if (databaseValues.MembershipId != clientValues.MembershipId)
                        ModelState.AddModelError("Member", "Current value: "
                                                              + databaseValues.MembershipId);
                    

                    ModelState.AddModelError(string.Empty, "The record you attempted to edit "
                                                           + "was modified by another user after you got the original value. The "
                                                           + "edit operation was canceled and the current values in the database "
                                                           + "have been displayed. If you still want to edit this record, click "
                                                           + "the Save button again. Otherwise click the Back to List hyperlink.");
                    player.rowversion = databaseValues.rowversion;
                }
            }
            catch (Exception dex)
            {
                //Log the error (uncomment dex variable name and add a line here to write a log.)
                ModelState.AddModelError("",
                    "Unable to save changes. Try again, and if the problem persists, see your system administrator.");
                ErrorSignal.FromCurrentContext().Raise(dex);
            }

            return View(player);
        }

        [Authorize(Roles = "Admin,LeagueAdmin")]
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
                ViewBag.Error = "The record you attempted to delete "
                                                  + "was modified by another user after you got the original values. "
                                                  + "The delete operation was canceled and the current values in the "
                                                  + "database have been displayed. If you still want to delete this "
                                                  + "record, click the Delete button again. Otherwise "
                                                  + "click the Back to List hyperlink.";
            }

            return View(player);
        }

        // POST: Players/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id, byte[] rowversion)
        {

            var player = db.Players.Find(id);
            if (player == null)
            {
                ViewBag.Message = "Record was delete by another user";
                return View(player);
            }
            if(db.Teams.Any(x=>x.Skip == player.id) || db.Teams.Any(x => x.ViceSkip == player.id) || db.Teams.Any(x => x.Lead == player.id))
            {
                ViewBag.Error = "Record was not deleted, the player is on a team.";
                return View(player);
            }
            try
            {
                db.Entry(player).Property("rowversion").OriginalValue = rowversion;
                db.Entry(player).State = EntityState.Deleted;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            catch (DbUpdateConcurrencyException ex)
            {
                return RedirectToAction("Delete", new { concurrencyError = true, id = id });
            }
            catch (DataException dex)
            {
                //Log the error (uncomment dex variable name after DataException and add a line here to write a log.
                ViewBag.Error = "Unable to delete record. Try again, and if the problem persists contact your system administrator.";
                ErrorSignal.FromCurrentContext().Raise(dex);
    
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

        private List<Membership> GetRemainingMembers
        {
            get
            {
                var players = db.Players;
                var list = new List<Membership>();
                int leagueid = (int)HttpContext.Session["leagueid"];
                foreach (var member in db.Memberships)
                {
                    if (!players.Any(x => x.MembershipId == member.id && x.Leagueid==leagueid))
                        list.Add(member);
                }
                return list;
            }
        }
    }
}
