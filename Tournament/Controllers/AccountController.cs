using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using Tournament.Code;
using Tournament.Models;
using System.Configuration;

namespace Tournament.Controllers
{
    public class AccountsController : Controller
    {
        private TournamentEntities db = new TournamentEntities();

        [AllowAnonymous]
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
                
                string username = model.Username;
                string password = model.Password;

                // Now if our password was enctypted or hashed we would have done the
                // same operation on the user entered password here, But for now
                // since the password is in plain text lets just authenticate directly

                var users = db.Users.Where(x => x.username == model.Username);
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



                    //if (Url.IsLocalUrl(returnUrl) && returnUrl.Length > 1 && returnUrl.StartsWith("/")
                    //    && !returnUrl.StartsWith("//") && !returnUrl.StartsWith("/\\"))
                    //{
                    //    return RedirectToAction("Index", "Home");
                    //}
                    //else
                    //{
                    //    return RedirectToAction("Index", "Home");
                    //}
                    HttpContext.Session["user"] = username;
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ModelState.AddModelError("", "The user name or password provided is incorrect.");
                }
    
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        [Authorize]
        public ActionResult LogOff()
        {
            FormsAuthentication.SignOut();

            return RedirectToAction("Index", "Home");
        }

        [AllowAnonymous]
        public ActionResult ChangePassword()
        {
            var item = new ChangePasswordViewModel()
            {
                EmailAddress = User.Identity.Name
            };
            return View(item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChangePassword(
            [Bind(Include = "EmailAddress,OldPassword, Password,Confirm")] ChangePasswordViewModel chpvm)
        {
            if (ModelState.IsValid)
            {
                var users = db.Users.Where(x => x.username.ToLower() == chpvm.EmailAddress.ToString());
                if (!users.Any())
                {
                    ModelState.AddModelError(string.Empty, "User Id or Current Password not found");
                    return View(chpvm);
                }
                var user = users.First();
                if (user.password != chpvm.OldPassword)
                {
                    ModelState.AddModelError(string.Empty, "User Id or Current Password not found");
                    return View(chpvm);
                }
                if (chpvm.Password != chpvm.Confirm)
                {
                    ModelState.AddModelError(string.Empty, "New Password and Confirming Password are not the same");
                    return View(chpvm);
                }

                try
                {
                    user.password = chpvm.Password;
                    db.Entry(user).State = EntityState.Modified;
                    db.SaveChanges();
                    return RedirectToAction("PasswordAccepted", "Accounts");
                }
                catch
                {
                    ModelState.AddModelError(string.Empty, "Password was not updated, try again");
                }
 
            }
            return View(chpvm);
        }

        [AllowAnonymous]
        public ActionResult PasswordAccepted()
        {
            return View();
        }

        [AllowAnonymous]
        public ActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ForgotPassword([Bind(Include = "EmailId")] ForgotPasswordViewModel forgotPassword)
        {
            if (ModelState.IsValid)
            {
                var users = db.Users.Where(x => x.username.ToLower() == forgotPassword.EmailId.ToLower());
                if (!users.Any())
                {
                    ModelState.AddModelError(string.Empty, "User Id not found");
                    return View(forgotPassword);
                }
                var user = users.First();
                var activationCode =
                    HttpUtility.UrlEncode(Secret.Protect(
                        $"{user.id},{DateTime.Now.AddMinutes(15).ToString("M/d/yyyy hh:mm:ss tt")}"));

                var verifyUrl = $"/Accounts/ResetPassword/{activationCode}";
                var link = $"http://{Request.Url.Host}:{Request.Url.Port}{verifyUrl}";

                var emailUser =db.Users.First(x => x.Roles == "Mailer");
                var fromEmail = new MailAddress(emailUser.username, "Lawn Bowling Pittsburgh");
                var toEmail = new MailAddress(forgotPassword.EmailId);
                var fromEmailPassword = emailUser.password; 

                var subject = "Reset Password for League Application";
                var body =
                    $"Hi,<br/><br/>We got request for reset your account password. Please click on the below link to reset your password<br/><br/><a href={link}>Reset Password link</a>";
                using (var smtp = new SmtpClient())
                {

                    smtp.Host = ConfigurationManager.AppSettings["smtp"];
                    smtp.Port = 587;
                    smtp.EnableSsl = true;
                    smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                    smtp.UseDefaultCredentials = false;
                    smtp.Credentials = new NetworkCredential(fromEmail.Address, fromEmailPassword);

                    using (var message = new MailMessage(fromEmail, toEmail))
                    {

                        message.Subject = subject;
                        message.Body = body;
                        message.IsBodyHtml = true;
                        try
                        {
                            smtp.Send(message);
                            return RedirectToAction("SentEmail", "Accounts");
                        }
                        catch (Exception ex)
                        {
                            ViewBag.Error = $"Could not send request, Error {ex.Message}";
                        }
                    }
                }
            }
            return View(forgotPassword);
        }

        public ActionResult SentEmail()
        {
            return View();
        }

        [AllowAnonymous]
        public ActionResult ResetPassword(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var item = new ResetPasswordViewModel()
            {
                ActivationCode = id
            };
            return View(item);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ResetPassword([Bind(Include = "ActivationCode,Password,Confirm")] ResetPasswordViewModel resetPassword)
        {
            if (ModelState.IsValid)
            {
                var code = Secret.Unprotect(resetPassword.ActivationCode);
                if (string.IsNullOrWhiteSpace(code))
                {
                    ModelState.AddModelError(string.Empty, "Link has been tampered with.");
                    return View(resetPassword);
                }
                var codes = code.Split(new char[] {','});
                if (codes.Length != 2)
                {
                    ModelState.AddModelError(string.Empty, "Link has been tampered with.");
                    return View(resetPassword);
                }
                int userid;
                DateTime start;
                if(!int.TryParse(codes[0], out userid))
                {
                    ModelState.AddModelError(string.Empty, "Link has been tampered with.");
                    return View(resetPassword);
                }
                if(!DateTime.TryParse(codes[1], out start))
                {
                    ModelState.AddModelError(string.Empty, "Link has been tampered with.");
                    return View(resetPassword);
                }
                if(DateTime.Now.Subtract(start).TotalMinutes>0)
                {
                    ModelState.AddModelError(string.Empty, "Link is more than 15 minutes old");
                    return View(resetPassword);
                }
                var user = db.Users.Find(userid);
                if (user == null)
                {
                    ModelState.AddModelError(string.Empty, "Link has been tampered with.");
                    return View(resetPassword);
                }
                if (resetPassword.Password != resetPassword.Confirm)
                {
                    ModelState.AddModelError(string.Empty, "New Password and Confirming Password are not the same");
                    return View(resetPassword);
                }
                try
                {
                    user.password = resetPassword.Password;
                    db.Entry(user).State = EntityState.Modified;
                    db.SaveChanges();
                    return RedirectToAction("PasswordAccepted", "Accounts");
                }
                catch 
                {
                    ModelState.AddModelError(string.Empty, "Password not changed, try again");
                }
            }
            return View(resetPassword);
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