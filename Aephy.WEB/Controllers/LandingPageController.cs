using Aephy.WEB.Models;
using Aephy.WEB.Provider;
using Azure.Storage.Blobs;
using Azure.Storage.Sas;
using Azure.Storage;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using static System.Net.Mime.MediaTypeNames;
using System.Reflection;

namespace Aephy.WEB.Controllers
{
    public class LandingPageController : Controller
    {
        private readonly IApiRepository _apiRepository;
        private const string ContainerName = "cvfiles";
        private const string ImageContainerName = "profileimages";
        private readonly IConfiguration _configuration;
        private readonly string _connectionString;

        public LandingPageController(IConfiguration configuration, IApiRepository apiRepository)
        {
            _apiRepository = apiRepository;
            _configuration = configuration;
            _connectionString = configuration.GetConnectionString("AzureBlobStorage");
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

        public ActionResult BrowseSolution()
        {

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
    }
}
