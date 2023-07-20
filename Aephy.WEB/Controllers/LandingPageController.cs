using Aephy.WEB.Models;
using Aephy.WEB.Provider;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Aephy.WEB.Controllers
{
    public class LandingPageController : Controller
    {
		private readonly IApiRepository _apiRepository;
		public LandingPageController(IApiRepository apiRepository)
		{

			_apiRepository = apiRepository;
		}
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

        [HttpPost]
        public async Task<string> ApplyForOpenGigRoles([FromBody] OpenGigRolesModel OpenGigRolesData)
        {
			if (OpenGigRolesData != null)
			{
                var currentDateTime = DateTime.Now;
                OpenGigRolesData.CreatedDateTime = currentDateTime;
				var industryData = await _apiRepository.MakeApiCallAsync("api/Freelancer/OpenGigRolesApply", HttpMethod.Post, OpenGigRolesData);
				dynamic jsonObj = JsonConvert.DeserializeObject(industryData);
				return industryData;
			}
			else
			{
				return "failed to receive data..";
			}
		}
	}
}
