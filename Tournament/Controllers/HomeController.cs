using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Tournament.Models;

namespace Tournament.Controllers
{
    public class HomeController : Controller
    {
        private TournamentEntities db = new TournamentEntities();

        public ActionResult Index(int? id)
        {

            if (!id.HasValue)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var leagues = db.UserLeagues.Where(x => x.UserId == id.Value).Include("Leagues").ToList();
            if (!leagues.Any())
            {
                return HttpNotFound();
            }
            if (leagues.Count() == 1)
            {
                var league = db.Leagues.Find(leagues.First().LeagueId);
                if (league == null)
                {
                    return HttpNotFound();
                }
                HttpContext.Session["leaguename"] = league.LeagueName;
                HttpContext.Session["leagueid"] = leagues.First().LeagueId;
                HttpContext.Session["leaguerole"] = leagues.First().Roles;
                HttpContext.Session["teamsize"] = league.TeamSize;
                return RedirectToAction("Welcome");
            }
            var list = new List<LeagueViewModel>();
            foreach (var item in leagues)
            {
                var league = db.Leagues.Find(item.LeagueId);
                if (league == null)
                {
                    return HttpNotFound();
                }
                list.Add(new LeagueViewModel()
                {
                    id = item.id,
                    League =league.LeagueName,
                });
            }
            return View(list);
        }

        public ActionResult Register(int? id)
        {
            if (!id.HasValue)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            
            var item = db.UserLeagues.Find(id.Value);
            if (item == null)
            {
                return HttpNotFound();
            }
            var league = db.Leagues.Find(item.LeagueId);
            if (league == null)
            {
                return HttpNotFound();
            }
            HttpContext.Session["teamsize"] = league.TeamSize;
            HttpContext.Session["leaguename"] = league.LeagueName;
            HttpContext.Session["leagueid"] = item.LeagueId;
            HttpContext.Session["leaguerole"] = item.Roles;
           
            
            return RedirectToAction("Welcome");
        }

        public ActionResult Welcome(int id)
        {
            ViewBag.League = HttpContext.Session["leaguename"];
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}