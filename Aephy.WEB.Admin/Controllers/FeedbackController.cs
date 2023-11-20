using Aephy.WEB.Admin.Models;
using Aephy.WEB.Provider;
using Microsoft.AspNetCore.Mvc;
using System.Runtime.CompilerServices;

namespace Aephy.WEB.Admin.Controllers
{
    public class FeedbackController : Controller
    {
        private readonly IApiRepository _apiRepository;

        public FeedbackController(IConfiguration configuration, IApiRepository apiRepository)
        {
            _apiRepository = apiRepository;
        }
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Feedback()
        {
            return View();
        }

        public async Task<string> GetFreelancers()
        {
            var FreelancerList = await _apiRepository.MakeApiCallAsync("api/Admin/GetFreelancersNameList", HttpMethod.Get);
            return FreelancerList;
        }

        [HttpPost]
        public async Task<string> SaveFreelancerReviewData([FromBody] FreelancerReview model)
        {
            if (model != null)
            {
                var userId = HttpContext.Session.GetString("LoggedAdmin");
                if (userId != null)
                {
                    model.UserId = userId;
                    model.CreateDateTime = DateTime.Now;
                    var response = await _apiRepository.MakeApiCallAsync("api/Admin/SaveAdminToFreelancerReview", HttpMethod.Post, model);
                    return response;
                }
            }
            return "Failed to submit feedback !!";
        }

        //CheckAdminToFreelancerReviewExits
        [HttpPost]
        public async Task<string> CheckAdminToFreelancerReviewExits([FromBody] string? FreelancerId)
        {
            if (FreelancerId != null)
            {
                try
                {
                    var userId = HttpContext.Session.GetString("LoggedAdmin");
                    FreelancerReview model = new FreelancerReview();
                    model.UserId = userId;
                    model.FreelancerId = FreelancerId;
                    var data = await _apiRepository.MakeApiCallAsync("api/Admin/CheckAdminToFreelancerReviewExits", HttpMethod.Post, model);
                    return data;
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
            return "";
        }
    }
}
