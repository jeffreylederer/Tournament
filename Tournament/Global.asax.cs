using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.Security;

namespace Tournament
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            SqlServerTypes.Utilities.LoadNativeAssemblies(Server.MapPath("~/bin"));
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        protected void FormsAuthentication_OnAuthenticate(Object sender, FormsAuthenticationEventArgs e)
        {
            if (FormsAuthentication.CookiesSupported == true)
            {
                if (Request.Cookies[FormsAuthentication.FormsCookieName] != null)
                {
                    try
                    {
                        //let us take out the username now                
                        string username = FormsAuthentication.Decrypt(Request.Cookies[FormsAuthentication.FormsCookieName].Value).Name;
                        //let us extract the roles from our own custom cookie
                        HttpCookie cookie = HttpContext.Current.Request.Cookies[FormsAuthentication.FormsCookieName];

                        if (cookie != null)
                        {
                            FormsAuthenticationTicket ticket = FormsAuthentication.Decrypt(cookie.Value);

                            if (ticket != null && !ticket.Expired)
                            {
                                var roles = ticket.UserData.Split(',');

                                //Let us set the Pricipal with our user specific details
                                e.User = new System.Security.Principal.GenericPrincipal(
                                    new System.Security.Principal.GenericIdentity(username, "Forms"), roles);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        var message = ex.Message;
                    }
                }
            }
        }
    }
}
