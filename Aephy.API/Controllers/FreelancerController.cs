using Aephy.API.DBHelper;
using Aephy.API.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Data;
using System.Linq;
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
                var checkGigApplication = _db.OpenGigRolesApplications.Where(x => x.FreelancerID == OpenGigRolesData.FreelancerID && x.GigOpenRoleId == OpenGigRolesData.GigOpenRoleId).FirstOrDefault();
                if (checkGigApplication != null)
                {
                    return StatusCode(StatusCodes.Status200OK, new APIResponseModel { StatusCode = StatusCodes.Status403Forbidden, Message = "You already have applied for this role!" });
                }
                var opengigroles_data = new OpenGigRolesApplications()
                {

                    FreelancerID = OpenGigRolesData.FreelancerID,
                    GigOpenRoleId = OpenGigRolesData.GigOpenRoleId,
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

        [HttpGet]
        [Route("ApprovedRolesList")]
        public async Task<IActionResult> ApprovedRolesList([FromBody] GetUserProfileRequestModel model)
        {
            try
            {
                var listDB = _db.GigOpenRoles.ToList();
                var listSolution = _db.Solutions.ToList();
                var listServiceSol = _db.SolutionServices.ToList();
                var listService = _db.Services.ToList();
                var listIndustry = _db.Industries.ToList();
                var listIndustrySol = _db.SolutionIndustry.ToList();
                var approvedJobs = _db.OpenGigRolesApplications.Where(x => x.FreelancerID == model.UserId
                && x.IsApproved).ToList();
                List<dynamic> finalList = new List<dynamic>();
                listDB.ForEach(x =>
                {
                    if (approvedJobs.Where(a => a.GigOpenRoleId == x.ID).Count() > 0)
                    {
                        var solSer = listServiceSol.Where(t => t.SolutionId == x.SolutionId).FirstOrDefault();
                        var serviceName = "";
                        if (solSer != null)
                        {
                            serviceName = listService.Where(s => s.Id == solSer.ServicesId).FirstOrDefault()?.ServicesName;
                        }

                        var solInd = listIndustrySol.Where(t1 => t1.SolutionId == x.SolutionId).FirstOrDefault();
                        var IndName = "";
                        if (solInd != null)
                        {
                            IndName = listIndustry.Where(s1 => s1.Id == x.IndustryId).FirstOrDefault()?.IndustryName;
                        }
                        var solutionName = listSolution.Where(m => m.Id == x.SolutionId).FirstOrDefault()?.Title;
                        dynamic obj = new
                        {
                            x.Level,
                            x.Title,
                            x.CreatedDateTime,
                            x.ID,
                            x.SolutionId,
                            x.IndustryId,
                            x.Description,
                            SolutionName = solutionName,
                            ServiceName = serviceName,
                            IndustryName = IndName,

                        };
                        finalList.Add(obj);
                    }
                });
                return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                {
                    StatusCode = StatusCodes.Status403Forbidden,
                    Message = "Success",
                    Result = finalList
                });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new APIResponseModel
                {
                    StatusCode = StatusCodes.Status403Forbidden,
                    Message = ex.Message + ex.InnerException
                });
            }
        }

        
        [HttpPost]
        [Route("SaveMileStoneData")]
        public async Task<IActionResult> SaveMileStoneData([FromBody] MileStoneModel model)
        {
            if(model != null)
            {
                var milestone = new SolutionMilestone()
                {
                    Title = model.Title,
                    Description = model.Description,
                    IndustryId = model.IndustryId,
                    SolutionId = model.SolutionId,
                    DueDate = model.DueDate,
                    FreelancerId = model.FreelancerId
                };
                _db.SolutionMilestone.Add(milestone);
                _db.SaveChanges();
                return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                {
                    StatusCode = StatusCodes.Status200OK,
                    Message = "Data Saved Succesfully!."
                });
            }

            return StatusCode(StatusCodes.Status500InternalServerError, new APIResponseModel { StatusCode = StatusCodes.Status403Forbidden, Message = "Something Went Wrong." });

        }

       
        [HttpPost]
        [Route("UpdateIndustryOutline")]
        public async Task<IActionResult> UpdateIndustryOutline([FromBody] SolutionIndustryDetailsModel model)
        {
            if (model != null)
            {
                var data = _db.SolutionIndustryDetails.Where(x => x.IndustryId == model.IndustryId && x.SolutionId == model.SolutionId).FirstOrDefault();
                if(data != null)
                {
                    data.ProjectOutline = model.ProjectOutline;
                    _db.SaveChanges();
                    return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                    {
                        StatusCode = StatusCodes.Status200OK,
                        Message = "Data Saved Succesfully!."
                    });
                }
                return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                {
                    StatusCode = StatusCodes.Status200OK,
                    Message = "Data Not Found!."
                });
            }

            return StatusCode(StatusCodes.Status500InternalServerError, new APIResponseModel { StatusCode = StatusCodes.Status403Forbidden, Message = "Something Went Wrong." });

        }

        
        [HttpGet]
        [Route("GetMiletoneList")]
        public async Task<IActionResult> GetMiletoneList([FromBody] GetUserProfileRequestModel model)
        {
            try
            {
                var milestoneList = _db.SolutionMilestone.Where(x => x.FreelancerId == model.UserId).ToList();
                return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                {
                    StatusCode = StatusCodes.Status200OK,
                    Message = "Success",
                    Result = milestoneList
                });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new APIResponseModel
                {
                    StatusCode = StatusCodes.Status403Forbidden,
                    Message = ex.Message + ex.InnerException
                });
            }
        }
    }
}
