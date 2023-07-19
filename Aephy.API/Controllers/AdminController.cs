using Aephy.API.DBHelper;
using Aephy.API.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using static Aephy.API.Models.AdminViewModel;
using static Azure.Core.HttpHeader;

namespace Aephy.API.Controllers
{
    [Route("api/Admin/")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly AephyAppDbContext _db;
        public AdminController(AephyAppDbContext dbContext)
        {
            _db = dbContext;
        }

        [HttpPost]
        [Route("SaveServices")]
        public async Task<IActionResult> SaveServices([FromBody] ServicesModel model)
        {
            try
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new APIResponseModel { StatusCode = StatusCodes.Status403Forbidden, Message = "Something Went Wrong." });
            }
            catch (Exception ex)
            {
               return StatusCode(StatusCodes.Status500InternalServerError, new APIResponseModel { StatusCode = StatusCodes.Status403Forbidden, Message = "Something Went Wrong." });
            }
        }

    }
}
