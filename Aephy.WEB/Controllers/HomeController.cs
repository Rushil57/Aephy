using Aephy.WEB.Models;
using Aephy.WEB.Provider;
using Aephy.WEB.Repository;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.Net;
using System.Reflection;
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
                var UserId = string.Empty;
                dynamic jsonObj = JsonConvert.DeserializeObject(test);
                if (jsonObj != null)
                {
                    UserId = jsonObj.Result;
                }
                HttpContext.Session.SetString("LoggedUser", UserId);
                return test;
            }
            catch (Exception ex)
            {

            }
            return "";
        }

        [HttpPost]
        public async Task<string> AddorEditUserData([FromBody] ResgisterNewUser registerModel)
        {
            if(registerModel.Id == null)
            {
                try
                {
                    var test = await _apiRepository.MakeApiCallAsync("api/Authenticate/Register", HttpMethod.Post, registerModel);
                    return test;
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
            else
            {
                try
                {
                    var userId = HttpContext.Session.GetString("LoggedUser");
                    if (userId != null)
                    {
                        var userData = await _apiRepository.MakeApiCallAsync("api/User/UpdateProfile", HttpMethod.Post, registerModel);
                        return userData;
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
          
            return "";
        }

        [HttpPost]
        public async Task<string> GetUserData()
        {
            try
            {
                GetUserProfileRequestModel model = new GetUserProfileRequestModel();
                var userId = HttpContext.Session.GetString("LoggedUser");
                if (userId != null)
                {
                    model.UserId = userId;
                    var userData = await _apiRepository.MakeApiCallAsync("api/User/GetUserProfile", HttpMethod.Post, model);
                    return userData;
                }
            }
            catch (Exception ex)
            {

            }
            return "";
        }


        [HttpPost]
        public async Task<string> UpdateUserPassword([FromBody] ChangePasswordModel changePassword)
        {
            try
            {
                var userId = HttpContext.Session.GetString("LoggedUser");
                if (userId != null)
                {
                    var userPasswordData = await _apiRepository.MakeApiCallAsync("api/User/ChangePassword", HttpMethod.Post, changePassword);
                    return userPasswordData;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return "";
        }
    }
}
