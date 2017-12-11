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

        public ActionResult CBS()
        {
            //string[] arg = "";
            //CPF_experiment.Program.Main(/*arg*/);
            var output = CPF_experiment.Program.RunCBS();
            //var output = "abc" + "\r\n" + "bcd";
            return View("CBS");

            //return Content("<pre>" + output + "</pre>");
        }

        [HttpPost]
        [ActionName("RunCBS")]
        public ActionResult CBS2(string map, string algorithm)
        {
            //string[] arg = "";
            //CPF_experiment.Program.Main(/*arg*/);
            var output = CPF_experiment.Program.RunCBS(map, algorithm);
            ////var output = "abc" + "\r\n" + "bcd";
            //return View("CBS");

            return Content("<pre>" + output + "</pre>");
        }
    }
}