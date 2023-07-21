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

        public IActionResult Solution()
        {
            return View();
        }
        [HttpPost]
        public async Task<string> AddorEditSolution(IFormFile httpPostedFileBase, string SolutionData)
        {
            var result = JsonConvert.DeserializeObject<SolutionsModel>(SolutionData);
            IFormFile imageFile = httpPostedFileBase;

            var solutionData = await _apiRepository.MakeApiCallAsync("api/Admin/AddorEditSolutionData", HttpMethod.Post, result);
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
                    if(imageFile != null)
                    {
                        int Id = result.Id;
                        string Imagepath = data.Result;
                        var editFileData = await EditImageFile(imageFile, Id, Imagepath);
                        await _apiRepository.MakeApiCallAsync("api/Admin/UpdateImageById", HttpMethod.Post, editFileData);
                    }

                }
            }

            return solutionData;
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
                    string sasToken = GenerateSasToken(imagepath);
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

                    string fileName = Guid.NewGuid().ToString() + "_" + imageFile.FileName;

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
                    solutions.Id = (int)(Id);

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

                string fileName = Guid.NewGuid().ToString() + "_" + imageFile.FileName;

                BlobServiceClient createBlobServiceClient = new BlobServiceClient(_connectionString);
                BlobContainerClient createContainerClient = createBlobServiceClient.GetBlobContainerClient("profileimages");

                BlobClient createBlobClient = createContainerClient.GetBlobClient(fileName);

                using (var stream = imageFile.OpenReadStream())
                {
                    await createBlobClient.UploadAsync(stream, overwrite: true);
                }

                Imagepath = createBlobClient.Uri.ToString();
                solutions.Id = (int)Id;
                solutions.ImagePath = Imagepath;
            }
               
            return solutions;


        }

    }




}


