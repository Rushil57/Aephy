using Aephy.WEB.Admin.Models;
using Microsoft.AspNetCore.Mvc;

namespace Aephy.WEB.Admin.Controllers
{
    public class LookupController : Controller
    {
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
        public async Task<JsonResult> AddServiceForm([FromBody] ServicesModel ServiceData)
        {
            if (ServiceData != null) { 
            return Json(new { message = "you have selected ServiceName= " + ServiceData.ServiceName + " And Isactive= "+ServiceData.isActive });
            }
            else
            {
                return Json(new { message = "failed to receive data.." });
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
