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

        public IActionResult Solutions()
        {
            return View();
        }

        [HttpPost]
        public async Task<string> AddServiceForm([FromBody] ServicesModel ServiceData)
        {
            if (ServiceData != null)
            {
                var serviceData = await _apiRepository.MakeApiCallAsync("api/Admin/AddorEditServices", HttpMethod.Post, ServiceData);
                if(serviceData != "")
                {
                    return serviceData;
                }
                else
                {
                    return "failed to save data..";
                }
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

        [HttpPost]
        public async Task<string> GetServicesById([FromBody] ServicesModel ServiceData)
        {
            var serviceListById = await _apiRepository.MakeApiCallAsync("api/Admin/GetServicesById", HttpMethod.Post, ServiceData);
            return serviceListById;
        }


        [HttpPost]
        public async Task<string> DeleteServices([FromBody] ServicesModel ServiceData)
        {
            if (ServiceData != null)
            {
                var serviceData = await _apiRepository.MakeApiCallAsync("api/Admin/DeleteServicesById", HttpMethod.Post, ServiceData);
                if(serviceData != "")
                {
                    return serviceData;
                }
                else
                {
                    return "Failed to delete data";
                }
                
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
                var industryData = await _apiRepository.MakeApiCallAsync("api/Admin/AddIndustries", HttpMethod.Post, IndustryData);
                return industryData;
            }
            else
            {
                return "failed to receive data..";
            }
        }
        public async Task<string> GetIndustries()
        {
            var industryData = await _apiRepository.MakeApiCallAsync("api/Admin/IndustriesList", HttpMethod.Post);
            return industryData;
        }

        [HttpPost]
        public async Task<string> GetIndustriesRecord([FromBody] IndustriesModel IndustryData)
        {
            var industryrecord = await _apiRepository.MakeApiCallAsync("api/Admin/GetIndustryById", HttpMethod.Post, IndustryData);
            return industryrecord;
        }

        [HttpPost]
        public async Task<string> DeleteIndustry([FromBody] IndustriesModel IndustryData)
        {
            var industryrecord = await _apiRepository.MakeApiCallAsync("api/Admin/DeleteIndustryById", HttpMethod.Post, IndustryData);
            return industryrecord;
        }
    }
}
