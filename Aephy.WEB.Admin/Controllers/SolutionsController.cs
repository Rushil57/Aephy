using Aephy.WEB.Admin.Models;
using Aephy.WEB.Provider;
using Azure.Core.Pipeline;
using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Diagnostics.Metrics;
using System.Reflection.Metadata;
using System.Text.RegularExpressions;
using static Azure.Core.HttpHeader;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Aephy.WEB.Admin.Controllers
{
    public class SolutionsController : Controller
    {
        private readonly IApiRepository _apiRepository;
        private const string ContainerName = "profileimages";
        private readonly IConfiguration _configuration;
        private readonly string _connectionString;

        public SolutionsController(IConfiguration configuration, IApiRepository apiRepository)
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

        public IActionResult DescribeSolution()
        {
            return View();
        }
        [HttpPost]
        public async Task<string> AddorEditSolution(IFormFile httpPostedFileBase, string SolutionData)
        {
            try
            {
                var result = JsonConvert.DeserializeObject<SolutionsModel>(SolutionData);
                IFormFile imageFile = httpPostedFileBase;

                var solutionData = await _apiRepository.MakeApiCallAsync("api/Admin/AddorEditSolutionData", HttpMethod.Post, result);
                if (solutionData != "")
                {
                    dynamic data = JsonConvert.DeserializeObject(solutionData);
                    if (data["StatusCode"] == 200)
                    {
                        if (result.Id == 0)
                        {

                            int Id = data.Result;
                            var fileData = await SaveImageFile(imageFile, Id);
                            await _apiRepository.MakeApiCallAsync("api/Admin/UpdateImage", HttpMethod.Post, fileData);

                        }
                        else
                        {
                            if (imageFile != null)
                            {
                                int Id = result.Id;
                                string Imagepath = data.Result;
                                var editFileData = await EditImageFile(imageFile, Id, Imagepath);
                                await _apiRepository.MakeApiCallAsync("api/Admin/UpdateImage", HttpMethod.Post, editFileData);
                            }

                        }
                    }
                }

                return solutionData;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }


        [HttpGet]
        public async Task<string> GetSolutionsList()
        {
            var serviceList = await _apiRepository.MakeApiCallAsync("api/Admin/SolutionList", HttpMethod.Get);
            // Set the BlobStorageBaseUrl property for each service and generate the SAS token

            dynamic data = JsonConvert.DeserializeObject(serviceList);
            // var result = JsonConvert.DeserializeObject<SolutionsModel>(serviceList);

            try
            {
                if (data.Result != null)
                {
                    foreach (var service in data.Result)
                    {
                        string imagepath = service.ImagePath;
                        string sasToken = GenerateSasToken(imagepath);
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


        [HttpPost]
        public async Task<string> DeleteSolutions([FromBody] SolutionIdModel solutionsModel)
        {
            try
            {
                if (solutionsModel != null)
                {
                    // Get the Azure Blob Storage connection string from configuration
                    var connectionString = _configuration.GetConnectionString("AzureBlobStorage");

                    if (!string.IsNullOrEmpty(solutionsModel.ImagePath))
                    {
                        BlobServiceClient blobServiceClient = new BlobServiceClient(connectionString);
                        BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient("profileimages");

                        // Ensure that the image path contains only the blob name without the container URL
                        string blobName = Path.GetFileName(solutionsModel.ImagePath);

                        BlobClient blobClient = containerClient.GetBlobClient(blobName);

                        await blobClient.DeleteIfExistsAsync();
                    }


                    var serviceData = await _apiRepository.MakeApiCallAsync("api/Admin/DeleteSolutionById", HttpMethod.Post, solutionsModel);
                    return serviceData;
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

        [HttpGet]
        public async Task<string> GetFreelancers()
        {
            var userList = await _apiRepository.MakeApiCallAsync("api/Admin/getFreelancerByLevel", HttpMethod.Get);
            return userList;
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
                    string imagepath = data.Result.Solution.ImagePath;
                    string sasToken = GenerateSasToken(imagepath);
                    imageUrlWithSas = $"{data.Result.Solution.ImagePath}?{sasToken}";
                    data.Result.Solution.ImageUrlWithSas = imageUrlWithSas;
                }
            }
            catch (Exception ex)
            {
                return ex.Message + ex.InnerException;
            }
            string convertjsonTostring = JsonConvert.SerializeObject(data, Formatting.Indented);
            return convertjsonTostring;
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

        public async Task<SolutionImage> SaveImageFile(IFormFile imageFile, object Id)
        {
            SolutionImage solutions = new SolutionImage();
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
                    BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(ContainerName);

                    BlobClient blobClient = containerClient.GetBlobClient(fileName);

                    using (var stream = imageFile.OpenReadStream())
                    {
                        await blobClient.UploadAsync(stream, overwrite: true);
                    }

                    ImagePath = blobClient.Uri.ToString();


                    string sasToken = GenerateSasToken(ImagePath);


                    string imageUrlWithSas = ImagePath + sasToken;



                    BlobStorageBaseUrl = containerClient.Uri.ToString();


                    ImageUrlWithSas = imageUrlWithSas;

                    solutions.BlobStorageBaseUrl = BlobStorageBaseUrl;
                    solutions.ImagePath = ImagePath;
                    solutions.ImageUrlWithSas = ImageUrlWithSas;
                    solutions.HasImageFile = true;
                    if(Id is string)
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

        public async Task<SolutionImage> EditImageFile(IFormFile imageFile, object Id, string Imagepath)
        {
            SolutionImage solutions = new SolutionImage();
            if (imageFile != null && imageFile.Length > 0)
            {
                if (!string.IsNullOrEmpty(Imagepath))
                {
                    BlobServiceClient updateBlobServiceClient = new BlobServiceClient(_connectionString);
                    BlobContainerClient updateContainerClient = updateBlobServiceClient.GetBlobContainerClient("profileimages");

                    BlobClient updateBlobClient = updateContainerClient.GetBlobClient(Path.GetFileName(Imagepath));

                    await updateBlobClient.DeleteIfExistsAsync();
                }
                var newfileName = Regex.Replace(imageFile.FileName, @"[^a-zA-Z]", "");
                string fileName = Guid.NewGuid().ToString() + "_" + newfileName;

                BlobServiceClient createBlobServiceClient = new BlobServiceClient(_connectionString);
                BlobContainerClient createContainerClient = createBlobServiceClient.GetBlobContainerClient("profileimages");

                BlobClient createBlobClient = createContainerClient.GetBlobClient(fileName);

                using (var stream = imageFile.OpenReadStream())
                {
                    await createBlobClient.UploadAsync(stream, overwrite: true);
                }

                Imagepath = createBlobClient.Uri.ToString();
                solutions.HasImageFile = false;
                solutions.Id = (int)Id;
                solutions.ImagePath = Imagepath;
            }

            return solutions;


        }


        //New
        [HttpGet]
        public async Task<string> GetSolutionsIndustryDetailList()
        {
            var serviceList = await _apiRepository.MakeApiCallAsync("api/Admin/SolutionDetailsList", HttpMethod.Get);


            dynamic data = JsonConvert.DeserializeObject(serviceList);
            // var result = JsonConvert.DeserializeObject<SolutionsModel>(serviceList);

            try
            {
                if (data.Result != null)
                {
                    foreach (var service in data.Result)
                    {
                        if (service.ImagePath != null)
                        {
                            string imagepath = service.ImagePath;
                            string sasToken = GenerateSasToken(imagepath);
                            string imageUrlWithSas = $"{service.ImagePath}?{sasToken}";
                            service.ImageUrlWithSas = imageUrlWithSas;
                        }


                    }

                }

            }
            catch (Exception ex)
            {

            }
            string jsonString = JsonConvert.SerializeObject(data, Formatting.Indented);
            return jsonString;
        }

        [HttpPost]
        public async Task<string> GetSolutionDetailsById([FromBody] SolutionIdModel solutionsModel)
        {
            var serviceList = await _apiRepository.MakeApiCallAsync("api/Admin/SolutionDetailsDataById", HttpMethod.Post, solutionsModel);

            string imageUrlWithSas = string.Empty;
            dynamic data = JsonConvert.DeserializeObject(serviceList);
            try
            {
                if (data["StatusCode"] == 200)
                {
                    string imagepath = data.Result.Solution.ImagePath;
                    if (imagepath != null)
                    {
                        string sasToken = GenerateSasToken(imagepath);
                        imageUrlWithSas = $"{data.Result.Solution.ImagePath}?{sasToken}";
                        data.Result.Solution.ImageUrlWithSas = imageUrlWithSas;
                    }

                    string industryimage = data.Result.SolutionIndustryDetails.ImagePath;
                    if (industryimage != null)
                    {
                        string tokn = GenerateSasToken(industryimage);
                        var imageUrlWithSass = $"{data.Result.SolutionIndustryDetails.ImagePath}?{tokn}";
                        data.Result.SolutionIndustryDetails.ImageUrlWithSas = imageUrlWithSass;
                    }

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
        public async Task<string> AddorEditSolutionDetails(IFormFile httpPostedFileBase, string SolutionData)
        {
            try
            {
                var result = JsonConvert.DeserializeObject<SolutionDescribeModel>(SolutionData);
                var solutionData = await _apiRepository.MakeApiCallAsync("api/Admin/AddSolutionDescribedData", HttpMethod.Post, result);
                if (solutionData != "")
                {
                    dynamic data = JsonConvert.DeserializeObject(solutionData);
                    if (data["StatusCode"] == 200)
                    {
                        if (httpPostedFileBase != null)
                        {
                            int Id = result.Id;
                            var fileData = await SaveImageFile(httpPostedFileBase, Id);
                            var ok = await _apiRepository.MakeApiCallAsync("api/Admin/SolutionIndustriesUpdateImage", HttpMethod.Post, fileData);
                        }

                    }
                }
                return solutionData;
            }
            catch (Exception ex)
            {

            }

            return "";
        }

        [HttpPost]
        public async Task<string> ActionByAdminOnSolution(string solutionIndustryDetailsId, string actionType)
        {
            try
            {
                var solutionData = await _apiRepository.MakeApiCallAsync("api/Admin/ActionByAdminOnSolution?solutionIndustryDetailsId=" + solutionIndustryDetailsId + "&action=" + actionType, HttpMethod.Post, null);

                return solutionData;
            }
            catch (Exception ex)
            {

            }

            return "";
        }

        public async Task<string> GetFreeLancerByType(string freeLancerLavel)
        {
            var userList = await _apiRepository.MakeApiCallAsync("api/User/GetFreeLancerByType", HttpMethod.Post, freeLancerLavel);
            dynamic data = JsonConvert.DeserializeObject(userList);
            string jsonString = JsonConvert.SerializeObject(data, Formatting.Indented);
            return jsonString;
        }

        [HttpPost]
        public async Task<string> GetUsersByIds([FromBody] UserIdsModel userIdsModel)
        {
            var userList = await _apiRepository.MakeApiCallAsync("api/User/GetFreeLancerByIds", HttpMethod.Post, userIdsModel);
            dynamic data = JsonConvert.DeserializeObject(userList);
            string jsonString = JsonConvert.SerializeObject(data, Formatting.Indented);
            return jsonString;
        }

        [HttpPost]
        public async Task<string> GetSolutionDefineData([FromBody] SolutionDefineRequestViewModel model)
        {
            var aprroveList = await _apiRepository.MakeApiCallAsync("api/Admin/GetSolutionDefineData", HttpMethod.Post, model);
            return aprroveList;
        }

        [HttpGet]
        public async Task<string> GetFreelancersName()
        {
            var freelancerList = await _apiRepository.MakeApiCallAsync("api/Admin/GetFreelancersNameList", HttpMethod.Get);
            return freelancerList;
        }

        [HttpPost]
        public async Task<string> SaveTopProfessionalData(IFormFile httpPostedFileBase, string TopProfessionalData)
        {
            var result = JsonConvert.DeserializeObject<SolutionTopProfessionalViewModel>(TopProfessionalData);
            var Data = await _apiRepository.MakeApiCallAsync("api/Admin/AddTopProfessionalData", HttpMethod.Post, result);
            dynamic data = JsonConvert.DeserializeObject(Data);
            if (data["Message"] != "indexOverflow".Trim())
            {
                if (httpPostedFileBase != null)
                {
                    var fileData = await SaveImageFile(httpPostedFileBase, result.FreelancerId);
                    await _apiRepository.MakeApiCallAsync("api/Admin/UpdateUserProfileImage", HttpMethod.Post, fileData);
                }

            }
            return Data;
        }

        [HttpPost]
        public async Task<string> GetTopProfessionalList([FromBody] SolutionIndustryViewModel model)
        {
            var freelancerList = await _apiRepository.MakeApiCallAsync("api/Admin/GetProfessionalList", HttpMethod.Post, model);
            return freelancerList;
        }

        [HttpPost]
        public async Task<string> DeleteTopProfessional([FromBody] SolutionIdModel model)
        {
            try
            {
                if (model != null)
                {
                    var data = await _apiRepository.MakeApiCallAsync("api/Admin/DeleteTopProfessionalData", HttpMethod.Post, model);
                    return data;
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
        public async Task<string> AddSuccessfullProjectResult([FromBody] SolutionSuccessfullProjectResultViewModel model)
        {
            var list = await _apiRepository.MakeApiCallAsync("api/Admin/SaveSuccessfullProjectResult", HttpMethod.Post, model);
            return list;
        }

        [HttpPost]
        public async Task<string> AddSuccessfullProject([FromBody] SolutionSuccessfullProjectViewModel model)
        {
            var data = await _apiRepository.MakeApiCallAsync("api/Admin/SaveSuccessfullProject", HttpMethod.Post, model);
            return data;
        }

        [HttpPost]
        public async Task<string> GetSuccessfullProjectList([FromBody] SolutionIndustryViewModel model)
        {
            var list = await _apiRepository.MakeApiCallAsync("api/Admin/GetSuccessfullProjectList", HttpMethod.Post, model);
            return list;
        }

        [HttpPost]
        public async Task<string> GetProjectResultList([FromBody] SolutionIndustryViewModel model)
        {
            var list = await _apiRepository.MakeApiCallAsync("api/Admin/GetProjectResultList", HttpMethod.Post, model);
            return list;
        }

        [HttpPost]
        public async Task<string> DeleteProjectResult([FromBody] SolutionIdModel model)
        {
            try
            {
                if (model != null)
                {
                    var data = await _apiRepository.MakeApiCallAsync("api/Admin/DeleteProjectResultData", HttpMethod.Post, model);
                    return data;
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
        public async Task<string> DeleteSuccessfullProject([FromBody] SolutionIdModel model)
        {
            try
            {
                if (model != null)
                {
                    var data = await _apiRepository.MakeApiCallAsync("api/Admin/DeleteSuccessfullProjectData", HttpMethod.Post, model);
                    return data;
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
        public async Task<string> GetSuccessfullProjectDetailsById([FromBody] SolutionIdModel model)
        {
            if (model != null)
            {
                var data = await _apiRepository.MakeApiCallAsync("api/Admin/GetSuccessfullProjectDetailsById", HttpMethod.Post, model);
                return data;
            }
            else
            {
                return "failed to receive data..";
            }
        }

        [HttpPost]
        public async Task<string> GetProjectResultById([FromBody] SolutionIdModel model)
        {
            if (model != null)
            {
                var data = await _apiRepository.MakeApiCallAsync("api/Admin/GetProjectDetailsById", HttpMethod.Post, model);
                return data;
            }
            else
            {
                return "failed to receive data..";
            }
        }

        [HttpPost]
        public async Task<string> RemoveMileStoneData([FromBody] SolutionIdModel model)
        {
            if (model.Id != 0)
            {
                var data = await _apiRepository.MakeApiCallAsync("api/Freelancer/DeleteMileStoneById", HttpMethod.Post, model);
                return data;

            }
            else
            {
                return "failed to receive data..";
            }
        }


        [HttpPost]
        public async Task<string> RemoveHighlightData([FromBody] SolutionIdModel model)
        {
            if (model.Id != 0)
            {
                var data = await _apiRepository.MakeApiCallAsync("api/Freelancer/DeletePointsById", HttpMethod.Post, model);
                return data;

            }
            else
            {
                return "failed to receive data..";
            }
        }


        [HttpPost]
        public async Task<string> GetTopProfessionalDetails([FromBody] SolutionIdModel model)
        {
            if (model != null)
            {
                var professionadata = await _apiRepository.MakeApiCallAsync("api/Admin/GetTopProfessionalDetailsById", HttpMethod.Post, model);
                dynamic data = JsonConvert.DeserializeObject(professionadata);
                try
                {
                    if (data["Message"] == "Success".Trim())
                    {
                        string imagepath = data.Result.FreelancerData.ImagePath;
                        if(imagepath != null)
                        {
                            string sasToken = GenerateSasToken(imagepath);
                            var imageUrlWithSas = $"{data.Result.FreelancerData.ImagePath}?{sasToken}";
                            data.Result.FreelancerData.ImageUrlWithSas = imageUrlWithSas;
                        }

                    }
                }
                catch (Exception ex)
                {
                    return ex.Message + ex.InnerException;
                }
                string convertjsonTostring = JsonConvert.SerializeObject(data, Formatting.Indented);
                return convertjsonTostring;
            }
            else
            {
                return "failed to receive data..";
            }
        }

        public ActionResult DisputeRaised()
        {
            return View();
        }

        [HttpGet]
        public async Task<string> GetDisputeList()
        {
            var disputeList = await _apiRepository.MakeApiCallAsync("api/Admin/GetDisputeList", HttpMethod.Get);
            return disputeList;
        }

        //GetDisputeData
        [HttpPost]
        public async Task<string> GetDisputeData([FromBody] SolutionDisputeModel model)
        {
            var disputeData = await _apiRepository.MakeApiCallAsync("api/Admin/GetDisputeData", HttpMethod.Post, model);
            return disputeData;
        }

        //GetFreelancerConnectedId
        [HttpPost]
        public async Task<string> GetFreelancerConnectedId([FromBody] SolutionDisputeModel model)
        {
            var disputeData = await _apiRepository.MakeApiCallAsync("api/Admin/GetFreelancerConnectedId", HttpMethod.Post, model);
            return disputeData;
        }

        //RefundUserAmount
        [HttpPost]
        public async Task<string> RefundUserAmount([FromBody] SolutionDisputeModel model)
        {
            var userdata = await _apiRepository.MakeApiCallAsync("api/Admin/RefundUserAmount", HttpMethod.Post, model);
            return userdata;
        }

        //DisputeResolved
        [HttpPost]
        public async Task<string> DisputeResolved([FromBody] SolutionDisputeModel model)
        {
            var Disputedata = await _apiRepository.MakeApiCallAsync("api/Admin/DisputeResolved", HttpMethod.Post, model);
            return Disputedata;
        }

        //GetContractFreelancerList
        [HttpPost]
        public async Task<string> GetContractFreelancerList([FromBody] SolutionDisputeModel model)
        {
            var Disputedata = await _apiRepository.MakeApiCallAsync("api/Admin/GetContractFreelancerList", HttpMethod.Post, model);
            return Disputedata;
        }

        public ActionResult ActiveProjects()
        {
            return View();
        }

        //GetActiveProjectList
        [HttpGet]
        public async Task<string> GetActiveProjectList()
        {
            var disputeList = await _apiRepository.MakeApiCallAsync("api/Admin/GetActiveProjectList", HttpMethod.Get);
            return disputeList;
        }

        //StopClientPayment
        [HttpPost]
        public async Task<string> StopClientPayment([FromBody] SolutionDisputeModel model)
        {
            var clientPayment = await _apiRepository.MakeApiCallAsync("api/Admin/StopClientPayment", HttpMethod.Post,model);
            return clientPayment;
        }

        //GetClientDetailsForRefund
        [HttpPost]
        public async Task<string> GetClientDetailsForRefund([FromBody] SolutionDisputeModel model)
        {
            var clientData = await _apiRepository.MakeApiCallAsync("api/Admin/GetClientDetailsForRefund", HttpMethod.Post, model);
            return clientData;
        }

        //RefundClientAmount
        [HttpPost]
        public async Task<string> RefundClientAmount([FromBody] SolutionDisputeModel model)
        {
            var clientTransfertData = await _apiRepository.MakeApiCallAsync("api/Admin/RefundClientAmount", HttpMethod.Post, model);
            return clientTransfertData;
        }


    }
}


