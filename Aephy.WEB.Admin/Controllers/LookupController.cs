using Aephy.WEB.Admin.Models;
using Aephy.WEB.Provider;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Aephy.WEB.Admin.Controllers
{
    public class LookupController : Controller
    {
        private readonly IApiRepository _apiRepository;
        public LookupController(IApiRepository apiRepository)
        {
            _apiRepository = apiRepository;
        }
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Services()
        {
            return View();
        }
        public IActionResult Industries()
        {
            return View();
        }

        [HttpPost]
        public async Task<string> AddServiceForm([FromBody] ServicesModel ServiceData)
        {
            if (ServiceData != null)
            {
                var serviceData = await _apiRepository.MakeApiCallAsync("api/Admin/AddServices", HttpMethod.Post, ServiceData);
                return serviceData;
            }
            else
            {
                return "failed to receive data..";
            }
        }


        [HttpGet]
        public async Task<string> GetServices()
        {
            var serviceList = await _apiRepository.MakeApiCallAsync("api/Admin/ServiceList", HttpMethod.Get);
            return serviceList;
        }

        [HttpGet]
        public async Task<string> GetServicesById(int Id)
        {
            var serviceListById = await _apiRepository.MakeApiCallAsync("api/Admin/GetServicesById", HttpMethod.Get, Id);
            return serviceListById;
        }


        [HttpPost]
        public async Task<string> DeleteServices([FromBody] ServicesModel ServiceData)
        {
            if (ServiceData != null)
            {
                var serviceData = await _apiRepository.MakeApiCallAsync("api/Admin/DeleteServicesById", HttpMethod.Post, ServiceData);
                return serviceData;
            }
            else
            {
                return "failed to receive data..";
            }
        }
        [HttpPost]
        public async Task<string> AddIndustriesForm([FromBody] IndustriesModel IndustryData)
        {
            var messageSatus = string.Empty;
            if (IndustryData != null)
            { 
                var industryData = await _apiRepository.MakeApiCallAsync("api/Admin/SaveIndustries", HttpMethod.Post, IndustryData);
                dynamic jsonObj = JsonConvert.DeserializeObject(industryData);
                return industryData;
            }
            else
            {
                return "failed to receive data..";
            }
        }
        public async Task<string> GetIndustries()
        {
            var industryData = await _apiRepository.MakeApiCallAsync("api/Admin/GetAllIndustries", HttpMethod.Post);
            return industryData;
        }

        [HttpPost]
        public async Task<string> GetIndustriesRecord([FromBody] IndustriesModel IndustryData)
        {
            var industryrecord = await _apiRepository.MakeApiCallAsync("api/Admin/GetIndustry", HttpMethod.Post, IndustryData);
            return industryrecord;
        }

        [HttpPost]
        public async Task<string> DeleteIndustry([FromBody] IndustriesModel IndustryData)
        {
            var industryrecord = await _apiRepository.MakeApiCallAsync("api/Admin/DeleteIndustry", HttpMethod.Post, IndustryData);
            return industryrecord;
        }
    }
}
