﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace Tournament
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                "Admin_elmah",
                "elmah/{type}",
                new { action = "Index", controller = "Elmah", type = UrlParameter.Optional });

            //routes.MapRoute(
            //    name: "match",
            //    url: "{controller}/{action}/{id}/weekid",
            //    defaults: new { controller = "Matches", action = "Index", id = UrlParameter.Optional }
            //);

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );

        }
    }
}
