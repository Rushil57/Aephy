using Aephy.API.DBHelper;
using Aephy.API.Models;
using Aephy.API.Stripe;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
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
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Aephy.API.Controllers
{
    [Route("api/Freelancer/")]
    [ApiController]
    public class FreelancerController : ControllerBase
    {
        private readonly AephyAppDbContext _db;
        private readonly IStripeAccountService _stripeAccountService;
        public FreelancerController(AephyAppDbContext dbContext, IStripeAccountService stripeAccountService)
        {
            _db = dbContext;
            _stripeAccountService = stripeAccountService;
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
            if (OpengigroleCv.AlreadyExist)
            {
                var applicantsDetails = await _db.OpenGigRolesApplications.Where(x => x.ID == OpengigroleCv.ID).FirstOrDefaultAsync();
                var freelancerDetails = _db.FreelancerDetails.Where(x => x.UserId == OpengigroleCv.FreelancerId).FirstOrDefault();
                if (freelancerDetails != null)
                {
                    applicantsDetails.CVUrlWithSas = freelancerDetails.CVUrlWithSas;
                    applicantsDetails.CVPath = freelancerDetails.CVPath;
                    applicantsDetails.BlobStorageBaseUrl = freelancerDetails.BlobStorageBaseUrl;
                    _db.SaveChanges();

                    return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                    {
                        StatusCode = StatusCodes.Status200OK,
                        Message = "Applied Successfully !"
                    });
                }
            }
            else
            {
                if (OpengigroleCv.ID != 0)
                {
                    var UpdateImage = _db.OpenGigRolesApplications.Where(x => x.ID == OpengigroleCv.ID).FirstOrDefault();
                    if (UpdateImage != null)
                    {

                        UpdateImage.BlobStorageBaseUrl = OpengigroleCv.BlobStorageBaseUrl;
                        UpdateImage.CVPath = OpengigroleCv.CVPath;
                        UpdateImage.CVUrlWithSas = OpengigroleCv.CVUrlWithSas;
                        _db.SaveChanges();

                        var freelancerDetails = _db.FreelancerDetails.Where(x => x.UserId == UpdateImage.FreelancerID).FirstOrDefault();
                        if(freelancerDetails != null)
                        {
                            freelancerDetails.BlobStorageBaseUrl = OpengigroleCv.BlobStorageBaseUrl;
                            freelancerDetails.CVPath = OpengigroleCv.CVPath;
                            freelancerDetails.CVUrlWithSas = OpengigroleCv.CVUrlWithSas;
                            _db.SaveChanges();
                        }
                        return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                        {
                            StatusCode = StatusCodes.Status200OK,
                            Message = "Applied Successfully !"
                        });
                    }
                }
            }
            //return StatusCode(StatusCodes.Status500InternalServerError, new APIResponseModel { StatusCode = StatusCodes.Status403Forbidden, Message = "Something Went Wrong." });

            return StatusCode(StatusCodes.Status200OK, new APIResponseModel
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "Something Went Wrong"
            });
        }



        [HttpPost]
        [Route("UpdateUserCV")]
        public async Task<IActionResult> UpdateUserCV([FromBody] UserCvFileModel usercv)
        {
            if (usercv.UserId != null)
            {
                var userData = await _db.FreelancerDetails.Where(x => x.UserId == usercv.UserId).FirstOrDefaultAsync();
                if (userData != null)
                {
                    userData.BlobStorageBaseUrl = usercv.BlobStorageBaseUrl;
                    userData.CVPath = usercv.CVPath;
                    userData.CVUrlWithSas = usercv.CVUrlWithSas;
                    _db.SaveChanges();
                }

                return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                {
                    StatusCode = StatusCodes.Status200OK,
                    Message = "Updated Successfully !"
                });
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
                var freelancerPools = _db.FreelancerPool.Where(fl => fl.FreelancerID == model.UserId).ToList();
                var approvedJobs = _db.OpenGigRolesApplications.Where(x => x.FreelancerID == model.UserId
                && x.IsApproved).ToList();

                var freelancerDetails = await _db.FreelancerDetails.Where(flnc => flnc.UserId == model.UserId).FirstOrDefaultAsync();

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
                            IsDefine = freelancerPools.Any(p => p.IndustryId == x.IndustryId && p.SolutionID == x.SolutionId
                            && p.IsProjectArchitect)
                        };
                        finalList.Add(obj);
                    }
                });
                freelancerPools.ForEach(fp =>
                {
                    var solutionName = listSolution.Where(m => m.Id == fp.SolutionID).FirstOrDefault()?.Title;
                    var solSer = listServiceSol.Where(t => t.SolutionId == fp.SolutionID).FirstOrDefault();
                    var serviceName = "";
                    if (solSer != null)
                    {
                        serviceName = listService.Where(s => s.Id == solSer.ServicesId).FirstOrDefault()?.ServicesName;
                    }
                    var solInd = listIndustrySol.Where(t1 => t1.SolutionId == fp.SolutionID).FirstOrDefault();
                    var IndName = "";
                    if (solInd != null)
                    {
                        IndName = listIndustry.Where(s1 => s1.Id == fp.IndustryId).FirstOrDefault()?.IndustryName;
                    }

                    dynamic obj = new
                    {
                        Level = fp.IsProjectArchitect ? "Project Manager/Project Architect" : freelancerDetails?.FreelancerLevel,
                        Title = "-",
                        CreatedDateTime = "",
                        ID = 0,
                        SolutionId = fp?.SolutionID,
                        fp?.IndustryId,
                        Description = "",
                        SolutionName = solutionName,
                        ServiceName = serviceName,
                        IndustryName = IndName,
                        IsDefine = fp?.IsProjectArchitect
                    };
                    finalList.Add(obj);
                });
                List<dynamic> list = new List<dynamic>();
                if (finalList.Count > 0)
                {

                    if (finalList.Count > 0)
                    {
                        foreach (var item in finalList)
                        {
                            if (list.Count > 0)
                            {
                                if (!list.Any(x => x.SolutionId == item.SolutionId && x.IndustryId == item.IndustryId))
                                {
                                    list.Add(item);
                                }
                            }
                            else
                            {
                                list.Add(item);
                            }
                        }
                    }
                }
                return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                {
                    StatusCode = StatusCodes.Status200OK,
                    Message = "Success",
                    Result = list
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
            if (model != null)
            {
                if (model.Id == 0)
                {
                    var milestone = new SolutionMilestone()
                    {
                        Title = model.Title,
                        Description = model.Description,
                        IndustryId = model.IndustryId,
                        SolutionId = model.SolutionId,
                        DueDate = DateTime.MinValue,
                        FreelancerId = model.FreelancerId,
                        ProjectType = model.ProjectType,
                        Days = model.Days
                    };
                    _db.SolutionMilestone.Add(milestone);
                    _db.SaveChanges();
                    return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                    {
                        StatusCode = StatusCodes.Status200OK,
                        Message = "Data Saved Succesfully!."
                    });
                }
                else
                {
                    var data = await _db.SolutionMilestone.Where(x => x.Id == model.Id).FirstOrDefaultAsync();
                    if (data != null)
                    {
                        data.Description = model.Description;
                        data.DueDate = DateTime.MinValue;
                        data.Title = model.Title;
                        data.ProjectType = model.ProjectType;
                        data.Days = model.Days;
                        _db.SaveChanges();
                        return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                        {
                            StatusCode = StatusCodes.Status200OK,
                            Message = "Data Updated Succesfully!."
                        });
                    }
                }

            }

            return StatusCode(StatusCodes.Status500InternalServerError, new APIResponseModel { StatusCode = StatusCodes.Status403Forbidden, Message = "Something Went Wrong." });

        }

        [HttpPost]
        [Route("UpdateFreelancerById")]
        public async Task<IActionResult> UpdateFreelancerById([FromBody] UserViewModel userData)
        {
            try
            {
                FreelancerDetails openRolesdata = await _db.FreelancerDetails.Where(x => x.UserId == userData.Id).FirstOrDefaultAsync();
                if (openRolesdata != null)
                {
                    openRolesdata.FreelancerLevel = userData.FreelancerLevel;
                    _db.SaveChanges();
                }

                return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                {
                    StatusCode = StatusCodes.Status200OK,
                    Message = "User Updated Succesfully!."
                });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new APIResponseModel { StatusCode = StatusCodes.Status403Forbidden, Message = "Something Went Wrong." });
            }
        }

        [HttpPost]
        [Route("UpdateIndustryOutline")]
        public async Task<IActionResult> UpdateIndustryOutline([FromBody] SolutionIndustryDetailsModel model)
        {
            if (model != null)
            {
                var solutionIndustryDetails = await _db.SolutionIndustryDetails.Where(x => x.IndustryId == model.IndustryId
                                                && x.SolutionId == model.SolutionId).FirstOrDefaultAsync();
                if (solutionIndustryDetails != null)
                {
                    //data.ProjectOutline = model.ProjectOutline;
                    //data.ProjectDetails = model.ProjectDetails;
                    //_db.SaveChanges();

                    var solutionDefineData = _db.SolutionDefine.Where(x => x.SolutionIndustryDetailsId == solutionIndustryDetails.Id
                                                && x.ProjectType == model.ProjectType).FirstOrDefault();

                    var solutionDefineModel = new SolutionDefine()
                    {
                        SolutionIndustryDetailsId = solutionIndustryDetails.Id,
                        ProjectOutline = model.ProjectOutline,
                        ProjectDetails = model.ProjectDetails,
                        ProjectType = model.ProjectType,
                        Duration = model.Duration,
                        TeamSize = model.TeamSize,
                        IsActive = true
                    };

                    if (solutionDefineData != null)
                    {
                        solutionDefineData.ProjectOutline = model.ProjectOutline;
                        solutionDefineData.ProjectDetails = model.ProjectDetails;
                        solutionDefineData.Duration = model.Duration;
                        solutionDefineData.TeamSize = model.TeamSize;
                        _db.SolutionDefine.Update(solutionDefineData);
                    }
                    else
                    {
                        solutionDefineModel.CreatedDateTime = DateTime.Now;
                        await _db.SolutionDefine.AddAsync(solutionDefineModel);
                    }

                    await _db.SaveChangesAsync();

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


        [HttpPost]
        [Route("GetMiletoneList")]
        public async Task<IActionResult> GetMiletoneList([FromBody] MileStoneDetailsViewModel model)
        {
            try
            {
                var milestoneList = await _db.SolutionMilestone.Where(x => x.FreelancerId == model.FreelancerId
                && x.IndustryId == model.IndustryId && x.SolutionId == model.SolutionId
                && x.ProjectType == model.ProjectType).ToListAsync();

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


        [HttpPost]
        [Route("GetMileStoneById")]
        public async Task<IActionResult> GetMileStoneById([FromBody] MileStoneIdViewModel model)
        {
            if (model != null)
            {
                var data = await _db.SolutionMilestone.Where(x => x.Id == model.Id).FirstOrDefaultAsync();
                return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                {
                    StatusCode = StatusCodes.Status200OK,
                    Result = data
                });
            }

            return StatusCode(StatusCodes.Status500InternalServerError, new APIResponseModel { StatusCode = StatusCodes.Status403Forbidden, Message = "Something Went Wrong." });

        }
        [HttpPost]
        [Route("SavePointsData")]
        public async Task<IActionResult> SavePointsData([FromBody] SolutionPoints model)
        {
            if (model != null)
            {
                if (model.Id == 0)
                {
                    var points = new SolutionPoints()
                    {
                        point = model.point,
                        PointKey= model.PointKey,
                        PointValue = model.PointValue,
                        IndustryId = model.IndustryId,
                        SolutionId = model.SolutionId,
                        FreelancerId = model.FreelancerId,
                        ProjectType = model.ProjectType
                    };
                    _db.SolutionPoints.Add(points);
                    _db.SaveChanges();
                    return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                    {
                        StatusCode = StatusCodes.Status200OK,
                        Message = "Data Saved Succesfully!."
                    });
                }
                else
                {
                    var pointsData = await _db.SolutionPoints.Where(x => x.Id == model.Id).FirstOrDefaultAsync();
                    if (pointsData != null)
                    {
                        pointsData.PointValue = model.PointValue;
                        pointsData.PointKey = model.PointKey;
                        _db.SaveChanges();
                        return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                        {
                            StatusCode = StatusCodes.Status200OK,
                            Message = "Data Updated Succesfully!."
                        });
                    }
                }
            }
            return StatusCode(StatusCodes.Status500InternalServerError, new APIResponseModel { StatusCode = StatusCodes.Status403Forbidden, Message = "Something Went Wrong." });
        }

        [HttpPost]
        [Route("GetPointsList")]
        public async Task<IActionResult> GetPointsList([FromBody] MileStoneDetailsViewModel model)
        {
            try
            {
                var pointsList = await _db.SolutionPoints.Where(x => x.FreelancerId == model.FreelancerId
                && x.IndustryId == model.IndustryId && x.SolutionId == model.SolutionId
                && x.ProjectType == model.ProjectType).ToListAsync();

                return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                {
                    StatusCode = StatusCodes.Status200OK,
                    Message = "Success",
                    Result = pointsList
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
        [Route("DeletePointsById")]
        public async Task<IActionResult> DeletePointsById([FromBody] MileStoneIdViewModel model)
        {
            try
            {
                var pointsData = await _db.SolutionPoints.Where(x => x.Id == model.Id).FirstOrDefaultAsync();
                if (pointsData != null)
                {
                    _db.SolutionPoints.Remove(pointsData);
                    _db.SaveChanges();
                }
                return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                {
                    StatusCode = StatusCodes.Status200OK,
                    Message = "Delete Succesfully"
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
        [Route("DeleteMileStoneById")]
        public async Task<IActionResult> DeleteMileStoneById([FromBody] MileStoneIdViewModel model)
        {
            try
            {
                var milestoneData = await _db.SolutionMilestone.Where(x => x.Id == model.Id).FirstOrDefaultAsync();
                if (milestoneData != null)
                {
                    _db.SolutionMilestone.Remove(milestoneData);
                    _db.SaveChanges();
                }
                return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                {
                    StatusCode = StatusCodes.Status200OK,
                    Message = "Delete Succesfully"
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
        [Route("GetSolutionDefineData")]
        public async Task<IActionResult> GetSolutionDefineData([FromBody] MileStoneDetailsViewModel model)
        {
            try
            {
                int solutionIndustryDetailsId = await _db.SolutionIndustryDetails.Where(x => x.IndustryId == model.IndustryId
                && x.SolutionId == model.SolutionId).Select(x => x.Id).FirstOrDefaultAsync();

                if (solutionIndustryDetailsId > 0)
                {
                    var solutionDefineMpdel = await _db.SolutionDefine.Where(x => x.SolutionIndustryDetailsId == solutionIndustryDetailsId
                                                && x.ProjectType == model.ProjectType).FirstOrDefaultAsync();

                    if (solutionDefineMpdel != null)
                    {
                        return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                        {
                            StatusCode = StatusCodes.Status200OK,
                            Message = "Success",
                            Result = solutionDefineMpdel
                        });
                    }
                    else
                    {
                        return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                        {
                            StatusCode = StatusCodes.Status200OK,
                            Message = "Failed"
                        });
                    }
                }

                return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                {
                    StatusCode = StatusCodes.Status200OK,
                    Message = "Failed"
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
        [Route("DeleteFreelancerSolution")]
        public async Task<IActionResult> DeleteFreelancerSolution([FromBody] MileStoneDetailsViewModel model)
        {
            try
            {
                var freelancerPooldata = _db.FreelancerPool.Where(x => x.SolutionID == model.SolutionId && x.IndustryId == model.IndustryId && x.FreelancerID == model.FreelancerId).FirstOrDefault();
                if(freelancerPooldata != null)
                {
                    _db.FreelancerPool.Remove(freelancerPooldata);
                    _db.SaveChanges();
                }

                var gigId = _db.GigOpenRoles.Where(x => x.SolutionId == model.SolutionId && x.IndustryId == model.IndustryId && x.Level == model.FreelancerLevel.Trim()).Select(x => x.ID).FirstOrDefault();
                if(gigId != 0)
                {
                    var freelancerData = _db.OpenGigRolesApplications.Where(x => x.GigOpenRoleId == gigId && x.FreelancerID == model.FreelancerId).FirstOrDefault();
                    if(freelancerData != null)
                    {
                        _db.OpenGigRolesApplications.Remove(freelancerData);
                        await _db.SaveChangesAsync();

                        return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                        {
                            StatusCode = StatusCodes.Status200OK,
                            Message = "Delete Succesfully"
                        });
                    }
                }

                return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                {
                    StatusCode = StatusCodes.Status200OK,
                    Message = "Data Not Found"
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
        [Route("GetPointsDataById")]
        public async Task<IActionResult> GetPointsDataById([FromBody] MileStoneIdViewModel model)
        {
            if (model != null)
            {
                var data = await _db.SolutionPoints.Where(x => x.Id == model.Id).FirstOrDefaultAsync();
                if (data != null)
                {
                    return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                    {
                        StatusCode = StatusCodes.Status200OK,
                        Result = data
                    });
                }
                else
                {
                    return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                    {
                        StatusCode = StatusCodes.Status200OK,
                        Message = "No data found"
                    });
                }

            }

            return StatusCode(StatusCodes.Status500InternalServerError, new APIResponseModel { StatusCode = StatusCodes.Status403Forbidden, Message = "Something Went Wrong." });

        }

        [HttpPost]
        [Route("GetSolutionBasedOnServices")]
        public async Task<IActionResult> GetSolutionBasedOnServices([FromBody] MileStoneIdViewModel model)
        {
            if (model.Id != 0)
            {
                try
                {
                    var serviceData = await _db.SolutionServices.Where(x => x.ServicesId == model.Id).Select(x => x.SolutionId).ToListAsync();
                    List<SolutionsModel> solutionsModel = new List<SolutionsModel>();
                    List<Industries> industryDetailList = new List<Industries>();
                    if (serviceData.Count > 0)
                    {
                        foreach (var data in serviceData)
                        {
                            var solutiondata = _db.Solutions.Where(x => x.Id == data).FirstOrDefault();
                            var industryList = _db.SolutionIndustry.Where(x => x.SolutionId == data).Select(x => x.IndustryId).ToList();
                            if (industryList.Count > 0)
                            {
                                foreach (var industryId in industryList)
                                {
                                    var industry = _db.Industries.Where(x => x.Id == industryId).FirstOrDefault();
                                    industryDetailList.Add(industry);
                                }
                                SolutionsModel dataStore = new SolutionsModel();
                                dataStore.solutionServices = model.Id;
                                dataStore.Id = solutiondata.Id;
                                dataStore.Title = solutiondata.Title;
                                solutionsModel.Add(dataStore);
                            }

                        }
                    }
                    return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                    {
                        StatusCode = StatusCodes.Status200OK,
                        Message = "Success",
                        Result = new
                        {
                            SolutionData = solutionsModel,
                            IndustriesData = industryDetailList.Distinct()
                        }
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

            return StatusCode(StatusCodes.Status200OK, new APIResponseModel
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "Data not Foundd"
            });
        }


        [HttpPost]
        [Route("GetPopularSolutionBasedOnSolution")]
        public async Task<IActionResult> GetPopularSolutionBasedOnSolution([FromBody] MileStoneIdViewModel model)
        {
            if (model.Id != 0)
            {
                try
                {
                    var solutionData = _db.Solutions.Where(x => x.Id == model.Id).FirstOrDefault();
                    List<Industries> industryDetailList = new List<Industries>();

                    var industryList = _db.SolutionIndustry.Where(x => x.SolutionId == solutionData.Id).Select(x => x.IndustryId).ToList();
                    if (industryList.Count > 0)
                    {
                        foreach (var industryId in industryList)
                        {
                            var industry = _db.Industries.Where(x => x.Id == industryId).FirstOrDefault();
                            industryDetailList.Add(industry);
                        }
                    }


                    return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                    {
                        StatusCode = StatusCodes.Status200OK,
                        Message = "Success",
                        Result = industryDetailList.Distinct()

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

            return StatusCode(StatusCodes.Status200OK, new APIResponseModel
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "Data not Found"
            });
        }


        [HttpPost]
        [Route("GetAllUnReadNotification")]
        public async Task<IActionResult> GetAllUnReadNotification([FromBody] MileStoneIdViewModel model)
        {
            if (model != null)
            {
                try
                {
                    var notificationData = await _db.Notifications.Where(x => x.ToUserId == model.UserId && x.IsRead == false).ToListAsync();
                    List<Notifications> notifications = new List<Notifications>();
                    if(notificationData.Count > 0)
                    {
                        foreach(var data in notificationData)
                        {
                            Notifications dataStore = new Notifications();
                            var fullname = _db.Users.Where(x => x.Id == data.FromUserId).Select(x => new { x.FirstName, x.LastName }).FirstOrDefault();
                            dataStore.FromUserId = fullname.FirstName + " " + fullname.LastName;
                            dataStore.NotificationText = data.NotificationText;
                            dataStore.NotificationTime = data.NotificationTime;
                            dataStore.Id = data.Id;
                            notifications.Add(dataStore);
                        }
                    }

                    var list = notifications.OrderByDescending(x => x.NotificationTime);
                    return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                    {
                        StatusCode = StatusCodes.Status200OK,
                        Message = "Success",
                        Result = list

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

            return StatusCode(StatusCodes.Status200OK, new APIResponseModel
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "Data not Found"
            });
        }


        [HttpPost]
        [Route("GetAllNotification")]
        public async Task<IActionResult> GetAllNotification([FromBody] MileStoneIdViewModel model)
        {
            if (model != null)
            {
                try
                {
                    var notificationData = await _db.Notifications.Where(x => x.ToUserId == model.UserId).ToListAsync();
                    List<Notifications> notifications = new List<Notifications>();
                    if (notificationData.Count > 0)
                    {
                        foreach (var data in notificationData)
                        {
                            Notifications dataStore = new Notifications();
                            var fullname = _db.Users.Where(x => x.Id == data.FromUserId).Select(x => new { x.FirstName, x.LastName }).FirstOrDefault();
                            dataStore.FromUserId = fullname.FirstName + " " + fullname.LastName;
                            dataStore.NotificationText = data.NotificationText;
                            dataStore.NotificationTime = data.NotificationTime;
                            dataStore.Id = data.Id;
                            notifications.Add(dataStore);
                        }
                    }

                    var list = notifications.OrderByDescending(x => x.NotificationTime);
                    return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                    {
                        StatusCode = StatusCodes.Status200OK,
                        Message = "Success",
                        Result = list

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

            return StatusCode(StatusCodes.Status200OK, new APIResponseModel
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "Data not Found"
            });
        }

        [HttpPost]
        [Route("SetNotificationIsRead")]
        public async Task<IActionResult> SetNotificationIsRead([FromBody] MileStoneIdViewModel model)
        {
            if (model != null)
            {
                try
                {
                    var notificationData = await _db.Notifications.Where(x => x.Id == model.Id).FirstOrDefaultAsync();
                    if(notificationData != null)
                    {
                        notificationData.IsRead = true;
                        _db.SaveChanges();
                    }

                    return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                    {
                        StatusCode = StatusCodes.Status200OK,
                        Message = "Success",
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

            return StatusCode(StatusCodes.Status200OK, new APIResponseModel
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "Data not Found"
            });
        }


        [HttpPost]
        [Route("SaveProject")]
        public async Task<IActionResult> SaveProject([FromBody] SavedProjects model)
        {
            if (model != null)
            {
                try
                {
                    var data = new SavedProjects()
                    {
                        SavedDateTime = DateTime.Now,
                        SolutionId = model.SolutionId,
                        IndustryId = model.IndustryId,
                        UserId = model.UserId,
                    };
                    _db.SavedProjects.Add(data);
                    await _db.SaveChangesAsync();

                    return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                    {
                        StatusCode = StatusCodes.Status200OK,
                        Message = "Project Saved Successfully!",
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

            return StatusCode(StatusCodes.Status200OK, new APIResponseModel
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "Data not Found"
            });
        }



        [HttpPost]
        [Route("GetSavedProjectList")]
        public async Task<IActionResult> GetSavedProjectList([FromBody] MileStoneIdViewModel model)
        {
            if (model != null)
            {
                if (model.UserId != null)
                {
                    try
                    {
                        var saveprojectData = await _db.SavedProjects.Where(x => x.UserId == model.UserId).Take(2).OrderByDescending(x => x.Id).ToListAsync();
                        List<SolutionsModel> solutionsModel = new List<SolutionsModel>();
                        if (saveprojectData.Count > 0)
                        {
                            List<string> industrylist = new List<string>();
                            foreach (var data in saveprojectData)
                            {
                                SolutionsModel solutionsdataStore = new SolutionsModel();
                                var solutionData = _db.Solutions.Where(x => x.Id == data.SolutionId).FirstOrDefault();
                                var industryIdlist = _db.SolutionIndustry.Where(x => x.SolutionId == data.SolutionId).Select(x => x.IndustryId).ToList();
                                if (industryIdlist.Count > 0)
                                {
                                    foreach (var industryId in industryIdlist)
                                    {
                                        var industryname = _db.Industries.Where(x => x.Id == industryId).Select(x => x.IndustryName).FirstOrDefault();
                                        industrylist.Add(industryname);
                                    }
                                }
                                solutionsdataStore.Industries = string.Join(",", industrylist);
                                solutionsdataStore.Id = solutionData.Id;
                                solutionsdataStore.Title = solutionData.Title;
                                solutionsdataStore.Description = solutionData.Description;
                                solutionsdataStore.ImagePath = solutionData.ImagePath;
                                solutionsModel.Add(solutionsdataStore);
                                industrylist.Clear();
                            }
                        }
                        return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                        {
                            StatusCode = StatusCodes.Status200OK,
                            Message = "success",
                            Result = solutionsModel
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

            return StatusCode(StatusCodes.Status200OK, new APIResponseModel
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "Data not Found"
            });
        }


        [HttpPost]
        [Route("UnSavedProject")]
        public async Task<IActionResult> UnSavedProject([FromBody] MileStoneDetailsViewModel model)
        {
            if (model != null)
            {
                if (model.UserId != null)
                {
                    try
                    {
                        var projectData = await _db.SavedProjects.Where(x => x.SolutionId == model.SolutionId && x.UserId == x.UserId).FirstOrDefaultAsync();
                        if (projectData != null)
                        {
                            _db.Remove(projectData);
                            await _db.SaveChangesAsync();
                            return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                            {
                                StatusCode = StatusCodes.Status200OK,
                                Message = "Unsaved Project"
                            });
                        }
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

            return StatusCode(StatusCodes.Status200OK, new APIResponseModel
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "Data not Found"
            });
        }


        [HttpPost]
        [Route("GetAllSavedProjectList")]
        public async Task<IActionResult> GetAllSavedProjectList([FromBody] MileStoneIdViewModel model)
        {
            if (model != null)
            {
                if (model.UserId != null)
                {
                    try
                    {
                        var saveprojectData = await _db.SavedProjects.Where(x => x.UserId == model.UserId).ToListAsync();
                        List<SolutionsModel> solutionsModel = new List<SolutionsModel>();
                        if (saveprojectData.Count > 0)
                        {
                            List<string> industrylist = new List<string>();
                            foreach (var data in saveprojectData)
                            {
                                SolutionsModel solutionsdataStore = new SolutionsModel();
                                var solutionData = _db.Solutions.Where(x => x.Id == data.SolutionId).FirstOrDefault();
                                var serviceId = _db.SolutionServices.Where(x => x.SolutionId == data.SolutionId).Select(x => x.ServicesId).FirstOrDefault();
                                var serviceData = _db.Services.Where(x => x.Id == serviceId).Select(x => x.ServicesName).FirstOrDefault();
                                var industryIdlist = _db.SolutionIndustry.Where(x => x.SolutionId == data.SolutionId).Select(x => x.IndustryId).ToList();
                                if (industryIdlist.Count > 0)
                                {
                                    foreach (var industryId in industryIdlist)
                                    {
                                        var industryname = _db.Industries.Where(x => x.Id == industryId).Select(x => x.IndustryName).FirstOrDefault();
                                        industrylist.Add(industryname);
                                    }
                                }

                                solutionsdataStore.Services = serviceData;
                                solutionsdataStore.Industries = string.Join(",", industrylist);
                                solutionsdataStore.Id = data.Id;
                                solutionsdataStore.Title = solutionData.Title;
                                solutionsdataStore.Description = solutionData.Description;
                                solutionsdataStore.ImagePath = solutionData.ImagePath;
                                solutionsModel.Add(solutionsdataStore);
                                industrylist.Clear();
                            }
                        }
                        return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                        {
                            StatusCode = StatusCodes.Status200OK,
                            Message = "success",
                            Result = solutionsModel
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

            return StatusCode(StatusCodes.Status200OK, new APIResponseModel
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "Data not Found"
            });
        }

        //GetActiveProjectList
        [HttpPost]
        [Route("GetActiveProjectList")]
        public async Task<IActionResult> GetActiveProjectList([FromBody] MileStoneIdViewModel model)
        {
            if (model != null)
            {
                if (model.UserId != null)
                {
                    try
                    {
                        List<solutionFundViewModel> finalFundList = new List<solutionFundViewModel>();
                        List<SolutionsModel> solutionsModel = new List<SolutionsModel>();
                        var projectData = await _db.SolutionFund.Where(x => x.ClientId == model.UserId).ToListAsync();
                        if(projectData.Count > 0)
                        {
                            var grouped_teachers = projectData
                             .GroupBy(t => new { t.IndustryId, t.SolutionId, t.ProjectType })
                             .Select(g => new
                             {
                                 FundList = g.ToList(),
                             });

                            foreach (var group in grouped_teachers)
                            {
                                var list = group.FundList;
                                solutionFundViewModel grouping = new solutionFundViewModel();
                                if (list.Count != 1)
                                {
                                    grouping.solutionFunds = list.Last();
                                    finalFundList.Add(grouping);
                                }
                                else
                                {
                                    grouping.solutionFunds = list.FirstOrDefault();
                                    finalFundList.Add(grouping);
                                }

                            }
                            if (finalFundList.Count > 0)
                            {
                                List<string> industrylist = new List<string>();
                                foreach (var data in finalFundList)
                                {
                                    SolutionsModel solutionsdataStore = new SolutionsModel();
                                    var solutionData = _db.Solutions.Where(x => x.Id == data.solutionFunds.SolutionId).FirstOrDefault();
                                    var serviceId = _db.SolutionServices.Where(x => x.SolutionId == data.solutionFunds.SolutionId).Select(x => x.ServicesId).FirstOrDefault();
                                    var serviceData = _db.Services.Where(x => x.Id == serviceId).Select(x => x.ServicesName).FirstOrDefault();
                                    var industryname = _db.Industries.Where(x => x.Id == data.solutionFunds.IndustryId).Select(x => x.IndustryName).FirstOrDefault();
                                    var contractStatus = _db.Contract.Where(x => x.SolutionFundId == data.solutionFunds.Id).Select(x => x.PaymentStatus).FirstOrDefault();
                                    var milestoneData = _db.SolutionMilestone.Where(x => x.Id == data.solutionFunds.MileStoneId).Select(x => x.Title).FirstOrDefault();
                                    var contractid = _db.Contract.Where(x => x.SolutionFundId == data.solutionFunds.Id).Select(x => x.Id).FirstOrDefault();

                                    solutionsdataStore.Services = serviceData;
                                    solutionsdataStore.ServiceId = serviceId;
                                    solutionsdataStore.SolutionId = data.solutionFunds.SolutionId;
                                    solutionsdataStore.IndustryId = data.solutionFunds.IndustryId;
                                    solutionsdataStore.Industries = industryname;
                                    solutionsdataStore.Id = data.solutionFunds.Id;
                                    solutionsdataStore.Title = solutionData.Title;
                                    solutionsdataStore.Description = solutionData.Description;
                                    solutionsdataStore.ImagePath = solutionData.ImagePath;
                                    solutionsdataStore.MileStoneTitle = milestoneData;
                                    solutionsdataStore.PaymentStatus = contractStatus.ToString();
                                    solutionsdataStore.ContractId = contractid;
                                    solutionsdataStore.ProjectStatus = data.solutionFunds.ProjectStatus;
                                    solutionsModel.Add(solutionsdataStore);
                                    industrylist.Clear();

                                }

                                return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                                {
                                    StatusCode = StatusCodes.Status200OK,
                                    Message = "success",
                                    Result = solutionsModel
                                });
                            }
                            return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                            {
                                StatusCode = StatusCodes.Status200OK,
                                Message = "success",
                                Result = solutionsModel
                            });
                        }
                        return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                        {
                            StatusCode = StatusCodes.Status200OK,
                            Message = "success",
                            Result = solutionsModel
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

            return StatusCode(StatusCodes.Status200OK, new APIResponseModel
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "Data not Found"
            });
        }

        //ApproveFreelancerPayment
        [HttpPost]
        [Route("ApproveFreelancerPayment")]
        public async Task<IActionResult> ApproveFreelancerPayment([FromBody] MileStoneIdViewModel model)
        {
            if (model != null)
            {
                if (model.UserId != null)
                {
                    try
                    {
                        var contract = await _db.Contract.Where(x => x.SolutionFundId == model.SolutionFundId).Include("ContractUsers").FirstOrDefaultAsync();

                        if (contract != null)
                        {
                            var mileStone = _db.SolutionMilestone.FirstOrDefault(x => x.Id == contract.MilestoneDataId);
                            var checkoutSession = _stripeAccountService.GetCheckOutSesssion(contract.SessionId);

                            if (checkoutSession != null)
                            {
                                contract.SessionStatus = _stripeAccountService.GetSesssionStatus(checkoutSession);

                                if (_stripeAccountService.GetPaymentStatus(checkoutSession) == Contract.PaymentStatuses.Paid)
                                {
                                    foreach (var contractUser in contract.ContractUsers.Where(x => !x.IsTransfered))
                                    {
                                        var user = _db.Users.FirstOrDefault(x => x.Id == contractUser.ApplicationUserId);

                                        if (user != null && user.StripeAccountStatus == ApplicationUser.StripeAccountStatuses.Complete)
                                        {
                                            var paymentIntent = _stripeAccountService.GetPaymentIntent(contract.PaymentIntentId);

                                            var priceToTransfer = (long)(Convert.ToDecimal(contractUser.Percentage) / 100 * 200 * 100);
                                            //var transferId = _stripeAccountService.CreateTransferonCharge(priceToTransfer, "usd", user.StripeConnectedId, contract.LatestChargeId, contract.Id.ToString());
                                            var transferId = _stripeAccountService.CreateTransferonCharge(priceToTransfer, "eur", user.StripeConnectedId, contract.LatestChargeId, contract.Id.ToString());

                                            if (transferId != null)
                                            {
                                                contractUser.StripeTranferId = transferId;
                                                contractUser.IsTransfered = true;
                                                _db.ContractUser.Update(contractUser);
                                                _db.SaveChanges();
                                            }
                                        }
                                        else
                                        {
                                            // this User(freelabncer or architect) has not completed his Stripe account please 
                                        }
                                    }

                                    var transferredcount = contract.ContractUsers.Where(x => x.IsTransfered).Count();

                                    if (transferredcount == contract.ContractUsers.Count())
                                    {
                                        contract.PaymentStatus = Contract.PaymentStatuses.Splitted;
                                        _db.Contract.Update(contract);
                                        _db.SaveChanges();
                                        var message = string.Format("Amount is transferred to all {0} users(freelancers) and its status is splitted Now.", transferredcount);
                                        return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                                        {
                                            StatusCode = StatusCodes.Status200OK,
                                            Message = message,
                                        });
                                    }
                                    else if (transferredcount > 0)
                                    {
                                        contract.PaymentStatus = Contract.PaymentStatuses.PartiallySplitted;
                                        _db.Contract.Update(contract);
                                        _db.SaveChanges();
                                        var message = string.Format("Amount is transferred to {0} users(freelancers) and its status is Partially Splitted Now. Please press transfer again and make sure all users are onboard(stripe)", transferredcount);
                                        return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                                        {
                                            StatusCode = StatusCodes.Status200OK,
                                            Message = message,
                                        });
                                    }
                                    else
                                    {
                                        var message = string.Format("Amount is transferred to {0} users(freelancers) and its status is not changed from previous. Please press transfer again and make sure all users(freelancers) are onboard(stripe)", transferredcount);
                                        return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                                        {
                                            StatusCode = StatusCodes.Status200OK,
                                            Message = message,
                                        });
                                    }
                                }
                            }

                        }

                        return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                        {
                            StatusCode = StatusCodes.Status200OK,
                            Message = "Contract Details Not Found",
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

            return StatusCode(StatusCodes.Status200OK, new APIResponseModel
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "Data not Found"
            });
        }

        //GetCountryList
        [HttpGet]
        [Route("GetCountryList")]
        public async Task<IActionResult> GetCountryList()
        {
            var countryList = await _db.Country.ToListAsync();
            return StatusCode(StatusCodes.Status200OK, new APIResponseModel
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "success",
                Result = countryList
            });
        }

        //GetInvoiceDetails
        [HttpPost]
        [Route("GetInvoiceDetails")]
        public async Task<IActionResult> GetInvoiceDetails([FromBody] SolutionDisputeViewModel model)
        {
            try
            {
                if (model != null)
                {
                    if(model.ContractId != 0)
                    {
                        var contarctData = await _db.Contract.Where(x => x.Id == model.ContractId).FirstOrDefaultAsync();

                        if(contarctData != null)
                        {
                            var solutionFundData = _db.SolutionFund.Where(x => x.Id == contarctData.SolutionFundId).FirstOrDefault();
                            var CheckDataExists = _db.Invoices.Where(x => x.SolutionFundId == contarctData.SolutionFundId).FirstOrDefault();
                            if(CheckDataExists == null)
                            {
                                var InvoiceNumber = 0;
                                var TotalInvoiceData = _db.Invoices.ToList().Count();
                                if(TotalInvoiceData == 0)
                                {
                                    InvoiceNumber = 100;
                                }
                                else
                                {
                                    var GetLastInvoiceNum = _db.Invoices.OrderByDescending(x => x.InvoiceNumber).FirstOrDefault();
                                    InvoiceNumber = Convert.ToInt32(GetLastInvoiceNum.InvoiceNumber) + 1;
                                }
                                var Title = string.Empty;
                                if (solutionFundData.FundType.ToString() == "MilestoneFund")
                                {
                                    Title = _db.SolutionMilestone.Where(x => x.Id == solutionFundData.MileStoneId).Select(x => x.Title).FirstOrDefault();
                                }
                                else
                                {
                                    Title = _db.Solutions.Where(x => x.Id == solutionFundData.SolutionId).Select(x => x.Title).FirstOrDefault();
                                }
                                var fullname = _db.Users.Where(x => x.Id == contarctData.ClientUserId).Select(x => new { x.FirstName, x.LastName }).FirstOrDefault();
                                var InvoiceData = new Invoices()
                                {
                                    SolutionId = contarctData.SolutionId,
                                    IndustryId = contarctData.IndustryId,
                                    SolutionFundId = contarctData.SolutionFundId,
                                    InvoiceNumber = InvoiceNumber.ToString(),
                                    Date = contarctData.CreatedDateTime,
                                    DueDate = contarctData.CreatedDateTime,
                                    TotalAmount = solutionFundData.ProjectPrice,
                                    DueAmount = solutionFundData.ProjectPrice,
                                    ClientId = contarctData.ClientUserId,
                                    ClientName = fullname.FirstName + " " + fullname.LastName,
                                    ClientAddress = _db.FreelancerDetails.Where(x => x.UserId == contarctData.ClientUserId).Select(x => x.Address).FirstOrDefault(),
                                    VatId = "1",
                                    TaxId = "1",
                                    Title = Title,
                                    Amount = solutionFundData.ProjectPrice,
                                    VatPercentage = "20%",
                                    VatAmount = "200"
                                };
                                _db.Invoices.Add(InvoiceData);
                                _db.SaveChanges();
                            }

                            var Invoicedetails = _db.Invoices.Where(x => x.SolutionFundId == solutionFundData.Id).FirstOrDefault();
                            var Fundtype = solutionFundData.FundType.ToString();

                            if (Invoicedetails != null)
                            {
                                return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                                {
                                    StatusCode = StatusCodes.Status200OK,
                                    Message = "success",
                                    Result = new
                                    {
                                        InvoiceDetails = Invoicedetails,
                                        FundType = Fundtype
                                    }
                                });
                            }
                        }
                    }

                }
                return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                {
                    StatusCode = StatusCodes.Status200OK,
                    Message = "Data Not Found"
                });
            }
            catch(Exception ex)
            {
                return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                {
                    StatusCode = StatusCodes.Status200OK,
                    Message = ex.Message
                });
            }
        }

        [HttpPost]
        [Route("GetChatResponse")]
        public async Task<IActionResult> GetChatResponse([FromBody] ChatPopupRequestViewModel model)
        {
            try
            {
                // [-- Method Needs To Be Update --]
                // [-- This Method Needs to be updated for generating freelancers list --]
                var userList = await _db.Users.ToListAsync();
                var ids = await _db.FreelancerPool.Where(x => x.IndustryId == model.IndustryId &&
                x.SolutionID == model.SolutionID).Select(x => x.FreelancerID).ToListAsync();

                if (model.UserRole != "Client")
                {
                    ids.Add(model.UserId);
                    ids.Remove(model.LoginFreelancerId);
                }

                var finalData = userList.Where(x => ids.Contains(x.Id)).Select(x => new ChatPopupResponseViewModel
                {
                    FreelancerId = x.Id,
                    FreelancerName = x.FirstName + " " + x.LastName
                }).ToList();

                return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                {
                    StatusCode = StatusCodes.Status200OK,
                    Message = "Success",
                    Result = finalData

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

        //GetFreelancerActiveProjectList
        [HttpPost]
        [Route("GetFreelancerActiveProjectList")]
        public async Task<IActionResult> GetFreelancerActiveProjectList([FromBody] MileStoneIdViewModel model)
        {
            if (model != null)
            {
                if (model.UserId != null)
                {
                    try
                    {
                        List<SolutionFund> solutionList = new List<SolutionFund>();
                        List<SolutionsModel> solutionsModel = new List<SolutionsModel>();
                        List<solutionFundViewModel> finalFundList = new List<solutionFundViewModel>();
                        var projectData = await _db.ContractUser.Where(x => x.ApplicationUserId == model.UserId).ToListAsync();
                        if (projectData.Count > 0)
                        {
                            foreach (var solution in projectData)
                            {
                                SolutionFund dataRestore = new SolutionFund();
                                var contractData = _db.Contract.Where(x => x.Id == solution.ContractId).FirstOrDefault();
                                if(contractData != null)
                                {
                                    var solutionFundData = _db.SolutionFund.Where(x => x.Id == contractData.SolutionFundId).FirstOrDefault();
                                    dataRestore.SolutionId = solutionFundData.SolutionId;
                                    dataRestore.IndustryId = solutionFundData.IndustryId;
                                    dataRestore.ProjectPrice = solutionFundData.ProjectPrice;
                                    dataRestore.Id = solutionFundData.Id;
                                }
                                solutionList.Add(dataRestore);

                            }
                            var grouped_teachers = solutionList
                             .GroupBy(t => new { t.IndustryId, t.SolutionId, t.ProjectType })
                             .Select(g => new
                             {
                                 FundList = g.ToList(),
                             });

                            foreach (var group in grouped_teachers)
                            {
                                var list = group.FundList;
                                solutionFundViewModel grouping = new solutionFundViewModel();
                                if (list.Count != 1)
                                {
                                    grouping.solutionFunds = list.Last();
                                    finalFundList.Add(grouping);
                                }
                                else
                                {
                                    grouping.solutionFunds = list.FirstOrDefault();
                                    finalFundList.Add(grouping);
                                }


                            }
                            if (finalFundList.Count > 0)
                            {
                                List<string> industrylist = new List<string>();
                                foreach (var data in finalFundList)
                                {
                                    SolutionsModel solutionsdataStore = new SolutionsModel();
                                    var solutionData = _db.Solutions.Where(x => x.Id == data.solutionFunds.SolutionId).FirstOrDefault();
                                    var serviceId = _db.SolutionServices.Where(x => x.SolutionId == data.solutionFunds.SolutionId).Select(x => x.ServicesId).FirstOrDefault();
                                    var serviceData = _db.Services.Where(x => x.Id == serviceId).Select(x => x.ServicesName).FirstOrDefault();
                                    var industryname = _db.Industries.Where(x => x.Id == data.solutionFunds.IndustryId).Select(x => x.IndustryName).FirstOrDefault();
                                    var contractStatus = _db.Contract.Where(x => x.SolutionFundId == data.solutionFunds.Id).Select(x => x.PaymentStatus).FirstOrDefault();
                                    var milestoneData = _db.SolutionMilestone.Where(x => x.Id == data.solutionFunds.MileStoneId).Select(x => x.Title).FirstOrDefault();
                                    var contractid = _db.Contract.Where(x => x.SolutionFundId == data.solutionFunds.Id).Select(x => x.Id).FirstOrDefault();
                                    var clientId = _db.SolutionFund.Where(x => x.Id == data.solutionFunds.Id).Select(f => f.ClientId).FirstOrDefault();

                                    solutionsdataStore.Services = serviceData;
                                    solutionsdataStore.ServiceId = serviceId;
                                    solutionsdataStore.SolutionId = data.solutionFunds.SolutionId;
                                    solutionsdataStore.IndustryId = data.solutionFunds.IndustryId;
                                    solutionsdataStore.Industries = industryname;
                                    solutionsdataStore.Id = data.solutionFunds.Id;
                                    solutionsdataStore.Title = solutionData.Title;
                                    solutionsdataStore.Description = solutionData.Description;
                                    solutionsdataStore.ImagePath = solutionData.ImagePath;
                                    solutionsdataStore.MileStoneTitle = milestoneData;
                                    solutionsdataStore.PaymentStatus = contractStatus.ToString();
                                    solutionsdataStore.ContractId = contractid;
                                    solutionsdataStore.ClientId = clientId;
                                    solutionsdataStore.ProjectStatus = data.solutionFunds.ProjectStatus;
                                    solutionsModel.Add(solutionsdataStore);
                                    industrylist.Clear();

                                }

                                return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                                {
                                    StatusCode = StatusCodes.Status200OK,
                                    Message = "success",
                                    Result = solutionsModel
                                });
                            }
                            return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                            {
                                StatusCode = StatusCodes.Status200OK,
                                Message = "success",
                                Result = solutionsModel
                            });
                        }
                        return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                        {
                            StatusCode = StatusCodes.Status200OK,
                            Message = "success",
                            Result = solutionsModel
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

            return StatusCode(StatusCodes.Status200OK, new APIResponseModel
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "Data not Found"
            });
        }

        //EditActiveSolutionDefineDetails
        [HttpPost]
        [Route("EditActiveSolutionDefineDetails")]
        public async Task<IActionResult> EditActiveSolutionDefineDetails([FromBody] SolutionIndustryDetailsModel model)
        {
            if (model != null)
            {
                if(model.Id != 0)
                {
                    var solutiondefineData = await _db.SolutionDefine.Where(x => x.Id == model.Id).FirstOrDefaultAsync();
                    if(solutiondefineData != null)
                    {
                        solutiondefineData.ProjectOutline = model.ProjectOutline;
                        solutiondefineData.ProjectDetails = model.ProjectDetails;
                        _db.SaveChanges();

                        return StatusCode(StatusCodes.Status200OK, new APIResponseModel {
                            StatusCode = StatusCodes.Status200OK,
                            Message = "Data Update Successfully!"
                        });
                    }
                }
            }

            return StatusCode(StatusCodes.Status500InternalServerError, new APIResponseModel { StatusCode = StatusCodes.Status403Forbidden, Message = "No Data Found" });

        }


        //get project count on dashbord
        [HttpGet]
        [Route("GetContractUser")]
        public async Task<IActionResult> GetContractUser([FromBody] string userId)
        {
            var contractUser = await _db.ContractUser.Where(x => x.ApplicationUserId == userId).ToListAsync();

            return StatusCode(StatusCodes.Status200OK, new APIResponseModel
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "success",
                Result = contractUser.Select(m => m.ContractId).Distinct().Count()
            });
        }

        //GetActiveProjectInvoices
        [HttpPost]
        [Route("GetActiveProjectInvoices")]
        public async Task<IActionResult> GetActiveProjectInvoices([FromBody] MileStoneIdViewModel model)
        {
            if (model != null)
            {
                if (model.UserId != null)
                {
                    try
                    {
                        List<SolutionsModel> solutionsModel = new List<SolutionsModel>();
                        var projectData = await _db.SolutionFund.Where(x => x.ClientId == model.UserId && x.ProjectStatus != "INITIATED").ToListAsync();
                        if (projectData.Count > 0)
                        {
                            foreach (var data in projectData)
                            {
                                SolutionsModel solutionsdataStore = new SolutionsModel();
                                solutionsdataStore.Industries = _db.Industries.Where(x => x.Id == data.IndustryId).Select(x => x.IndustryName).FirstOrDefault();
                                solutionsdataStore.Title = _db.Solutions.Where(x => x.Id == data.SolutionId).Select(x => x.Title).FirstOrDefault();
                                if(data.FundType.ToString() == "MilestoneFund")
                                {
                                    solutionsdataStore.MileStoneTitle = _db.SolutionMilestone.Where(x => x.Id == data.MileStoneId).Select(x => x.Title).FirstOrDefault();
                                }
                                solutionsModel.Add(solutionsdataStore);
                            }
                        }
                        return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                        {
                            StatusCode = StatusCodes.Status200OK,
                            Message = "success",
                            Result = solutionsModel
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

            return StatusCode(StatusCodes.Status200OK, new APIResponseModel
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "Data not Found"
            });
        }
    }
}
