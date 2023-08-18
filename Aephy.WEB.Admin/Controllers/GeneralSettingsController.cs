using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Aephy.WEB.Provider;
using Aephy.WEB.Models;

namespace Aephy.WEB.Admin.Controllers
{
    public class GeneralSettingsController : Controller
    {
        private readonly IApiRepository _apiRepository;
        private readonly string _rootPath;
        private readonly IConfiguration _configuration;
        private readonly string _connectionString;

        public GeneralSettingsController(IApiRepository apiRepository, IWebHostEnvironment hostEnvironment, IConfiguration configuration)
        {
            _apiRepository = apiRepository;
            _configuration = configuration;
            _rootPath = hostEnvironment.WebRootPath;
            _connectionString = "DefaultEndpointsProtocol=https;AccountName=aephystorageaccount;AccountKey=nEy6xh4P4m2d94iDgqq+yNB99bucjGMD1wp2L6sbsNFjHPaUQiCHgc5b4hmBmeRtYsiA/WvudVmV+AStwz3djw==;EndpointSuffix=core.windows.net";
        }

        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Settings()
        {
            return View();
        }

        [HttpPost]
        public async Task<string> SaveLevelData(string FormData)
        {
            var LevelDataList = JsonConvert.DeserializeObject<List<LevelRange>>(FormData);
            try
            {
                    var Response = await _apiRepository.MakeApiCallAsync("api/Admin/SaveLevelData", HttpMethod.Post, LevelDataList);
                    return Response;
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return "";
        }

        [HttpGet]
        public async Task<string> getSavedLevelsdata()
        {
            try
            {
                var Response = await _apiRepository.MakeApiCallAsync("api/Admin/GetSavedLevelsList", HttpMethod.Get);
                return Response;
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return "";
        }

    }
}