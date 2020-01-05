using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using Microsoft.Ajax.Utilities;
using Tournament.Models;

namespace Tournament.Controllers
{
    public class HomeController : Controller
    {
        private TournamentEntities db = new TournamentEntities();

        [Authorize]
        public ActionResult Index()
        {
            var username = User.Identity.Name;
            if (HttpContext.Session["teamsize"] != null)
                HttpContext.Session["teamsize"] = null;

            if (HttpContext.Session["leaguename"] != null)
                HttpContext.Session["leaguename"] = null;

            RemoveCookie("leagueid");

            var user = db.Users.Where(x => x.username == username).First();
            ViewBag.Error = "";
            var leagues = new List<League>();
            if (!string.IsNullOrWhiteSpace(user.Roles) &&  user.Roles.Contains("Admin"))
            {
                leagues = db.Leagues.Where(x=>x.Active).ToList();
            }
            else
            {
               user.UserLeagues.ForEach((a)=>leagues.Add(a.League));
            }
            if (!leagues.Any())
            {
                ViewBag.Error = "You are not assigned to any league";
                leagues = new List<League>();
                return View(leagues);
            }
            if (leagues.Count() == 1)
            {
                return RedirectToAction("Register", new {id = leagues.First().id});

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
            var username = User.Identity.Name;
            if (!id.HasValue)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var user = db.Users.Where(x => x.username == username).First();
            if(user == null)
            {
                return HttpNotFound();
            }
            var role = "";
            if (!string.IsNullOrWhiteSpace(user.Roles))
            {
                role = user.Roles;
            }
            var items = db.UserLeagues.Where(x => x.UserId == user.id && x.LeagueId == id);
            if (items.Any())
            {
                var item = items.First();
                if (!string.IsNullOrWhiteSpace(item.Roles))
                {
                    role = string.IsNullOrWhiteSpace(role) ? item.Roles : $"{role},{item.Roles}";
                }
            }



            var league = db.Leagues.Find(id);
            if (league == null)
            {
                return HttpNotFound();
            }
            HttpContext.Session["teamsize"] = league.TeamSize;
            HttpContext.Session["leaguename"] = league.LeagueName;
            SetCookie("leagueid", id.Value);
  

            FormsAuthentication.SetAuthCookie(user.username, false);
            FormsAuthenticationTicket ticket = new FormsAuthenticationTicket(
                1, // Ticket version 
                user.username, // username to be used by ticket 
                DateTime.Now, // ticket issue date-time
                DateTime.Now.AddMinutes(60), // Date and time the cookie will expire 
                false, // persistent cookie?
                role,
                FormsAuthentication.FormsCookiePath);

            string encryptCookie = FormsAuthentication.Encrypt(ticket);
            HttpCookie cookie = new HttpCookie(FormsAuthentication.FormsCookieName, encryptCookie);
            HttpContext.Response.Cookies.Add(cookie);


            return RedirectToAction("Welcome");
        }

        [Authorize]
        public ActionResult Welcome()
        {
            if(User.IsInRole("Admin"))
                ViewBag.Info = "You are a site administrator";
            else if(User.IsInRole("LeagueAdmin"))
                ViewBag.Info = "You are a league administrator";
            else if(User.IsInRole("Scorer"))
                ViewBag.Info = "You are a league scorer";
            else
                ViewBag.Info = "You are a league observer";
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

        private void RemoveCookie(string name)
        {
            var cookie = Request.Cookies[name];
            if (cookie != null)
            {
                cookie.Expires = DateTime.Now.AddDays(-1);
                Response.Cookies.Add(cookie);
            }
        }

        private void SetCookie(string name, int value)
        {
            var cookie = new HttpCookie(name, value.ToString());
            Response.Cookies.Add(cookie);
        }
    }
}