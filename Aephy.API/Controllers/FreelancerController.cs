using Aephy.API.DBHelper;
using Aephy.API.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Data;
using System.Security.Cryptography;
using System.Security.Policy;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using static Aephy.API.Models.AdminViewModel;
using static Azure.Core.HttpHeader;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace Aephy.API.Controllers
{
	[Route("api/Freelancer/")]
	[ApiController]
	public class FreelancerController : ControllerBase
	{
		private readonly AephyAppDbContext _db;
		public FreelancerController(AephyAppDbContext dbContext)
		{
			_db = dbContext;
		}

		[HttpPost]
		[Route("OpenGigRolesApply")]
		public async Task<IActionResult> OpenGigRolesApply([FromBody] OpenGigRolesModel OpenGigRolesData)
		{
			try
			{
				var checkGigApplication = _db.OpenGigRolesApplications.Where(x => x.ID == OpenGigRolesData.ID).FirstOrDefault();
				if (checkGigApplication != null)
				{
					return StatusCode(StatusCodes.Status200OK, new APIResponseModel { StatusCode = StatusCodes.Status403Forbidden, Message = "You already have applied for this role!" });
				}
				var opengigroles_data = new OpenGigRolesApplications()
				{

					FreelancerID = OpenGigRolesData.FreelancerID,
					ServiceID = OpenGigRolesData.ServiceID,
                    IndustriesID = OpenGigRolesData.IndustriesID,
                    SolutionID = OpenGigRolesData.SolutionID,
                    Title = OpenGigRolesData.Title,
                    Level = OpenGigRolesData.Level,
                    IsApproved = OpenGigRolesData.IsApproved,
                    CreatedDateTime = OpenGigRolesData.CreatedDateTime,
                    Description = OpenGigRolesData.Description
				};

				await _db.OpenGigRolesApplications.AddAsync(opengigroles_data);
				var result = _db.SaveChanges();
				if (result != 0)
				{
					return StatusCode(StatusCodes.Status200OK, new APIResponseModel
					{
						StatusCode = StatusCodes.Status200OK,
						Message = "Applied Successfully",
						Result = opengigroles_data.ID
					});
				}
				else
				{
					return StatusCode(StatusCodes.Status500InternalServerError, new APIResponseModel { StatusCode = StatusCodes.Status403Forbidden, Message = "Something Went Wrong." });
				}
			}
			catch (Exception ex)
			{
				return StatusCode(StatusCodes.Status500InternalServerError, new APIResponseModel { StatusCode = StatusCodes.Status403Forbidden, Message = "Something Went Wrong." });
			}
		}

		[HttpPost]
		[Route("UpdateCV")]
		public async Task<IActionResult> UpdateCV([FromBody] OpenGigRolesCV OpengigroleCv)
		{
			//return StatusCode(StatusCodes.Status500InternalServerError, new APIResponseModel { StatusCode = StatusCodes.Status403Forbidden, Message = "Something Went Wrong." });
			if (OpengigroleCv.ID != 0)
			{
				var UpdateImage = _db.OpenGigRolesApplications.Where(x => x.ID == OpengigroleCv.ID).FirstOrDefault();
				if (UpdateImage != null)
				{

					UpdateImage.BlobStorageBaseUrl = OpengigroleCv.BlobStorageBaseUrl;
					UpdateImage.CVPath = OpengigroleCv.CVPath;
					UpdateImage.CVUrlWithSas = OpengigroleCv.CVUrlWithSas;

					_db.SaveChanges();

					return StatusCode(StatusCodes.Status200OK, new APIResponseModel
					{
						StatusCode = StatusCodes.Status200OK,
						Message = "Applied Successfully !"
					});
				}
			}
			return StatusCode(StatusCodes.Status200OK, new APIResponseModel
			{
				StatusCode = StatusCodes.Status200OK,
				Message = "Something Went Wrong"
			});
		}
	}
}
