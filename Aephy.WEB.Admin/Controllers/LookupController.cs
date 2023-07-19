using Aephy.WEB.Admin.Models;
using Aephy.WEB.Provider;
using Microsoft.AspNetCore.Mvc;

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
        public async Task<JsonResult> AddIndustriesForm([FromBody] IndustriesModel IndustryData)
        {
            if (IndustryData != null)
            {
                return Json(new { message = "you have selected IndustriesName= " + IndustryData.IndustryName + " And Isactive= " + IndustryData.isActive });
            }
            else
            {
                return Json(new { message = "failed to receive data.." });
            }
        }
    }
}
