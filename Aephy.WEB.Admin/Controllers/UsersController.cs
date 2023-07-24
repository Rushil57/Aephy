using Aephy.WEB.Provider;
using Microsoft.AspNetCore.Mvc;

namespace Aephy.WEB.Admin.Controllers
{
    public class UsersController : Controller
    {
        private readonly IApiRepository _apiRepository;

        public UsersController(IConfiguration configuration, IApiRepository apiRepository)
        {
            _apiRepository = apiRepository;
        }
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult List()
        {
            return View();
        }

        [HttpGet]
        public async Task<string> GetUsers()
        {
            var userList = await _apiRepository.MakeApiCallAsync("api/Admin/UserList", HttpMethod.Get);
            return userList;
        }


    }
}
