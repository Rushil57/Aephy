using Aephy.WEB.Admin.Models;
using Aephy.WEB.Models;
using Aephy.WEB.Provider;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Data;
using System.Text.Json.Nodes;

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
            var userId = HttpContext.Session.GetString("LoggedAdmin");
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
            var userId = HttpContext.Session.GetString("LoggedAdmin");
            if (userId == null)
            {
                return RedirectToAction("Login", "Home");
            }
            return View();
        }
        public void LogOut()
        {
            HttpContext.Session.Remove("LoggedAdmin");
        }

        [HttpPost]
        public async Task<JsonResult> CheckExistingUser([FromBody] LoginModel loginModel)
        {
            var messageSatus = string.Empty; 
            try
            {
                var test = await _apiRepository.MakeApiCallAsync("api/Authenticate/Login", HttpMethod.Post, loginModel);
                var UserId = string.Empty;
                var UserType = string.Empty;
                var FirstName = string.Empty;
                var LastName = string.Empty;
                var Role = string.Empty;
                dynamic jsonObj = JsonConvert.DeserializeObject(test);
                if (jsonObj != null)
                {
                    messageSatus = jsonObj.Message;
                    UserId = jsonObj.Result.UserId;
                    FirstName = jsonObj.Result.FirstName;
                    LastName = jsonObj.Result.LastName;
                    Role = jsonObj.Result.Role;

                    var cookieOptions = new CookieOptions();
                    if (loginModel.RememberMe == true)
                    {
                        cookieOptions.Expires = DateTime.Now.AddDays(1);
                        //cookieOptions.Path = "/";
                        Response.Cookies.Append("userName", loginModel.Username, cookieOptions);
                        Response.Cookies.Append("password", loginModel.Password, cookieOptions);
                    }

                    if (Role != "Admin")
                    {
                        return Json(new { message = "Invalid Credentials" });
                    }
                    HttpContext.Session.SetString("FullName", FirstName + " " + LastName);
                    HttpContext.Session.SetString("LoggedUserRole", Role);
                    HttpContext.Session.SetString("LoggedAdmin", UserId);
                    return Json(new { message = "Login Success" });
                }
                /*return test;*/
            }
            catch (Exception ex)
            {
                return Json(new { message = "Something went wrong!" });
            }
            return Json(new { message = messageSatus });
        }

        //GetAllUnReadNotification
        [HttpGet]
        public async Task<string> GetAllUnReadNotification()
        {
            var userId = HttpContext.Session.GetString("LoggedAdmin");
            if (userId == null)
            {
                return "No Data Found";
            }
            SolutionFundModel model = new SolutionFundModel();
            model.UserId = userId;
            var notificationdata = await _apiRepository.MakeApiCallAsync("api/Freelancer/GetAllUnReadNotification", HttpMethod.Post, model);
            return notificationdata;

        }

    }
}
