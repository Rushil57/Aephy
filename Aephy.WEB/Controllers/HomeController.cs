using Aephy.Helper.Helpers;
using Aephy.WEB.Models;
using Aephy.WEB.Provider;
using Aephy.WEB.Repository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Data;
using System.Diagnostics;
using System.Net;
using System.Reflection;
using WebApplication7.Models;
using static System.Net.WebRequestMethods;

namespace Aephy.WEB.Controllers
{
    public class HomeController : Controller
    {

        private readonly IApiRepository _apiRepository;
        private readonly string _rootPath;
        public HomeController(IApiRepository apiRepository, IWebHostEnvironment hostEnvironment)
        {
            _apiRepository = apiRepository;
            _rootPath = hostEnvironment.WebRootPath;
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
            HttpContext.Session.Remove("FullName");
            HttpContext.Session.Remove("LoggedUserRole");
        }

        [HttpPost]
        public async Task<string> CheckExistingUser([FromBody] LoginModel loginModel)
        {
            try
            {
                var test = await _apiRepository.MakeApiCallAsync("api/Authenticate/Login", HttpMethod.Post, loginModel);
                var UserId = string.Empty;
                var FirstName = string.Empty;
                var LastName = string.Empty;
                var Role = string.Empty;
                dynamic jsonObj = JsonConvert.DeserializeObject(test);
                if (jsonObj["StatusCode"] == 200)
                {
                    UserId = jsonObj.Result.UserId;
                    FirstName = jsonObj.Result.FirstName;
                    LastName = jsonObj.Result.LastName;
                    Role = jsonObj.Result.Role;

                    HttpContext.Session.SetString("FullName", FirstName + " " + LastName);
                    HttpContext.Session.SetString("LoggedUserRole", Role);
                    HttpContext.Session.SetString("LoggedUser", UserId);
                }

                return test;
            }
            catch (Exception ex)
            {

            }
            return "";
        }

        [HttpPost]
        public async Task<string> AddorEditUserData([FromBody] RegisterNewUser registerModel)
        {
            if (registerModel.Id == null)
            {
                try
                {
                    var test = await _apiRepository.MakeApiCallAsync("api/Authenticate/Register", HttpMethod.Post, registerModel);

                    #region SendCongratulation Email

                    dynamic jsonObj = JsonConvert.DeserializeObject(test);

                    if (jsonObj["StatusCode"] == 200)
                    {
                        string body = System.IO.File.ReadAllText(_rootPath + "/EmailTemplates/congratulation.html");
                        body = body.Replace("##UserName##", registerModel.FirstName);
                        if (registerModel.UserType == "Client")
                        {
                            body = body.Replace("##UserInformation##", "<h4><b>UserType : </b> Client </h4>");
                        }
                        else
                        {
                            body = body.Replace("##UserInformation##", "<h4><b>UserType : </b> Freelancer </h4> <h4><b>Lavel : </b> " + registerModel.FreelancerLevel + " </h4>");
                        }

                        bool send = SendEmailHelper.SendEmail(registerModel.Email, "Welcome to Ephey", body);

                        if (!send)
                        {
                            return "Registration Success but email not send.";
                        }
                    }
                    #endregion

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
