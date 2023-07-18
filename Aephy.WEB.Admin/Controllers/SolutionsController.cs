using Aephy.WEB.Admin.Models;
using Microsoft.AspNetCore.Mvc;

namespace Aephy.WEB.Admin.Controllers
{
    public class SolutionsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Solution() {
            return View();
        }
        [HttpPost]
        public async Task<JsonResult> addSolutionForm([FromBody]SolutionsModel solutionModel) {
            if (solutionModel != null)
            {
                return Json(new { message = "you have selected \n Solution Title : " + solutionModel.SolutionTitle + "\n Solution Subtitle : " + solutionModel.Solution_SubTitle + "\n Solution Description : " + solutionModel.SolutionDescription + "\n Solution Image : " + solutionModel.SolutionImage + "\n Services : " + solutionModel.Services + "\n Industries : " + solutionModel.Industries });
            }
            else
            {
                return Json(new { message = "failed to receive data.." });
            }
        }
    }
}
