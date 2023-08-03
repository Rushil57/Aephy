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
                dynamic jsonObj = JsonConvert.DeserializeObject(test);
                if (jsonObj["StatusCode"] == 200)
                {
                    FirstName = jsonObj.Result.FirstName;
                    LastName = jsonObj.Result.LastName;
                    Role = jsonObj.Result.Role;
                    Level = jsonObj.Result.Level;
                    UserId = jsonObj.Result.UserId;

                    HttpContext.Session.SetString("FullName", FirstName + " " + LastName);
                    HttpContext.Session.SetString("LoggedUserRole", Role);

                    if(Level != "none")
                    {
                        HttpContext.Session.SetString("LoggedUserLevel", Level);
                    }
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
                    throw ex;
                }
            }

            return "";
        }


        [HttpPost]
        public async Task<string> EditUserData(IFormFile httpPostedFileBase, string Userdata)
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
                if (data != null)
                {
                    if (data.Message == "Applied Successfully")
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
                    else
                    {
                        return data.Message;
                    }
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
                    opengigroles.ID = (int)(Id);

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


        [HttpPost]
        public async Task<string> SaveMileStone([FromBody] MileStoneViewModel mileStone)
        {
            if (mileStone != null)
            {
                try
                {
                    var userId = HttpContext.Session.GetString("LoggedUser");
                    mileStone.FreelancerId = userId;
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
                model.FreelancerId = userId;
                var aprroveList = await _apiRepository.MakeApiCallAsync("api/Freelancer/GetMiletoneList", HttpMethod.Post, model);
                return aprroveList;
            }
            return "Something Went Wrong";
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
            var Industrydata = await _apiRepository.MakeApiCallAsync("api/Admin/IndustriesList", HttpMethod.Post);
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
            var Solutiondata = await _apiRepository.MakeApiCallAsync("api/Admin/GetSolutionList", HttpMethod.Get);
            if(Solutiondata != null)
            {
                dynamic data = JsonConvert.DeserializeObject(Solutiondata);
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

                }
                string jsonString = JsonConvert.SerializeObject(data, Formatting.Indented);
                return jsonString;
            }
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
                model.FreelancerId = userId;
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
            var solutionList = await _apiRepository.MakeApiCallAsync("api/Admin/SolutionList", HttpMethod.Get);
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
    }
}
