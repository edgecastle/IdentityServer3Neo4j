using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace IdentityServer3Neo4J.Samples.MVC.Controllers
{
    public class JsClientController : Controller
    {
        // GET: JsClient
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Popup()
        {
            return View();
        }
    }
}