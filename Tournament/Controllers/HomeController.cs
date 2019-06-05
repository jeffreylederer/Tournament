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

        [Authorize]
        public ActionResult Index()
        {
            var username = (string) HttpContext.Session["user"];
            var role = "";
            var user = db.Users.Where(x => x.username == username).First();
            ViewBag.Error = "";
            List<League> leagues;
            if (!string.IsNullOrWhiteSpace(user.Roles) &&  user.Roles.Contains("Admin"))
            {
                role = "LeagueAdmin";
                leagues = db.Leagues.ToList();
            }
            else
            {
                var userleagues = db.UserLeagues.Include(u => u.League).Where(x => x.UserId == user.id);
                if (userleagues.Any())
                    role = userleagues.First().Roles;
                leagues = new List<League>();
                foreach (var item in userleagues)
                    leagues.Add(item.League);
            }
            if (!leagues.Any())
            {
                ViewBag.Error = "You are not assigned to any league";
                leagues = new List<League>();
                return View(leagues);
            }
            if (leagues.Count() == 1)
            {
                var league = leagues.First();
                HttpContext.Session["leaguename"] = league.LeagueName;
                HttpContext.Session["leagueid"] = league.id;
                HttpContext.Session["leaguerole"] = role;
                HttpContext.Session["teamsize"] = league.TeamSize;
                return RedirectToAction("Welcome");
            }
            
           return View(leagues);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id">league id</param>
        /// <returns></returns>
        public ActionResult Register(int? id)
        {
            if (!id.HasValue)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var username = (string)HttpContext.Session["user"];
            var user = db.Users.Where(x => x.username == username).First();
            if(user == null)
            {
                return HttpNotFound();
            }
            var role = "";
            if (user.Roles.Contains("Admin"))
            {
                role = "LeagueAdmin";
            }
            else
            {
                var item = db.UserLeagues.Where(x => x.UserId == user.id && x.LeagueId == id).First();
                if (item == null)
                {
                    return HttpNotFound();
                }
                role = item.Roles;
            }
                
            
            var league = db.Leagues.Find(id);
            if (league == null)
            {
                return HttpNotFound();
            }
            HttpContext.Session["teamsize"] = league.TeamSize;
            HttpContext.Session["leaguename"] = league.LeagueName;
            HttpContext.Session["leagueid"] = id.Value;
            HttpContext.Session["leaguerole"] = role;
           
            
            return RedirectToAction("Welcome");
        }

        public ActionResult Welcome()
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