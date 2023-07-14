using Aephy.WEB.Models;
using Aephy.WEB.Provider;
using Aephy.WEB.Repository;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using WebApplication7.Models;
using static System.Net.WebRequestMethods;

namespace Aephy.WEB.Controllers
{
    public class HomeController : Controller
    {

        //ApiProvider _apiProvider = new ApiProvider();


        private readonly IApiRepository _apiRepository;
        public HomeController(IApiRepository apiRepository)
        {

            _apiRepository = apiRepository;
        }
        public IActionResult Dashboard()
        {
            var userEmail = HttpContext.Session.GetString("LoggedUser");
            if (userEmail == null)
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
        public ActionResult Form()
        {
            return View();
        }

        public ActionResult Login()
        {
            return View();
        }

        public ActionResult Register()
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
                //var jsonObj = JsonConvert.DeserializeObject(test);
                //var un = jsonObj;
                var UserName = test.Split(",")[2].Split(":")[1].Replace("}", string.Empty);
                HttpContext.Session.SetString("LoggedUser", UserName);
                return test;
            }
            catch(Exception ex)
            {

            }
            return "";
        }

        [HttpPost]
        public async Task<string> RegisterNewUser([FromBody] ResgisterNewUser registerModel)
        {
            try
            {
                var test = await _apiRepository.MakeApiCallAsync("api/Authenticate/Register", HttpMethod.Post, registerModel);
                return test;
            }
            catch (Exception ex)
            {

            }
            return "";
        }
    }
}
