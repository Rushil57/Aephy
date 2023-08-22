using Aephy.Helper.Helpers;
using Aephy.WEB.Admin.Models;
using Aephy.WEB.Provider;
using Azure;
using Azure.Core.Pipeline;
using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using static System.Net.Mime.MediaTypeNames;

namespace Aephy.WEB.Admin.Controllers
{
    public class OpenGigRolesController : Controller
    {
        private readonly IApiRepository _apiRepository;
        private readonly IConfiguration _configuration;
        private readonly string _connectionString;
        private readonly string _rootPath;

        private const string ContainerName = "cvfiles";

        public OpenGigRolesController(IConfiguration configuration, IApiRepository apiRepository, IWebHostEnvironment hostEnvironment)
        {
            _apiRepository = apiRepository;
            _configuration = configuration;
            _connectionString = configuration.GetConnectionString("AzureBlobStorage");
            _connectionString = "DefaultEndpointsProtocol=https;AccountName=aephystorageaccount;AccountKey=nEy6xh4P4m2d94iDgqq+yNB99bucjGMD1wp2L6sbsNFjHPaUQiCHgc5b4hmBmeRtYsiA/WvudVmV+AStwz3djw==;EndpointSuffix=core.windows.net";
            _rootPath = hostEnvironment.WebRootPath;
        }
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Roles()
        {
            return View();
        }

        public IActionResult Applicants()
        {
            return View();
        }

        [HttpPost]
        public async Task<string> AddorEditRoles([FromBody] GigOpenRolesModel model)
        {
            try
            {
                var date = DateTime.Now;
                model.CreatedDateTime = date;
                var solutionData = await _apiRepository.MakeApiCallAsync("api/Admin/AddorEditRoles", HttpMethod.Post, model);
                return solutionData;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }


        [HttpGet]
        public async Task<string> GetRolesList()
        {
            var RolesList = await _apiRepository.MakeApiCallAsync("api/Admin/RolesList", HttpMethod.Get);
            return RolesList;
        }

        [HttpGet]
        public async Task<string> GetSolutionsList()
        {
            var solutionList = await _apiRepository.MakeApiCallAsync("api/Admin/SolutionList", HttpMethod.Get);
            return solutionList;
        }

        [HttpPost]
        public async Task<string> DeleteRoles([FromBody] SolutionIdModel rolesModel)
        {
            try
            {
                if (rolesModel != null)
                {
                    var rolesData = await _apiRepository.MakeApiCallAsync("api/Admin/DeleteRolesById", HttpMethod.Post, rolesModel);
                    return rolesData;
                }
                else
                {
                    return "failed to receive data..";
                }
            }
            catch (Exception ex)
            {

            }
            return "";

        }

        [HttpPost]
        public async Task<string> GetSolutiondataById([FromBody] SolutionIdModel solutionsModel)
        {
            var serviceList = await _apiRepository.MakeApiCallAsync("api/Admin/SolutionDataById", HttpMethod.Post, solutionsModel);

            string imageUrlWithSas = string.Empty;
            dynamic data = JsonConvert.DeserializeObject(serviceList);
            try
            {
                if (data["StatusCode"] == 200)
                {
                    string imagepath = data.Result.ImagePath;
                    string sasToken = "";
                    imageUrlWithSas = $"{data.Result.ImagePath}?{sasToken}";
                    data.Result.ImageUrlWithSas = imageUrlWithSas;
                }
            }
            catch (Exception ex)
            {
                return ex.Message + ex.InnerException;
            }
            string convertjsonTostring = JsonConvert.SerializeObject(data, Formatting.Indented);
            return convertjsonTostring;
        }

        [HttpPost]
        public async Task<string> GetRolesdataById([FromBody] GigOpenRolesModel model)
        {
            var rolesList = await _apiRepository.MakeApiCallAsync("api/Admin/RolesDataById", HttpMethod.Post, model);
            return rolesList;
        }


        [HttpGet]
        public async Task<string> GetApplicationList()
        {
            var applicationList = await _apiRepository.MakeApiCallAsync("api/Admin/GetApplicationList", HttpMethod.Get);
            return applicationList;
        }

        [HttpPost]
        public async Task<string> GetApplicantsdataById([FromBody] GigOpenRolesModel solutionsModel)
        {
            var applicationdata = await _apiRepository.MakeApiCallAsync("api/Admin/GetApplicantsdataById", HttpMethod.Post, solutionsModel);
            if (applicationdata != "")
            {
                dynamic data = JsonConvert.DeserializeObject(applicationdata);
                try
                {
                    if (data["StatusCode"] == 200)
                    {
                        string imagepath = data.Result.OpenGigRoles.CVUrlWithSas;
                        string sasToken = GenerateSasToken(imagepath);
                        var imageUrlWithSas = $"{data.Result.OpenGigRoles.CVUrlWithSas}?{sasToken}";
                        data.Result.OpenGigRoles.CVUrlWithSas = imageUrlWithSas;
                    }
                }
                catch (Exception ex)
                {
                    return ex.Message + ex.InnerException;
                }

                string convertjsonTostring = JsonConvert.SerializeObject(data, Formatting.Indented);
                return convertjsonTostring;
            }
            return applicationdata;
        }

        [HttpPost]
        public async Task<string> DeleteApplicationById([FromBody] GigOpenRolesModel solutionsModel)
        {
            var Response = await _apiRepository.MakeApiCallAsync("api/Admin/DeleteUserApplication", HttpMethod.Post,solutionsModel);
            return Response;
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


        [HttpPost]
        public async Task<string> ApproveOrRejectFreelancer([FromBody] GigOpenRolesModel solutionsModel)
        {
            var LoggedInUser = HttpContext.Session.GetString("LoggedAdmin");
            solutionsModel.CurrentLoggedInId = LoggedInUser;
            var applicationdata = await _apiRepository.MakeApiCallAsync("api/Admin/ApproveOrRejectFreelancer", HttpMethod.Post, solutionsModel);

            #region Send Application Status Email
            dynamic jsonObj = JsonConvert.DeserializeObject(applicationdata);

            string userName = Convert.ToString(jsonObj["Result"]["FirstName"]) + " " + Convert.ToString(jsonObj["Result"]["LastName"]);
            string emailAddress = Convert.ToString(jsonObj["Result"]["Email"]);

            string templateName = solutionsModel.ApproveOrReject == "Approve" ? "ApproveApplicationTemplate.html" : "RejectApplicationTemplate.html";
            string body = System.IO.File.ReadAllText(_rootPath + "/EmailTemplates/" + templateName + "");
            body = body.Replace("{{ user_name }}", userName);

            bool send = SendEmailHelper.SendEmail(emailAddress, "Application Status", body);
            #endregion

            return applicationdata;
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
    }
}


