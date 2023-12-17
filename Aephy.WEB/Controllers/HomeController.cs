using Aephy.Helper.Helpers;
using Aephy.WEB.Models;
using Aephy.WEB.Provider;
using Aephy.WEB.Repository;
using Azure.Storage.Blobs;
using Azure.Storage.Sas;
using Azure.Storage;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Data;
using System.Diagnostics;
using System.Net;
using System.Reflection;
using WebApplication7.Models;
using static System.Net.Mime.MediaTypeNames;
using static System.Net.WebRequestMethods;
using Aephy.API.DBHelper;
using Azure.Storage.Blobs.Models;
using System.Text.RegularExpressions;
using Stripe;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Aephy.API.Migrations;
using Azure;
using System.Net.Mime;
using iTextSharp.text.pdf;
using iTextSharp.text;
using Elfie.Serialization;
using static Aephy.WEB.Models.AddNonRevolutCounterpartyReq;
using System.Globalization;

namespace Aephy.WEB.Controllers
{
    public class HomeController : Controller
    {
        private readonly IApiRepository _apiRepository;
        private const string ContainerName = "cvfiles";
        private const string ImageContainerName = "profileimages";
        private readonly string _rootPath;
        private readonly IConfiguration _configuration;
        private readonly string _connectionString;

        public HomeController(IApiRepository apiRepository, IWebHostEnvironment hostEnvironment, IConfiguration configuration)
        {
            _apiRepository = apiRepository;
            _configuration = configuration;
            _connectionString = configuration.GetConnectionString("AzureBlobStorage");
            _rootPath = hostEnvironment.WebRootPath;
            _connectionString = "DefaultEndpointsProtocol=https;AccountName=aephystorageaccount;AccountKey=nEy6xh4P4m2d94iDgqq+yNB99bucjGMD1wp2L6sbsNFjHPaUQiCHgc5b4hmBmeRtYsiA/WvudVmV+AStwz3djw==;EndpointSuffix=core.windows.net";
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

        public ActionResult Register(bool isInvite)
        {
            ViewBag.IsInvited = isInvite;
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
            HttpContext.Session.Remove("LoggedUserLevel");
            HttpContext.Session.Remove("ClientPreferredCurrency");
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
                var Level = string.Empty;
                var imageUrlWithSas = string.Empty;
                var ClientCurrency = string.Empty;

                dynamic jsonObj = JsonConvert.DeserializeObject(test);
                if (jsonObj["Message"] == "Login Success")
                {
                    FirstName = jsonObj.Result.FirstName;
                    LastName = jsonObj.Result.LastName;
                    Role = jsonObj.Result.Role;
                    Level = jsonObj.Result.Level;
                    UserId = jsonObj.Result.UserId;
                    ClientCurrency = jsonObj.Result.ClientPreferredCurrency;
                    //UserStripeAccount = jsonObj.Result.StripeStatus;

                    HttpContext.Session.SetString("FullName", FirstName + " " + LastName);
                    HttpContext.Session.SetString("LoggedUserRole", Role);
                    HttpContext.Session.SetString("ShortName", FirstName[0] + "" + LastName[0]);

                    // REMEMBER ME FUNCTANALITY
                    var cookieOptions = new CookieOptions();
                    if (loginModel.RememberMe == true)
                    {

                        cookieOptions.Expires = DateTime.Now.AddDays(1);
                        //cookieOptions.Path = "/";
                        Response.Cookies.Append("userName", loginModel.Username, cookieOptions);
                        Response.Cookies.Append("password", loginModel.Password, cookieOptions);
                    }
                    // REMEMBER ME FUNCTANALITY

                    if (Level != "none")
                    {
                        HttpContext.Session.SetString("LoggedUserLevel", Level);
                    }
                    HttpContext.Session.SetString("LoggedUser", UserId);
                    if(ClientCurrency != null)
                        HttpContext.Session.SetString("ClientPreferredCurrency", ClientCurrency);

                    string imagepath = jsonObj.Result.ImagePath;
                    if (imagepath != null)
                    {
                        string sasToken = GenerateImageSasToken(imagepath);
                        imageUrlWithSas = $"{jsonObj.Result.ImagePath}?{sasToken}";
                    }

                    HttpContext.Session.SetString("UserProfileImage", imageUrlWithSas);
                    // HttpContext.Session.SetString("CompleteUserStripeAccount", UserStripeAccount);
                }

                return test;
            }
            catch (Exception ex)
            {

            }
            return "";
        }
        [HttpPost]
        public async Task<string> AddUserData([FromBody] RegisterNewUser registerModel)
        {
            if (registerModel.Id == null)
            {
                try
                {
                    var test = await _apiRepository.MakeApiCallAsync("api/Authenticate/Register", HttpMethod.Post, registerModel);

                    #region SendCongratulation Email

                    dynamic jsonObj = JsonConvert.DeserializeObject(test);

                    if (jsonObj["StatusCode"] == 200 && registerModel.UserType != "Admin")
                    {
                        string userId = Convert.ToString(jsonObj["Result"]["Id"]);
                        string body = System.IO.File.ReadAllText(_rootPath + "/EmailTemplates/VerificationTemplate.html");
                        string verifyUrl = _configuration.GetValue<string>("VerifyURL:Url").Replace("{{UserId}}", userId);

                        body = body.Replace("{{ first_name }}", registerModel.FirstName);
                        body = body.Replace("{{ url }}", verifyUrl);

                        bool send = SendEmailHelper.SendEmail(registerModel.Email, "Welcome to Ephylink", body);

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
                    return ex.Message + ex.InnerException;
                }
            }

            return "";
        }


        [HttpPost]
        public async Task<string> EditUserData(IFormFile httpPostedFileBaseProfile, IFormFile httpPostedFileBase, string Userdata)
        {
            var registerModel = JsonConvert.DeserializeObject<RegisterNewUser>(Userdata);

            try
            {
                var userId = HttpContext.Session.GetString("LoggedUser");
                if (userId != null)
                {
                    var userData = await _apiRepository.MakeApiCallAsync("api/User/UpdateProfile", HttpMethod.Post, registerModel);
                    dynamic data = JsonConvert.DeserializeObject(userData);
                    if (data.StatusCode == 200)
                    {
                        if (httpPostedFileBase != null)
                        {
                            var d = await SaveUserCVFile(httpPostedFileBase, registerModel.Id);
                            var ok = await _apiRepository.MakeApiCallAsync("api/Freelancer/UpdateUserCV", HttpMethod.Post, d);
                        }

                        if (httpPostedFileBaseProfile != null)
                        {
                            var userProfileDetails = await SaveImageFile(httpPostedFileBaseProfile, registerModel.Id);
                            await _apiRepository.MakeApiCallAsync("api/Admin/UpdateUserProfileImage", HttpMethod.Post, userProfileDetails);
                        }

                    }
                    return userData;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }


            return "";
        }

        [HttpPost]
        public async Task<string> SaveCalenderData(string CalendarData)
        {
            var model = JsonConvert.DeserializeObject<CalendarData>(CalendarData);
            try
            {
                var userId = HttpContext.Session.GetString("LoggedUser");
                var userRole = HttpContext.Session.GetString("LoggedUserRole");

                if (userId != null)
                {
                    model.Id = userId;
                    if (userRole == "Freelancer")
                    {
                        if (model.IsWorkEarlier && model.IsWorkLater && !model.IsNotAvailableForNextSixMonth)
                        {
                            DateTime startHourEarlier = model.StartHoursEarlier ?? DateTime.Now;
                            DateTime startHourLater = model.StartHoursLater ?? DateTime.Now;
                            DateTime endHourEarlier = model.EndHoursEarlier ?? DateTime.Now;
                            DateTime endHourLater = model.EndHoursLater ?? DateTime.Now;

                            if (model.StartHour <= startHourEarlier && model.StartHour <= startHourLater)
                            {
                                model.StartHoursFinal = model.StartHour;
                            }
                            else if (startHourEarlier <= startHourLater)
                            {
                                model.StartHoursFinal = startHourEarlier;
                            }
                            else
                            {
                                model.StartHoursFinal = startHourLater;
                            }

                            if (model.EndHour >= endHourEarlier && model.EndHour >= endHourLater)
                            {
                                model.EndHoursFinal = model.EndHour;
                            }
                            else if (endHourEarlier >= endHourLater)
                            {
                                model.EndHoursFinal = endHourEarlier;
                            }
                            else
                            {
                                model.EndHoursFinal = endHourLater;
                            }
                        }
                        else if (!model.IsNotAvailableForNextSixMonth && model.IsWorkEarlier)
                        {
                            DateTime startHourEarlier = model.StartHoursEarlier ?? DateTime.Now;
                            DateTime endHourEarlier = model.EndHoursEarlier ?? DateTime.Now;

                            if (model.StartHour <= startHourEarlier)
                            {
                                model.StartHoursFinal = model.StartHour;
                            }
                            else
                            {
                                model.StartHoursFinal = startHourEarlier;
                            }

                            if (model.EndHour >= endHourEarlier)
                            {
                                model.EndHoursFinal = model.EndHour;
                            }
                            else
                            {
                                model.EndHoursFinal = endHourEarlier;
                            }

                        }
                        else if (!model.IsNotAvailableForNextSixMonth && model.IsWorkLater)
                        {
                            DateTime startHourLater = model.StartHoursLater ?? DateTime.Now;
                            DateTime endHourLater = model.EndHoursLater ?? DateTime.Now;

                            if (model.StartHour >= startHourLater)
                            {
                                model.StartHoursFinal = model.StartHour;
                            }
                            else
                            {
                                model.StartHoursFinal = startHourLater;
                            }

                            if (model.EndHour >= endHourLater)
                            {
                                model.EndHoursFinal = model.EndHour;
                            }
                            else
                            {
                                model.EndHoursFinal = endHourLater;
                            }
                        }
                        model.onMonday = null;
                        model.onTuesday = null;
                        model.onWednesday = null;
                        model.onThursday = null;
                        model.onFriday = null;
                        model.onSaturday = null;
                        model.onSunday = null;
                    }
                    else
                    {
                        model.IsNotAvailableForNextSixMonth = false;
                        model.StartHoursEarlier = null;
                        model.StartHoursLater = null;
                        model.EndHoursEarlier = null;
                        model.EndHoursLater = null;
                        model.StartHoursFinal = null;
                        model.EndHoursFinal = null;
                        model.IsWorkEarlier = true;
                        model.IsWorkLater = true;
                    }

                    var userData = await _apiRepository.MakeApiCallAsync("api/User/UpdateCalendarData", HttpMethod.Post, model);
                    return userData;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return "";
        }

        public async Task<ImageClass> SaveImageFile(IFormFile imageFile, object Id)
        {
            ImageClass solutions = new ImageClass();
            try
            {
                if (imageFile != null && imageFile.Length > 0)
                {
                    string BlobStorageBaseUrl = string.Empty;
                    string ImagePath = string.Empty;
                    string ImageUrlWithSas = string.Empty;

                    var newfileName = Regex.Replace(imageFile.FileName, @"[^a-zA-Z]", "");
                    string fileName = Guid.NewGuid().ToString() + "_" + newfileName;

                    // Get the Azure Blob Storage connection string from configuration
                    var connectionString = _configuration.GetConnectionString("AzureBlobStorage");

                    BlobServiceClient blobServiceClient = new BlobServiceClient(connectionString);
                    BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(ImageContainerName);

                    BlobClient blobClient = containerClient.GetBlobClient(fileName);

                    using (var stream = imageFile.OpenReadStream())
                    {
                        await blobClient.UploadAsync(stream, overwrite: true);
                    }

                    ImagePath = blobClient.Uri.ToString();


                    string sasToken = GenerateImageSasToken(ImagePath);


                    string imageUrlWithSas = ImagePath + sasToken;



                    BlobStorageBaseUrl = containerClient.Uri.ToString();


                    ImageUrlWithSas = imageUrlWithSas;

                    solutions.BlobStorageBaseUrl = BlobStorageBaseUrl;
                    solutions.ImagePath = ImagePath;
                    solutions.ImageUrlWithSas = ImageUrlWithSas;
                    if (Id is string)
                    {
                        solutions.FreelancerId = (string)Id;
                    }
                    else
                    {
                        solutions.Id = (int)(Id);
                    }


                    return solutions;
                }
                else
                {
                    ModelState.AddModelError("ImageFile", "Please select an image file.");
                }
            }
            catch (Exception ex)
            {

            }
            return solutions;


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
                    dynamic data = JsonConvert.DeserializeObject(userData);
                    try
                    {
                        if (data.Result != null)
                        {
                            string imagepath = data.Result.ImagePath;
                            if (imagepath != null)
                            {
                                string sasToken = GenerateImageSasToken(imagepath);
                                string imageUrlWithSas = $"{data.Result.ImagePath}?{sasToken}";
                                data.Result.ImageUrlWithSas = imageUrlWithSas;
                                HttpContext.Session.SetString("UserProfileImage", imageUrlWithSas);
                            }

                            string updatedCurrency = data.Result.PreferredCurrency;
                            if (updatedCurrency != null)
                            {
                                var Currentcurrency = HttpContext.Session.GetString("ClientPreferredCurrency");
                                if (Currentcurrency != updatedCurrency)
                                {
                                    HttpContext.Session.Remove("ClientPreferredCurrency");
                                    HttpContext.Session.SetString("ClientPreferredCurrency", updatedCurrency);
                                }
                            }

                        }
                    }
                    catch (Exception ex)
                    {
                        return ex.Message + ex.InnerException;
                    }
                    string jsonString = JsonConvert.SerializeObject(data, Formatting.Indented);
                    return jsonString;
                }
            }
            catch (Exception ex)
            {
                return ex.Message + ex.InnerException;
            }
            return "";
        }

        //[HttpPost]
        //public async Task<string> getSavedLevelsdataByName([FromBody] LevelRangeModel level)
        //{
        //    try
        //    {
        //        var Response = await _apiRepository.MakeApiCallAsync("api/Admin/GetSavedLevelByName", HttpMethod.Post, level);
        //        return Response;
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }
        //}

        [HttpPost]
        public async Task<string> getSavedLevelsdataByName([FromBody] FindExchangeRateModel level)
        {
            try
            {
                level.FreelancerId = HttpContext.Session.GetString("LoggedUser");
                var Response = await _apiRepository.MakeApiCallAsync("api/Admin/GetSavedLevelByName", HttpMethod.Post, level);
                return Response;
            }
            catch (Exception ex)
            {
                throw ex;
            }
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

        [HttpGet]
        public async Task<string> GetRolesList()
        {
            var RolesList = await _apiRepository.MakeApiCallAsync("api/Admin/ActiveRolesList", HttpMethod.Get);
            return RolesList;
        }

        public async Task<IActionResult> VerifyAccount(string userId)
        {
            ViewBag.Active = false;
            try
            {
                if (userId != null)
                {
                    var verifyData = await _apiRepository.MakeApiCallAsync("api/Authenticate/VerifyAccount", HttpMethod.Post, userId);
                    dynamic jsonObj = JsonConvert.DeserializeObject(verifyData);
                    if (jsonObj["StatusCode"] == 200)
                    {
                        ViewBag.Active = true;
                        var emailId = jsonObj.Result.EmailId;
                        var sendmail = await SendUnclockFeatureMail(emailId);

                        return View();

                    }
                }
            }
            catch (Exception ex)
            {
                //throw ex;
                ViewBag.Active = false;
            }
            return View();
        }

        public async Task<string> SendUnclockFeatureMail(object email)
        {
            #region SendNewFeature Email

            string body = System.IO.File.ReadAllText(_rootPath + "/EmailTemplates/ApplyAndUnlockTemplate.html");

            bool send = SendEmailHelper.SendEmail(email.ToString(), "Apply & Unlock Future Projects", body);
            

            if (!send)
            {
                return "Registration Success but email not send.";
            }
            return "success";

            #endregion
          
        }

        [HttpPost]
        public async Task<string> ApplyForOpenGigRoles(IFormFile httpPostedFileBase, string GigOpenRolesData)
        {
            try
            {

                IFormFile CVFile = httpPostedFileBase;
                var result = JsonConvert.DeserializeObject<OpenGigRolesModel>(GigOpenRolesData);


                var freelancer = HttpContext.Session.GetString("LoggedUser");
                result.FreelancerID = freelancer;
                var currentDateTime = DateTime.Now;
                result.CreatedDateTime = currentDateTime;
                var openGigRolesData = await _apiRepository.MakeApiCallAsync("api/Freelancer/OpenGigRolesApply", HttpMethod.Post, result);

                dynamic data = JsonConvert.DeserializeObject(openGigRolesData);
                if (data.Message == "Applied Successfully")
                {
                    if (result.AlreadyExistCv)
                    {
                        int Id = data.Result;
                        OpenGigRolesCV opengigroles = new OpenGigRolesCV();
                        opengigroles.AlreadyExist = true;
                        opengigroles.ID = Id;
                        opengigroles.FreelancerId = freelancer;
                        var ok = await _apiRepository.MakeApiCallAsync("api/Freelancer/UpdateCV", HttpMethod.Post, opengigroles);
                        return data.Message;
                    }
                    else
                    {
                        if (data != null)
                        {

                            int Id = data.Result;
                            var d = await SaveCVFile(CVFile, Id);
                            var ok = await _apiRepository.MakeApiCallAsync("api/Freelancer/UpdateCV", HttpMethod.Post, d);
                            dynamic ImageResponse = JsonConvert.DeserializeObject(ok);
                            if (ImageResponse != null)
                            {
                                return ImageResponse.Message;
                            }
                            else
                            {
                                return "Failed to Apply !";
                            }
                        }
                    }
                }
                else
                {
                    return data.Message;
                }


            }
            catch (Exception ex)
            {
                return ex.Message;
            }
            return "Failed to Apply !";
        }
        private static string GetEndpointSuffixFromConnectionString(string connectionString)
        {
            string[] parts = connectionString.Split(";");
            foreach (var part in parts)
            {
                string[] subparts = part.Split("=");
                if (subparts.Length == 2 && subparts[0].Equals("EndpointSuffix"))
                {
                    return subparts[1];
                }
            }
            return null;
        }

        private string GenerateSasToken(string imageUrl)
        {
            // Get the blob container name and blob name from the image URL
            string blobName = Path.GetFileName(imageUrl);

            // Create a shared access policy that allows read access to the blob
            BlobSasBuilder sasBuilder = new BlobSasBuilder()
            {
                BlobContainerName = ContainerName,
                BlobName = blobName,
                Resource = "b",
                StartsOn = DateTime.UtcNow.AddMinutes(-5), // Adjust the start time if needed
                ExpiresOn = DateTime.UtcNow.AddMinutes(10), // Adjust the expiry time if needed,
                Protocol = SasProtocol.Https
            };
            sasBuilder.SetPermissions(BlobSasPermissions.Read);

            // Get the Azure Blob Storage connection string from configuration
            var connectionString = _configuration.GetConnectionString("AzureBlobStorage");

            // Extract the AccountName and AccountKey from the connection string
            var accountName = GetAccountNameFromConnectionString(connectionString);
            var accountKey = GetAccountKeyFromConnectionString(connectionString);

            // Create a StorageSharedKeyCredential using the AccountName and AccountKey
            StorageSharedKeyCredential credential = new StorageSharedKeyCredential(accountName, accountKey);

            // Generate the SAS token
            string sasToken = sasBuilder.ToSasQueryParameters(credential).ToString();

            return sasToken;
        }


        private string GetAccountNameFromConnectionString(string connectionString)
        {
            try
            {
                var accountNameStartIndex = connectionString.IndexOf("AccountName=", StringComparison.InvariantCultureIgnoreCase) + "AccountName=".Length;
                var accountNameEndIndex = connectionString.IndexOf(";", accountNameStartIndex, StringComparison.InvariantCultureIgnoreCase);
                var accountNameLength = accountNameEndIndex - accountNameStartIndex;
                return connectionString.Substring(accountNameStartIndex, accountNameLength);
            }
            catch (Exception ex)
            {

            }


            return "";
        }

        private string GetAccountKeyFromConnectionString(string connectionString)
        {
            try
            {
                var accountKeyStartIndex = connectionString.IndexOf("AccountKey=", StringComparison.InvariantCultureIgnoreCase) + "AccountKey=".Length;
                var accountKeyEndIndex = connectionString.IndexOf(";", accountKeyStartIndex, StringComparison.InvariantCultureIgnoreCase);
                var accountKeyLength = accountKeyEndIndex - accountKeyStartIndex;
                return connectionString.Substring(accountKeyStartIndex, accountKeyLength);
            }
            catch (Exception ex)
            {

            }


            return "";
        }

        public async Task<OpenGigRolesCV> SaveCVFile(IFormFile CVFile, object Id)
        {
            OpenGigRolesCV opengigroles = new OpenGigRolesCV();
            try
            {
                if (CVFile != null)
                {
                    string BlobStorageBaseUrl = string.Empty;
                    string CVPath = string.Empty;
                    string CVUrlWithSas = string.Empty;

                    string fileName = Guid.NewGuid().ToString() + "_" + CVFile.FileName;

                    // Get the Azure Blob Storage connection string from configuration
                    var connectionString = _configuration.GetConnectionString("AzureBlobStorage");

                    BlobServiceClient blobServiceClient = new BlobServiceClient(connectionString);
                    BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(ContainerName);

                    BlobClient blobClient = containerClient.GetBlobClient(fileName);

                    using (var stream = CVFile.OpenReadStream())
                    {
                        await blobClient.UploadAsync(stream, overwrite: true);
                    }

                    CVPath = blobClient.Uri.ToString();


                    string sasToken = GenerateSasToken(CVPath);


                    string cvUrlWithSas = CVPath + sasToken;



                    BlobStorageBaseUrl = containerClient.Uri.ToString();


                    CVUrlWithSas = cvUrlWithSas;

                    opengigroles.BlobStorageBaseUrl = BlobStorageBaseUrl;
                    opengigroles.CVPath = CVPath;
                    opengigroles.CVUrlWithSas = CVUrlWithSas;
                    opengigroles.ID = (int)(Id);
                    opengigroles.AlreadyExist = false;

                    return opengigroles;
                }
                else
                {
                    ModelState.AddModelError("ImageFile", "Please select a CV file.");
                }
            }
            catch (Exception ex)
            {

            }
            return opengigroles;


        }


        public async Task<UserCvFile> SaveUserCVFile(IFormFile CVFile, object Id)
        {
            UserCvFile opengigroles = new UserCvFile();
            try
            {
                if (CVFile != null && CVFile.Length > 0)
                {
                    string BlobStorageBaseUrl = string.Empty;
                    string CVPath = string.Empty;
                    string CVUrlWithSas = string.Empty;

                    string fileName = Guid.NewGuid().ToString() + "_" + CVFile.FileName;

                    // Get the Azure Blob Storage connection string from configuration
                    var connectionString = _configuration.GetConnectionString("AzureBlobStorage");

                    BlobServiceClient blobServiceClient = new BlobServiceClient(connectionString);
                    BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(ContainerName);

                    BlobClient blobClient = containerClient.GetBlobClient(fileName);

                    using (var stream = CVFile.OpenReadStream())
                    {
                        await blobClient.UploadAsync(stream, overwrite: true);
                    }

                    CVPath = blobClient.Uri.ToString();


                    string sasToken = GenerateSasToken(CVPath);


                    string cvUrlWithSas = CVPath + sasToken;



                    BlobStorageBaseUrl = containerClient.Uri.ToString();


                    CVUrlWithSas = cvUrlWithSas;

                    opengigroles.BlobStorageBaseUrl = BlobStorageBaseUrl;
                    opengigroles.CVPath = CVPath;
                    opengigroles.CVUrlWithSas = CVUrlWithSas;
                    opengigroles.UserId = (string)Id;

                    return opengigroles;
                }
                else
                {
                    ModelState.AddModelError("ImageFile", "Please select a CV file.");
                }
            }
            catch (Exception ex)
            {

            }
            return opengigroles;


        }


        [HttpGet]
        public async Task<string> GetApprovedList()
        {
            var userId = HttpContext.Session.GetString("LoggedUser");
            if (userId != null)
            {
                GetUserProfileRequestModel UserId = new GetUserProfileRequestModel();
                UserId.UserId = userId;
                var aprroveList = await _apiRepository.MakeApiCallAsync("api/Freelancer/ApprovedRolesList", HttpMethod.Get, UserId);
                return aprroveList;
            }
            return "Something Went Wrong";
        }

        [HttpGet]
        public async Task<string> GetRequestedList()
        {
            var userId = HttpContext.Session.GetString("LoggedUser");
            if (userId != null)
            {
                GetUserProfileRequestModel UserId = new GetUserProfileRequestModel();
                UserId.UserId = userId;
                var aprroveList = await _apiRepository.MakeApiCallAsync("api/Freelancer/GetRequestList", HttpMethod.Get, UserId);
                return aprroveList;
            }
            return "Something Went Wrong";
        }


        [HttpPost]
        public async Task<string> SaveMileStone([FromBody] MileStoneViewModel mileStone)
        {
            if (mileStone != null)
            {
                try
                {
                    var userId = HttpContext.Session.GetString("LoggedUser");
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

        [HttpPost]
        public async Task<string> UpdateIndustryOutline([FromBody] SolutionIndustryDetailsViewModel model)
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

        [HttpPost]
        public async Task<string> GetMiletoneList([FromBody] MileStoneDetailsViewModel model)
        {
            var userId = HttpContext.Session.GetString("LoggedUser");
            if (userId != null)
            {
                //GetUserProfileRequestModel UserId = new GetUserProfileRequestModel();
                model.UserId = userId;
                var aprroveList = await _apiRepository.MakeApiCallAsync("api/Freelancer/GetMiletoneList", HttpMethod.Post, model);
                return aprroveList;
            }
            return "User not found";
        }

        [HttpPost]
        public async Task<string> GetMileStoneById([FromBody] MileStoneIdViewModel model)
        {
            var milestonedata = await _apiRepository.MakeApiCallAsync("api/Freelancer/GetMileStoneById", HttpMethod.Post, model);
            return milestonedata;

        }


        [HttpPost]
        public async Task<string> GetIndustryList()
        {
            var userId = HttpContext.Session.GetString("LoggedUser");
            if (userId == null)
            {
                userId = "";
            }
            GetUserProfileRequestModel model = new GetUserProfileRequestModel();
            model.UserId = userId;
            var Industrydata = await _apiRepository.MakeApiCallAsync("api/Client/IndustriesListBasedonUserType", HttpMethod.Post, model);
            return Industrydata;

        }


        [HttpGet]
        public async Task<string> GetServicesList()
        {
            var Servicesdata = await _apiRepository.MakeApiCallAsync("api/Admin/ServiceList", HttpMethod.Get);
            return Servicesdata;

        }

        [HttpGet]
        public async Task<string> GetSolutionList()
        {
            var userId = HttpContext.Session.GetString("LoggedUser");
            if (userId == null)
            {
                userId = "";
            }
            GetUserProfileRequestModel model = new GetUserProfileRequestModel();
            model.UserId = userId;
            var Solutiondata = await _apiRepository.MakeApiCallAsync("api/Client/GetSolutionListBasedonType", HttpMethod.Post, model);

            return Solutiondata;

        }

        [HttpPost]
        public async Task<string> SavePoints([FromBody] SolutionPoints model)
        {
            if (model != null)
            {
                try
                {
                    var userId = HttpContext.Session.GetString("LoggedUser");
                    model.FreelancerId = userId;
                    model.ClientId = userId;
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

        [HttpPost]
        public async Task<string> GetPointsList([FromBody] MileStoneDetailsViewModel model)
        {
            var userId = HttpContext.Session.GetString("LoggedUser");
            if (userId != null)
            {
                model.UserId = userId;
                var pointsList = await _apiRepository.MakeApiCallAsync("api/Freelancer/GetPointsList", HttpMethod.Post, model);
                return pointsList;
            }
            return "Something Went Wrong";
        }


        [HttpPost]
        public async Task<string> DeletePoints([FromBody] MileStoneIdViewModel model)
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

        [HttpPost]
        public async Task<string> DeleteMileStone([FromBody] MileStoneIdViewModel model)
        {
            if (model.Id != 0)
            {
                var pointsData = await _apiRepository.MakeApiCallAsync("api/Freelancer/DeleteMileStoneById", HttpMethod.Post, model);
                return pointsData;

            }
            else
            {
                return "failed to receive data..";
            }
        }

        public IActionResult DownloadApplicantCV(string Cvname)
        {
            string connectionString = _configuration.GetConnectionString("AzureBlobStorage");
            string containerName = ContainerName;
            try
            {
                string blobName = Path.GetFileName(Cvname);
                BlobContainerClient container = new BlobContainerClient(connectionString, containerName);
                BlobClient blob = container.GetBlobClient(blobName);
                if (!blob.Exists())
                {
                    return NotFound();
                }
                BlobDownloadInfo downloadInfo = blob.Download();
                var stream = new MemoryStream();
                downloadInfo.Content.CopyTo(stream);
                stream.Position = 0;
                string suggestedFileName = blobName;
                Response.Headers.Add("Content-Disposition", new Microsoft.Extensions.Primitives.StringValues(new[] { "attachment; filename=" + suggestedFileName }));
                return new FileStreamResult(stream, "pdf/application");
            }
            catch (Exception ex)
            {
                return NotFound();
            }
        }

        private string GenerateImageSasToken(string imageUrl)
        {
            // Get the blob container name and blob name from the image URL
            string blobName = Path.GetFileName(imageUrl);

            // Create a shared access policy that allows read access to the blob
            BlobSasBuilder sasBuilder = new BlobSasBuilder()
            {
                BlobContainerName = ImageContainerName,
                BlobName = blobName,
                Resource = "b",
                StartsOn = DateTime.UtcNow.AddMinutes(-5), // Adjust the start time if needed
                ExpiresOn = DateTime.UtcNow.AddMinutes(10), // Adjust the expiry time if needed,
                Protocol = SasProtocol.Https
            };
            sasBuilder.SetPermissions(BlobSasPermissions.Read);

            // Get the Azure Blob Storage connection string from configuration
            var connectionString = _configuration.GetConnectionString("AzureBlobStorage");

            // Extract the AccountName and AccountKey from the connection string
            var accountName = GetAccountNameFromConnectionString(connectionString);
            var accountKey = GetAccountKeyFromConnectionString(connectionString);

            // Create a StorageSharedKeyCredential using the AccountName and AccountKey
            StorageSharedKeyCredential credential = new StorageSharedKeyCredential(accountName, accountKey);

            // Generate the SAS token
            string sasToken = sasBuilder.ToSasQueryParameters(credential).ToString();

            return sasToken;
        }

        [HttpGet]
        public async Task<string> BindPopularSolutionsList()
        {
            var userId = HttpContext.Session.GetString("LoggedUser");
            if (userId == null)
            {
                userId = "";
            }
            GetUserProfileRequestModel model = new GetUserProfileRequestModel();
            model.UserId = userId;
            var solutionList = await _apiRepository.MakeApiCallAsync("api/Client/GetPopularSolutionList", HttpMethod.Post, model);
            dynamic data = JsonConvert.DeserializeObject(solutionList);
            try
            {
                if (data.Result != null)
                {
                    foreach (var service in data.Result)
                    {
                        string imagepath = service.ImagePath;
                        string sasToken = GenerateImageSasToken(imagepath);
                        string imageUrlWithSas = $"{service.ImagePath}?{sasToken}";
                        service.ImageUrlWithSas = imageUrlWithSas;

                    }

                }

            }
            catch (Exception ex)
            {
                return ex.Message + ex.InnerException;
            }
            string jsonString = JsonConvert.SerializeObject(data, Formatting.Indented);
            return jsonString;
        }


        [HttpPost]
        public async Task<string> GetSolutionBasedonServices([FromBody] MileStoneIdViewModel model)
        {
            if (model != null)
            {
                var userId = HttpContext.Session.GetString("LoggedUser");
                if (userId == null)
                {
                    userId = "";
                }
                model.UserId = userId;
                var solutionData = await _apiRepository.MakeApiCallAsync("api/Client/GetPopularSolutionBasedOnServices", HttpMethod.Post, model);
                dynamic data = JsonConvert.DeserializeObject(solutionData);
                try
                {
                    if (data.Result != null)
                    {
                        foreach (var service in data.Result.SolutionData)
                        {
                            string imagepath = service.ImagePath;
                            string sasToken = GenerateImageSasToken(imagepath);
                            string imageUrlWithSas = $"{service.ImagePath}?{sasToken}";
                            service.ImageUrlWithSas = imageUrlWithSas;

                        }

                    }

                }
                catch (Exception ex)
                {
                    return ex.Message + ex.InnerException;
                }
                string jsonString = JsonConvert.SerializeObject(data, Formatting.Indented);
                return jsonString;

            }
            else
            {
                return "failed to receive data..";
            }
        }


        [HttpPost]
        public async Task<string> GetSolutionBasedonSolutionSelected([FromBody] MileStoneIdViewModel model)
        {
            if (model != null)
            {
                var userId = HttpContext.Session.GetString("LoggedUser");
                if (userId == null)
                {
                    userId = "";
                }
                model.UserId = userId;
                var solutionData = await _apiRepository.MakeApiCallAsync("api/Client/GetPopularSolutionBasedOnSolutionSelected", HttpMethod.Post, model);
                dynamic data = JsonConvert.DeserializeObject(solutionData);
                try
                {
                    if (data.Result != null)
                    {
                        foreach (var service in data.Result.SolutionData)
                        {
                            string imagepath = service.ImagePath;
                            string sasToken = GenerateImageSasToken(imagepath);
                            string imageUrlWithSas = $"{service.ImagePath}?{sasToken}";
                            service.ImageUrlWithSas = imageUrlWithSas;

                        }

                    }

                }
                catch (Exception ex)
                {
                    return ex.Message + ex.InnerException;
                }
                string jsonString = JsonConvert.SerializeObject(data, Formatting.Indented);
                return jsonString;




            }
            else
            {
                return "failed to receive data..";
            }
        }

        [HttpPost]
        public async Task<string> GetSolutionDefineData([FromBody] MileStoneDetailsViewModel model)
        {
            var userId = HttpContext.Session.GetString("LoggedUser");
            if (userId != null)
            {
                //model.FreelancerId = userId;
                var aprroveList = await _apiRepository.MakeApiCallAsync("api/Freelancer/GetSolutionDefineData", HttpMethod.Post, model);
                return aprroveList;
            }
            return "Something Went Wrong";
        }


        [HttpPost]
        public async Task<string> GetProjectDetailsbasedOnType([FromBody] MileStoneDetailsViewModel model)
        {
            if (model != null)
            {
                var userId = HttpContext.Session.GetString("LoggedUser");
                var preferredCurrency = HttpContext.Session.GetString("ClientPreferredCurrency");
                if (userId == null)
                {
                    userId = "";

                }
                model.UserId = userId;
                model.ClientPreferredCurrency = preferredCurrency;
                var solutionData = await _apiRepository.MakeApiCallAsync("api/Client/GetSolutionDetailsInProject", HttpMethod.Post, model);
                dynamic data = JsonConvert.DeserializeObject(solutionData);
                try
                {
                    if (data.Result != null)
                    {
                        foreach (var service in data.Result.TopProfessional)
                        {
                            if (service.ImagePath != null)
                            {
                                string imagepath = service.ImagePath;
                                string sasToken = GenerateImageSasToken(imagepath);
                                string imageUrlWithSas = $"{service.ImagePath}?{sasToken}";
                                service.ImageUrlWithSas = imageUrlWithSas;
                            }


                        }

                    }

                }
                catch (Exception ex)
                {
                    return ex.Message + ex.InnerException;
                }
                string jsonString = JsonConvert.SerializeObject(data, Formatting.Indented);
                return jsonString;
            }
            else
            {
                return "failed to receive data..";
            }
        }


        [HttpPost]
        public async Task<string> GetActiveProjectDetails([FromBody] MileStoneDetailsViewModel model)
        {
            if (model != null)
            {
                var userId = HttpContext.Session.GetString("LoggedUser");
                var preferredCurrency = HttpContext.Session.GetString("ClientPreferredCurrency");
                if (userId == null)
                {
                    userId = "";

                }
                model.UserId = userId;
                model.ClientPreferredCurrency = preferredCurrency;
                var solutionData = await _apiRepository.MakeApiCallAsync("api/Client/GetActiveSolutionDetailsInProject", HttpMethod.Post, model);
                dynamic data = JsonConvert.DeserializeObject(solutionData);
                try
                {
                    if (data.Result != null)
                    {
                        foreach (var service in data.Result.SolutionTeam)
                        {
                            if (service.ImagePath != null)
                            {
                                string imagepath = service.ImagePath;
                                string sasToken = GenerateImageSasToken(imagepath);
                                string imageUrlWithSas = $"{service.ImagePath}?{sasToken}";
                                service.ImageUrlWithSas = imageUrlWithSas;
                            }


                        }

                        foreach (var service in data.Result.DocumentDataList)
                        {
                            if (service.DocumentPath != null)
                            {
                                string documentpath = service.DocumentPath;
                                string sasToken = GenerateSasToken(documentpath);
                                string documentUrlWithSas = $"{service.DocumentPath}?{sasToken}";
                                service.DocumentUrlWithSas = documentUrlWithSas;
                            }


                        }

                    }

                }
                catch (Exception ex)
                {
                    return ex.Message + ex.InnerException;
                }
                string jsonString = JsonConvert.SerializeObject(data, Formatting.Indented);
                return jsonString;
            }
            else
            {
                return "failed to receive data..";
            }
        }

        [HttpPost]
        public async Task<string> SaveSolutionReviewData([FromBody] SolutionReview model)
        {
            if (model != null)
            {
                var clientId = HttpContext.Session.GetString("LoggedUser");
                if (clientId != null)
                {
                    model.ClientId = clientId;
                    model.CreateDateTime = DateTime.Now;
                    var response = await _apiRepository.MakeApiCallAsync("api/Admin/SaveProjectReview", HttpMethod.Post, model);
                    return response;
                }
            }
            return "Failed to submit feedback !!";
        }

        [HttpPost]
        public async Task<string> GetChatResponse([FromBody] ChatPopupRequestViewModel model)
        {
            model.UserRole = HttpContext.Session.GetString("LoggedUserRole");
            model.LoginFreelancerId = HttpContext.Session.GetString("LoggedUser");
            var freelanceChatdata = await _apiRepository.MakeApiCallAsync("api/Freelancer/GetChatResponse", HttpMethod.Post, model);
            return freelanceChatdata;
        }

        [HttpPost]
        public async Task<string> SaveFreelancerReviewData([FromBody] SolutionTeamReview model)
        {
            if (model != null)
            {
                var clientId = HttpContext.Session.GetString("LoggedUser");
                if (clientId != null)
                {
                    model.ClientId = clientId;
                    model.CreateDateTime = DateTime.Now;
                    var response = await _apiRepository.MakeApiCallAsync("api/Admin/SaveFreelancerReview", HttpMethod.Post, model);
                    return response;
                }
            }
            return "Failed to submit feedback !!";
        }

        [HttpPost]
        public async Task<string> checkSolutionReviewExists([FromBody] SolutionReview model)
        {
            if (model != null)
            {
                var clientId = HttpContext.Session.GetString("LoggedUser");
                if (clientId != null)
                {
                    model.ClientId = clientId;
                    var response = await _apiRepository.MakeApiCallAsync("api/Admin/GetProjectReviewById", HttpMethod.Post, model);
                    return response;
                }
            }
            return "Failed to submit feedback !!";
        }

        [HttpPost]
        public async Task<string> checkSolutionTeamReviewExists([FromBody] SolutionTeamReview model)
        {
            if (model != null)
            {
                var clientId = HttpContext.Session.GetString("LoggedUser");
                if (clientId != null)
                {
                    model.ClientId = clientId;
                    var response = await _apiRepository.MakeApiCallAsync("api/Admin/GetFreelancerReviewById", HttpMethod.Post, model);
                    return response;
                }
            }
            return "Failed to submit feedback !!";
        }

        [HttpPost]
        public async Task<string> GetProjectDetails([FromBody] MileStoneDetailsViewModel model)
        {
            if (model != null)
            {

                var solutionData = await _apiRepository.MakeApiCallAsync("api/Client/GetProjectDetails", HttpMethod.Post, model);
                dynamic data = JsonConvert.DeserializeObject(solutionData);
                try
                {
                    if (data.Result != null)
                    {

                        string imagepath = data.Result.ProjectData.ImagePath;
                        string sasToken = GenerateImageSasToken(imagepath);
                        string imageUrlWithSas = $"{data.Result.ProjectData.ImagePath}?{sasToken}";
                        data.Result.ProjectData.ImageUrlWithSas = imageUrlWithSas;



                    }

                }
                catch (Exception ex)
                {
                    return ex.Message + ex.InnerException;
                }
                string jsonString = JsonConvert.SerializeObject(data, Formatting.Indented);
                return jsonString;




            }
            else
            {
                return "failed to receive data..";
            }
        }

        [HttpPost]
        public async Task<string> DeleteFreelancerSolution([FromBody] MileStoneDetailsViewModel model)
        {
            if (model != null)
            {
                var userId = HttpContext.Session.GetString("LoggedUser");
                model.FreelancerId = userId;
                var freelancerData = await _apiRepository.MakeApiCallAsync("api/Freelancer/DeleteFreelancerSolution", HttpMethod.Post, model);
                return freelancerData;
            }
            else
            {
                return "failed to receive data..";
            }
        }


        [HttpGet]
        public async Task<string> BindTopThreePopularSolutionsList()
        {
            var userId = HttpContext.Session.GetString("LoggedUser");
            if (userId == null)
            {
                userId = "";
            }
            GetUserProfileRequestModel model = new GetUserProfileRequestModel();
            model.UserId = userId;
            var solutionList = await _apiRepository.MakeApiCallAsync("api/Client/GetTopThreePopularSolutionsList", HttpMethod.Post, model);
            dynamic data = JsonConvert.DeserializeObject(solutionList);
            try
            {
                if (data.Result != null)
                {
                    foreach (var service in data.Result.SolutionData)
                    {
                        string imagepath = service.ImagePath;
                        string sasToken = GenerateImageSasToken(imagepath);
                        string imageUrlWithSas = $"{service.ImagePath}?{sasToken}";
                        service.ImageUrlWithSas = imageUrlWithSas;

                    }

                }

            }
            catch (Exception ex)
            {
                return ex.Message + ex.InnerException;
            }
            string jsonString = JsonConvert.SerializeObject(data, Formatting.Indented);
            return jsonString;
        }

        [HttpPost]
        public async Task<string> changeSolutionsListByPagination([FromBody] MileStoneIdViewModel model)
        {
            var userId = HttpContext.Session.GetString("LoggedUser");
            if (userId == null)
            {
                userId = "";
            }
            model.UserId = userId;
            var solutionList = await _apiRepository.MakeApiCallAsync("api/Client/changeSolutionListByPagination", HttpMethod.Post, model);
            dynamic data = JsonConvert.DeserializeObject(solutionList);
            try
            {
                if (data.Result != null)
                {
                    foreach (var service in data.Result.SolutionData)
                    {
                        string imagepath = service.ImagePath;
                        string sasToken = GenerateImageSasToken(imagepath);
                        string imageUrlWithSas = $"{service.ImagePath}?{sasToken}";
                        service.ImageUrlWithSas = imageUrlWithSas;

                    }

                }

            }
            catch (Exception ex)
            {
                return ex.Message + ex.InnerException;
            }
            string jsonString = JsonConvert.SerializeObject(data, Formatting.Indented);
            return jsonString;
        }

        [HttpPost]
        public async Task<string> GetTopThreeSolutionBasedonServices([FromBody] MileStoneIdViewModel model)
        {
            if (model != null)
            {
                var userId = HttpContext.Session.GetString("LoggedUser");
                if (userId == null)
                {
                    userId = "";
                }
                model.UserId = userId;
                var solutionData = await _apiRepository.MakeApiCallAsync("api/Client/GetTopThreePopularSolutionBasedOnServices", HttpMethod.Post, model);
                dynamic data = JsonConvert.DeserializeObject(solutionData);
                try
                {
                    if (data.Result != null)
                    {
                        foreach (var service in data.Result.SolutionData)
                        {
                            string imagepath = service.ImagePath;
                            string sasToken = GenerateImageSasToken(imagepath);
                            string imageUrlWithSas = $"{service.ImagePath}?{sasToken}";
                            service.ImageUrlWithSas = imageUrlWithSas;

                        }

                    }

                }
                catch (Exception ex)
                {
                    return ex.Message + ex.InnerException;
                }
                string jsonString = JsonConvert.SerializeObject(data, Formatting.Indented);
                return jsonString;

            }
            else
            {
                return "failed to receive data..";
            }
        }


        [HttpPost]
        public async Task<string> GetPointsDataById([FromBody] MileStoneIdViewModel model)
        {
            var pointsdata = await _apiRepository.MakeApiCallAsync("api/Freelancer/GetPointsDataById", HttpMethod.Post, model);
            return pointsdata;

        }


        [HttpGet]
        public async Task<string> GetAllUnReadNotification()
        {
            var userId = HttpContext.Session.GetString("LoggedUser");
            if (userId == null)
            {
                return "No Data Found";
            }
            MileStoneIdViewModel model = new MileStoneIdViewModel();
            model.UserId = userId;
            var notificationdata = await _apiRepository.MakeApiCallAsync("api/Freelancer/GetAllUnReadNotification", HttpMethod.Post, model);
            return notificationdata;

        }

        [HttpGet]
        public async Task<string> GetAllNotification()
        {
            var userId = HttpContext.Session.GetString("LoggedUser");
            if (userId == null)
            {
                return "No Data Found";
            }
            MileStoneIdViewModel model = new MileStoneIdViewModel();
            model.UserId = userId;
            var notificationdata = await _apiRepository.MakeApiCallAsync("api/Freelancer/GetAllNotification", HttpMethod.Post, model);
            return notificationdata;

        }

        [HttpPost]
        public async Task<string> SetNotificationIsRead([FromBody] MileStoneIdViewModel model)
        {
            var userId = HttpContext.Session.GetString("LoggedUser");
            if (userId == null)
            {
                return "No Data Found";
            }
            model.UserId = userId;
            var data = await _apiRepository.MakeApiCallAsync("api/Freelancer/SetNotificationIsRead", HttpMethod.Post, model);
            return data;

        }

        //SaveProject
        [HttpPost]
        public async Task<string> SaveProject([FromBody] MileStoneDetailsViewModel model)
        {
            var userId = HttpContext.Session.GetString("LoggedUser");
            if (userId == null)
            {
                return "No Data Found";
            }
            model.UserId = userId;
            var data = await _apiRepository.MakeApiCallAsync("api/Freelancer/SaveProject", HttpMethod.Post, model);
            return data;

        }

        [HttpGet]
        public async Task<string> GetSavedProjectList()
        {
            var userId = HttpContext.Session.GetString("LoggedUser");
            if (userId == null)
            {
                return "No Data Found";
            }
            MileStoneIdViewModel model = new MileStoneIdViewModel();
            model.UserId = userId;
            var projectData = await _apiRepository.MakeApiCallAsync("api/Freelancer/GetSavedProjectList", HttpMethod.Post, model);
            dynamic data = JsonConvert.DeserializeObject(projectData);
            try
            {
                if (data.Result != null)
                {
                    foreach (var service in data.Result)
                    {
                        string imagepath = service.ImagePath;
                        if (imagepath != null)
                        {
                            string sasToken = GenerateImageSasToken(imagepath);
                            string imageUrlWithSas = $"{service.ImagePath}?{sasToken}";
                            service.ImageUrlWithSas = imageUrlWithSas;

                        }

                    }

                }

            }
            catch (Exception ex)
            {
                return ex.Message + ex.InnerException;
            }
            string jsonString = JsonConvert.SerializeObject(data, Formatting.Indented);
            return jsonString;

        }


        [HttpPost]
        public async Task<string> UnSavedProject([FromBody] MileStoneDetailsViewModel model)
        {
            var userId = HttpContext.Session.GetString("LoggedUser");
            if (userId == null)
            {
                return "No Data Found";
            }
            model.UserId = userId;
            var data = await _apiRepository.MakeApiCallAsync("api/Freelancer/UnSavedProject", HttpMethod.Post, model);
            return data;
        }

        [HttpGet]
        public async Task<string> GetAllSavedProjectList()
        {
            var userId = HttpContext.Session.GetString("LoggedUser");
            if (userId == null)
            {
                return "No Data Found";
            }
            MileStoneIdViewModel model = new MileStoneIdViewModel();
            model.UserId = userId;
            var projectData = await _apiRepository.MakeApiCallAsync("api/Freelancer/GetAllSavedProjectList", HttpMethod.Post, model);
            dynamic data = JsonConvert.DeserializeObject(projectData);
            try
            {
                if (data.Result != null)
                {
                    foreach (var service in data.Result)
                    {
                        string imagepath = service.ImagePath;
                        if (imagepath != null)
                        {
                            string sasToken = GenerateImageSasToken(imagepath);
                            string imageUrlWithSas = $"{service.ImagePath}?{sasToken}";
                            service.ImageUrlWithSas = imageUrlWithSas;

                        }

                    }

                }

            }
            catch (Exception ex)
            {
                return ex.Message + ex.InnerException;
            }
            string jsonString = JsonConvert.SerializeObject(data, Formatting.Indented);
            return jsonString;

        }

        //RegisterStripeUser
        [HttpGet]
        public async Task RegisterStripeUser()
        {
            var userId = HttpContext.Session.GetString("LoggedUser");
            if (userId == null)
            {
                Response.Redirect("UserProfile");
            }
            MileStoneIdViewModel model = new MileStoneIdViewModel();
            model.UserId = userId;
            var userData = await _apiRepository.MakeApiCallAsync("api/Client/CreateUserStripeAccount", HttpMethod.Post, model);
            dynamic data = JsonConvert.DeserializeObject(userData);
            try
            {
                if (data.Result != null)
                {
                    StripeConfiguration.ApiKey = "sk_test_51NaxGxLHv0zYK8g4ZEh9KncjP5T6hbERI8VIn5bKUZvuY36xCSfp99bdrH5Td65cXkJ5FgDdMFVbmAao6xfm8Wje00pAJrWOjf";
                    if (data.Result.StripeAccountStatus == ApplicationUser.StripeAccountStatuses.Initiated)
                    {
                        var accountLinkOptions = new AccountLinkCreateOptions
                        {
                            Account = data.Result.StripeConnectedId,
                            RefreshUrl = _configuration.GetValue<string>("StripeApiUrl:RefreshUrl"),
                            ReturnUrl = _configuration.GetValue<string>("StripeApiUrl:ReturnUrl"),
                            Type = "account_onboarding"
                        };

                        var accountLinkService = new AccountLinkService();
                        var accountLinks = accountLinkService.Create(accountLinkOptions);

                        Response.Redirect(accountLinks.Url);
                    }
                }

            }
            catch (Exception ex)
            {
                Response.Redirect("UserProfile");
            }

        }

        public ActionResult StripeWelcome()
        {
            return View();
        }

        [HttpGet]
        public async Task GetUserStripeDetails()
        {
            var userId = HttpContext.Session.GetString("LoggedUser");
            if (userId == null)
            {
                Response.Redirect("UserProfile");
            }
            MileStoneIdViewModel model = new MileStoneIdViewModel();
            model.UserId = userId;
            var userData = await _apiRepository.MakeApiCallAsync("api/Client/GetUserStripeDetails", HttpMethod.Post, model);
            dynamic data = JsonConvert.DeserializeObject(userData);
            try
            {
                if (data.Result != null)
                {
                    if (data.Result.IsCompleted == true)
                    {
                        //HttpContext.Session.SetString("StripeStatus", "Created Successfully!");
                        Response.Redirect("UserProfile");
                    }
                    else
                    {
                        StripeConfiguration.ApiKey = "sk_test_51NaxGxLHv0zYK8g4ZEh9KncjP5T6hbERI8VIn5bKUZvuY36xCSfp99bdrH5Td65cXkJ5FgDdMFVbmAao6xfm8Wje00pAJrWOjf";
                        if (data.Result.UserDetails.StripeAccountStatus == ApplicationUser.StripeAccountStatuses.Initiated)
                        {
                            var accountLinkOptions = new AccountLinkCreateOptions
                            {
                                Account = data.Result.UserDetails.StripeConnectedId,
                                RefreshUrl = _configuration.GetValue<string>("StripeApiUrl:RefreshUrl"),
                                ReturnUrl = _configuration.GetValue<string>("StripeApiUrl:ReturnUrl"),
                                Type = "account_onboarding"
                            };

                            var accountLinkService = new AccountLinkService();
                            var accountLinks = accountLinkService.Create(accountLinkOptions);
                            Response.Redirect(accountLinks.Url);
                        }

                    }

                }

            }
            catch (Exception ex)
            {
                Response.Redirect("UserProfile");
            }

        }

        //GetActiveProjectList
        [HttpGet]
        public async Task<string> GetActiveProjectList()
        {
            var userId = HttpContext.Session.GetString("LoggedUser");
            if (userId == null)
            {
                return "No Data Found";
            }
            MileStoneIdViewModel model = new MileStoneIdViewModel();
            model.UserId = userId;
            var projectData = await _apiRepository.MakeApiCallAsync("api/Freelancer/GetActiveProjectList", HttpMethod.Post, model);
            dynamic data = JsonConvert.DeserializeObject(projectData);
            try
            {
                if (data.Result != null)
                {
                    foreach (var service in data.Result)
                    {
                        string imagepath = service.ImagePath;
                        if (imagepath != null)
                        {
                            string sasToken = GenerateImageSasToken(imagepath);
                            string imageUrlWithSas = $"{service.ImagePath}?{sasToken}";
                            service.ImageUrlWithSas = imageUrlWithSas;

                        }

                    }

                }

            }
            catch (Exception ex)
            {
                return ex.Message + ex.InnerException;
            }
            string jsonString = JsonConvert.SerializeObject(data, Formatting.Indented);
            return jsonString;

        }

        //GetCountryList
        [HttpGet]
        public async Task<string> GetCountryList()
        {
            var countryData = await _apiRepository.MakeApiCallAsync("api/Freelancer/GetCountryList", HttpMethod.Get);
            return countryData;
        }

        public async Task<IActionResult> DownloadInvoice(int InvoiceId)
        {
            var invoiceDetails = "";
            SolutionFundModel solutionFund = new SolutionFundModel();
            var userId = HttpContext.Session.GetString("LoggedUser");
            var userRole = HttpContext.Session.GetString("LoggedUserRole");
            solutionFund.InvoiceId = InvoiceId;
            solutionFund.UserId = userId;
            if (userRole == "Freelancer")
            {
                invoiceDetails = await _apiRepository.MakeApiCallAsync("api/Freelancer/GetFreelancerInvoiceDetails", HttpMethod.Post, solutionFund);

            }
            else
            {
                invoiceDetails = await _apiRepository.MakeApiCallAsync("api/Freelancer/GetInvoiceDetails", HttpMethod.Post, solutionFund);
            }
            //var invoiceDetails = await _apiRepository.MakeApiCallAsync("api/Freelancer/GetInvoiceDetails", HttpMethod.Post, solutionFund);
            dynamic datas = JObject.Parse(invoiceDetails);
            var Details = datas.Result;
            var FundDetails = datas.Result.FundType;

            float cellHeight = 100f;
            Document document = new Document();
            var memoryStream = new MemoryStream();
            PdfWriter writer = PdfWriter.GetInstance(document, memoryStream);

            document.Open();
            PdfPTable centertable = new PdfPTable(1);
            PdfPCell centerCell = new PdfPCell(new Phrase(" "));
            centerCell.HorizontalAlignment = Element.ALIGN_CENTER;
            centerCell.Padding = 7.5f;

            IPdfPCellEvent roundedRectangle = new RoundedCellEvent(5, new BaseColor(System.Drawing.Color.Gray)); // Specify corner radius and fill color
            centerCell.Border = Rectangle.NO_BORDER;
            centerCell.PaddingBottom = 5f;
            centertable.DefaultCell.Border = 0;
            centertable.AddCell(centerCell);


            PdfPTable righttable = new PdfPTable(1);
            PdfPCell rightCell = new PdfPCell();
            rightCell.HorizontalAlignment = Element.ALIGN_RIGHT;
            rightCell.Border = Rectangle.NO_BORDER;
            righttable.DefaultCell.Border = 0;
            iTextSharp.text.Image logo = iTextSharp.text.Image.GetInstance("https://aephyweb.azurewebsites.net/assets/img/ephylink_no_background_blue.png");


            logo.ScaleAbsolute(70f, 70f);
            rightCell.PaddingLeft = 80f;
            rightCell.PaddingBottom = -75f;
            rightCell.PaddingTop = 0f;
            rightCell.PaddingRight = 0f;

            logo.SetAbsolutePosition(iTextSharp.text.PageSize.A4.Rotate().Width - 0, 20);
            rightCell.AddElement(logo);


            righttable.AddCell(rightCell);
            AddClientTables(document, writer, Details);
            document.Add(new Paragraph("\r\n  "));
            AddTwoContentTables(document, writer, Details);
            document.Add(new Paragraph("\r\n  "));
            AddSingleTableWithCustomColumnWidths(document, writer, Details, FundDetails);
            document.NewPage();
            document.NewPage();
            document.Close();
            Response.ContentType = MediaTypeNames.Application.Pdf;
            var contentDisposition = new ContentDisposition
            {
                FileName = "Inv-" + Details.InvoiceNumber + ".pdf",
                Inline = false
            };
            Response.Headers.Add("Content-Disposition", contentDisposition.ToString());
            return File(memoryStream.ToArray(), Response.ContentType);
        }


        private void AddClientTables(Document doc, PdfWriter writer, dynamic data)
        {
            //dynamic datas = JObject.Parse(data);
            //var Details = datas.Result;

            BaseFont baseF14 = BaseFont.CreateFont();

            // Master Table: Contains two side-by-side tables
            PdfPTable masterTable = new PdfPTable(2);
            masterTable.WidthPercentage = 100;
            masterTable.SetWidths(new float[] { 68f, 30f });
            masterTable.DefaultCell.Border = 3;

            // Left Table: Display the logo
            PdfPTable leftTable = new PdfPTable(1);
            leftTable.HorizontalAlignment = Element.ALIGN_LEFT;
            float[] widths = new float[] { 40f };

            leftTable.SetWidths(widths);
            iTextSharp.text.Image img = iTextSharp.text.Image.GetInstance("https://aephyweb.azurewebsites.net/assets/img/ephylink_no_background_blue.png");
            img.ScaleToFit(150f, 150f);
            PdfPCell logoCell = new PdfPCell(img);
            logoCell.HorizontalAlignment = Element.ALIGN_LEFT;
            logoCell.Border = PdfPCell.NO_BORDER;
            leftTable.AddCell(logoCell);

            // Right Table: Labels and Text Fields
            PdfPTable rightTable = new PdfPTable(2);
            // rightTable.WidthPercentage = 30;
            float[] widths1 = new float[] { 0f, 100f };
            rightTable.SetWidths(widths1);
            rightTable.HorizontalAlignment = Element.ALIGN_RIGHT;

            // Set the font for the entire table
            rightTable.DefaultCell.Phrase = new Phrase() { Font = FontFactory.GetFont(FontFactory.HELVETICA, 8) };

            // Create label cells and set font styles
            PdfPCell labelCell1 = new PdfPCell(new Phrase("               Invoice                   ", FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 11)));
            labelCell1.BackgroundColor = new BaseColor(0xD9, 0xD9, 0xD9); // Set background color
            labelCell1.VerticalAlignment = Element.ALIGN_MIDDLE; // Center the text vertically
            labelCell1.Colspan = 5;
            labelCell1.FixedHeight = 23; ;

            //labelCell1.Colspan = 2;
            //labelCell1.PaddingLeft = 40;


            PdfPCell labelCell6 = new PdfPCell(new Phrase("Invoice  # " + data.InvoiceNumber, FontFactory.GetFont(FontFactory.HELVETICA, 8)));
            PdfPCell labelCell2 = new PdfPCell(new Phrase("Date " + data.InvoiceDate.ToString("dd MMMM yyyy"), FontFactory.GetFont(FontFactory.HELVETICA, 8)));
            PdfPCell labelCell3 = new PdfPCell(new Phrase("Due Date " + data.InvoiceDate.ToString("dd MMMM yyyy"), FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 8))); // Bold font for "Due Date" label
            PdfPCell labelCell4 = new PdfPCell(new Phrase("Total Amount " + data.PreferredCurrency + data.TotalAmount, FontFactory.GetFont(FontFactory.HELVETICA, 8)));
            PdfPCell labelCell5 = new PdfPCell(new Phrase("Total Due " + data.TotalAmount, FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 8)));

            // Create textField cells
            PdfPCell textFieldCell1 = new PdfPCell(); // You need to add a text field here
            PdfPCell textFieldCell2 = new PdfPCell(); // You need to add a text field here
            PdfPCell textFieldCell3 = new PdfPCell(); // You need to add a text field here
            PdfPCell textFieldCell4 = new PdfPCell(); // You need to add a text field here
            PdfPCell textFieldCell5 = new PdfPCell(); // You need to add a text field here
            PdfPCell textFieldCell6 = new PdfPCell(); // You need to add a text field here

            // Set cell properties
            PdfPCell[] cells = { labelCell1, textFieldCell1, labelCell6, textFieldCell6, labelCell2, textFieldCell2, labelCell3, textFieldCell3, labelCell4, textFieldCell4, labelCell5, textFieldCell5 };

            foreach (PdfPCell cell in cells)
            {
                cell.Border = PdfPCell.NO_BORDER;
                rightTable.AddCell(cell);
            }

            // Add the left and right tables to the master table
            PdfPCell leftCell = new PdfPCell(leftTable);
            leftCell.Border = PdfPCell.NO_BORDER;

            PdfPCell rightCell = new PdfPCell(rightTable);
            rightCell.Border = PdfPCell.NO_BORDER;

            masterTable.AddCell(leftCell);
            masterTable.AddCell(rightCell);

            // Add the master table to the document
            doc.Add(masterTable);
        }

        private void AddTwoContentTables(Document doc, PdfWriter writer, dynamic data)
        {
            // Master Table: Contains two side-by-side tables
            PdfPTable masterTable = new PdfPTable(2);
            masterTable.WidthPercentage = 100;

            // Table 1: Content 1
            PdfPTable table1 = new PdfPTable(1);
            table1.WidthPercentage = 100;
            table1.HorizontalAlignment = Element.ALIGN_LEFT;

            // Content 1
            string content1 = "From\nEphylink\nDimitrios Vamvakas\nAristofanous 5, Patras, 26500\nAchaia, Greece\nVAT ID: 148366653";

            // Split content1 into lines
            string[] content1Lines = content1.Split('\n');

            foreach (string line in content1Lines)
            {
                Font font = FontFactory.GetFont(FontFactory.HELVETICA, 9);
                if (line == "From")
                {
                    font.Color = new BaseColor(195, 120, 19);
                    font.Size = 14;


                }
                if (line == "Ephylink")
                {
                    PdfPCell spaceCell1 = new PdfPCell();
                    spaceCell1.Border = 0;
                    spaceCell1.FixedHeight = 6f;
                    table1.AddCell(spaceCell1);
                    font.Size = 9;
                    font.SetStyle(Font.BOLD);

                }

                Chunk chunk = new Chunk(line, font);
                PdfPCell contentCell1 = new PdfPCell(new Phrase(chunk));
                contentCell1.Border = PdfPCell.NO_BORDER;
                table1.AddCell(contentCell1);

                PdfPCell spaceCell2 = new PdfPCell();
                spaceCell2.Border = 0;
                spaceCell2.FixedHeight = 3f;
                table1.AddCell(spaceCell2);


            }

            // Table 2: Content 2
            PdfPTable table2 = new PdfPTable(1);
            table2.WidthPercentage = 100;
            table2.HorizontalAlignment = Element.ALIGN_LEFT;

            // Content 2
            var taxDetails = data.TaxType.ToString();
            string content2 = string.Empty;
            if (taxDetails != null && taxDetails != "" && taxDetails != "null")
            {
                content2 = "Bill to\n" + data.ClientFullName + "\nName \nAddress: " + data.ClientAddress + "\n" + data.TaxType.ToString() + " ID: " + data.TaxId.ToString();
            }
            else
            {
                content2 = "Bill to\n" + data.ClientFullName + "\nName \nAddress: " + data.ClientAddress + "\n";
            }


            // Split content2 into lines
            string[] content2Lines = content2.Split('\n');

            foreach (string line in content2Lines)
            {
                Font font = FontFactory.GetFont(FontFactory.HELVETICA, 9);
                if (line == "Bill to")
                {
                    font.Color = new BaseColor(195, 120, 19);
                    font.Size = 14;
                }
                if (line == "Client A")
                {
                    PdfPCell spaceCell3 = new PdfPCell();
                    spaceCell3.Border = 0;
                    spaceCell3.FixedHeight = 6f;
                    table2.AddCell(spaceCell3);
                    font.SetStyle(Font.BOLD);
                    font.Size = 9;
                }
                Chunk chunk = new Chunk(line, font);
                PdfPCell contentCell2 = new PdfPCell(new Phrase(chunk));
                contentCell2.Border = PdfPCell.NO_BORDER;
                table2.AddCell(contentCell2);

                PdfPCell spaceCell4 = new PdfPCell();
                spaceCell4.Border = 0;
                spaceCell4.FixedHeight = 3f;
                table2.AddCell(spaceCell4);
            }

            // Add the left and right tables to the master table
            PdfPCell leftCell = new PdfPCell(table1);
            leftCell.Border = PdfPCell.NO_BORDER;

            PdfPCell rightCell = new PdfPCell(table2);
            rightCell.Border = PdfPCell.NO_BORDER;

            masterTable.AddCell(leftCell);
            masterTable.AddCell(rightCell);

            // Add the master table to the document
            doc.Add(masterTable);
        }

        private void AddSingleTableWithCustomColumnWidths(Document doc, PdfWriter writer, dynamic data, dynamic fundType)
        {
            PdfPTable table = new PdfPTable(2);
            table.WidthPercentage = 100;
            table.SetWidths(new float[] { 80f, 20f }); // Set column widths to 70% and 30%
            table.DefaultCell.Border = PdfPCell.BOX;
            table.DefaultCell.Phrase = new Phrase() { Font = FontFactory.GetFont(FontFactory.HELVETICA, 8) };

            // Cell 1: Description
            PdfPCell cell1 = new PdfPCell(new Phrase("DESCRIPTION", FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 8)));
            cell1.Border = PdfPCell.BOX;
            cell1.PaddingBottom = 10;
            cell1.BackgroundColor = new BaseColor(0xD9, 0xD9, 0xD9); // Background color #D9D9D9
            table.AddCell(cell1);

            // Cell 2: Amount
            PdfPCell cell2 = new PdfPCell(new Phrase("AMOUNT", FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 8)));
            cell2.Border = PdfPCell.BOX;
            cell2.PaddingBottom = 10;
            cell2.BackgroundColor = new BaseColor(0xD9, 0xD9, 0xD9); // Background color #D9D9D9
            table.AddCell(cell2);

            // Row 1: Invoice Description
            //PdfPCell invoiceDescCell = new PdfPCell(new Phrase("Invoice for Milestone 1: \"Title of Milestone\"", FontFactory.GetFont(FontFactory.HELVETICA, 9)));
            //invoiceDescCell.Border = PdfPCell.BOX;
            //table.AddCell(invoiceDescCell);

            foreach (var datas in data.InvoicelistDetails)
            {
                PdfPCell invoiceDescCell = new PdfPCell(new Phrase(datas.Description.ToString(), FontFactory.GetFont(FontFactory.HELVETICA, 8)));
                invoiceDescCell.Border = PdfPCell.BOX;
                table.AddCell(invoiceDescCell);

                PdfPCell amount1Cell = new PdfPCell(new Phrase(datas.Amount.ToString(), FontFactory.GetFont(FontFactory.HELVETICA, 9)));
                amount1Cell.Border = PdfPCell.BOX;
                amount1Cell.PaddingBottom = 10;
                table.AddCell(amount1Cell);
            }


            // Add the table to the document
            doc.Add(table);
        }

        public async Task<IActionResult> DownloadCreditMemo()   //int ContractId
        {
            /*SolutionFundModel solutionFund = new SolutionFundModel();
            solutionFund.ContractId = ContractId;
            var invoiceDetails = await _apiRepository.MakeApiCallAsync("api/Freelancer/GetInvoiceDetails", HttpMethod.Post, solutionFund);
            dynamic datas = JObject.Parse(invoiceDetails);
            var Details = datas.Result;*/

            float cellHeight = 100f;
            Document document = new Document();
            var memoryStream = new MemoryStream();
            PdfWriter writer = PdfWriter.GetInstance(document, memoryStream);

            document.Open();
            AddClientTables1(document, writer);
            document.Add(new Paragraph("\r\n  "));
            AddTwoContentTables1(document, writer);
            document.Add(new Paragraph("\r\n  "));
            AddSingleTableWithCustomColumnWidths1(document, writer);
            document.NewPage();
            document.NewPage();
            document.Close();
            Response.ContentType = MediaTypeNames.Application.Pdf;
            var contentDisposition = new ContentDisposition
            {
                FileName = "CN-00129.pdf",
                Inline = false
            };
            Response.Headers.Add("Content-Disposition", contentDisposition.ToString());
            return File(memoryStream.ToArray(), Response.ContentType);
        }

        private void AddClientTables1(Document doc, PdfWriter writer)
        {
            BaseFont baseF14 = BaseFont.CreateFont();

            // Master Table: Contains two side-by-side tables
            PdfPTable masterTable = new PdfPTable(2);
            masterTable.WidthPercentage = 100;
            masterTable.SetWidths(new float[] { 68f, 30f });
            masterTable.DefaultCell.Border = 3;

            // Left Table: Display the logo
            PdfPTable leftTable = new PdfPTable(1);
            // leftTable.WidthPercentage = 80;
            leftTable.HorizontalAlignment = Element.ALIGN_LEFT;
            float[] widths = new float[] { 40f };



            leftTable.SetWidths(widths);
            iTextSharp.text.Image img = iTextSharp.text.Image.GetInstance("https://aephyweb.azurewebsites.net/assets/img/ephylink_no_background_blue.png");
            img.ScaleToFit(150f, 150f);
            PdfPCell logoCell = new PdfPCell(img);
            logoCell.HorizontalAlignment = Element.ALIGN_LEFT;
            logoCell.Border = PdfPCell.NO_BORDER;
            leftTable.AddCell(logoCell);

            // Right Table: Labels and Text Fields
            PdfPTable rightTable = new PdfPTable(2);
            // rightTable.WidthPercentage = 30;
            float[] widths1 = new float[] { 0f, 100f };
            rightTable.SetWidths(widths1);
            rightTable.HorizontalAlignment = Element.ALIGN_RIGHT;

            //rightTable.SpacingBefore = 30;

            // Set the font for the entire table
            rightTable.DefaultCell.Phrase = new Phrase() { Font = FontFactory.GetFont(FontFactory.HELVETICA, 8) };

            // Create label cells and set font styles
            PdfPCell labelCell1 = new PdfPCell(new Phrase("           CREDIT MEMO                   ", FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 11)));
            labelCell1.BackgroundColor = new BaseColor(0xD9, 0xD9, 0xD9); // Set background color
            labelCell1.VerticalAlignment = Element.ALIGN_MIDDLE; // Center the text vertically
            labelCell1.Colspan = 5;

            labelCell1.FixedHeight = 23; ;

            //labelCell1.Colspan = 2;
            //labelCell1.PaddingLeft = 40;


            PdfPCell labelCell6 = new PdfPCell(new Phrase("Invoice", FontFactory.GetFont(FontFactory.HELVETICA, 9)));


            PdfPCell labelCell2 = new PdfPCell(new Phrase("Date", FontFactory.GetFont(FontFactory.HELVETICA, 9)));
            //labelCell2.PaddingLeft = 10;
            PdfPCell labelCell3 = new PdfPCell(new Phrase("Due Date", FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 9))); // Bold font for "Due Date" label
            //labelCell3.PaddingLeft = 10;
            PdfPCell labelCell4 = new PdfPCell(new Phrase("Total Amount", FontFactory.GetFont(FontFactory.HELVETICA, 9)));
            //labelCell4.PaddingLeft = 10;
            PdfPCell labelCell5 = new PdfPCell(new Phrase("Total Due ", FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 9)));
            //labelCell5.PaddingLeft = 10;

            // Create textField cells
            PdfPCell textFieldCell1 = new PdfPCell();
            PdfPCell textFieldCell2 = new PdfPCell();
            PdfPCell textFieldCell3 = new PdfPCell();
            PdfPCell textFieldCell4 = new PdfPCell();
            PdfPCell textFieldCell5 = new PdfPCell();
            PdfPCell textFieldCell6 = new PdfPCell();

            // Set cell properties
            PdfPCell[] cells = { labelCell1, textFieldCell1, labelCell6, textFieldCell6, labelCell2, textFieldCell2, labelCell3, textFieldCell3, labelCell4, textFieldCell4, labelCell5, textFieldCell5 };

            foreach (PdfPCell cell in cells)
            {
                cell.Border = PdfPCell.NO_BORDER;
                rightTable.AddCell(cell);
            }

            // Add the left and right tables to the master table
            PdfPCell leftCell = new PdfPCell(leftTable);
            leftCell.Border = PdfPCell.NO_BORDER;

            PdfPCell rightCell = new PdfPCell(rightTable);
            rightCell.Border = PdfPCell.NO_BORDER;

            masterTable.AddCell(leftCell);
            masterTable.AddCell(rightCell);

            // Add the master table to the document
            doc.Add(masterTable);
        }

        private void AddTwoContentTables1(Document doc, PdfWriter writer)
        {
            // Master Table: Contains two side-by-side tables
            PdfPTable masterTable = new PdfPTable(2);
            masterTable.WidthPercentage = 100;

            // Table 1: Content 1
            PdfPTable table1 = new PdfPTable(1);
            table1.WidthPercentage = 100;
            table1.HorizontalAlignment = Element.ALIGN_LEFT;

            // Content 1
            string content1 = "From\nEphylink\nDimitrios Vamvakas\nEllispontou 39, Patra, 26226\nAchaia, Greece\nVAT ID: 148366653";

            // Split content1 into lines
            string[] content1Lines = content1.Split('\n');

            foreach (string line in content1Lines)
            {
                Font font = FontFactory.GetFont(FontFactory.HELVETICA, 9);
                if (line == "From")
                {
                    font.Color = new BaseColor(195, 120, 19);
                    font.Size = 14;
                }
                if (line == "Ephylink")
                {

                    font.Size = 9;
                    font.SetStyle(Font.BOLD);
                }

                Chunk chunk = new Chunk(line, font);
                PdfPCell contentCell1 = new PdfPCell(new Phrase(chunk));
                contentCell1.Border = PdfPCell.NO_BORDER;
                table1.AddCell(contentCell1);
            }

            // Table 2: Content 2
            PdfPTable table2 = new PdfPTable(1);
            table2.WidthPercentage = 100;
            table2.HorizontalAlignment = Element.ALIGN_LEFT;

            // Content 2
            string content2 = "Bill to\nClient 1 International Client\nName \nAddress:\nVAT ID: (If Applicable )\nTax ID: (other)";

            // Split content2 into lines
            string[] content2Lines = content2.Split('\n');

            foreach (string line in content2Lines)
            {
                Font font = FontFactory.GetFont(FontFactory.HELVETICA, 9);
                if (line == "Bill to")
                {
                    font.Color = new BaseColor(195, 120, 19);
                    font.Size = 14;
                }
                if (line == "Client 1 International Client")
                {
                    font.SetStyle(Font.BOLD);
                    font.Size = 9;
                }
                Chunk chunk = new Chunk(line, font);
                PdfPCell contentCell2 = new PdfPCell(new Phrase(chunk));
                contentCell2.Border = PdfPCell.NO_BORDER;
                table2.AddCell(contentCell2);
            }

            // Add the left and right tables to the master table
            PdfPCell leftCell = new PdfPCell(table1);
            leftCell.Border = PdfPCell.NO_BORDER;

            PdfPCell rightCell = new PdfPCell(table2);
            rightCell.Border = PdfPCell.NO_BORDER;

            masterTable.AddCell(leftCell);
            masterTable.AddCell(rightCell);

            // Add the master table to the document
            doc.Add(masterTable);
        }
        private void AddSingleTableWithCustomColumnWidths1(Document doc, PdfWriter writer)
        {
            PdfPTable table = new PdfPTable(2);
            table.WidthPercentage = 100;
            table.SetWidths(new float[] { 80f, 20f }); // Set column widths to 70% and 30%
            table.DefaultCell.Border = PdfPCell.BOX;
            table.DefaultCell.Phrase = new Phrase() { Font = FontFactory.GetFont(FontFactory.HELVETICA, 9) };

            // Cell 1: Description
            PdfPCell cell1 = new PdfPCell(new Phrase("DESCRIPTION", FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 9)));
            cell1.Border = PdfPCell.BOX;
            cell1.PaddingBottom = 10;
            cell1.BackgroundColor = new BaseColor(0xD9, 0xD9, 0xD9); // Background color #D9D9D9
            table.AddCell(cell1);

            // Cell 2: Amount
            PdfPCell cell2 = new PdfPCell(new Phrase("AMOUNT", FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 9)));
            cell2.Border = PdfPCell.BOX;
            cell2.PaddingBottom = 10;
            cell2.BackgroundColor = new BaseColor(0xD9, 0xD9, 0xD9); // Background color #D9D9D9
            table.AddCell(cell2);

            // Row 1: Invoice Description
            PdfPCell invoiceDescCell = new PdfPCell(new Phrase("Paid from escrow for \"Title of Milestone\"", FontFactory.GetFont(FontFactory.HELVETICA, 9)));
            invoiceDescCell.Border = PdfPCell.BOX;
            table.AddCell(invoiceDescCell);

            PdfPCell amount1Cell = new PdfPCell(new Phrase("-1,198.92", FontFactory.GetFont(FontFactory.HELVETICA, 9)));
            amount1Cell.Border = PdfPCell.BOX;
            amount1Cell.PaddingBottom = 10;
            table.AddCell(amount1Cell);

            // Row 2: VAT
            PdfPCell vatDescCell = new PdfPCell(new Phrase("VAT (0%)", FontFactory.GetFont(FontFactory.HELVETICA, 9)));
            vatDescCell.Border = PdfPCell.BOX;
            vatDescCell.PaddingBottom = 10;
            table.AddCell(vatDescCell);

            PdfPCell amount2Cell = new PdfPCell(new Phrase(" ", FontFactory.GetFont(FontFactory.HELVETICA, 9)));
            amount2Cell.Border = PdfPCell.BOX;
            amount2Cell.PaddingBottom = 10;
            table.AddCell(amount2Cell);

            // Row 3: Total Amount
            PdfPCell totalDescCell = new PdfPCell(new Phrase("Total amount", FontFactory.GetFont(FontFactory.HELVETICA, 9)));
            totalDescCell.Border = PdfPCell.BOX;
            totalDescCell.PaddingBottom = 10;
            table.AddCell(totalDescCell);

            PdfPCell totalAmountCell = new PdfPCell(new Phrase("€ -1,198.92", FontFactory.GetFont(FontFactory.HELVETICA, 9)));
            totalAmountCell.Border = PdfPCell.BOX;
            totalAmountCell.PaddingBottom = 10;
            table.AddCell(totalAmountCell);

            // Add the table to the document
            doc.Add(table);
        }


        //GetClientInvoiceDetails
        [HttpPost]
        public async Task<string> GetClientInvoiceDetails([FromBody] SolutionFundModel model)
        {
            var userId = HttpContext.Session.GetString("LoggedUser");
            model.UserId = userId;
            var invoiceData = await _apiRepository.MakeApiCallAsync("api/Freelancer/GetInvoiceDetails", HttpMethod.Post, model);
            return invoiceData;

        }


        //GetClientInvoiceDetails
        [HttpPost]
        public async Task<string> GetFreelancerInvoiceDetails([FromBody] SolutionFundModel model)
        {
            var userId = HttpContext.Session.GetString("LoggedUser");
            model.UserId = userId;
            var invoiceData = await _apiRepository.MakeApiCallAsync("api/Freelancer/GetFreelancerInvoiceDetails", HttpMethod.Post, model);
            return invoiceData;

        }

        //GetFreelancerActiveProjectList
        [HttpGet]
        public async Task<string> GetFreelancerActiveProjectList()
        {
            var userId = HttpContext.Session.GetString("LoggedUser");
            if (userId == null)
            {
                return "No Data Found";
            }
            MileStoneIdViewModel model = new MileStoneIdViewModel();
            model.UserId = userId;
            var projectData = await _apiRepository.MakeApiCallAsync("api/Freelancer/GetFreelancerActiveProjectList", HttpMethod.Post, model);
            dynamic data = JsonConvert.DeserializeObject(projectData);
            try
            {
                if (data.Result != null)
                {
                    foreach (var service in data.Result)
                    {
                        string imagepath = service.ImagePath;
                        if (imagepath != null)
                        {
                            string sasToken = GenerateImageSasToken(imagepath);
                            string imageUrlWithSas = $"{service.ImagePath}?{sasToken}";
                            service.ImageUrlWithSas = imageUrlWithSas;

                        }

                    }

                }

            }
            catch (Exception ex)
            {
                return ex.Message + ex.InnerException;
            }
            string jsonString = JsonConvert.SerializeObject(data, Formatting.Indented);
            return jsonString;

        }

        //EditActiveSolutionDefineDetails
        [HttpPost]
        public async Task<string> EditActiveSolutionDefineDetails([FromBody] SolutionIndustryDetailsViewModel model)
        {
            if (model != null)
            {
                try
                {
                    var test = await _apiRepository.MakeApiCallAsync("api/Freelancer/EditActiveSolutionDefineDetails", HttpMethod.Post, model);
                    return test;
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
            return "";
        }

        public async Task<string> GetDashbordContractUser()
        {
            string userId = HttpContext.Session.GetString("LoggedUser");
            var contractCount = string.Empty;
            if (userId != null)
            {
                contractCount = await _apiRepository.MakeApiCallAsync("api/Freelancer/GetContractUser", HttpMethod.Get, userId);
            }

            return contractCount;
        }

        //GetActiveProjectInvoices
        [HttpGet]
        public async Task<string> GetActiveProjectInvoices()
        {
            var userId = HttpContext.Session.GetString("LoggedUser");
            if (userId == null)
            {
                return "No Data Found";
            }
            MileStoneIdViewModel model = new MileStoneIdViewModel();
            model.UserId = userId;
            var projectData = await _apiRepository.MakeApiCallAsync("api/Freelancer/GetActiveProjectInvoices", HttpMethod.Post, model);
            return projectData;

        }

        //GetFreelancerInvoices
        [HttpGet]
        public async Task<string> GetFreelancerInvoices()
        {
            var userId = HttpContext.Session.GetString("LoggedUser");
            if (userId == null)
            {
                return "No Data Found";
            }
            MileStoneIdViewModel model = new MileStoneIdViewModel();
            model.UserId = userId;
            var projectData = await _apiRepository.MakeApiCallAsync("api/Freelancer/GetFreelancerInvoices", HttpMethod.Post, model);
            return projectData;

        }

        //GetArchivesProject
        [HttpGet]
        public async Task<string> GetArchivesProject()
        {
            var userId = HttpContext.Session.GetString("LoggedUser");
            if (userId == null)
            {
                return "No Data Found";
            }
            MileStoneIdViewModel model = new MileStoneIdViewModel();
            model.UserId = userId;
            var projectData = await _apiRepository.MakeApiCallAsync("api/Freelancer/GetArchivesProject", HttpMethod.Post, model);
            return projectData;

        }

        //GetArchivesProject
        [HttpGet]
        public async Task<string> GetClientProjectExpense()
        {
            var userId = HttpContext.Session.GetString("LoggedUser");
            var PreferredCurrency = HttpContext.Session.GetString("ClientPreferredCurrency");
            if (userId == null)
            {
                return "No Data Found";
            }
            MileStoneIdViewModel model = new MileStoneIdViewModel();
            model.UserId = userId;
            model.ClientPreferredCurrency = PreferredCurrency;
            var projectData = await _apiRepository.MakeApiCallAsync("api/Freelancer/GetProjectsExpense", HttpMethod.Post, model);
            return projectData;

        }

        //getFreelancerProjectExpense
        [HttpGet]
        public async Task<string> getFreelancerProjectExpense()
        {
            var userId = HttpContext.Session.GetString("LoggedUser");
            var PreferredCurrency = HttpContext.Session.GetString("ClientPreferredCurrency");
            if (userId == null)
            {
                return "No Data Found";
            }
            MileStoneIdViewModel model = new MileStoneIdViewModel();
            model.UserId = userId;
            model.ClientPreferredCurrency = PreferredCurrency;
            var projectData = await _apiRepository.MakeApiCallAsync("api/Freelancer/getFreelancerProjectExpense", HttpMethod.Post, model);
            return projectData;

        }

        [HttpPost]
        public async Task<string> filterSolutionBySolutionName([FromBody] MileStoneIdViewModel model)
        {
            if (model != null)
            {
                var userId = HttpContext.Session.GetString("LoggedUser");
                if (userId == null)
                {
                    userId = "";
                }
                model.UserId = userId;
                var solutionData = await _apiRepository.MakeApiCallAsync("api/Client/filterSolutionBySolutionName", HttpMethod.Post, model);
                dynamic data = JsonConvert.DeserializeObject(solutionData);
                try
                {
                    if (data.Result != null)
                    {
                        foreach (var service in data.Result.SolutionData)
                        {
                            string imagepath = service.ImagePath;
                            string sasToken = GenerateImageSasToken(imagepath);
                            string imageUrlWithSas = $"{service.ImagePath}?{sasToken}";
                            service.ImageUrlWithSas = imageUrlWithSas;

                        }
                    }
                }
                catch (Exception ex)
                {
                    return ex.Message + ex.InnerException;
                }
                string jsonString = JsonConvert.SerializeObject(data, Formatting.Indented);
                return jsonString;
            }
            else
            {
                return "failed to receive data..";
            }
        }

        //GetFreelancerArchivesProject
        [HttpGet]
        public async Task<string> GetFreelancerArchivesProject()
        {
            var userId = HttpContext.Session.GetString("LoggedUser");
            if (userId == null)
            {
                return "No Data Found";
            }
            MileStoneIdViewModel model = new MileStoneIdViewModel();
            model.UserId = userId;
            var projectData = await _apiRepository.MakeApiCallAsync("api/Freelancer/GetFreelancerArchivesProject", HttpMethod.Post, model);
            return projectData;

        }

        //GetFreelancerList
        [HttpPost]
        public async Task<string> GetFreelancerListSolutionWise([FromBody] SolutionFundModel model)
        {
            if (model != null)
            {
                try
                {
                    var userId = HttpContext.Session.GetString("LoggedUser");
                    model.UserId = userId;
                    var data = await _apiRepository.MakeApiCallAsync("api/Freelancer/GetFreelancerListSolutionWise", HttpMethod.Post, model);
                    return data;
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
            return "";
        }

        //SaveFreelancerToFreelancerReview
        [HttpPost]
        public async Task<string> SaveFreelancerToFreelancerReview([FromBody] FreelancerToFreelancerReviewViewModel model)
        {
            if (model != null)
            {
                try
                {
                    var userId = HttpContext.Session.GetString("LoggedUser");
                    model.FromFreelancerId = userId;
                    var data = await _apiRepository.MakeApiCallAsync("api/Freelancer/SaveFreelancerToFreelancerReview", HttpMethod.Post, model);
                    return data;
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
            return "";
        }

        //CheckFreelancerToFreelancerReviewExits
        [HttpPost]
        public async Task<string> CheckFreelancerToFreelancerReviewExits([FromBody] FreelancerToFreelancerReviewViewModel model)
        {
            if (model != null)
            {
                try
                {
                    var userId = HttpContext.Session.GetString("LoggedUser");
                    model.FromFreelancerId = userId;
                    var data = await _apiRepository.MakeApiCallAsync("api/Freelancer/CheckFreelancerToFreelancerReviewExits", HttpMethod.Post, model);
                    return data;
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
            return "";
        }

        [HttpPost]
        public async Task<string> OnboardUserRevoultAccount([FromBody] AddNonRevolutCounterpartyReq addNonRevolutCounterpartyReq)
        {
            if (addNonRevolutCounterpartyReq != null)
            {
                try
                {
                    var userId = HttpContext.Session.GetString("LoggedUser");
                    addNonRevolutCounterpartyReq.UserId = userId;
                    var data = await _apiRepository.MakeApiCallAsync("api/Revoult/OnboardUserRevoultAccount", HttpMethod.Post, addNonRevolutCounterpartyReq);
                    return data;
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
            return "";
        }

        //DeleteFreelancerRevolutAccount
        [HttpGet]
        public async Task<string> DeleteFreelancerRevolutAccount()
        {

            try
            {
                var userId = HttpContext.Session.GetString("LoggedUser");
                if (userId == null)
                {
                    return "Please login to delete account";
                }
                AddNonRevolutCounterpartyReq model = new AddNonRevolutCounterpartyReq();
                model.UserId = userId;
                var data = await _apiRepository.MakeApiCallAsync("api/Revoult/DeleteFreelancerRevolutAccount", HttpMethod.Post, model);
                return data;
            }
            catch (Exception ex)
            {
                return ex.Message + ex.InnerException;
            }
        }


        public ActionResult RevoultCheckout()
        {
            // var revoultToken = HttpContext.Session.GetString("RevoultToken");
            var revoultToken = HttpContext.Session.GetString("RevoultToken");
            var orderId = HttpContext.Session.GetString("RevoultOrderId");
            var solutionFundId = HttpContext.Session.GetString("SolutionFundId");
            ViewData["Token"] = revoultToken;
            ViewData["OrderId"] = orderId;
            ViewData["SolutionFundId"] = solutionFundId;
            return View();
        }

        //GetInvoiceTranscationTypeDetails
        [HttpPost]
        public async Task<string> GetInvoiceTranscationTypeDetails([FromBody] SolutionFundModel model)
        {
            if (model != null)
            {
                try
                {
                    var userId = HttpContext.Session.GetString("LoggedUser");
                    model.ClientId = userId;
                    var data = await _apiRepository.MakeApiCallAsync("api/Client/GetInvoiceTranscationTypeDetails", HttpMethod.Post, model);
                    return data;
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
            return "";
        }


        //GetFreelancerInvoiceTypeDetails
        [HttpPost]
        public async Task<string> GetFreelancerInvoiceTypeDetails([FromBody] SolutionFundModel model)
        {
            if (model != null)
            {
                try
                {
                    var userId = HttpContext.Session.GetString("LoggedUser");
                    model.ClientId = userId;
                    var data = await _apiRepository.MakeApiCallAsync("api/Client/GetFreelancerInvoiceTypeDetails", HttpMethod.Post, model);
                    return data;
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
            return "";
        }

        [HttpPost]
        public async Task<string> FreelancerLeaveProject([FromBody] SolutionFundModel model)
        {
            if (model != null)
            {
                try
                {
                    var userId = HttpContext.Session.GetString("LoggedUser");
                    model.UserId = userId;
                    var data = await _apiRepository.MakeApiCallAsync("api/Freelancer/FreelancerLeaveProject", HttpMethod.Post, model);
                    dynamic clientdata = JsonConvert.DeserializeObject(data);

                    #region Send freelancer leave Email

                    string emailAddress = clientdata.Result.ClientEmailId;
                    string solutionName = clientdata.Result.SolutionName;
                    string industryName = clientdata.Result.IndustryName;
                    string freelancername = clientdata.Result.FreelancerFullName;

                    if (clientdata.Message == "Project leave Successfully !")
                    {
                        string body = System.IO.File.ReadAllText(_rootPath + "/EmailTemplates/FreelancerLeaveTemplate.html");
                        body = body.Replace("{{Project_Name}}", solutionName);
                        body = body.Replace("{{Industry_Name}}", industryName);
                        body = body.Replace("{{Freelancer_Name}}", freelancername);

                        bool send = SendEmailHelper.SendEmail(emailAddress, "Update on Your Project – Action Required", body);
                       
                    }

                    #endregion
                    return data;
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
            return "";
        }

        //SaveCustomSolutionData
        [HttpPost]
        public async Task<string> SaveCustomSolutionData([FromBody] CustomSolutionViewModel model)
        {
            if (model != null)
            {
                try
                {
                    var userId = HttpContext.Session.GetString("LoggedUser");
                    if (userId == null)
                    {
                        return "Please login to purchase solution";
                    }
                    var userRole = HttpContext.Session.GetString("LoggedUserRole");
                    if (userRole == "Freelancer")
                    {
                        return "Please login as a client to purchase solution";
                    }
                    model.UserId = userId;
                    model.CustomStartDate = DateTime.Now.ToString();
                    model.CustomEndDate = DateTime.Now.ToString();
                    model.CustomStartHour = DateTime.Now.ToString();
                    model.CustomEndHour = DateTime.Now.ToString();
                    model.CustomProjectDuration = "0";

                    var data = await _apiRepository.MakeApiCallAsync("api/Client/SaveCustomSolutionData", HttpMethod.Post, model);
                    return data;
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
            return "No Data Found";
        }

        [HttpPost]
        public async Task<string> SaveFreelancerExcludeDate([FromBody] ExcludeDateRequestModel model)
        {
            if (model != null)
            {
                try
                {
                    string[] dateStrings = model.DateRange.Split(" - ");
                    if (dateStrings.Length == 2)
                    {
                        DateTime startDate = DateTime.ParseExact(dateStrings[0], "dd/MM/yyyy", CultureInfo.InvariantCulture);
                        DateTime endDate = DateTime.ParseExact(dateStrings[1], "dd/MM/yyyy", CultureInfo.InvariantCulture);

                        List<DateTime> dateRange = GenerateDateRange(startDate, endDate);

                        var requestModel = new ExcludeDateModel();
                        var userId = HttpContext.Session.GetString("LoggedUser");
                        requestModel.FreelancerId = userId;
                        requestModel.ExcludeDateList = dateRange;

                        var data = await _apiRepository.MakeApiCallAsync("api/Freelancer/SaveFreelancerExcludeDate", HttpMethod.Post, requestModel);
                        return data;
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
            return "No Data Found";
        }

        [HttpGet]
        public async Task<string> GetFreelancerExcludeDateData()
        {
            var model = new ExcludeDateGridModel();
            model.FreelancerId = HttpContext.Session.GetString("LoggedUser");
            var RolesList = await _apiRepository.MakeApiCallAsync("api/Freelancer/GetFreelancerExcludeDateData", HttpMethod.Post, model);
            return RolesList;
        }

        [HttpPost]
        public async Task<string> RemoveFreelancerExcludeDate([FromBody] ExcludeDateGridModel model)
        {
            var RolesList = await _apiRepository.MakeApiCallAsync("api/Freelancer/RemoveFreelancerExcludeDate", HttpMethod.Post, model);
            return RolesList;
        }
        private static List<DateTime> GenerateDateRange(DateTime startDate, DateTime endDate)
        {
            List<DateTime> dateRange = new List<DateTime>();

            if (startDate == endDate)
            {
                dateRange.Add(startDate);
            }
            else
            {
                while (startDate <= endDate)
                {
                    dateRange.Add(startDate);
                    startDate = startDate.AddDays(1);
                }
            }

            return dateRange;
        }

        [HttpPost]
        public async Task<string> FreelancerRequest([FromBody] FreelancerRequestModel model)
        {
            var RolesList = await _apiRepository.MakeApiCallAsync("api/Freelancer/FreelancerRequest", HttpMethod.Post, model);
            return RolesList;
        }
    }
}
