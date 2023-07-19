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
        public IActionResult Industries() {
            return View();
        }

        [HttpPost]
        public async Task<string> AddServiceForm([FromBody] ServicesModel ServiceData)
        {
            if (ServiceData != null) 
            {
                var serviceData = await _apiRepository.MakeApiCallAsync("api/Admin/SaveServices", HttpMethod.Post, ServiceData);
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
