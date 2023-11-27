using Aephy.WEB.Models;
using Aephy.WEB.Provider;
using Azure.Storage.Blobs;
using Azure.Storage.Sas;
using Azure.Storage;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using static System.Net.Mime.MediaTypeNames;
using System.Reflection;
using Microsoft.AspNetCore.Http;
using Aephy.Helper.Helpers;

namespace Aephy.WEB.Controllers
{
    public class LandingPageController : Controller
    {
        private readonly IApiRepository _apiRepository;
        private const string ContainerName = "cvfiles";
        private const string ImageContainerName = "profileimages";
        private const string customSolutionContainer = "customsolutionfiles";
        private readonly IConfiguration _configuration;
        private readonly string _connectionString;
        private readonly string _rootPath;

        public LandingPageController(IConfiguration configuration, IApiRepository apiRepository, IWebHostEnvironment hostEnvironment)
        {
            _apiRepository = apiRepository;
            _configuration = configuration;
            _connectionString = configuration.GetConnectionString("AzureBlobStorage");
            _rootPath = hostEnvironment.WebRootPath;
            _connectionString = "DefaultEndpointsProtocol=https;AccountName=aephystorageaccount;AccountKey=nEy6xh4P4m2d94iDgqq+yNB99bucjGMD1wp2L6sbsNFjHPaUQiCHgc5b4hmBmeRtYsiA/WvudVmV+AStwz3djw==;EndpointSuffix=core.windows.net";
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

        public ActionResult WhyAephy()
        {
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

        public ActionResult BrowseSolutions()
        {

            return View();
        }

        /*        [HttpGet]
                public ActionResult Project()
                {
                    return View();
                }
        */
        [HttpGet]
        public ActionResult Project(int? Service, int? Solution, int? Industry)
        {
            ViewData["Solution"] = Solution;
            ViewData["Industry"] = Industry;
            ViewData["Service"] = Service;

            return View();
        }

        [HttpGet]
        public int BusinessDaysUntil(DateTime firstDay, DateTime lastDay, DateTime[]? bankHolidays) //,DateTime[]? bankHolidays
        {
            firstDay = firstDay.Date;
            lastDay = lastDay.Date;
            int days = 0;

            for (DateTime date = firstDay; date <= lastDay; date = date.AddDays(1))
            {
                if (firstDay.DayOfWeek != DayOfWeek.Saturday && firstDay.DayOfWeek != DayOfWeek.Sunday && !bankHolidays.Contains(date)) // && !bankHolidays.Contains(date)
                {
                    days++;
                }
                firstDay = firstDay.AddDays(1);
            }

            return days;
        }

        [HttpPost]
        public async Task<string> SaveClientAvailability([FromBody] ClientAvailabilityModel model)
        {
            string userType = HttpContext.Session.GetString("LoggedUserRole");
            string clientID = HttpContext.Session.GetString("LoggedUser");

            if (!string.IsNullOrEmpty(userType) && userType == "Client" && !string.IsNullOrEmpty(clientID))
            {
                model.ClientId = clientID.ToString();
            }
            else
            {
                model.ClientId = null;
            }
            string holidaysList = model.HolidaysList;
            DateTime firstDay = model.StartDate != null ? Convert.ToDateTime(model.StartDate) : DateTime.Now;
            DateTime lastDay = model.EndDate != null ? Convert.ToDateTime(model.EndDate) : DateTime.Now;
            DateTime[] holidays = new DateTime[0];
            var response = string.Empty;
            if (holidaysList != null && holidaysList != "")
            {
                var holidaysLst = holidaysList.Split(';');
                holidays = holidaysLst.Select(x => DateTime.Parse(x)).ToArray();
            }
            model.Holidays = holidays;
            if (firstDay < lastDay)
            {
                response = await _apiRepository.MakeApiCallAsync("api/Client/saveClientAvailabilityData", HttpMethod.Post, model);
            }
            return response;
        }

        [HttpPost]
        public async Task<string> SaveRequestedProposal(IFormFile httpPostedFileBase, string SolutionData)
        {
            try
            {
                IFormFile CustomSolutionFile = httpPostedFileBase;
                var result = JsonConvert.DeserializeObject<CustomSolutionsModel>(SolutionData);


                string userType = HttpContext.Session.GetString("LoggedUserRole");
                string clientID = HttpContext.Session.GetString("LoggedUser");

                if (!string.IsNullOrEmpty(userType) && userType == "Client" && !string.IsNullOrEmpty(clientID))
                {
                    result.ClientId = clientID;
                }
                var response = await _apiRepository.MakeApiCallAsync("api/Client/SaveRequestedProposal", HttpMethod.Post, result);

                dynamic data = JsonConvert.DeserializeObject(response);

                //return response;
                if (data.Message == "Submitted Successfully")
                {
                    if (result.AlreadyExistDocument)
                    {
                        int Id = data.Result;
                        CustomSolutionDocument customSolution = new CustomSolutionDocument();
                        customSolution.AlreadyExistDocument = true;
                        customSolution.ID = Id;
                        //opengigroles.FreelancerId = freelancer;
                        var ok = await _apiRepository.MakeApiCallAsync("api/Client/UpdateSolutionDocument", HttpMethod.Post, customSolution);
                        return response;
                    }
                    else
                    {
                        if (data != null)
                        {

                            int Id = data.Result;
                            var d = await UploadSolutionDocument(CustomSolutionFile, Id);
                            var ok = await _apiRepository.MakeApiCallAsync("api/Client/UpdateSolutionDocument", HttpMethod.Post, d);
                            dynamic UploadResponse = JsonConvert.DeserializeObject(ok);
                            if (UploadResponse != null)
                            {
                                return ok;
                            }
                            else
                            {
                                return "Failed to submit your Request !";
                            }
                        }
                    }
                }
                else
                {
                    return response;
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
            return "Failed to submit your Request !";
        }

        public ActionResult FAQ()
        {
            return View();
        }

        [HttpGet]
        public async Task<string> GetRolesList()
        {
            var RolesList = await _apiRepository.MakeApiCallAsync("api/Admin/ActiveRolesList", HttpMethod.Get);
            return RolesList;
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

        [HttpGet]
        public async Task<string> GetCareerOpenRoles()
        {
            var RolesList = await _apiRepository.MakeApiCallAsync("api/Admin/GetEmployeeRolesList", HttpMethod.Get);
            return RolesList;
        }

        [HttpPost]
        public async Task<string> FilterEmployeeRolesList([FromBody] EmployeeOpenRole model)
        {
            var RolesList = await _apiRepository.MakeApiCallAsync("api/Admin/FilterEmployeeRolesList", HttpMethod.Post, model);
            return RolesList;
        }

        [HttpGet]
        public async Task<string> GetServices()
        {
            var serviceList = await _apiRepository.MakeApiCallAsync("api/Admin/ServiceList", HttpMethod.Get);
            return serviceList;
        }

        [HttpGet]
        public async Task<string> GetIndustries()
        {
            var industryData = await _apiRepository.MakeApiCallAsync("api/Admin/IndustriesList", HttpMethod.Post);
            return industryData;
        }

        [HttpGet]
        public async Task<string> GetSolutions()
        {
            var industryData = await _apiRepository.MakeApiCallAsync("api/Admin/GetSolutionList", HttpMethod.Get);
            return industryData;
        }

        [HttpGet]
        public async Task<string> GetFilteredRolesList(int service, int solution, string level, int industry)
        {
            dynamic obj = new
            {
                service = service,
                level = level,
                solution = solution,
                industry = industry
            };
            var RolesList = await _apiRepository.MakeApiCallAsync("api/Admin/FilteredRolesList?serviceId=" + service + "&solutionId=" + solution + "&level=" + level + "&industryId=" + industry, HttpMethod.Get);
            return RolesList;
        }

        [HttpPost]
        public async Task<string> GetRolesDataById(int ID)
        {

            if (ID != 0)
            {
                var userId = HttpContext.Session.GetString("LoggedUser");
                OpenGigRolesModel model = new OpenGigRolesModel();
                model.ID = ID;
                model.FreelancerID = userId;
                var RolesList = await _apiRepository.MakeApiCallAsync("api/Admin/RolesDataById", HttpMethod.Post, model);
                return RolesList;
            }
            return "";
        }

        //[HttpPost]
        //public async Task<string> getSolutioncheckOut([FromBody] MileStoneDetailsViewModel model)
        //{
        //    var checkOutResponse = await _apiRepository.MakeApiCallAsync("api/Admin/checkOut", HttpMethod.Post, model);
        //    return checkOutResponse;
        //}

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
            var Solutiondata = await _apiRepository.MakeApiCallAsync("api/Admin/GetSolutionList", HttpMethod.Get);
            if (Solutiondata != null)
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
                    return ex.Message + ex.InnerException;
                }
                string jsonString = JsonConvert.SerializeObject(data, Formatting.Indented);
                return jsonString;
            }
            return Solutiondata;

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

        public async Task<CustomSolutionDocument> UploadSolutionDocument(IFormFile CustomSolutionFile, object Id)
        {
            CustomSolutionDocument solutionDocumentModel = new CustomSolutionDocument();
            try
            {
                if (CustomSolutionFile != null && CustomSolutionFile.Length > 0)
                {
                    string BlobStorageBaseUrl = string.Empty;
                    string DocumentPath = string.Empty;
                    string DocumentUrlWithSas = string.Empty;

                    string fileName = Guid.NewGuid().ToString() + "_" + CustomSolutionFile.FileName;

                    // Get the Azure Blob Storage connection string from configuration
                    var connectionString = _configuration.GetConnectionString("AzureBlobStorage");

                    BlobServiceClient blobServiceClient = new BlobServiceClient(connectionString);
                    BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(customSolutionContainer);

                    BlobClient blobClient = containerClient.GetBlobClient(fileName);

                    using (var stream = CustomSolutionFile.OpenReadStream())
                    {
                        await blobClient.UploadAsync(stream, overwrite: true);
                    }

                    DocumentPath = blobClient.Uri.ToString();


                    string sasToken = GenerateSasToken(DocumentPath);


                    string cvUrlWithSas = DocumentPath + sasToken;



                    BlobStorageBaseUrl = containerClient.Uri.ToString();


                    DocumentUrlWithSas = cvUrlWithSas;

                    solutionDocumentModel.BlobStorageBaseUrl = BlobStorageBaseUrl;
                    solutionDocumentModel.DocumentPath = DocumentPath;
                    solutionDocumentModel.DocumentUrlWithSas = DocumentUrlWithSas;
                    solutionDocumentModel.ID = (int)(Id);

                    return solutionDocumentModel;
                }
                else
                {
                    ModelState.AddModelError("DocumentFile", "Please select a document file.");
                }
            }
            catch (Exception ex)
            {

            }
            return solutionDocumentModel;


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
                    ModelState.AddModelError("ImageFile", "Please select an image file.");
                }
            }
            catch (Exception ex)
            {

            }
            return opengigroles;


        }

        [HttpGet]
        public async Task<string> GetSuccessfullProjectList()
        {
            var data = await _apiRepository.MakeApiCallAsync("api/Client/GetSuccessfullProjectList", HttpMethod.Get);
            return data;

        }

        [HttpGet]
        public async Task<string> GetTopProfessionalDetails()
        {
            var Professionaldata = await _apiRepository.MakeApiCallAsync("api/Client/GetTopProfessionalDetails", HttpMethod.Get);
            dynamic data = JsonConvert.DeserializeObject(Professionaldata);

            if (data.Result != null)
            {
                foreach (var service in data.Result)
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
            string jsonString = JsonConvert.SerializeObject(data, Formatting.Indented);
            return jsonString;

        }


        public async Task<string> CheckOut([FromBody] SolutionFundModel model)
        {
            try
            {
                var userId = HttpContext.Session.GetString("LoggedUser");
                if (userId == null)
                {
                    userId = "";
                }
                model.UserId = userId;
                //var result = JsonConvert.DeserializeObject<MileStoneViewModel>(data);
                var checkOutResponse = await _apiRepository.MakeApiCallAsync("api/Client/CheckOut", HttpMethod.Post, model);
                return checkOutResponse;
            }
            catch (Exception ex)
            {

            }

            return "Something went wrong";
        }

        //GetCheckoutMileStoneData
        [HttpPost]
        public async Task<string> GetCheckoutMileStoneData([FromBody] MileStoneDetailsViewModel model)
        {
            var userId = HttpContext.Session.GetString("LoggedUser");
            if (userId == null)
            {
                userId = "";
            }
            model.UserId = userId;
            var milestoneData = await _apiRepository.MakeApiCallAsync("api/Client/GetCheckoutMileStoneData", HttpMethod.Post, model);
            return milestoneData;
        }

        public IActionResult CheckoutSuccess()
        {
            return View();
        }

        public IActionResult CheckoutCancel()
        {
            return View();
        }

        //GetUserSuccessCheckoutDetails
        [HttpPost]
        public async Task<string> GetUserSuccessCheckoutDetails([FromBody] RevoultCheckOutViewModel model)
        {
            var userId = HttpContext.Session.GetString("LoggedUser");
            if (userId == null)
            {
                userId = "";
            }
            model.UserId = userId;
            var userData = await _apiRepository.MakeApiCallAsync("api/Client/GetUserSuccessCheckoutDetails", HttpMethod.Post, model);
            return userData;
        }

        //GetUserCancelCheckoutDetails
        [HttpPost]
        public async Task<string> GetUserCancelCheckoutDetails([FromBody] MileStoneIdViewModel model)
        {
            var userId = HttpContext.Session.GetString("LoggedUser");
            if (userId == null)
            {
                userId = "";
            }
            model.UserId = userId;
            var userData = await _apiRepository.MakeApiCallAsync("api/Client/GetUserCancelCheckoutDetails", HttpMethod.Post, model);
            return userData;
        }

        //SaveProjectInitiated
        [HttpPost]
        public async Task<string> SaveProjectInitiated([FromBody] SolutionFundModel model)
        {
            
            var userId = HttpContext.Session.GetString("LoggedUser");
            var preferredcurrency = HttpContext.Session.GetString("ClientPreferredCurrency");
            if (userId == null)
            {
                userId = "";
            }
            model.ClientId = userId;
            model.ClientPreferredCurrency = preferredcurrency;
            var userData = await _apiRepository.MakeApiCallAsync("api/Client/SaveProjectInitiated", HttpMethod.Post, model);

            dynamic data = JsonConvert.DeserializeObject(userData);
            if (data.Message == "CompleteProcess")
            {
                if (data.Result.RevoultToken != "")
                {
                    string SolutionFundId = data.Result.SolutionFundId;
                    var token = data.Result.RevoultToken;
                    string stringToken = token.ToString();
                    string[] SplittokenandOrderId = stringToken.Split('|');
                    var revoulttoken = SplittokenandOrderId[0];
                    var OrderId = SplittokenandOrderId[1];
                    HttpContext.Session.SetString("RevoultToken", revoulttoken);
                    HttpContext.Session.SetString("RevoultOrderId", OrderId);
                    HttpContext.Session.SetString("SolutionFundId", SolutionFundId);
                }
            }

            return userData;
        }

        [HttpPost]
        public async Task<string> UpdateUserActiveData([FromBody]CalendarData model)
        {
            
            var userId = HttpContext.Session.GetString("LoggedUser");
            if (userId == null)
            {
                userId = "";
            }
            model.Id = userId;
            var Response = await _apiRepository.MakeApiCallAsync("api/User/UpdateUserActiveDetails", HttpMethod.Post, model);

            return Response;
        }

        //RaiseDispute
        [HttpPost]
        public async Task<string> RaiseDispute([FromBody] SolutionFundModel model)
        {
            var userId = HttpContext.Session.GetString("LoggedUser");
            if (userId == null)
            {
                userId = "";
            }
            model.ClientId = userId;
            var Data = await _apiRepository.MakeApiCallAsync("api/Client/RaiseDispute", HttpMethod.Post, model);
            dynamic jsonObj = JsonConvert.DeserializeObject(Data);

            if (jsonObj["Message"] == "Dispute Raised")
            {
                string body = System.IO.File.ReadAllText(_rootPath + "/EmailTemplates/DisputeTemplate.html");
                var result = jsonObj.Result;
                var receiverEmailId = result.AdminEmailId.Value;
                var contractId = result.ContractId.Value.ToString();
                body = body.Replace("{{ contract_Id }}", contractId);
                body = body.Replace("{{ client_name }}", result.ClientName.Value);
                body = body.Replace("{{ solution_name }}", result.SolutionName.Value);
                body = body.Replace("{{ Industry_name }}", result.IndustryName.Value);

                bool send = SendEmailHelper.SendEmail(receiverEmailId, "Dispute Raised", body);

                if (!send)
                {
                    return "Dispute email not send.";
                }
            }


            return Data;
        }

        public ActionResult ActiveProject(int? Service, int? Solution, int? Industry)
        {
            ViewData["Solution"] = Solution;
            ViewData["Industry"] = Industry;
            ViewData["Service"] = Service;
            return View();
        }

        //SaveActiveProjectDocumentsDetails
        [HttpPost]
        public async Task<string> SaveActiveProjectDocumentsDetails(IFormFile httpPostedFileBase, string ActiveProjectsDocumentDetails)
        {
            try
            {
                var result = JsonConvert.DeserializeObject<ActiveProjectDocumentViewModel>(ActiveProjectsDocumentDetails);
                var UserId = HttpContext.Session.GetString("LoggedUser");
                if (User == null)
                {
                    return "User Not Found";
                }
                result.ClientId = UserId;
                var Documentsdata = await _apiRepository.MakeApiCallAsync("api/Client/SaveActiveProjectDocumentsDetails", HttpMethod.Post, result);

                dynamic data = JsonConvert.DeserializeObject(Documentsdata);
                if (data.Message == "Save Successfully!")
                {
                    if (data != null)
                    {
                        int Id = data.Result;
                        var DocumentDetails = await SaveActiveDocumentFile(httpPostedFileBase, Id);
                        var ok = await _apiRepository.MakeApiCallAsync("api/Client/SaveActiveProjectDocumentsDetails", HttpMethod.Post, DocumentDetails);
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
                else
                {
                    return data.Message;
                }

                return "abc";


            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public async Task<ActiveProjectDocumentViewModel> SaveActiveDocumentFile(IFormFile DocumentFile, object Id)
        {
            ActiveProjectDocumentViewModel DocumentDetails = new ActiveProjectDocumentViewModel();
            try
            {
                if (DocumentFile != null && DocumentFile.Length > 0)
                {
                    string BlobStorageBaseUrl = string.Empty;
                    string DocumentPath = string.Empty;
                    string DocumentUrlWithSas = string.Empty;

                    string fileName = Guid.NewGuid().ToString() + "_" + DocumentFile.FileName;

                    // Get the Azure Blob Storage connection string from configuration
                    var connectionString = _configuration.GetConnectionString("AzureBlobStorage");

                    BlobServiceClient blobServiceClient = new BlobServiceClient(connectionString);
                    BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(ContainerName);

                    BlobClient blobClient = containerClient.GetBlobClient(fileName);

                    using (var stream = DocumentFile.OpenReadStream())
                    {
                        await blobClient.UploadAsync(stream, overwrite: true);
                    }

                    DocumentPath = blobClient.Uri.ToString();


                    string sasToken = GenerateSasToken(DocumentPath);


                    string cvUrlWithSas = DocumentFile + sasToken;



                    BlobStorageBaseUrl = containerClient.Uri.ToString();


                    DocumentUrlWithSas = cvUrlWithSas;

                    DocumentDetails.DocumentBlobStorageBaseUrl = BlobStorageBaseUrl;
                    DocumentDetails.DocumentPath = DocumentPath;
                    DocumentDetails.DocumentUrlWithSas = DocumentUrlWithSas;
                    DocumentDetails.DocumentName = DocumentFile.FileName;
                    DocumentDetails.Id = (int)(Id);

                    return DocumentDetails;
                }
                else
                {
                    ModelState.AddModelError("ImageFile", "Please select an image file.");
                }
            }
            catch (Exception ex)
            {

            }
            return DocumentDetails;


        }

        //GetSolutionIndustry
        [HttpPost]
        public async Task<string> GetSolutionIndustry([FromBody] SolutionFundModel model)
        {
            var data = await _apiRepository.MakeApiCallAsync("api/Client/GetSolutionIndustry", HttpMethod.Post, model);
            return data;
        }

        //DeleteActiveSolutionDocument
        [HttpPost]
        public async Task<string> DeleteActiveSolutionDocument([FromBody] ActiveProjectDocumentViewModel model)
        {
            var data = await _apiRepository.MakeApiCallAsync("api/Client/DeleteActiveSolutionDocument", HttpMethod.Post, model);
            return data;
        }

        //StopActiveProject
        [HttpPost]
        public async Task<string> StopActiveProject([FromBody] SolutionFundModel model)
        {
            var userId = HttpContext.Session.GetString("LoggedUser");
            if (userId == null)
            {
                userId = "";
            }
            model.ClientId = userId;
            var data = await _apiRepository.MakeApiCallAsync("api/Client/StopActiveProject", HttpMethod.Post, model);
            return data;
        }

        //GetAllFeedbacks
        [HttpPost]
        public async Task<string> GetAllFeedbacks([FromBody] SolutionFundModel model)
        {
            var data = await _apiRepository.MakeApiCallAsync("api/Client/GetAllFeedbacks", HttpMethod.Post, model);
            return data;
        }

        //OpenClientWorkingHoursPopUp
        [HttpGet]
        public async Task<string> GetClientWorkingHours()
        {
            var userId = HttpContext.Session.GetString("LoggedUser");
            if(userId == null)
            {
                return "Please login to Initiate Project";
            }
            GetUserProfileRequestModel model = new GetUserProfileRequestModel();
            model.UserId = userId;
            var data = await _apiRepository.MakeApiCallAsync("api/Client/GetClientWorkingHours", HttpMethod.Post, model);
            return data;
        }

    }
}
