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
    public class OpenGigRolesController : Controller
    {
        private readonly IApiRepository _apiRepository;
        private readonly IConfiguration _configuration;
        private readonly string _connectionString;

        public OpenGigRolesController(IConfiguration configuration, IApiRepository apiRepository)
        {
            _apiRepository = apiRepository;
            _configuration = configuration;
            _connectionString = "DefaultEndpointsProtocol=https;AccountName=aephystorageaccount;AccountKey=nEy6xh4P4m2d94iDgqq+yNB99bucjGMD1wp2L6sbsNFjHPaUQiCHgc5b4hmBmeRtYsiA/WvudVmV+AStwz3djw==;EndpointSuffix=core.windows.net";
        }
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Roles()
        {
            return View();
        }
        [HttpPost]
        public async Task<string> AddorEditRoles([FromBody]GigOpenRolesModel model)
        {
            try
            {
                var date = DateTime.Now;
                model.CreatedDateTime = date;
                var solutionData = await _apiRepository.MakeApiCallAsync("api/Admin/AddorEditRoles", HttpMethod.Post, model);
                return solutionData;
            }
            catch(Exception ex)
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
        public async Task<string> GetRolesdataById([FromBody]GigOpenRolesModel model)
        {
            var rolesList = await _apiRepository.MakeApiCallAsync("api/Admin/RolesDataById", HttpMethod.Post, model);
            return rolesList;
        }
    }
}


