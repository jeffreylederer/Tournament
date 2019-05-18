using Elmah;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using Tournament.Models;

namespace Tournament.Controllers
{

    public class TeamsController : Controller
    {
        private TournamentEntities db = new TournamentEntities();
        private int _teamsize;

        public TeamsController()
        {
            var fact = db.Facts.Find(1);
            _teamsize = fact.SizeTeam;
        }

        // GET: Teams
        public ActionResult Index()
        {
            var Teams = db.Teams.Include(t => t.Player).Include(t => t.Player1);
            ViewBag.TeamSize = _teamsize;
            return View(Teams.OrderBy(x => x.id).ToList());
        }

        public ActionResult RemoveLead(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Team Team = db.Teams.Find(id);
            if (Team == null)
            {
                return HttpNotFound();
            }
            Team.Lead = null;
            db.Entry(Team).State = EntityState.Modified;
            try
            {
                db.SaveChanges();
            }
            catch (Exception e)
            {
                ErrorSignal.FromCurrentContext().Raise(e); ;
            }
            return RedirectToAction("Index");
        }

        public ActionResult RemoveViceSkip(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Team Team = db.Teams.Find(id);
            if (Team == null)
            {
                return HttpNotFound();
            }
            Team.ViceSkip = null;
            db.Entry(Team).State = EntityState.Modified;
            try
            {
                db.SaveChanges();
            }
            catch (Exception e)
            {
                ErrorSignal.FromCurrentContext().Raise(e); ;
            }
            return RedirectToAction("Index");
        }



        // GET: Teams/Create
        public ActionResult Create()
        {
            var items = db.Teams.ToList();
            int id = 1;
            if (items.Count > 1)
            {
                items.Sort((a, b) => a.id.CompareTo(b.id));
                id = items[items.Count - 1].id + 1;
            }

            var item = new Team()
            {
                id = id
            };
            var teams = db.Teams.OrderBy(x => x.id);
            var list = new List<Player>();
            foreach (var player in db.Players.Where(x => x.Active))
            {
                if (!teams.Any(x => x.Skip == player.id || x.Lead == player.id || x.ViceSkip == player.id))
                    list.Add(player);
            }
            ViewBag.Skip = new SelectList(list.OrderBy(x => x.LastName), "id", "FullName", " ");
            ViewBag.ViceSkip = new SelectList(list.OrderBy(x => x.LastName), "id", "FullName", " ");
            ViewBag.Lead = new SelectList(list.OrderBy(x => x.LastName), "id", "FullName", " ");
            ViewBag.Teams = teams;
            ViewBag.TeamSize = _teamsize;
            return View(item);
        }

        // POST: Teams/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "id,Skip,Lead,ViceSkip")] Team Team)
        {
            if (ModelState.IsValid)
            {
                db.Teams.Add(Team);
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

            var teams = db.Teams.OrderBy(x => x.id);
            var list = new List<Player>();
            foreach (var player in db.Players.Where(x => x.Active))
            {
                if (!teams.Any(x => x.Skip == player.id || x.Lead == player.id || x.ViceSkip == player.id))
                    list.Add(player);
            }
            ViewBag.Skip = new SelectList(list.OrderBy(x => x.LastName), "id", "FullName", Team.Skip);
            ViewBag.ViceSkip = new SelectList(list.OrderBy(x => x.LastName), "id", "FullName", Team.ViceSkip);
            ViewBag.Lead = new SelectList(list.OrderBy(x => x.LastName), "id", "FullName", Team.Lead);
            ViewBag.Teams = teams;
            ViewBag.TeamSize = _teamsize;
            return View(Team);
        }

        // GET: Teams/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Team Team = db.Teams.Find(id);
            if (Team == null)
            {
                return HttpNotFound();
            }
            var teams = db.Teams.OrderBy(x => x.id);
            var list = new List<Player>();
            foreach (var player in db.Players.Where(x => x.Active))
            {
                if(!teams.Any(x => x.Skip == player.id || x.Lead == player.id || x.ViceSkip == player.id))
                    list.Add(player);
            }
            if (Team.ViceSkip != null)
                list.Add(Team.Player1);
            if (Team.Lead != null)
                list.Add(Team.Player2);
            ViewBag.Lead = new SelectList(list.OrderBy(x => x.LastName), "id", "FullName", Team.Lead);
            ViewBag.ViceSkip = new SelectList(list.OrderBy(x => x.LastName), "id", "FullName", Team.ViceSkip);
            ViewBag.Teams = teams;
            ViewBag.TeamSize = _teamsize;
            return View(Team);
        }

        // POST: Teams/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "id,Skip,Lead,ViceSkip")] Team Team)
        {
            if (ModelState.IsValid)
            {
                db.Entry(Team).State = EntityState.Modified;
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
                    ModelState.AddModelError(string.Empty, e.Message);

                }
                catch (Exception e)
                {
                    ErrorSignal.FromCurrentContext().Raise(e);
                    ModelState.AddModelError(string.Empty, "Edit failed");
                }
            }
            var teams = db.Teams.OrderBy(x => x.id);

            var list = new List<Player>();
            foreach (var player in db.Players.Where(x => x.Active))
            {
                if (!teams.Any(x => x.Skip == player.id || x.Lead == player.id || x.ViceSkip == player.id))
                    list.Add(player);
            }
            if (Team.ViceSkip != null)
                list.Add(Team.Player1);
            if (Team.Lead != null)
                list.Add(Team.Player2);
            ViewBag.Lead = new SelectList(list.OrderBy(x => x.LastName), "id", "FullName", Team.Lead);
            ViewBag.ViceSkip = new SelectList(list.OrderBy(x => x.LastName), "id", "FullName", Team.ViceSkip);
            ViewBag.Teams = teams;
            ViewBag.TeamSize = _teamsize;
            return View(Team);
        }

        // GET: Teams/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Team Team = db.Teams.Find(id);
            if (Team == null)
            {
                return HttpNotFound();
            }
            ViewBag.TeamSize = _teamsize;
            return View(Team);
        }

        // POST: Teams/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Team Team = db.Teams.Find(id);
            db.Teams.Remove(Team);
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
                ViewBag.Error = ex.Message;
            }
            catch (Exception e)
            {
                ErrorSignal.FromCurrentContext().Raise(e);
                ViewBag.Error = "Delete failed";
            }
            return View(Team);
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
