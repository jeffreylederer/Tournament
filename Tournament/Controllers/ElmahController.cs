using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Tournament.Controllers
{
    public class ElmahController : Controller
    {
        // GET: Elmah
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public ActionResult Index(string type)
        {
            return new ElmahResult(type);
        }
    }
}