using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc;

namespace Aephy.WEB.Controllers
{
    public class LandingPageController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public ActionResult Industries()
        {
            return View();
        }

        public ActionResult Services()
        {
            return View();
        }

        public ActionResult WhyAephy() {
            return View();
        }

        public ActionResult GigOpenings()
        {
            return View();
        }

        public ActionResult OpenGigRoles()
        {
            return View();
        }

        public ActionResult AboutUs()
        {
            return View();
        }

        public ActionResult Careers()
        {
            return View();
        }

        public ActionResult CareerOpenRoles()
        {
            return View();
        }

        public ActionResult Contact()
        {
            return View();
        }

        public ActionResult PostYourRequest()
        {
            return View();
        }

        public ActionResult BrowseSolution()
        {

            return View();
        }

        [HttpPost]
        public ActionResult BrowseSolution(string service,string solution,string industry)
        {
            string pagePath;
            if (string.IsNullOrEmpty(service) || string.IsNullOrEmpty(solution) || string.IsNullOrEmpty(industry))
            {
                return View();
            }
            else
            {
                if (service != "default" && solution != "default" && industry != "default")
                {
                    pagePath = service + " > " + solution + " / " + industry ;

                    ViewBag.pagePath = pagePath;

                    TempData["pagePath"] = pagePath;

                    return RedirectToAction("Project");
                }
            }
            return View();
        }

        public ActionResult Project()
        {
            return View();
        }

        public ActionResult FAQ()
        {
            return View();
        }
    }
}
