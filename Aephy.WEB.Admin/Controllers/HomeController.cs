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

        [HttpPost]
        public async Task<string> GetSolutionDefineData([FromBody] SolutionIndustryViewModel model)
        {
            var userId = HttpContext.Session.GetString("LoggedAdmin");
            if (userId != null)
            {
                //model.FreelancerId = userId;
                var aprroveList = await _apiRepository.MakeApiCallAsync("api/Freelancer/GetSolutionDefineData", HttpMethod.Post, model);
                return aprroveList;
            }
            return "Something Went Wrong";
        }

        [HttpPost]
        public async Task<string> GetMiletoneList([FromBody] SolutionIndustryViewModel model)
        {
            var userId = HttpContext.Session.GetString("LoggedAdmin");
            if (userId != null)
            {
                model.UserId = userId;
                var milestoneList = await _apiRepository.MakeApiCallAsync("api/Freelancer/GetMiletoneList", HttpMethod.Post, model);
                return milestoneList;
            }
            return "Something Went Wrong";
        }

        //GetPointsList
        [HttpPost]
        public async Task<string> GetPointsList([FromBody] SolutionIndustryViewModel model)
        {
            var userId = HttpContext.Session.GetString("LoggedAdmin");
            if (userId != null)
            {
                model.UserId = userId;
                var pointsList = await _apiRepository.MakeApiCallAsync("api/Freelancer/GetPointsList", HttpMethod.Post, model);
                return pointsList;
            }
            return "Something Went Wrong";
        }

        //SaveMileStone
        [HttpPost]
        public async Task<string> SaveMileStone([FromBody] MileStoneViewModel mileStone)
        {
            if (mileStone != null)
            {
                try
                {
                    var userId = HttpContext.Session.GetString("LoggedAdmin");
                    mileStone.UserId = userId;
                    var test = await _apiRepository.MakeApiCallAsync("api/Freelancer/SaveMileStoneData", HttpMethod.Post, mileStone);
                    return test;
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
            return "";
        }

        //SavePoints
        [HttpPost]
        public async Task<string> SavePoints([FromBody] SolutionPointsViewModel model)
        {
            if (model != null)
            {
                try
                {
                    var userId = HttpContext.Session.GetString("LoggedAdmin");
                    model.FreelancerId = userId;
                    var test = await _apiRepository.MakeApiCallAsync("api/Freelancer/SavePointsData", HttpMethod.Post, model);
                    return test;
                }
                catch (Exception ex)
                {
                    return ex.Message + ex.InnerException;
                }
            }
            return "Something Went Wrong";
        }

        //DeletePoints
        [HttpPost]
        public async Task<string> DeletePoints([FromBody] SolutionPointsViewModel model)
        {
            if (model.Id != 0)
            {
                var pointsData = await _apiRepository.MakeApiCallAsync("api/Freelancer/DeletePointsById", HttpMethod.Post, model);
                return pointsData;

            }
            else
            {
                return "failed to receive data..";
            }
        }

        //DeleteMileStone
        [HttpPost]
        public async Task<string> DeleteMileStone([FromBody] MileStoneViewModel model)
        {
            if (model.Id != 0)
            {
                var milestonData = await _apiRepository.MakeApiCallAsync("api/Freelancer/DeleteMileStoneById", HttpMethod.Post, model);
                return milestonData;

            }
            else
            {
                return "failed to receive data..";
            }
        }

        [HttpPost]
        public async Task<string> GetMileStoneById([FromBody] MileStoneViewModel model)
        {
            var milestonedata = await _apiRepository.MakeApiCallAsync("api/Freelancer/GetMileStoneById", HttpMethod.Post, model);
            return milestonedata;

        }

        [HttpPost]
        public async Task<string> GetPointsDataById([FromBody] MileStoneViewModel model)
        {
            var pointsdata = await _apiRepository.MakeApiCallAsync("api/Freelancer/GetPointsDataById", HttpMethod.Post, model);
            return pointsdata;

        }

        [HttpPost]
        public async Task<string> UpdateIndustryOutline([FromBody] SolutionIndustryViewModel model)
        {
            if (model != null)
            {
                try
                {
                    var test = await _apiRepository.MakeApiCallAsync("api/Freelancer/UpdateIndustryOutline", HttpMethod.Post, model);
                    return test;
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
            return "";
        }
    }
}
