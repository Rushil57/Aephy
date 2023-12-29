using Aephy.API.AlgorithumHelper;
using Aephy.API.DBHelper;
using Aephy.API.Models;
using Aephy.API.NotificationMethod;

//using Aephy.API.Stripe;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Diagnostics;
using System.Xml.Linq;
using static Aephy.API.Models.AdminViewModel;

namespace Aephy.API.Controllers
{
    [Route("api/Freelancer/")]
    [ApiController]
    public class FreelancerController : ControllerBase
    {
        private readonly AephyAppDbContext _db;
        NotificationHelper notificationHelper = new NotificationHelper();


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
                    var freelancerType = _db.FreelancerDetails.Where(x => x.UserId == OpenGigRolesData.FreelancerID).FirstOrDefault();
                    var solutionId = _db.GigOpenRoles.Where(x => x.ID == OpenGigRolesData.GigOpenRoleId).Select(x => x.SolutionId).FirstOrDefault();
                    var solutionName = _db.Solutions.Where(x => x.Id == solutionId).Select(x => x.Title).FirstOrDefault();


                    List<Notifications> notificationsList = new List<Notifications>();
                    Notifications notifications = new Notifications();
                    notifications.NotificationText = "Thank you for submitting your application for the (" + freelancerType.FreelancerLevel + ") position in " + solutionName + ". Your application has been received and is currently under review.";
                    notifications.NotificationTitle = "Application Received";
                    notifications.ToUserId = "";
                    notifications.IsRead = false;
                    notifications.NotificationTime = DateTime.Now;
                    notifications.ToUserId = OpenGigRolesData.FreelancerID;
                    notificationsList.Add(notifications);

                    var adminDetails = _db.Users.Where(x => x.UserType == "Admin").FirstOrDefault();
                    var fisrtname = _db.Users.Where(x => x.Id == OpenGigRolesData.FreelancerID).FirstOrDefault();
                    Notifications adminnotifications = new Notifications();
                    adminnotifications.NotificationText = fisrtname.FirstName + " applied to " + freelancerType.FreelancerLevel;
                    adminnotifications.NotificationTitle = "Freelancer Application:";
                    adminnotifications.ToUserId = "";
                    adminnotifications.IsRead = false;
                    adminnotifications.NotificationTime = DateTime.Now;
                    adminnotifications.ToUserId = adminDetails.Id;
                    notificationsList.Add(adminnotifications);

                    await notificationHelper.SaveNotificationData(_db, notificationsList);

                    return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                    {
                        StatusCode = StatusCodes.Status200OK,
                        Message = "Applied Successfully",
                        Result = opengigroles_data.ID
                    });
                }
                else
                {
                    return StatusCode(StatusCodes.Status200OK, new APIResponseModel { StatusCode = StatusCodes.Status403Forbidden, Message = "Data not found" });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status200OK, new APIResponseModel { StatusCode = StatusCodes.Status403Forbidden, Message = ex.Message + ex.InnerException });
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
                        if (freelancerDetails != null)
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
                            IndustryName = IndName
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
                        Level = freelancerDetails?.FreelancerLevel,
                        Title = "-",
                        CreatedDateTime = "",
                        ID = 0,
                        SolutionId = fp?.SolutionID,
                        fp?.IndustryId,
                        Description = "",
                        SolutionName = solutionName,
                        ServiceName = serviceName,
                        IndustryName = IndName,
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

        [HttpGet]
        [Route("GetRequestList")]
        public async Task<IActionResult> GetRequestList([FromBody] GetUserProfileRequestModel model)
        {
            try
            {
                var requestdetails = await _db.FreelancerFindProcessDetails.Where(x => x.FreelancerId == model.UserId).ToListAsync();
                var solutionList = await _db.Solutions.ToListAsync();
                var industryList = await _db.Industries.ToListAsync();

                if (requestdetails.Any())
                {
                    var ids = requestdetails.Select(x => x.FreelancerFindProcessHeaderId).ToList();
                    var headers = await _db.FreelancerFindProcessHeader.Where(x => ids.Contains(x.Id)).ToListAsync();


                    List<dynamic> detailList = new List<dynamic>();

                    foreach (var item in headers)
                    {
                        var detailObject = requestdetails.Where(x => x.FreelancerFindProcessHeaderId == item.Id).FirstOrDefault();
                        var obj = new
                        {
                            FreelancerId = detailObject.FreelancerId,
                            SolutionName = solutionList.Where(x => x.Id == item.SolutionId).Select(x => x.Title).FirstOrDefault(),
                            IndustriesName = industryList.Where(x => x.Id == item.IndustryId).Select(x => x.IndustryName).FirstOrDefault(),
                            Id = item.Id,
                            ApproveStatus = detailObject.ApproveStatus,
                            DetailId = detailObject.Id
                        };

                        detailList.Add(obj);
                    }

                    return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                    {
                        StatusCode = StatusCodes.Status200OK,
                        Message = "Success",
                        Result = detailList
                    });
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


        [HttpPost]
        [Route("SaveMileStoneData")]
        public async Task<IActionResult> SaveMileStoneData([FromBody] MileStoneModel model)
        {
            if (model != null)
            {
                var customProjectId = 0;
                var IndustryDetails = _db.SolutionIndustryDetails.Where(x => x.SolutionId == model.SolutionId && x.IndustryId == model.IndustryId).FirstOrDefault();
                if (IndustryDetails != null)
                {
                    var solutionDefineData = _db.SolutionDefine.Where(x => x.SolutionIndustryDetailsId == IndustryDetails.Id && x.ProjectType == model.ProjectType && x.ClientId == model.UserId).FirstOrDefault();
                    if (solutionDefineData != null)
                    {
                        var customProjectDetails = _db.CustomProjectDetials.Where(x => x.SolutionDefineId == solutionDefineData.Id).FirstOrDefault();
                        if (customProjectDetails != null)
                        {
                            customProjectId = customProjectDetails.Id;
                        }
                    }
                }
                if (model.Id == 0)
                {
                    if (model.MilestoneSaveProjectIsActivePage && model.ProjectType.ToLower() != AppConst.ProjectType.CUSTOM_PROJECT)
                    {
                        var checkCustomProjectExists = _db.SolutionDefine.Where(x => x.SolutionIndustryDetailsId == IndustryDetails.Id && x.ProjectType == "custom" && x.ClientId == model.UserId).FirstOrDefault();
                        if (checkCustomProjectExists != null)
                        {
                            return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                            {
                                StatusCode = StatusCodes.Status200OK,
                                Message = "Custom Project already exists."
                            });
                        }
                        else
                        {
                            await ConvertPredefineProjectToCustom(model);
                        }
                    }
                    else
                    {
                        var milestone = new SolutionMilestone()
                        {
                            Title = model.Title,
                            Description = model.Description,
                            IndustryId = model.IndustryId,
                            SolutionId = model.SolutionId,
                            DueDate = DateTime.MinValue,
                            FreelancerId = model.UserId,
                            ProjectType = model.ProjectType,
                            Days = model.Days,
                            CustomProjectDetialsId = customProjectId,
                            ClientId = model.UserId
                        };

                        _db.SolutionMilestone.Add(milestone);
                        _db.SaveChanges();
                    }
                    if (model.ProjectType.ToLower() == AppConst.ProjectType.CUSTOM_PROJECT)
                    {
                        if (model.MilestoneSaveProjectIsActivePage)
                        {
                            var solutionFundData = _db.SolutionFund.Where(x => x.Id == model.SolutionFundId).FirstOrDefault();
                            if (solutionFundData != null)
                            {
                                solutionFundData.IsProjectPriceAlreadyCount = false;
                                _db.SaveChanges();
                            }
                        }
                    }

                    return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                    {
                        StatusCode = StatusCodes.Status200OK,
                        Message = "Data Saved Succesfully!."
                    });
                }
                else
                {
                    if (model.MilestoneSaveProjectIsActivePage)
                    {
                        if (model.ProjectType.ToLower() != AppConst.ProjectType.CUSTOM_PROJECT)
                        {
                            var checkCustomProjectExists = _db.SolutionDefine.Where(x => x.SolutionIndustryDetailsId == IndustryDetails.Id && x.ProjectType == "custom" && x.ClientId == model.UserId).FirstOrDefault();
                            if(checkCustomProjectExists != null)
                            {
                                return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                                {
                                    StatusCode = StatusCodes.Status200OK,
                                    Message = "Custom Project already exists."
                                });
                            }
                            else
                            {
                                await ConvertPredefineProjectToCustom(model);
                            }

                            
                        }
                        else
                        {
                            var data = await _db.SolutionMilestone.Where(x => x.Id == model.Id).FirstOrDefaultAsync();
                            if (data.Days != model.Days)
                            {
                                if (model.ProjectType.ToLower() == AppConst.ProjectType.CUSTOM_PROJECT)
                                {
                                    if (model.MilestoneSaveProjectIsActivePage)
                                    {
                                        var solutionFundData = _db.SolutionFund.Where(x => x.Id == model.SolutionFundId).FirstOrDefault();
                                        if (solutionFundData != null)
                                        {
                                            solutionFundData.IsProjectPriceAlreadyCount = false;
                                            _db.SaveChanges();
                                        }
                                    }
                                }
                            }
                            if (data != null)
                            {
                                data.Description = model.Description;
                                data.DueDate = DateTime.MinValue;
                                data.Title = model.Title;
                                data.ProjectType = model.ProjectType;
                                data.Days = model.Days;
                                _db.SaveChanges();
                            }
                        }
                    }
                    else
                    {
                        var data = await _db.SolutionMilestone.Where(x => x.Id == model.Id).FirstOrDefaultAsync();
                        if (data.Days != model.Days)
                        {
                            if (model.ProjectType.ToLower() == AppConst.ProjectType.CUSTOM_PROJECT)
                            {
                                if (model.MilestoneSaveProjectIsActivePage)
                                {
                                    var solutionFundData = _db.SolutionFund.Where(x => x.Id == model.SolutionFundId).FirstOrDefault();
                                    if (solutionFundData != null)
                                    {
                                        solutionFundData.IsProjectPriceAlreadyCount = false;
                                        _db.SaveChanges();
                                    }
                                }
                            }
                        }
                        if (data != null)
                        {
                            data.Description = model.Description;
                            data.DueDate = DateTime.MinValue;
                            data.Title = model.Title;
                            data.ProjectType = model.ProjectType;
                            data.Days = model.Days;
                            _db.SaveChanges();
                        }
                    }
                    return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                    {
                        StatusCode = StatusCodes.Status200OK,
                        Message = "Data Updated Succesfully!."
                    });

                }

            }

            return StatusCode(StatusCodes.Status500InternalServerError, new APIResponseModel { StatusCode = StatusCodes.Status403Forbidden, Message = "Something Went Wrong." });

        }

        //ConvertPredefineProjectToCustom
        [HttpPost]
        [Route("ConvertPredefineProjectToCustom")]
        public async Task<string> ConvertPredefineProjectToCustom(MileStoneModel model)
        {
            if (model != null)
            {
                if (model.SolutionFundId != 0)
                {
                    var solutionFundData = await _db.SolutionFund.Where(x => x.Id == model.SolutionFundId).FirstOrDefaultAsync();
                    if (solutionFundData != null)
                    {
                        var ClientDetails = _db.Users.Where(x => x.Id == solutionFundData.ClientId).FirstOrDefault();

                        var solutionndustryDetails = _db.SolutionIndustryDetails.Where(x => x.SolutionId == model.SolutionId && x.IndustryId == model.IndustryId).FirstOrDefault();
                        if (solutionndustryDetails != null)
                        {
                            var solutionDefineData = _db.SolutionDefine.Where(x => x.SolutionIndustryDetailsId == solutionndustryDetails.Id && x.ProjectType == model.ProjectType).FirstOrDefault();
                            if (solutionDefineData != null)
                            {
                                var defineData = new SolutionDefine()
                                {
                                    SolutionIndustryDetailsId = solutionndustryDetails.Id,
                                    ProjectOutline = solutionDefineData.ProjectOutline,
                                    ProjectDetails = solutionDefineData.ProjectDetails,
                                    ProjectType = "custom",
                                    CreatedDateTime = DateTime.Now,
                                    IsActive = true,
                                    Duration = solutionDefineData.Duration,
                                    TeamSize = solutionDefineData.TeamSize,
                                    ClientId = solutionFundData.ClientId
                                };
                                _db.SolutionDefine.Add(defineData);
                                _db.SaveChanges();


                                var customProject = new CustomProjectDetials()
                                {
                                    SolutionDefineId = defineData.Id,
                                    ProjectDuration = solutionDefineData.Duration,
                                    EstimatedPrice = decimal.Parse(solutionFundData.ProjectPrice),
                                    StartHour = ClientDetails.StartHours,
                                    EndHour = ClientDetails.EndHours,
                                    ClientId = solutionFundData.ClientId,
                                    IsSingleFreelancer = false,
                                    IsExcludeWeekend = false,
                                };
                                _db.CustomProjectDetials.Add(customProject);
                                _db.SaveChanges();

                                var milestoneData = _db.SolutionMilestone.Where(x => x.SolutionId == solutionFundData.SolutionId && x.IndustryId == solutionFundData.IndustryId && x.ProjectType == model.ProjectType).ToList();
                                var totalMilestoneDays = 0;

                                if (milestoneData.Count > 0)
                                {
                                    foreach (var data in milestoneData)
                                    {
                                        SolutionMilestone? milestone = null;
                                        if (data.Id == model.Id)
                                        {
                                            milestone = new SolutionMilestone()
                                            {
                                                Title = model.Title,
                                                Description = model.Description,
                                                IndustryId = data.IndustryId,
                                                SolutionId = data.SolutionId,
                                                DueDate = DateTime.MinValue,
                                                FreelancerId = solutionFundData.ClientId,
                                                ProjectType = "custom",
                                                Days = model.Days,
                                                CustomProjectDetialsId = customProject.Id,
                                                ClientId = solutionFundData.ClientId
                                            };
                                        }
                                        else
                                        {
                                            milestone = new SolutionMilestone()
                                            {
                                                Title = data.Title,
                                                Description = data.Description,
                                                IndustryId = data.IndustryId,
                                                SolutionId = data.SolutionId,
                                                DueDate = DateTime.MinValue,
                                                FreelancerId = solutionFundData.ClientId,
                                                ProjectType = "custom",
                                                Days = data.Days,
                                                CustomProjectDetialsId = customProject.Id,
                                                ClientId = solutionFundData.ClientId
                                            };
                                        }

                                        totalMilestoneDays += milestone.Days;
                                        _db.SolutionMilestone.Add(milestone);
                                        _db.SaveChanges();
                                    }

                                }

                                // when new mileston is added then add to custom project
                                if (model.Id == 0)
                                {
                                    var newmilestone = new SolutionMilestone()
                                    {
                                        Title = model.Title,
                                        Description = model.Description,
                                        IndustryId = model.IndustryId,
                                        SolutionId = model.SolutionId,
                                        DueDate = DateTime.MinValue,
                                        FreelancerId = solutionFundData.ClientId,
                                        ProjectType = "custom",
                                        Days = model.Days,
                                        CustomProjectDetialsId = customProject.Id,
                                        ClientId = solutionFundData.ClientId
                                    };

                                    totalMilestoneDays += newmilestone.Days;
                                    _db.SolutionMilestone.Add(newmilestone);
                                    _db.SaveChanges();
                                }

                                var solutionPointsData = _db.SolutionPoints.Where(x => x.SolutionId == solutionFundData.SolutionId && x.IndustryId == solutionFundData.IndustryId && x.ProjectType == model.ProjectType).ToList();
                                if (solutionPointsData.Count > 0)
                                {
                                    foreach (var data in solutionPointsData)
                                    {
                                        var highlightdata = new SolutionPoints()
                                        {
                                            PointKey = data.PointKey,
                                            PointValue = data.PointValue,
                                            IndustryId = data.IndustryId,
                                            SolutionId = data.SolutionId,
                                            FreelancerId = solutionFundData.ClientId,
                                            ProjectType = "custom",
                                            CustomProjectDetialsId = customProject.Id,
                                            ClientId = solutionFundData.ClientId
                                        };

                                        _db.SolutionPoints.Add(highlightdata);
                                        _db.SaveChanges();
                                    }
                                }


                                var solutionTeamData = _db.SolutionTeam.Where(x => x.SolutionFundId == model.SolutionFundId).ToList();
                                if(solutionTeamData.Count > 0)
                                {
                                    foreach(var teamdata in solutionTeamData)
                                    {
                                        var freelancerPreferedCurrency = _db.Users.Where(x => x.Id == teamdata.FreelancerId).FirstOrDefault().PreferredCurrency;
                                        var freelancerDetails = _db.FreelancerDetails.Where(x => x.UserId == teamdata.FreelancerId).FirstOrDefault();

                                        if (string.IsNullOrEmpty(freelancerPreferedCurrency))
                                        {
                                            freelancerPreferedCurrency = "EUR";
                                        }
                                        if (string.IsNullOrEmpty(ClientDetails.PreferredCurrency))
                                        {
                                            ClientDetails.PreferredCurrency = "EUR";
                                        }
                                        var exchangeRate = _db.ExchangeRates.Where(x => x.FromCurrency == freelancerPreferedCurrency && x.ToCurrency == ClientDetails.PreferredCurrency).FirstOrDefault();

                                        var HourlyRate = Convert.ToDecimal(freelancerDetails.HourlyRate);
                                        decimal ExchangeHourlyRate = HourlyRate;
                                        if (exchangeRate != null)
                                        {
                                            ExchangeHourlyRate = Convert.ToDecimal((decimal)(HourlyRate * exchangeRate.Rate));
                                        }

                                        decimal contractAmount = contractAmount = (totalMilestoneDays * 8 * ExchangeHourlyRate);
                                        var Platformfees = (contractAmount * AppConst.Commission.PLATFORM_COMM_FROM_FREELANCER_CUSTOM) / 100;

                                        teamdata.Amount = contractAmount;
                                        teamdata.PlatformFees = Platformfees;
                                        _db.SaveChanges();

                                    }
                                }
                            }
                        }

                        solutionFundData.ProjectType = "custom";
                        solutionFundData.IsProjectPriceAlreadyCount = false;
                        _db.SaveChanges();

                        return "Successfully Converted";
                    }
                }
            }
            return "Data not found";
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
                    solutionIndustryDetails.ActionOn = DateTime.Now;
                    solutionIndustryDetails.IsApproved = 1;
                    _db.SaveChanges();

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
                        IsActive = true,
                        ClientId = model.UserId
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
                decimal customPrice = 0;
                var milestoneList = await _db.SolutionMilestone.Where(x => x.IndustryId == model.IndustryId && x.SolutionId == model.SolutionId
                                    && x.ProjectType == model.ProjectType
                                    ).ToListAsync();

                if (model.ProjectType == "custom")
                {
                    milestoneList = milestoneList.Where(x => x.CustomProjectDetialsId != 0 && x.CustomProjectDetialsId == model.CustomProjectDetailId && x.ClientId == model.UserId).ToList();
                    var solutionFundData = _db.SolutionFund.Where(x => x.CustomProjectDetialsId == model.CustomProjectDetailId).FirstOrDefault();

                }

                return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                {
                    StatusCode = StatusCodes.Status200OK,
                    Message = "Success",
                    Result = new
                    {
                        MileStoneList = milestoneList,
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
                        PointKey = model.PointKey,
                        PointValue = model.PointValue,
                        IndustryId = model.IndustryId,
                        SolutionId = model.SolutionId,
                        FreelancerId = model.FreelancerId,
                        ProjectType = model.ProjectType,
                        ClientId = model.ClientId,
                        CustomProjectDetialsId = model.CustomProjectDetialsId
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
                var pointsList = await _db.SolutionPoints.Where(x => x.IndustryId == model.IndustryId && x.SolutionId == model.SolutionId
                                 && x.ProjectType == model.ProjectType).ToListAsync();

                if (model.ProjectType == "custom")
                {
                    pointsList = pointsList.Where(x => x.CustomProjectDetialsId != 0 && x.CustomProjectDetialsId == model.CustomProjectDetailId && x.ClientId == model.UserId).ToList();
                }

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
                    var solutionDefineModel = await _db.SolutionDefine.Where(x => x.SolutionIndustryDetailsId == solutionIndustryDetailsId
                                                && x.ProjectType == model.ProjectType).FirstOrDefaultAsync();


                    return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                    {
                        StatusCode = StatusCodes.Status200OK,
                        Message = "Success",
                        Result = solutionDefineModel
                    });

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
        [Route("DeleteFreelancerSolution")]
        public async Task<IActionResult> DeleteFreelancerSolution([FromBody] MileStoneDetailsViewModel model)
        {
            try
            {
                var freelancerPooldata = _db.FreelancerPool.Where(x => x.SolutionID == model.SolutionId && x.IndustryId == model.IndustryId && x.FreelancerID == model.FreelancerId).FirstOrDefault();
                if (freelancerPooldata != null)
                {
                    _db.FreelancerPool.Remove(freelancerPooldata);
                    _db.SaveChanges();
                }

                var gigId = _db.GigOpenRoles.Where(x => x.SolutionId == model.SolutionId && x.IndustryId == model.IndustryId && x.Level == model.FreelancerLevel.Trim()).Select(x => x.ID).FirstOrDefault();
                if (gigId != 0)
                {
                    var freelancerData = _db.OpenGigRolesApplications.Where(x => x.GigOpenRoleId == gigId && x.FreelancerID == model.FreelancerId).FirstOrDefault();
                    if (freelancerData != null)
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
                    var solutionData = await _db.Solutions.Where(x => x.Id == model.Id).FirstOrDefaultAsync();
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
                    if (notificationData.Count > 0)
                    {
                        foreach (var data in notificationData)
                        {
                            Notifications dataStore = new Notifications();
                            var fullname = _db.Users.Where(x => x.Id == data.FromUserId).Select(x => new { x.FirstName, x.LastName }).FirstOrDefault();
                            if (fullname != null)
                            {
                                dataStore.FromUserId = fullname.FirstName + " " + fullname.LastName;
                            }

                            dataStore.NotificationText = data.NotificationText.Split("||")[0];
                            dataStore.NotificationTime = data.NotificationTime;
                            dataStore.Id = data.Id;
                            dataStore.NotificationTitle = data.NotificationTitle;
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
                            if (fullname != null)
                            {
                                dataStore.FromUserId = fullname.FirstName + " " + fullname.LastName;
                            }

                            dataStore.NotificationText = data.NotificationText;
                            dataStore.NotificationTime = data.NotificationTime;
                            dataStore.Id = data.Id;
                            dataStore.NotificationTitle = data.NotificationTitle;
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
                    if (notificationData != null)
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
                    var saveprojectData = _db.SavedProjects.Where(x => x.SolutionId == model.SolutionId && x.IndustryId == model.IndustryId && x.UserId == model.UserId).FirstOrDefault();
                    if (saveprojectData != null)
                    {
                        return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                        {
                            StatusCode = StatusCodes.Status200OK,
                            Message = "Project is already saved!",
                        });
                    }
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
                                solutionsdataStore.Industries = _db.Industries.Where(x => x.Id == data.IndustryId).Select(x => x.IndustryName).FirstOrDefault();
                                solutionsdataStore.IndustryId = data.IndustryId;
                                solutionsdataStore.Id = solutionData.Id;
                                solutionsdataStore.Title = solutionData.Title;
                                solutionsdataStore.Description = solutionData.Description;
                                solutionsdataStore.ImagePath = solutionData.ImagePath;
                                solutionsdataStore.ServiceId = _db.SolutionServices.Where(x => x.SolutionId == solutionData.Id).Select(x => x.ServicesId).FirstOrDefault();
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
                        var projectData = await _db.SavedProjects.Where(x => x.SolutionId == model.SolutionId && x.IndustryId == model.IndustryId && x.UserId == model.UserId).FirstOrDefaultAsync();
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
                                //var industryIdlist = _db.SolutionIndustry.Where(x => x.SolutionId == data.SolutionId).Select(x => x.IndustryId).ToList();
                                //if (industryIdlist.Count > 0)
                                //{
                                //    foreach (var industryId in industryIdlist)
                                //    {
                                //        var industryname = _db.Industries.Where(x => x.Id == industryId).Select(x => x.IndustryName).FirstOrDefault();
                                //        industrylist.Add(industryname);
                                //    }
                                //}

                                solutionsdataStore.Services = serviceData;
                                solutionsdataStore.Industries = _db.Industries.Where(x => x.Id == data.IndustryId).Select(x => x.IndustryName).FirstOrDefault(); ;
                                solutionsdataStore.Id = data.Id;
                                solutionsdataStore.Title = solutionData.Title;
                                solutionsdataStore.Description = solutionData.Description;
                                solutionsdataStore.ImagePath = solutionData.ImagePath;
                                solutionsdataStore.SolutionId = data.SolutionId;
                                solutionsdataStore.IndustryId = data.IndustryId;
                                solutionsdataStore.ServiceId = serviceId;
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
                        if (projectData.Count > 0)
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
                                    var checkProjectStopByClient = _db.SolutionFund.Where(x => x.Id == data.solutionFunds.Id).FirstOrDefault();
                                    var checkContractCompleted = _db.Contract.Where(x => x.SolutionFundId == data.solutionFunds.Id).Select(x => x.PaymentStatus).FirstOrDefault();
                                    if (checkContractCompleted != Contract.PaymentStatuses.Splitted && data.solutionFunds.IsStoppedProject == false)
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
        public async Task<IActionResult> GetInvoiceDetails([FromBody] InvoiceListViewModel model)
        {
            try
            {
                if (model != null)
                {
                    if (model.InvoiceId != 0)
                    {
                        var invoicelistDetails = await _db.InvoiceList.Where(x => x.Id == model.InvoiceId && x.BillToClientId == model.UserId).FirstOrDefaultAsync();
                        if (invoicelistDetails != null)
                        {
                            InvoiceListViewModel InvoiceDetails = new InvoiceListViewModel();
                            InvoiceDetails.TransactionType = invoicelistDetails.TransactionType;
                            InvoiceDetails.InvoiceType = invoicelistDetails.InvoiceType;
                            var invoiceListDetails = await _db.InvoiceListDetails.Where(x => x.InvoiceListId == invoicelistDetails.Id).ToListAsync();
                            if (invoiceListDetails.Count > 0)
                            {
                                InvoiceDetails.InvoicelistDetails = invoiceListDetails;
                            }
                            InvoiceDetails.InvoiceNumber = invoicelistDetails.InvoiceNumber;
                            InvoiceDetails.InvoiceDate = invoicelistDetails.InvoiceDate;
                            var clientDetails = _db.Users.Where(x => x.Id == invoicelistDetails.BillToClientId).FirstOrDefault();
                            if (clientDetails != null)
                            {
                                var fullname = clientDetails.FirstName + " " + clientDetails.LastName;
                                InvoiceDetails.ClientFullName = fullname;
                                InvoiceDetails.TaxType = clientDetails.TaxType;
                                InvoiceDetails.TaxId = clientDetails.TaxNumber;
                                var CurrencySign = await notificationHelper.ConvertToCurrencySign(clientDetails.PreferredCurrency);
                                InvoiceDetails.PreferredCurrency = CurrencySign.ToString();
                                InvoiceDetails.ClientCountry = _db.Country.Where(x => x.Id == clientDetails.CountryId).Select(x => x.Code).FirstOrDefault();
                            }
                            var clientaddressDetails = _db.ClientDetails.Where(x => x.UserId == invoicelistDetails.BillToClientId).FirstOrDefault();
                            if (clientaddressDetails != null)
                            {
                                InvoiceDetails.ClientCompanyName = clientaddressDetails.CompanyName;
                                InvoiceDetails.ClientAddress = clientaddressDetails.Address;
                            }
                            if (invoicelistDetails.FreelancerId != null)
                            {
                                var freelancerDetails = _db.Users.Where(x => x.Id == invoicelistDetails.FreelancerId).FirstOrDefault();
                                if (freelancerDetails != null)
                                {
                                    var freelanceraddressDetails = _db.FreelancerDetails.Where(x => x.UserId == invoicelistDetails.FreelancerId).FirstOrDefault();
                                    var fullname = freelancerDetails.FirstName + " " + freelancerDetails.LastName;
                                    InvoiceDetails.FreelancerFullName = fullname;
                                    InvoiceDetails.FreelancerTaxType = freelancerDetails.TaxType;
                                    InvoiceDetails.FreelancerTaxId = freelancerDetails.TaxNumber;
                                    InvoiceDetails.FreelancerAddress = freelanceraddressDetails.Address;
                                    InvoiceDetails.FreelancerCountry = _db.Country.Where(x => x.Id == freelancerDetails.CountryId).Select(x => x.Code).FirstOrDefault();
                                    //var CurrencySign = await _clientcontroller.ConvertToCurrencySign(clientDetails.PreferredCurrency);

                                }
                            }
                            InvoiceDetails.TotalAmount = invoicelistDetails.TotalAmount;


                            return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                            {
                                StatusCode = StatusCodes.Status200OK,
                                Message = "success",
                                Result = InvoiceDetails
                            });
                        }
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
                return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                {
                    StatusCode = StatusCodes.Status200OK,
                    Message = ex.Message
                });
            }
        }

        //GetInvoiceDetails
        [HttpPost]
        [Route("GetFreelancerInvoiceDetails")]
        public async Task<IActionResult> GetFreelancerInvoiceDetails([FromBody] InvoiceListViewModel model)
        {
            try
            {
                if (model != null)
                {
                    if (model.InvoiceId != 0)
                    {
                        var invoicelistDetails = await _db.InvoiceList.Where(x => x.Id == model.InvoiceId && x.FreelancerId == model.UserId).FirstOrDefaultAsync();
                        if (invoicelistDetails != null)
                        {
                            InvoiceListViewModel InvoiceDetails = new InvoiceListViewModel();
                            InvoiceDetails.TransactionType = invoicelistDetails.TransactionType;
                            InvoiceDetails.InvoiceType = invoicelistDetails.InvoiceType;
                            var invoiceListDetails = await _db.InvoiceListDetails.Where(x => x.InvoiceListId == invoicelistDetails.Id).ToListAsync();
                            if (invoiceListDetails.Count > 0)
                            {
                                InvoiceDetails.InvoicelistDetails = invoiceListDetails;
                            }
                            InvoiceDetails.InvoiceNumber = invoicelistDetails.InvoiceNumber;
                            InvoiceDetails.InvoiceDate = invoicelistDetails.InvoiceDate;
                            var clientDetails = _db.Users.Where(x => x.Id == invoicelistDetails.BillToClientId).FirstOrDefault();
                            if (clientDetails != null)
                            {
                                var fullname = clientDetails.FirstName + " " + clientDetails.LastName;
                                InvoiceDetails.ClientFullName = fullname;
                                InvoiceDetails.TaxType = clientDetails.TaxType;
                                InvoiceDetails.TaxId = clientDetails.TaxNumber;
                                var CurrencySign = await notificationHelper.ConvertToCurrencySign(clientDetails.PreferredCurrency);
                                InvoiceDetails.PreferredCurrency = CurrencySign.ToString();
                                InvoiceDetails.ClientCountry = _db.Country.Where(x => x.Id == clientDetails.CountryId).Select(x => x.Code).FirstOrDefault();
                            }
                            var clientaddressDetails = _db.ClientDetails.Where(x => x.UserId == invoicelistDetails.BillToClientId).Select(x => x.Address).FirstOrDefault();
                            if (clientaddressDetails != null)
                            {
                                InvoiceDetails.ClientAddress = clientaddressDetails;
                            }
                            if (invoicelistDetails.FreelancerId != null)
                            {
                                var freelancerDetails = _db.Users.Where(x => x.Id == invoicelistDetails.FreelancerId).FirstOrDefault();
                                if (freelancerDetails != null)
                                {
                                    var freelanceraddressDetails = _db.FreelancerDetails.Where(x => x.UserId == invoicelistDetails.FreelancerId).FirstOrDefault();
                                    var fullname = freelancerDetails.FirstName + " " + freelancerDetails.LastName;
                                    InvoiceDetails.FreelancerFullName = fullname;
                                    InvoiceDetails.FreelancerTaxType = freelancerDetails.TaxType;
                                    InvoiceDetails.FreelancerTaxId = freelancerDetails.TaxNumber;
                                    InvoiceDetails.FreelancerAddress = freelanceraddressDetails.Address;
                                    InvoiceDetails.FreelancerCountry = _db.Country.Where(x => x.Id == freelancerDetails.CountryId).Select(x => x.Code).FirstOrDefault();
                                    //var CurrencySign = await _clientcontroller.ConvertToCurrencySign(clientDetails.PreferredCurrency);

                                }
                            }
                            InvoiceDetails.TotalAmount = invoicelistDetails.TotalAmount;


                            return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                            {
                                StatusCode = StatusCodes.Status200OK,
                                Message = "success",
                                Result = InvoiceDetails
                            });
                        }
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

                if (model.SolutionFundId == 0)
                {
                    var solutionFund = _db.SolutionFund.Where(x => x.IndustryId == model.IndustryId && x.SolutionId == model.SolutionID && x.ClientId == model.LoginFreelancerId).FirstOrDefault();
                    if (solutionFund != null)
                    {
                        model.SolutionFundId = solutionFund.Id;
                    }
                }

                var userList = await _db.Users.ToListAsync();
                var ids = await _db.SolutionTeam.Where(x => x.SolutionFundId == model.SolutionFundId).Select(x => x.FreelancerId).ToListAsync();

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
                        var projectData = await _db.SolutionTeam.Where(x => x.FreelancerId == model.UserId).ToListAsync();
                        if (projectData.Count > 0)
                        {
                            foreach (var solution in projectData)
                            {
                                SolutionFund dataRestore = new SolutionFund();
                                var solutionFundData = _db.SolutionFund.Where(x => x.Id == solution.SolutionFundId).FirstOrDefault();
                                if (solutionFundData != null)
                                {
                                    if (solutionFundData.IsStoppedProject == false)
                                    {
                                        dataRestore.SolutionId = solutionFundData.SolutionId;
                                        dataRestore.IndustryId = solutionFundData.IndustryId;
                                        dataRestore.ProjectPrice = solutionFundData.ProjectPrice;
                                        dataRestore.Id = solutionFundData.Id;
                                        dataRestore.MileStoneId = solutionFundData.MileStoneId;
                                        dataRestore.FundType = solutionFundData.FundType;
                                        solutionList.Add(dataRestore);
                                    }
                                }


                            }

                            if (solutionList.Count > 0)
                            {
                                var grouped_activeSolutions = solutionList
                           .GroupBy(t => new { t.IndustryId, t.SolutionId, t.ProjectType })
                           .Select(g => new
                           {
                               FundList = g.ToList(),
                           });

                                foreach (var group in grouped_activeSolutions)
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
                                        var checkContractCompleted = _db.Contract.Where(x => x.SolutionFundId == data.solutionFunds.Id).Select(x => x.PaymentStatus).FirstOrDefault();
                                        if (checkContractCompleted != Contract.PaymentStatuses.Splitted)
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
                                    }

                                    return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                                    {
                                        StatusCode = StatusCodes.Status200OK,
                                        Message = "success",
                                        Result = solutionsModel
                                    });
                                }
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

        //EditActiveSolutionDefineDetails
        [HttpPost]
        [Route("EditActiveSolutionDefineDetails")]
        public async Task<IActionResult> EditActiveSolutionDefineDetails([FromBody] SolutionIndustryDetailsModel model)
        {
            if (model != null)
            {
                if (model.Id != 0)
                {
                    var solutiondefineData = await _db.SolutionDefine.Where(x => x.Id == model.Id).FirstOrDefaultAsync();
                    if (solutiondefineData != null)
                    {
                        solutiondefineData.ProjectOutline = model.ProjectOutline;
                        solutiondefineData.ProjectDetails = model.ProjectDetails;
                        _db.SaveChanges();

                        return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                        {
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
                                solutionsdataStore.ContractId = _db.Contract.Where(x => x.SolutionFundId == data.Id).Select(x => x.Id).FirstOrDefault();
                                if (data.FundType == SolutionFund.FundTypes.MilestoneFund)
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

        [HttpPost]
        [Route("GetFreelancerInvoices")]
        public async Task<IActionResult> GetFreelancerInvoices([FromBody] MileStoneIdViewModel model)
        {
            if (model != null)
            {
                if (model.UserId != null)
                {
                    try
                    {
                        var fundIds = await _db.SolutionTeam.Where(x => x.FreelancerId == model.UserId).Select(y => y.SolutionFundId).Distinct().ToListAsync();
                        List<SolutionsModel> solutionsModel = new List<SolutionsModel>();
                        var projectData = await _db.SolutionFund.Where(x => fundIds.Contains(x.Id) && x.ProjectStatus != "INITIATED").ToListAsync();
                        if (projectData.Count > 0)
                        {
                            foreach (var data in projectData)
                            {
                                SolutionsModel solutionsdataStore = new SolutionsModel();
                                solutionsdataStore.Industries = _db.Industries.Where(x => x.Id == data.IndustryId).Select(x => x.IndustryName).FirstOrDefault();
                                solutionsdataStore.Title = _db.Solutions.Where(x => x.Id == data.SolutionId).Select(x => x.Title).FirstOrDefault();
                                solutionsdataStore.ContractId = _db.Contract.Where(x => x.SolutionFundId == data.Id).Select(x => x.Id).FirstOrDefault();
                                if (data.FundType == SolutionFund.FundTypes.MilestoneFund)
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

        //GetArchivesProject
        [HttpPost]
        [Route("GetArchivesProject")]
        public async Task<IActionResult> GetArchivesProject([FromBody] MileStoneIdViewModel model)
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
                        if (projectData.Count > 0)
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
                                    var checkContractCompleted = _db.Contract.Where(x => x.SolutionFundId == data.solutionFunds.Id).Select(x => x.PaymentStatus).FirstOrDefault();
                                    if (checkContractCompleted == Contract.PaymentStatuses.Splitted || data.solutionFunds.IsStoppedProject)
                                    {
                                        SolutionsModel solutionsdataStore = new SolutionsModel();
                                        solutionsdataStore.Industries = _db.Industries.Where(x => x.Id == data.solutionFunds.IndustryId).Select(x => x.IndustryName).FirstOrDefault();
                                        solutionsdataStore.Title = _db.Solutions.Where(x => x.Id == data.solutionFunds.SolutionId).Select(x => x.Title).FirstOrDefault();
                                        solutionsdataStore.ContractId = _db.Contract.Where(x => x.SolutionFundId == data.solutionFunds.Id).Select(x => x.Id).FirstOrDefault();
                                        solutionsdataStore.ServiceId = _db.SolutionServices.Where(x => x.SolutionId == data.solutionFunds.SolutionId).Select(x => x.ServicesId).FirstOrDefault();
                                        solutionsdataStore.SolutionId = data.solutionFunds.SolutionId;
                                        solutionsdataStore.IndustryId = data.solutionFunds.IndustryId;
                                        solutionsdataStore.Id = data.solutionFunds.Id;
                                        if (data.solutionFunds.FundType.ToString() == "MilestoneFund")
                                        {
                                            solutionsdataStore.MileStoneTitle = _db.SolutionMilestone.Where(x => x.Id == data.solutionFunds.MileStoneId).Select(x => x.Title).FirstOrDefault();
                                        }
                                        solutionsModel.Add(solutionsdataStore);
                                    }
                                }
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
        [Route("GetFreelancerArchivesProject")]
        public async Task<IActionResult> GetFreelancerArchivesProject([FromBody] MileStoneIdViewModel model)
        {
            if (model != null)
            {
                if (model.UserId != null)
                {
                    try
                    {
                        List<SolutionFund> solutionList = new List<SolutionFund>();
                        List<solutionFundViewModel> finalFundList = new List<solutionFundViewModel>();
                        List<SolutionsModel> solutionsModel = new List<SolutionsModel>();
                        var projectData = await _db.SolutionTeam.Where(x => x.FreelancerId == model.UserId).ToListAsync();
                        if (projectData.Count > 0)
                        {
                            foreach (var solution in projectData)
                            {

                                SolutionFund dataRestore = new SolutionFund();
                                var solutionFundData = _db.SolutionFund.Where(x => x.Id == solution.SolutionFundId).FirstOrDefault();
                                if (solutionFundData != null)
                                {
                                    var contractData = _db.Contract.Where(x => x.SolutionFundId == solutionFundData.Id).FirstOrDefault();
                                    var IsProjectCompleteed = false;
                                    if (contractData != null)
                                    {
                                        if (contractData.PaymentStatus == Contract.PaymentStatuses.Splitted)
                                        {
                                            IsProjectCompleteed = true;
                                        }
                                    }
                                    if (solutionFundData.IsStoppedProject || IsProjectCompleteed)
                                    {
                                        dataRestore.SolutionId = solutionFundData.SolutionId;
                                        dataRestore.IndustryId = solutionFundData.IndustryId;
                                        dataRestore.ProjectPrice = solutionFundData.ProjectPrice;
                                        dataRestore.MileStoneId = solutionFundData.MileStoneId;
                                        dataRestore.FundType = solutionFundData.FundType;
                                        dataRestore.Id = solutionFundData.Id;
                                        solutionList.Add(dataRestore);
                                    }
                                }

                            }

                            var grouped_solutions = solutionList
                             .GroupBy(t => new { t.IndustryId, t.SolutionId, t.ProjectType })
                             .Select(g => new
                             {
                                 FundList = g.ToList(),
                             });

                            foreach (var group in grouped_solutions)
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
                                    var industryname = _db.Industries.Where(x => x.Id == data.solutionFunds.IndustryId).Select(x => x.IndustryName).FirstOrDefault();
                                    var milestoneData = _db.SolutionMilestone.Where(x => x.Id == data.solutionFunds.MileStoneId).Select(x => x.Title).FirstOrDefault();
                                    var contractid = _db.Contract.Where(x => x.SolutionFundId == data.solutionFunds.Id).Select(x => x.Id).FirstOrDefault();
                                    solutionsdataStore.Industries = industryname;
                                    solutionsdataStore.Title = solutionData.Title;
                                    if (data.solutionFunds.FundType.ToString() == "MilestoneFund")
                                    {
                                        solutionsdataStore.MileStoneTitle = milestoneData;
                                    }
                                    solutionsdataStore.ContractId = contractid;
                                    solutionsdataStore.Id = data.solutionFunds.Id;
                                    solutionsModel.Add(solutionsdataStore);

                                }
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

        //GetFreelancerListSolutionWise
        [HttpPost]
        [Route("GetFreelancerListSolutionWise")]
        public async Task<IActionResult> GetFreelancerListSolutionWise([FromBody] SolutionTeamViewModel model)
        {
            if (model != null)
            {
                var solutionTeamData = await _db.SolutionTeam.Where(x => x.SolutionFundId == model.SolutionFundId).ToListAsync();
                List<SolutionTeamViewModel> TeamList = new List<SolutionTeamViewModel>();
                if (solutionTeamData.Count > 0)
                {
                    var solutionFunddata = _db.SolutionFund.Where(x => x.Id == model.SolutionFundId).FirstOrDefault();
                    foreach (var data in solutionTeamData)
                    {
                        SolutionTeamViewModel teamData = new SolutionTeamViewModel();
                        if (data.FreelancerId != model.UserId)
                        {
                            var fullname = _db.Users.Where(x => x.Id == data.FreelancerId).Select(x => new { x.FirstName, x.LastName }).FirstOrDefault();
                            teamData.FreelancerName = fullname.FirstName + " " + fullname.LastName;
                            teamData.FreelancerId = data.FreelancerId;
                            teamData.IndustryId = solutionFunddata.IndustryId;
                            teamData.SolutionId = solutionFunddata.SolutionId;
                            TeamList.Add(teamData);
                        }

                    }
                }
                return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                {
                    StatusCode = StatusCodes.Status200OK,
                    Message = "success",
                    Result = TeamList
                });

            }

            return StatusCode(StatusCodes.Status200OK, new APIResponseModel
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "Data not Found"
            });
        }

        //SaveFreelancerToFreelancerReview
        [HttpPost]
        [Route("SaveFreelancerToFreelancerReview")]
        public async Task<IActionResult> SaveFreelancerToFreelancerReview([FromBody] FreelancerToFreelancerReviewModel model)
        {
            if (model != null)
            {
                var reviewData = new FreelancerToFreelancerReview()
                {
                    ToFreelancerId = model.ToFreelancerId,
                    FromFreelancerId = model.FromFreelancerId,
                    Feedback_Message = model.Feedback_Message,
                    SolutionId = model.SolutionId,
                    IndustryId = model.IndustryId,
                    CollaborationAndTeamWork = model.CollaborationTeamWorkRating,
                    Responsiveness = model.ResponsivenessRating,
                    Communication = model.CommunicationRating,
                    Professionalism = model.ProfessionalismRating,
                    TechnicalSkills = model.TechnicalRating,
                    ProjectManagement = model.ProfessionalismRating,
                    WellDefinedProjectScope = model.WellDefinedProjectRating
                };
                await _db.FreelancerToFreelancerReview.AddAsync(reviewData);
                _db.SaveChanges();
                return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                {
                    StatusCode = StatusCodes.Status200OK,
                    Message = "Review Submitted Successfully!",
                });

            }

            return StatusCode(StatusCodes.Status200OK, new APIResponseModel
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "Data not Found"
            });
        }

        [HttpPost]
        [Route("GetProjectsExpense")]
        public async Task<IActionResult> GetProjectsExpense([FromBody] MileStoneIdViewModel model)
        {
            if (model != null)
            {
                if (model.UserId != null)
                {
                    try
                    {
                        decimal TotalExpense = 0;
                        var projectDuration = 0;
                        List<solutionFundViewModel> finalFundList = new List<solutionFundViewModel>();
                        List<SolutionsModel> solutionsModel = new List<SolutionsModel>();
                        var projectData = await _db.SolutionFund.Where(x => x.ClientId == model.UserId).ToListAsync();
                        if (projectData.Count > 0)
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
                                    var milestoneData = _db.SolutionMilestone.Where(x => x.SolutionId == data.solutionFunds.SolutionId && x.IndustryId == data.solutionFunds.IndustryId && x.ProjectType == data.solutionFunds.ProjectType).ToList();
                                    if (milestoneData.Count > 0)
                                    {
                                        var totalMilestonDays = milestoneData.Sum(x => x.Days);
                                        projectDuration += totalMilestonDays;
                                    }

                                    var expense = "";
                                    var record = _db.Contract.Where(x => x.SolutionFundId == data.solutionFunds.Id).FirstOrDefault();
                                    if (record != null)
                                    {
                                        expense = record.Amount;
                                        decimal exp = expense != "" ? Convert.ToDecimal(expense) : 0;
                                        TotalExpense += exp;
                                        if (record.IsClientRefund != false)
                                        {
                                            var refuncAmount = record.RefundAmount;
                                            int refund = refuncAmount != "" ? Convert.ToInt32(refuncAmount) : 0;
                                            TotalExpense -= refund;
                                        }
                                    }
                                }
                            }
                        }

                        var CurrencySign = await notificationHelper.ConvertToCurrencySign(model.ClientPreferredCurrency);

                        return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                        {
                            StatusCode = StatusCodes.Status200OK,
                            Message = "success",
                            Result = new
                            {
                                Expense = TotalExpense,
                                Projects = finalFundList.Count,
                                CurrentCurrency = CurrencySign,
                                ProjectDuartion = projectDuration
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

            }

            return StatusCode(StatusCodes.Status200OK, new APIResponseModel
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "Data not Found"
            });
        }

        //getFreelancerProjectExpense
        [HttpPost]
        [Route("getFreelancerProjectExpense")]
        public async Task<IActionResult> getFreelancerProjectExpense([FromBody] MileStoneIdViewModel model)
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
                        var projectData = await _db.SolutionTeam.Where(x => x.FreelancerId == model.UserId).ToListAsync();
                        if (projectData.Count > 0)
                        {
                            foreach (var solution in projectData)
                            {
                                SolutionFund dataRestore = new SolutionFund();
                                var solutionFundData = _db.SolutionFund.Where(x => x.Id == solution.SolutionFundId).FirstOrDefault();
                                if (solutionFundData != null)
                                {
                                    if (!solutionFundData.IsStoppedProject)
                                    {
                                        dataRestore.SolutionId = solutionFundData.SolutionId;
                                        dataRestore.IndustryId = solutionFundData.IndustryId;
                                        dataRestore.ProjectPrice = solutionFundData.ProjectPrice;
                                        dataRestore.Id = solutionFundData.Id;
                                        dataRestore.MileStoneId = solutionFundData.MileStoneId;
                                        dataRestore.FundType = solutionFundData.FundType;
                                        dataRestore.ProjectType = solutionFundData.ProjectType;
                                        solutionList.Add(dataRestore);
                                    }
                                }


                            }

                            if (solutionList.Count > 0)
                            {
                                var grouped_activeSolutions = solutionList
                                   .GroupBy(t => new { t.IndustryId, t.SolutionId, t.ProjectType })
                                   .Select(g => new
                                   {
                                       FundList = g.ToList(),
                                   });

                                foreach (var group in grouped_activeSolutions)
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
                                decimal revenueAmount = 0;
                                var projectDuration = 0;
                                if (finalFundList.Count > 0)
                                {
                                    List<string> industrylist = new List<string>();
                                    foreach (var data in finalFundList)
                                    {
                                        var milestoneData = _db.SolutionMilestone.Where(x => x.SolutionId == data.solutionFunds.SolutionId && x.IndustryId == data.solutionFunds.IndustryId && x.ProjectType == data.solutionFunds.ProjectType).ToList();
                                        if (milestoneData.Count > 0)
                                        {
                                            var totalMilestonDays = milestoneData.Sum(x => x.Days);
                                            projectDuration += totalMilestonDays;
                                        }

                                        var solutionTeamData = _db.SolutionTeam.Where(x => x.SolutionFundId == data.solutionFunds.SolutionId).ToList();
                                        if (solutionTeamData.Count > 0)
                                        {
                                            var freelancerAmount = solutionTeamData.Sum(x => x.Amount);
                                            revenueAmount += freelancerAmount;
                                        }

                                    }
                                }

                                var CurrencySign = await notificationHelper.ConvertToCurrencySign(model.ClientPreferredCurrency);

                                return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                                {
                                    StatusCode = StatusCodes.Status200OK,
                                    Message = "success",
                                    Result = new
                                    {
                                        ProjectDuration = projectDuration,
                                        RevenueAmount = revenueAmount,
                                        FreelancerActiveProject = finalFundList.Count(),
                                        FreelancerCurrency = CurrencySign
                                    }
                                });
                            }
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
                Message = "success",
            });

        }

        //CheckFreelancerToFreelancerReviewExits
        [HttpPost]
        [Route("CheckFreelancerToFreelancerReviewExits")]
        public async Task<IActionResult> CheckFreelancerToFreelancerReviewExits([FromBody] FreelancerToFreelancerReviewModel model)
        {
            if (model != null)
            {
                var checkReviewExits = await _db.FreelancerToFreelancerReview.Where(x => x.ToFreelancerId == model.ToFreelancerId && x.FromFreelancerId == model.FromFreelancerId && x.SolutionId == model.SolutionId && x.IndustryId == model.IndustryId).FirstOrDefaultAsync();
                if (checkReviewExits != null)
                {
                    return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                    {
                        StatusCode = StatusCodes.Status200OK,
                        Message = "Review Exists!",
                        Result = checkReviewExits
                    });
                }
                else
                {
                    return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                    {
                        StatusCode = StatusCodes.Status200OK,
                        Message = "Review Not Exists!",
                    });
                }


            }

            return StatusCode(StatusCodes.Status200OK, new APIResponseModel
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "Data not Found"
            });
        }

        //FreelancerLeaveProject
        [HttpPost]
        [Route("FreelancerLeaveProject")]
        public async Task<IActionResult> FreelancerLeaveProject([FromBody] solutionFundViewModel model)
        {
            if (model != null)
            {
                var solutionTeamData = await _db.SolutionTeam.Where(x => x.SolutionFundId == model.SolutionFundId && x.FreelancerId == model.UserId).FirstOrDefaultAsync();
                if (solutionTeamData != null)
                {
                    _db.SolutionTeam.Remove(solutionTeamData);
                    _db.SaveChanges();

                    var leaveSolution = new SolutionLeave()
                    {
                        SolutionFundId = model.SolutionFundId,
                        FreelancerId = model.UserId,
                        LeaveDateTime = DateTime.Now,
                    };

                    _db.SolutionLeave.Add(leaveSolution);
                    _db.SaveChanges();

                    var solutionFundData = _db.SolutionFund.Where(x => x.Id == model.SolutionFundId).FirstOrDefault();
                    if (solutionFundData != null)
                    {
                        solutionFundData.IsProjectPriceAlreadyCount = false;
                        _db.SaveChanges();
                    }

                    var solutionName = _db.Solutions.Where(x => x.Id == solutionFundData.SolutionId).Select(x => x.Title).FirstOrDefault();
                    var industryName = _db.Industries.Where(x => x.Id == solutionFundData.IndustryId).Select(x => x.IndustryName).FirstOrDefault();
                    var freelancerName = _db.Users.Where(x => x.Id == model.UserId).Select(x => new { x.FirstName, x.LastName }).FirstOrDefault();
                    var clientEmailId = _db.Users.Where(x => x.Id == solutionFundData.ClientId).Select(x => x.UserName).FirstOrDefault();

                    List<Notifications> notificationsList = new List<Notifications>();


                    // send to freelancer
                    Notifications notifications = new Notifications();
                    notifications.NotificationText = "You have successfully left the " + solutionName;
                    notifications.NotificationTime = DateTime.Now;
                    notifications.NotificationTitle = "Freelancer Assignment Update";
                    notifications.ToUserId = model.UserId;
                    notifications.IsRead = false;
                    notificationsList.Add(notifications);

                    // send to client
                    var fullname = freelancerName.FirstName + " " + freelancerName.LastName;
                    Notifications clientnotifications = new Notifications();
                    clientnotifications.NotificationText = "Regrettably, " + fullname + ", the assigned freelancer for “[" + solutionName + " / " + industryName + "],” has left the project. You have the option to continue, and we'll search for a new match.";
                    clientnotifications.NotificationTime = DateTime.Now;
                    clientnotifications.NotificationTitle = "Update on Your Project – Action Required";
                    clientnotifications.ToUserId = solutionFundData.ClientId;
                    clientnotifications.IsRead = false;
                    notificationsList.Add(clientnotifications);

                    // send to admin
                    var adminDetails = _db.Users.Where(x => x.UserType == "Admin").FirstOrDefault();
                    if (adminDetails != null)
                    {
                        Notifications adminnotifications = new Notifications();
                        adminnotifications.NotificationText = fullname + " removed from " + solutionName;
                        adminnotifications.NotificationTime = DateTime.Now;
                        adminnotifications.NotificationTitle = fullname + " removed from a project";
                        adminnotifications.ToUserId = adminDetails.Id;
                        adminnotifications.IsRead = false;
                        notificationsList.Add(adminnotifications);
                    }


                    await notificationHelper.SaveNotificationData(_db, notificationsList);

                    return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                    {
                        StatusCode = StatusCodes.Status200OK,
                        Message = "Project leave Successfully !",
                        Result = new
                        {
                            ClientEmailId = clientEmailId,
                            FreelancerFullName = fullname,
                            SolutionName = solutionName,
                            IndustryName = industryName
                        }
                    });
                }


            }

            return StatusCode(StatusCodes.Status200OK, new APIResponseModel
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "Data not Found"
            });
        }

        //GetClientInvoiceslist
        [HttpGet]
        [Route("GetClientInvoiceslist")]
        public async Task<IActionResult> GetClientInvoiceslist()
        {
            try
            {
                List<SolutionsModel> solutionsModel = new List<SolutionsModel>();
                var projectData = await _db.SolutionFund.Where(x => x.ProjectStatus != "INITIATED").ToListAsync();
                if (projectData.Count > 0)
                {
                    foreach (var data in projectData)
                    {
                        SolutionsModel solutionsdataStore = new SolutionsModel();
                        ApplicationUser client = await _db.Users.Where(x => x.Id == data.ClientId).FirstOrDefaultAsync();
                        solutionsdataStore.ClientName = client.FirstName + " " + client.LastName;
                        solutionsdataStore.ClientId = client.Id;
                        solutionsdataStore.Industries = _db.Industries.Where(x => x.Id == data.IndustryId).Select(x => x.IndustryName).FirstOrDefault();
                        solutionsdataStore.Title = _db.Solutions.Where(x => x.Id == data.SolutionId).Select(x => x.Title).FirstOrDefault();
                        solutionsdataStore.ContractId = _db.Contract.Where(x => x.SolutionFundId == data.Id).Select(x => x.Id).FirstOrDefault();
                        if (data.FundType == SolutionFund.FundTypes.MilestoneFund)
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

            return StatusCode(StatusCodes.Status200OK, new APIResponseModel
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "Data not Found"
            });
        }

        [HttpPost]
        [Route("SaveFreelancerExcludeDate")]
        public async Task<IActionResult> SaveFreelancerExcludeDate([FromBody] ExcludeDateModel model)
        {
            if (model != null)
            {
                var dbmodelList = new List<FreelancerExcludeDate>();
                foreach (var item in model.ExcludeDateList)
                {
                    var dbmodel = new FreelancerExcludeDate()
                    {
                        FreelancerId = model.FreelancerId,
                        ExcludeDate = item
                    };

                    dbmodelList.Add(dbmodel);
                }

                var duplicate = await _db.FreelancerExcludeDate.Where(x => model.ExcludeDateList.Contains(x.ExcludeDate) && x.FreelancerId == model.FreelancerId).FirstOrDefaultAsync();
                if (duplicate != null)
                {
                    return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                    {
                        StatusCode = StatusCodes.Status302Found,
                        Message = "Duplicate Data Found.",
                    });
                }
                else
                {
                    await _db.FreelancerExcludeDate.AddRangeAsync(dbmodelList);
                    await _db.SaveChangesAsync();

                    return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                    {
                        StatusCode = StatusCodes.Status200OK,
                        Message = "Success",
                        Result = dbmodelList
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
        [Route("GetFreelancerExcludeDateData")]
        public async Task<IActionResult> GetFreelancerExcludeDateData([FromBody] ExcludeDateGridModel model)
        {
            var modelList = await _db.FreelancerExcludeDate.Where(x => x.FreelancerId == model.FreelancerId).ToListAsync();

            return StatusCode(StatusCodes.Status200OK, new APIResponseModel
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "Success",
                Result = modelList
            });
        }

        [HttpPost]
        [Route("RemoveFreelancerExcludeDate")]
        public async Task<IActionResult> RemoveFreelancerExcludeDate([FromBody] ExcludeDateGridModel model)
        {
            try
            {
                var dbModel = await _db.FreelancerExcludeDate.Where(x => x.Id == model.Id).FirstOrDefaultAsync();
                if (dbModel != null)
                {
                    _db.FreelancerExcludeDate.Remove(dbModel);
                    await _db.SaveChangesAsync();
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

        //GetTopProfessionalsFeedback
        [HttpPost]
        [Route("GetTopProfessionalsFeedback")]
        public async Task<IActionResult> GetTopProfessionalsFeedback([FromBody] TopProfessionalReviews model)
        {
            if (model != null)
            {
                try
                {
                    var feedbackData = await _db.FreelancerReview.Where(x => x.FreelancerId == model.FreelancerId && x.SolutionId == model.SolutionId && x.IndustryId == model.IndustryId).ToListAsync();
                    List<TopProfessionalReviews> freelancerReviewList = new List<TopProfessionalReviews>();
                    if (feedbackData != null && feedbackData.Count > 0)
                    {
                        foreach (var feedbackdata in feedbackData)
                        {
                            TopProfessionalReviews freelanceReview = new TopProfessionalReviews();
                            var clientname = _db.Users.Where(x => x.Id == feedbackdata.ClientId).Select(x => new { x.FirstName, x.LastName }).FirstOrDefault();
                            if (clientname != null)
                            {
                                freelanceReview.ClientName = clientname.FirstName + " " + clientname.LastName;
                            }

                            var rate = feedbackdata.CommunicationRating + feedbackdata.CollaborationRating + feedbackdata.ProfessionalismRating + feedbackdata.TechnicalRating + feedbackdata.SatisfactionRating + feedbackdata.ResponsivenessRating;
                            double totalRate = (double)rate / 10;


                            freelanceReview.Feedback_Message = feedbackdata.Feedback_Message;
                            freelanceReview.CommunicationRating = feedbackdata.CommunicationRating;
                            freelanceReview.CollaborationRating = feedbackdata.CollaborationRating;
                            freelanceReview.ProfessionalismRating = feedbackdata.ProfessionalismRating;
                            freelanceReview.TechnicalRating = feedbackdata.TechnicalRating;
                            freelanceReview.SatisfactionRating = feedbackdata.SatisfactionRating;
                            freelanceReview.ResponsivenessRating = feedbackdata.ResponsivenessRating;
                            freelanceReview.ReviewDateTime = feedbackdata.CreateDateTime;
                            freelanceReview.Rate = totalRate.ToString();
                            if (freelanceReview.Feedback_Message != null && freelanceReview.Feedback_Message != "")
                            {
                                freelancerReviewList.Add(freelanceReview);
                            }

                        }
                    }
                    return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                    {
                        StatusCode = StatusCodes.Status200OK,
                        Message = "success",
                        Result = freelancerReviewList
                    });
                }
                catch (Exception ex)
                {
                    return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                    {
                        StatusCode = StatusCodes.Status200OK,
                        Message = ex.Message + ex.InnerException,
                    });
                }

            }
            return StatusCode(StatusCodes.Status200OK, new APIResponseModel
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "Data Not Found",
            });
        }


        [HttpPost]
        [Route("GetIndexPageTopProfessionalsFeedback")]
        public async Task<IActionResult> GetIndexPageTopProfessionalsFeedback([FromBody] TopProfessionalReviews model)
        {
            if (model != null)
            {
                try
                {
                    var feedbackData = await _db.FreelancerReview.Where(x => x.FreelancerId == model.FreelancerId).ToListAsync();
                    List<TopProfessionalReviews> freelancerReviewList = new List<TopProfessionalReviews>();
                    if (feedbackData != null && feedbackData.Count > 0)
                    {
                        foreach (var feedbackdata in feedbackData)
                        {
                            TopProfessionalReviews freelanceReview = new TopProfessionalReviews();
                            var clientname = _db.Users.Where(x => x.Id == feedbackdata.ClientId).Select(x => new { x.FirstName, x.LastName }).FirstOrDefault();
                            freelanceReview.ClientName = clientname.FirstName + " " + clientname.LastName;
                            freelanceReview.Feedback_Message = feedbackdata.Feedback_Message;
                            if (freelanceReview.Feedback_Message != null && freelanceReview.Feedback_Message != "")
                            {
                                freelancerReviewList.Add(freelanceReview);
                            }

                        }
                    }
                    return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                    {
                        StatusCode = StatusCodes.Status200OK,
                        Message = "success",
                        Result = freelancerReviewList
                    });
                }
                catch (Exception ex)
                {
                    return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                    {
                        StatusCode = StatusCodes.Status200OK,
                        Message = ex.Message + ex.InnerException,
                    });
                }

            }
            return StatusCode(StatusCodes.Status200OK, new APIResponseModel
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "Data Not Found",
            });
        }

        [HttpPost]
        [Route("FreelancerRequest")]
        public async Task<IActionResult> FreelancerRequest([FromBody] FreelancerRequestModel model)
        {
            try
            {
                //=== Getting old data ===//
                var oldRequestData = await _db.FreelancerFindProcessDetails.ToListAsync();

                //=== Find Current detail data ===//
                var dbModel = oldRequestData.Where(x => x.Id == model.Id).FirstOrDefault();
                if (dbModel != null)
                {
                    //=== find if team completed or not ===//
                    var isTeamCreated = oldRequestData.Where(x => x.FreelancerFindProcessHeaderId == dbModel.FreelancerFindProcessHeaderId && x.ApproveStatus == 0).ToList();

                    if (isTeamCreated.Any())
                    {
                        //=== If tem not completed then update approve status ===//
                        var isActiveProject = oldRequestData.Where(x => x.FreelancerId == dbModel.FreelancerId && x.ApproveStatus == 1).Any();
                        if (!isActiveProject)
                        {
                            dbModel.ApproveStatus = model.RequestStatus;
                            _db.FreelancerFindProcessDetails.Update(dbModel);
                            await _db.SaveChangesAsync();

                            var updatedRequestData = await _db.FreelancerFindProcessDetails.ToListAsync();
                            var dbModelUpdated = updatedRequestData.Where(x => x.Id == model.Id).FirstOrDefault();

                            if (updatedRequestData != null)
                            {
                                var isPendingCompleted = updatedRequestData.Where(x => x.FreelancerFindProcessHeaderId == dbModelUpdated.FreelancerFindProcessHeaderId && x.ApproveStatus == 0).ToList();
                                if (!isPendingCompleted.Any())
                                {
                                    await TeamCompleteCheck(dbModelUpdated, updatedRequestData);
                                }
                            }

                            return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                            {
                                StatusCode = StatusCodes.Status200OK,
                                Message = "Succesfully"
                            });
                        }

                        if (model.RequestStatus == 2)
                        {
                            var reviewData = await _db.AdminToFreelancerReview.Where(x => x.FreelancerId == dbModel.FreelancerId).FirstOrDefaultAsync();
                            reviewData.ProjectAcceptance = reviewData.ProjectAcceptance - 1;

                            _db.AdminToFreelancerReview.Update(reviewData);
                            await _db.SaveChangesAsync();

                            return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                            {
                                StatusCode = StatusCodes.Status200OK,
                                Message = "Succesfully"
                            });
                        }

                        return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                        {
                            StatusCode = StatusCodes.Status200OK,
                            Message = "Alrady working on project."
                        });
                    }
                    else
                    {
                        await TeamCompleteCheck(dbModel, oldRequestData);
                    }
                }

                return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                {
                    StatusCode = StatusCodes.Status200OK,
                    Message = "Expire"
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

            async Task TeamCompleteCheck(FreelancerFindProcessDetails? dbModel, List<FreelancerFindProcessDetails> detailList)
            {
                //=== change tem completed status true. ===//
                var header = await _db.FreelancerFindProcessHeader.Where(x => x.Id == dbModel.FreelancerFindProcessHeaderId).FirstOrDefaultAsync();
                List<SolutionTeamViewModel> teamList = new List<SolutionTeamViewModel>();

                if (header != null)
                {
                    header.IsTeamCompleted = true;
                    _db.FreelancerFindProcessHeader.Update(header);
                    await _db.SaveChangesAsync();

                    if (detailList.Any())
                    {
                        var listOfFreelancer = detailList.Where(x => x.FreelancerFindProcessHeaderId == header.Id && x.ApproveStatus == 1).ToList();
                        foreach (var data in listOfFreelancer)
                        {
                            SolutionTeamViewModel teamData = new SolutionTeamViewModel();
                            teamData.FreelancerId = data.FreelancerId;
                            teamData.SolutionFundId = _db.SolutionFund.Where(x => x.SolutionId == header.SolutionId && x.IndustryId == header.IndustryId && x.ClientId == header.ClientId && x.ProjectType == header.ProjectType).Select(x => x.Id).FirstOrDefault();
                            teamList.Add(teamData);
                        }
                    }
                }

                if (teamList.Count > 0)
                {
                    await SaveSolutionTeamData(teamList);
                }

                List<Notifications> notificationsList = new List<Notifications>();
                var adminDetails = _db.Users.Where(x => x.UserType == "Admin").FirstOrDefault();
                var solutionName = _db.Solutions.Where(x => x.Id == header.SolutionId).Select(x => x.Title).FirstOrDefault();

                if (adminDetails != null)
                {
                    var assembleTeam = new Notifications
                    {
                        NotificationTitle = "Team Assembled",
                        NotificationText = $"Team ready for [{solutionName}]",
                        NotificationTime = DateTime.Now,
                        IsRead = false,
                        ToUserId = adminDetails.Id
                    };
                    notificationsList.Add(assembleTeam);
                    await notificationHelper.SaveNotificationData(_db, notificationsList);
                }
            }
        }

        [HttpGet]
        [Route("RunAlgorithm")]
        public async Task RunAlgorithm()
        {
            var headerData = await _db.FreelancerFindProcessHeader.Where(x => !x.IsTeamCompleted && x.CurrentStatus == 0).ToListAsync();

            if (headerData != null)
            {
                FreelancerFinderHelper helper = new FreelancerFinderHelper();
                foreach (var item in headerData)
                {
                    if (item.ExecuteDate.Hour <= 24)
                    {
                        if (item.ProjectType == "Client")
                        {
                            await helper.FindFreelancersAsync(_db, item.ClientId, item.ProjectType, item.SolutionId, item.IndustryId, item.TotalProjectManager, item.TotalExpert, item.TotalAssociate);
                        }
                        else
                        {
                            await helper.FindFreelancersAsync(_db, item.ClientId, item.ProjectType, item.SolutionId, item.IndustryId, 0, 0, 0);
                        }
                    }
                }
            }
        }

        [HttpPost]
        [Route("CountFinalProjectPricing")]
        public async Task<decimal> CountFinalProjectPricing([FromBody] SolutionFund model)
        {

            try
            {
                var solutionTeamdata = await _db.SolutionTeam.Where(x => x.SolutionFundId == model.Id).ToListAsync();
                var ClientPrefferendCurrency = await _db.Users.Where(x => x.Id == model.ClientId).Select(x => x.PreferredCurrency).FirstOrDefaultAsync();

                decimal finalProjectpricing = 0;
                var HoursPerDay = 8;

                if (solutionTeamdata.Count > 0)
                {
                    foreach (var data in solutionTeamdata)
                    {
                        var freelancerDetails = _db.FreelancerDetails.Where(q => q.UserId == data.FreelancerId).FirstOrDefault();
                        if (freelancerDetails != null)
                        {
                            var freelancerPrefferedCurrency = _db.Users.Where(x => x.Id == freelancerDetails.UserId).Select(x => x.PreferredCurrency).FirstOrDefault();
                            var exchangeRate = _db.ExchangeRates.Where(x => x.FromCurrency == freelancerPrefferedCurrency && x.ToCurrency == ClientPrefferendCurrency).FirstOrDefault();
                            var HourlyRate = Convert.ToDecimal(freelancerDetails.HourlyRate);
                            decimal ExchangeHourlyRate = 0;
                            if (exchangeRate != null)
                            {
                                ExchangeHourlyRate = (decimal)(HourlyRate * exchangeRate.Rate);
                            }
                            else
                            {
                                ExchangeHourlyRate = HourlyRate;
                            }
                            var solutionMilestoneData = _db.SolutionMilestone.Where(x => x.SolutionId == model.SolutionId && x.IndustryId == model.IndustryId && x.ProjectType == model.ProjectType).ToList();
                            if (solutionMilestoneData.Count > 0)
                            {
                                var totalMilestoneDays = solutionMilestoneData.Sum(x => x.Days);
                                finalProjectpricing += HoursPerDay * ExchangeHourlyRate * totalMilestoneDays;
                            }
                        }
                    }
                }

                decimal clientFees = 0;

                if (model.ProjectType == AppConst.ProjectType.SMALL_PROJECT)
                {
                    clientFees = (finalProjectpricing * Convert.ToDecimal(AppConst.Commission.PLATFORM_COMM_FROM_CLIENT_SMALL)) / 100;
                    //projectManagerFees = (finalProjectpricing * Convert.ToDecimal(AppConst.Commission.PROJECT_MANAGER_SMALL)) / 100;
                }
                if (model.ProjectType == AppConst.ProjectType.MEDIUM_PROJECT)
                {
                    clientFees = (finalProjectpricing * Convert.ToDecimal(AppConst.Commission.PLATFORM_COMM_FROM_CLIENT_MEDIUM)) / 100;
                    //projectManagerFees = (finalProjectpricing * Convert.ToDecimal(AppConst.Commission.PROJECT_MANAGER_MEDIUM)) / 100;
                }
                if (model.ProjectType == AppConst.ProjectType.LARGE_PROJECT)
                {
                    clientFees = (finalProjectpricing * Convert.ToDecimal(AppConst.Commission.PLATFORM_COMM_FROM_CLIENT_LARGE)) / 100;
                    //projectManagerFees = (finalProjectpricing * Convert.ToDecimal(AppConst.Commission.PROJECT_MANAGER_LARGE)) / 100;
                }
                if (model.ProjectType == AppConst.ProjectType.CUSTOM_PROJECT)
                {
                    if (solutionTeamdata.Count <= 3)
                    {
                        clientFees = (finalProjectpricing * Convert.ToDecimal(AppConst.Commission.PLATFORM_COMM_FROM_CLIENT_CUSTOM_LESS_THAN_THREE_GIGS)) / 100;
                    }
                    else
                    {
                        clientFees = (finalProjectpricing * Convert.ToDecimal(AppConst.Commission.PLATFORM_COMM_FROM_CLIENT_CUSTOM)) / 100;
                    }
                    //projectManagerFees = (finalProjectpricing * Convert.ToDecimal(AppConst.Commission.PROJECT_MANAGER_LARGE)) / 100;
                }
                // var finalPrice = Convert.ToDecimal(model.ProjectPrice) + projectManagerPlatformFees + clientFees;
                var finalPrice = finalProjectpricing + clientFees;// + projectManagerFees;
                return finalPrice;
            }
            catch (Exception ex)
            {

            }

            return 0;
        }

        //=== This function use when team created then add team in solution team ==//
        [HttpPost]
        [Route("SaveSolutionTeamData")]
        public async Task<string> SaveSolutionTeamData(List<SolutionTeamViewModel> teamList)
        {
            List<SolutionTeam> solutionTeam = new List<SolutionTeam>();
            try
            {
                List<Notifications> notificationsList = new List<Notifications>();
                if (teamList.Count > 0)
                {
                    foreach (var data in teamList)
                    {
                        var solutionFundData = _db.SolutionFund.Where(x => x.Id == data.SolutionFundId).FirstOrDefault();

                        var totalMileStoneData = await _db.SolutionMilestone.Where(x => x.SolutionId == solutionFundData.SolutionId && x.IndustryId == solutionFundData.IndustryId && x.ProjectType == solutionFundData.ProjectType).ToListAsync();
                        var totalMilestoneDays = totalMileStoneData.Sum(x => x.Days);

                        var freelancerPreferedCurrency = "";
                        var freelancerData = _db.Users.Where(x => x.Id == data.FreelancerId).FirstOrDefault();
                        var freelancerDetailsData = _db.FreelancerDetails.Where(x => x.UserId == data.FreelancerId).FirstOrDefault();

                        if (string.IsNullOrEmpty(freelancerData.PreferredCurrency))
                        {
                            freelancerPreferedCurrency = "EUR";
                        }

                        var clientPreferedCurrency = _db.Users.Where(x => x.Id == data.ClientId).Select(x => x.PreferredCurrency).FirstOrDefault();

                        var exchangeRate = _db.ExchangeRates.Where(x => x.FromCurrency == freelancerPreferedCurrency
                        && x.ToCurrency == clientPreferedCurrency).FirstOrDefault();
                        var HourlyRate = Convert.ToDecimal(freelancerDetailsData.HourlyRate);
                        decimal ExchangeHourlyRate = HourlyRate;
                        if (exchangeRate != null)
                        {
                            ExchangeHourlyRate = Convert.ToDecimal((decimal)(HourlyRate * exchangeRate.Rate));
                        }

                        //var projectManager = false;
                        decimal contractAmount = contractAmount = (totalMilestoneDays * 8 * ExchangeHourlyRate);
                        decimal Platformfees = 0;
                        if (solutionFundData.ProjectType == AppConst.ProjectType.SMALL_PROJECT)
                        {
                            Platformfees = (contractAmount * AppConst.Commission.PLATFORM_COMM_FROM_FREELANCER_SMALL) / 100;
                        }
                        if (solutionFundData.ProjectType == AppConst.ProjectType.MEDIUM_PROJECT)
                        {
                            Platformfees = (contractAmount * AppConst.Commission.PLATFORM_COMM_FROM_FREELANCER_MEDIUM) / 100;
                        }
                        if (solutionFundData.ProjectType == AppConst.ProjectType.LARGE_PROJECT)
                        {
                            Platformfees = (contractAmount * AppConst.Commission.PLATFORM_COMM_FROM_FREELANCER_LARGE) / 100;
                        }
                        if (solutionFundData.ProjectType == AppConst.ProjectType.CUSTOM_PROJECT)
                        {
                            Platformfees = (contractAmount * AppConst.Commission.PLATFORM_COMM_FROM_FREELANCER_CUSTOM) / 100;
                        }

                        solutionTeam.Add(new SolutionTeam()
                        {
                            FreelancerId = data.FreelancerId,
                            SolutionFundId = solutionFundData.Id,
                            IsProjectManager = false,
                            Amount = contractAmount,
                            PlatformFees = Platformfees
                        });

                        //var freelancerName = _db.Users.Where(x => x.Id == data.UserId).FirstOrDefault();
                        var solutionName = "";
                        if (solutionFundData != null)
                        {
                            solutionName = _db.Solutions.Where(x => x.Id == solutionFundData.SolutionId).Select(x => x.Title).FirstOrDefault();
                        }

                        if (freelancerData.FirstName != null)
                        {
                            var adminDetails = _db.Users.Where(x => x.UserType == "Admin").FirstOrDefault();

                            Notifications notifications = new Notifications();
                            notifications.NotificationTitle = "Freelancer Selection:";
                            notifications.NotificationText = "\"" + freelancerData.FirstName + " selected for '" + solutionName + "'.\"";
                            notifications.NotificationTime = DateTime.Now;
                            notifications.IsRead = false;
                            notifications.ToUserId = adminDetails.Id;
                            notificationsList.Add(notifications);
                        }
                    }
                    _db.SolutionTeam.AddRange(solutionTeam);
                    _db.SaveChanges();

                    await notificationHelper.SaveNotificationData(_db, notificationsList);
                }

                return "success";
            }
            catch (Exception ex)
            {
                return ex.InnerException + ex.Message;
            }
        }
    }
}
