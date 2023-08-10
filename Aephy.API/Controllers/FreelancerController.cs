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
            if (OpengigroleCv.AlreadyExist)
            {
                var applicantsDetails = _db.OpenGigRolesApplications.Where(x => x.ID == OpengigroleCv.ID).FirstOrDefault();
                var freelancerDetails = _db.FreelancerDetails.Where(x => x.UserId == OpengigroleCv.FreelancerId).FirstOrDefault();
                if (freelancerDetails != null)
                {
                    applicantsDetails.CVUrlWithSas = freelancerDetails.CVUrlWithSas;
                    applicantsDetails.CVPath = freelancerDetails.CVPath;
                    applicantsDetails.BlobStorageBaseUrl = freelancerDetails.BlobStorageBaseUrl;
                    _db.SaveChanges();
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
                var userData = _db.FreelancerDetails.Where(x => x.UserId == usercv.UserId).FirstOrDefault();
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

                var freelancerDetails = _db.FreelancerDetails.Where(flnc => flnc.UserId == model.UserId).FirstOrDefault();

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
                    var data = _db.SolutionMilestone.Where(x => x.Id == model.Id).FirstOrDefault();
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
                FreelancerDetails openRolesdata = _db.FreelancerDetails.Where(x => x.UserId == userData.Id).FirstOrDefault();
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
                var solutionIndustryDetails = _db.SolutionIndustryDetails.Where(x => x.IndustryId == model.IndustryId
                                                && x.SolutionId == model.SolutionId).FirstOrDefault();
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
                var milestoneList = _db.SolutionMilestone.Where(x => x.FreelancerId == model.FreelancerId
                && x.IndustryId == model.IndustryId && x.SolutionId == model.SolutionId
                && x.ProjectType == model.ProjectType).ToList();

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
                var data = _db.SolutionMilestone.Where(x => x.Id == model.Id).FirstOrDefault();
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
            }
            return StatusCode(StatusCodes.Status500InternalServerError, new APIResponseModel { StatusCode = StatusCodes.Status403Forbidden, Message = "Something Went Wrong." });
        }

        [HttpPost]
        [Route("GetPointsList")]
        public async Task<IActionResult> GetPointsList([FromBody] MileStoneDetailsViewModel model)
        {
            try
            {
                var pointsList = _db.SolutionPoints.Where(x => x.FreelancerId == model.FreelancerId
                && x.IndustryId == model.IndustryId && x.SolutionId == model.SolutionId
                && x.ProjectType == model.ProjectType).ToList();

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
                var pointsData = _db.SolutionPoints.Where(x => x.Id == model.Id).FirstOrDefault();
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
                var milestoneData = _db.SolutionMilestone.Where(x => x.Id == model.Id).FirstOrDefault();
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
                        _db.SaveChanges();

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
    }
}
