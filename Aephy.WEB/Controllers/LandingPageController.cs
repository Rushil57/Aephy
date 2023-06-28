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
    }
}
