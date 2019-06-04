using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using Tournament.Models;

namespace Tournament.Controllers
{
    public class AccountsController : Controller
    {
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LogUserViewModel model, string returnUrl)
        {
            // Lets first check if the Model is valid or not
            if (ModelState.IsValid)
            {
                using (var entities = new TournamentEntities())
                {
                    string username = model.Username;
                    string password = model.Password;

                    // Now if our password was enctypted or hashed we would have done the
                    // same operation on the user entered password here, But for now
                    // since the password is in plain text lets just authenticate directly

                    var users = entities.Users.Where(x => x.username == model.Username);
                    // User found in the database
                    if (users.Count() == 1 && users.First().password == model.Password)
                    {
                        var user = users.First();
                        FormsAuthentication.SetAuthCookie(username, false);
                        string roles = user.Roles;

                        FormsAuthenticationTicket ticket = new FormsAuthenticationTicket(
                            1, // Ticket version 
                            user.username, // username to be used by ticket 
                            DateTime.Now, // ticket issue date-time
                            DateTime.Now.AddMinutes(60), // Date and time the cookie will expire 
                            false, // persistent cookie?
                            roles ?? "", // user data, role of the user 
                            FormsAuthentication.FormsCookiePath);

                        string encryptCookie = FormsAuthentication.Encrypt(ticket);
                        HttpCookie cookie = new HttpCookie(FormsAuthentication.FormsCookieName, encryptCookie);
                        HttpContext.Response.Cookies.Add(cookie);



                        if (Url.IsLocalUrl(returnUrl) && returnUrl.Length > 1 && returnUrl.StartsWith("/")
                            && !returnUrl.StartsWith("//") && !returnUrl.StartsWith("/\\"))
                        {
                            return RedirectToAction("Index", "Home");
                        }
                        else
                        {
                            return RedirectToAction("Index", "Home");
                        }
                    }
                    else
                    {
                        ModelState.AddModelError("", "The user name or password provided is incorrect.");
                    }
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        public ActionResult LogOff()
        {
            FormsAuthentication.SignOut();

            return RedirectToAction("Index", "Home");
        }

        
    }
}