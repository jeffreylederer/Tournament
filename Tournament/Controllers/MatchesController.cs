using Elmah;
using System;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using Tournament.Code;
using Tournament.Models;

namespace Tournament.Controllers
{
    [Authorize]
    public class MatchesController : Controller
    {
        private TournamentEntities db = new TournamentEntities();

        // GET: Matches
        public ActionResult Index(int? RoundId)
        {
            var Matches = db.Matches.Where(x => x.RoundId == RoundId.Value && x.Rink != -1); ;
            ViewBag.ScheduleID = new SelectList(db.Schedules.ToList(), "id", "RoundNo", RoundId ?? 0);
            ViewBag.RoundId = db.Schedules.Find(RoundId.Value).RoundNo;
            ViewBag.WeekID = RoundId;
            return View(Matches.OrderBy(x => x.Rink).ToList());
        }

        public ActionResult MoveUp(int id, int weekid)
        {
            var Matches = db.Matches.Where(x => x.RoundId == weekid).OrderBy(x => x.Rink);
            var match = Matches.First(x => x.Rink == id);
            var match1 = Matches.First(x => x.Rink == id + 1);
            match1.Rink = id;
            match.Rink = id + 1;
            db.Entry(match).State = EntityState.Modified;
            try
            {
                db.SaveChanges();
            }
            catch (Exception e)
            {
                ErrorSignal.FromCurrentContext().Raise(e);
                throw;
            }
            
            return RedirectToAction("Index", new { ScheduleID = weekid });
        }

        [Authorize(Roles = "Admin")]
        public ActionResult CreateMatches()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateMatches(string DeleteIT)
        {
            var numOfWeeks = db.Schedules.Count();
            var numofTeams = db.Teams.Count();
            db.Matches.RemoveRange(db.Matches);
            db.SaveChanges();
            var cs = new CreateSchedule();

            var matches = numofTeams % 2 == 0 ? cs.NoByes(numOfWeeks, numofTeams) : cs.Byes(numOfWeeks, numofTeams);

            foreach (var match in matches)
            {
                db.Matches.Add(new Match()
                {
                    id=0,
                    RoundId = match.Week + 1,
                    Rink = match.Rink == -1 ? -1 : match.Rink + 1,
                    TeamNo1 = match.Team1 + 1,
                    TeamNo2 = match.Team2 + 1,
                    Team1Score = 0,
                    Team2Score = 0
                });
               
            }
            try
            {
                db.SaveChanges();
            }
            catch (Exception e)
            {
                ErrorSignal.FromCurrentContext().Raise(e);
            }
           
            return RedirectToAction("index", new { RoundId = 1 });
        }

        [Authorize(Roles = "Admin")]
        public ActionResult ClearSchedule()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ClearSchedule(string DeleteIT)
        {
            db.Schedules.RemoveRange(db.Schedules);
            db.Teams.RemoveRange(db.Teams);
            db.Matches.RemoveRange(db.Matches);
            try
            {
                db.SaveChanges();
            }
            catch (Exception e)
            {
                ErrorSignal.FromCurrentContext().Raise(e);
            }
            
            return RedirectToAction("index", "Home");
        }

        public ActionResult Scoring(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Match Match = db.Matches.Find(id);
            if (Match == null)
            {
                return HttpNotFound();
            }
            return View(Match);
        }

        // POST: Matches/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Scoring([Bind(Include = "id,GameDate,Rink,Team1,Team2,Team1Score,Team2Score")] Match Match)
        {
            if (ModelState.IsValid)
            {
                db.Entry(Match).State = EntityState.Modified;
                try
                {
                    db.SaveChanges();
                    return RedirectToAction("Index", new { ScheduleID = Match.RoundId });
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
                    ModelState.AddModelError(string.Empty, "Edit failed");
                }
            }
            return View(Match);
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
