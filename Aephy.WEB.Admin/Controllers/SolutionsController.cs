using Aephy.WEB.Admin.Models;
using Aephy.WEB.Provider;
using Microsoft.AspNetCore.Mvc;

namespace Aephy.WEB.Admin.Controllers
{
    public class SolutionsController : Controller
    {
        private readonly IApiRepository _apiRepository;
        public SolutionsController(IApiRepository apiRepository)
        {
            _apiRepository = apiRepository;
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
        public async Task<string> AddorEditSolution([FromBody] SolutionsModel solutionModel)
        {
            var solutionData = await _apiRepository.MakeApiCallAsync("api/Admin/AddorEditSolutionData", HttpMethod.Post, solutionModel);
            return solutionData;
        }

        
        [HttpGet]
        public async Task<string> GetSolutionsList()
        {
            var serviceList = await _apiRepository.MakeApiCallAsync("api/Admin/SolutionList", HttpMethod.Get);
            return serviceList;
        }

        
            [HttpPost]
        public async Task<string> DeleteSolutions([FromBody] SolutionsModel solutionsModel)
        {
            try
            {
                if (solutionsModel != null)
                {
                    var serviceData = await _apiRepository.MakeApiCallAsync("api/Admin/DeleteSolutionById", HttpMethod.Post, solutionsModel);
                    return serviceData;
                }
                else
                {
                    return "failed to receive data..";
                }
            }
            catch(Exception ex)
            {

            }
            return "";
           
        }

        //GetSolutiondataById
        [HttpPost]
        public async Task<string> GetSolutiondataById([FromBody] SolutionsModel solutionsModel)
        {
            var serviceList = await _apiRepository.MakeApiCallAsync("api/Admin/SolutionDataById", HttpMethod.Post, solutionsModel);
            return serviceList;
        }
    }
}
