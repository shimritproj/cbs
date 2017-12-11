using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CBS.WEB2.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
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

        public ActionResult CBS()
        {
            //string[] arg = "";
            //CPF_experiment.Program.Main(/*arg*/);
            var output = CPF_experiment.Program.RunCBS();
            //var output = "abc" + "\r\n" + "bcd";
            return Content("<pre>" + output + "</pre>");
        }
    }
}