using Aephy.WEB.Admin.Models;
using Aephy.WEB.Models;
using Aephy.WEB.Provider;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Data;


namespace Aephy.WEB.Admin.Controllers
{
    public class HomeController : Controller
    {

        private readonly IApiRepository _apiRepository;
        public HomeController(IApiRepository apiRepository)
        {

            _apiRepository = apiRepository;
        }
        public IActionResult Index()
        {
            var userId = HttpContext.Session.GetString("LoggedUser");
            if (userId == null)
            {
                return RedirectToAction("Login", "Home");
            }
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public ActionResult Login()
        {
            return View();
        }

        public ActionResult UserProfile()
        {
            return View();
        }
        public void LogOut()
        {
            HttpContext.Session.Remove("LoggedUser");
        }

        [HttpPost]
        public async Task<string> CheckExistingUser([FromBody] LoginModel loginModel)
        {
            try
            {
                var test = await _apiRepository.MakeApiCallAsync("api/Authenticate/Login", HttpMethod.Post, loginModel);
                var UserId = string.Empty;
                dynamic jsonObj = JsonConvert.DeserializeObject(test);
                if (jsonObj != null)
                {
                    UserId = jsonObj.Result.UserId;
                }
                HttpContext.Session.SetString("LoggedUser", UserId);
                return test;
            }
            catch (Exception ex)
            {

            }
            return "";
        }
    }
}
