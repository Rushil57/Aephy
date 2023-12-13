using Aephy.API.AlgorithumHelper;
using Aephy.API.DBHelper;
using Aephy.API.Models;
using Aephy.API.Revoult;
using Aephy.API.Stripe;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using RevolutAPI.Models.BusinessApi.Payment;
//using Stripe;
//using Stripe.Checkout;
//using Stripe.Identity;
using System.Collections.Generic;
using System.Net;
using System.Reflection.Metadata;
using System.Xml.Schema;
using static Aephy.API.DBHelper.ApplicationUser;
using static Aephy.API.Models.AdminViewModel;
using static Aephy.API.Models.AppConst;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Aephy.API.Controllers
{
    [Route("api/Client/")]
    [ApiController]
    public class ClientController : ControllerBase
    {
        private readonly AephyAppDbContext _db;
        // private readonly IStripeAccountService _stripeAccountService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;
        private readonly IRevoultService _revoultService;
        public ClientController(AephyAppDbContext dbContext, UserManager<ApplicationUser> userManager, IConfiguration configuration, IRevoultService revoultService)
        {
            _db = dbContext;
            _configuration = configuration;
            // _stripeAccountService = stripeAccountService;
            _userManager = userManager;
            _revoultService = revoultService;
        }

        [HttpPost]
        [Route("GetPopularSolutionBasedOnServices")]
        public async Task<IActionResult> GetPopularSolutionBasedOnServices([FromBody] MileStoneIdViewModel model)
        {
            if (model.Id != 0)
            {
                try
                {
                    var CheckType = _db.Users.Where(x => x.Id == model.UserId).Select(x => x.UserType).FirstOrDefault();
                    List<SolutionsModel> solutionsModel = new List<SolutionsModel>();
                    List<Industries> industrylistDetails = new List<Industries>();
                    List<string> industrylist = new List<string>();
                    List<int> industryList = new List<int>();
                    bool IsSavedProject = false;
                    var serviceData = await _db.SolutionServices.Where(x => x.ServicesId == model.Id).Select(x => x.SolutionId).ToListAsync();

                    if (serviceData.Count > 0)
                    {
                        foreach (var data in serviceData)
                        {
                            var solutiondata = _db.Solutions.Where(x => x.Id == data).FirstOrDefault();
                            if (model.UserId != "")
                            {
                                var SavedProjectData = _db.SavedProjects.Where(x => x.SolutionId == solutiondata.Id && x.UserId == model.UserId).FirstOrDefault();
                                if (SavedProjectData != null)
                                {
                                    IsSavedProject = true;
                                }
                                else
                                {
                                    IsSavedProject = false;
                                }
                            }
                            if (model.UserId != "" && CheckType != "Client")
                            {
                                industryList = _db.SolutionIndustryDetails.Where(x => x.SolutionId == data && x.IsActiveForFreelancer == true).Select(x => x.IndustryId).ToList();
                            }
                            else
                            {
                                industryList = _db.SolutionIndustryDetails.Where(x => x.SolutionId == data && x.IsActiveForClient == true).Select(x => x.IndustryId).ToList();
                            }
                            //var industryList = _db.SolutionIndustryDetails.Where(x => x.SolutionId == data && x.IsActiveForClient == true).Select(x => x.IndustryId).ToList();
                            if (industryList.Count > 0)
                            {
                                foreach (var industryId in industryList)
                                {
                                    var industry = _db.Industries.Where(x => x.Id == industryId).FirstOrDefault();
                                    industrylistDetails.Add(industry);
                                    var industryname = _db.Industries.Where(x => x.Id == industryId).Select(x => x.IndustryName).FirstOrDefault();
                                    industrylist.Add(industryname);
                                }
                                SolutionsModel dataStore = new SolutionsModel();
                                dataStore.IsProjectSaved = IsSavedProject;
                                dataStore.solutionServices = model.Id;
                                dataStore.Industries = string.Join(",", industrylist);
                                //dataStore.solutionIndustriesList = industrylistDetails.Distinct().ToList();
                                dataStore.Id = solutiondata.Id;
                                dataStore.Description = solutiondata.Description;
                                dataStore.ImagePath = solutiondata.ImagePath;
                                dataStore.ImageUrlWithSas = solutiondata.ImageUrlWithSas;
                                dataStore.Title = solutiondata.Title;
                                solutionsModel.Add(dataStore);
                                industrylist.Clear();
                            }

                        }
                    }
                    float totalSolutions = solutionsModel.Count();
                    double pagesCount = 0;
                    if (totalSolutions > 0)
                    {
                        double val = Convert.ToDouble((float)totalSolutions / 6);
                        pagesCount = Math.Ceiling(val);

                    }
                    List<SolutionsModel> mainlist = solutionsModel.Take(6).ToList();
                    if (model.pageNumber > 1 && model.pageNumber != null)
                    {
                        var prevPage = (int)model.pageNumber - 1;
                        var current = (int)model.pageNumber;
                        int start = (prevPage * 6) - 1;
                        int end = (current * 6) - 1;
                        int indexOfLastElement = (solutionsModel.Count) - 1;
                        mainlist = solutionsModel.Where((value, index) => index > start && index <= end).ToList();
                    }
                    return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                    {
                        StatusCode = StatusCodes.Status200OK,
                        Message = "Success",
                        Result = new
                        {
                            SolutionData = solutionsModel,
                            IndustriesData = industrylistDetails.Distinct(),
                            PageCount = pagesCount
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
            else
            {
                try
                {
                    var CheckType = _db.Users.Where(x => x.Id == model.UserId).Select(x => x.UserType).FirstOrDefault();
                    List<Solutions> solutionList = _db.Solutions.ToList();
                    List<SolutionsModel> solutionsModel = new List<SolutionsModel>();
                    List<Industries> industrylistDetails = new List<Industries>();
                    List<string> industrylist = new List<string>();
                    List<int> industryIdlist = new List<int>();
                    bool IsSavedProject = false;

                    if (solutionList.Count > 0)
                    {
                        foreach (var list in solutionList)
                        {
                            var serviceId = _db.SolutionServices.Where(x => x.SolutionId == list.Id).Select(x => x.ServicesId).FirstOrDefault();
                            var Servicename = _db.Services.Where(x => x.Id == serviceId).Select(x => x.ServicesName).FirstOrDefault();
                            if (model.UserId != "")
                            {
                                var SavedProjectData = _db.SavedProjects.Where(x => x.SolutionId == list.Id && x.UserId == model.UserId).FirstOrDefault();
                                if (SavedProjectData != null)
                                {
                                    IsSavedProject = true;
                                }
                                else
                                {
                                    IsSavedProject = false;
                                }
                            }
                            if (model.UserId != "" && CheckType != "Client")
                            {
                                industryIdlist = _db.SolutionIndustryDetails.Where(x => x.SolutionId == list.Id && x.IsActiveForFreelancer == true).Select(x => x.IndustryId).ToList();
                            }
                            else
                            {
                                industryIdlist = _db.SolutionIndustryDetails.Where(x => x.SolutionId == list.Id && x.IsActiveForClient == true).Select(x => x.IndustryId).ToList();
                            }
                            //var industryIdlist = _db.SolutionIndustryDetails.Where(x => x.SolutionId == list.Id && x.IsActiveForClient == true).Select(x => x.IndustryId).ToList();
                            if (industryIdlist.Count > 0)
                            {
                                foreach (var industryId in industryIdlist)
                                {
                                    var industry = _db.Industries.Where(x => x.Id == industryId).FirstOrDefault();
                                    industrylistDetails.Add(industry);
                                    var industryname = _db.Industries.Where(x => x.Id == industryId).Select(x => x.IndustryName).FirstOrDefault();
                                    industrylist.Add(industryname);
                                }
                                SolutionsModel dataStore = new SolutionsModel();
                                dataStore.IsProjectSaved = IsSavedProject;
                                dataStore.Services = Servicename;
                                dataStore.solutionServices = serviceId;
                                dataStore.Industries = string.Join(",", industrylist);
                                dataStore.Id = list.Id;
                                dataStore.Description = list.Description;
                                dataStore.ImagePath = list.ImagePath;
                                dataStore.ImageUrlWithSas = list.ImageUrlWithSas;
                                dataStore.Title = list.Title;
                                dataStore.SubTitle = list.SubTitle;
                                solutionsModel.Add(dataStore);
                                industrylist.Clear();
                            }

                        }
                    }
                    int totalSolutions = solutionsModel.Count();
                    decimal pagesCount = 0;
                    if (totalSolutions > 0)
                    {
                        decimal val = Convert.ToDecimal(totalSolutions / 6);
                        pagesCount = Math.Ceiling(val);
                    }
                    return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                    {
                        StatusCode = StatusCodes.Status200OK,
                        Message = "Success",
                        Result = new
                        {
                            SolutionData = solutionsModel,
                            IndustriesData = industrylistDetails.Distinct(),
                            PageCount = pagesCount
                        }
                    });

                }
                catch (Exception ex)
                {
                    return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                    {
                        StatusCode = StatusCodes.Status403Forbidden,
                        Message = ex.Message + ex.InnerException

                    });
                }
            }
        }


        [HttpPost]
        [Route("GetPopularSolutionBasedOnSolutionSelected")]
        public async Task<IActionResult> GetPopularSolutionBasedOnSolutionSelected([FromBody] MileStoneIdViewModel model)
        {
            if (model.Id != 0)
            {
                try
                {
                    var CheckType = _db.Users.Where(x => x.Id == model.UserId).Select(x => x.UserType).FirstOrDefault();
                    List<SolutionsModel> solutionsModel = new List<SolutionsModel>();
                    List<Industries> industrylistDetails = new List<Industries>();
                    List<string> industrylist = new List<string>();
                    List<int> industryList = new List<int>();
                    bool IsSavedProject = false;

                    var solutiondata = _db.Solutions.Where(x => x.Id == model.Id).FirstOrDefault();
                    var servicesData = _db.SolutionServices.Where(x => x.SolutionId == model.Id).Select(x => x.ServicesId).FirstOrDefault();

                    if (model.UserId != "")
                    {
                        var SavedProjectData = _db.SavedProjects.Where(x => x.SolutionId == model.Id && x.UserId == model.UserId).FirstOrDefault();
                        if (SavedProjectData != null)
                        {
                            IsSavedProject = true;
                        }
                        else
                        {
                            IsSavedProject = false;
                        }
                    }

                    if (model.UserId != "" && CheckType != "Client")
                    {
                        industryList = await _db.SolutionIndustryDetails.Where(x => x.SolutionId == solutiondata.Id && x.IsActiveForFreelancer == true).Select(x => x.IndustryId).ToListAsync();
                    }
                    else
                    {
                        industryList = _db.SolutionIndustryDetails.Where(x => x.SolutionId == solutiondata.Id && x.IsActiveForClient == true).Select(x => x.IndustryId).ToList();
                    }
                    // var industryList = _db.SolutionIndustryDetails.Where(x => x.SolutionId == solutiondata.Id && x.IsActiveForClient == true).Select(x => x.IndustryId).ToList();
                    if (industryList.Count > 0)
                    {
                        foreach (var industryId in industryList)
                        {
                            var industry = _db.Industries.Where(x => x.Id == industryId).FirstOrDefault();
                            industrylistDetails.Add(industry);
                            var industryname = _db.Industries.Where(x => x.Id == industryId).Select(x => x.IndustryName).FirstOrDefault();
                            industrylist.Add(industryname);
                        }
                        SolutionsModel dataStore = new SolutionsModel();
                        dataStore.IsProjectSaved = IsSavedProject;
                        dataStore.Industries = string.Join(",", industrylist);
                        dataStore.solutionServices = servicesData;
                        // dataStore.solutionIndustriesList = industrylistDetails;
                        dataStore.Id = solutiondata.Id;
                        dataStore.Description = solutiondata.Description;
                        dataStore.ImagePath = solutiondata.ImagePath;
                        dataStore.ImageUrlWithSas = solutiondata.ImageUrlWithSas;
                        dataStore.Title = solutiondata.Title;
                        solutionsModel.Add(dataStore);

                    }
                    float totalSolutions = solutionsModel.Count();
                    double pagesCount = 0;
                    if (totalSolutions > 0)
                    {
                        double val = Convert.ToDouble((float)totalSolutions / 6);
                        pagesCount = Math.Ceiling(val);

                    }
                    List<SolutionsModel> mainlist = solutionsModel.Take(6).ToList();
                    if (model.pageNumber > 1 && model.pageNumber != null)
                    {
                        var prevPage = (int)model.pageNumber - 1;
                        var current = (int)model.pageNumber;
                        int start = (prevPage * 6) - 1;
                        int end = (current * 6) - 1;
                        int indexOfLastElement = (solutionsModel.Count) - 1;
                        mainlist = solutionsModel.Where((value, index) => index > start && index <= end).ToList();
                    }

                    return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                    {
                        StatusCode = StatusCodes.Status200OK,
                        Message = "Success",
                        Result = new
                        {
                            SolutionData = mainlist,
                            IndustriesData = industrylistDetails,
                            PageCount = pagesCount
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
                StatusCode = StatusCodes.Status403Forbidden,
                Message = "Something Wennt Wrong"

            });

        }

        [HttpPost]
        [Route("GetPopularSolutionList")]
        public async Task<IActionResult> GetPopularSolutionList(GetUserProfileRequestModel model)
        {
            try
            {
                var CheckType = _db.Users.Where(x => x.Id == model.UserId).Select(x => x.UserType).FirstOrDefault();
                List<Solutions> solutionList = _db.Solutions.ToList();
                List<SolutionsModel> solutionsModel = new List<SolutionsModel>();
                List<string> industrylist = new List<string>();
                List<int> industryIdlist = new List<int>();
                bool IsSavedProject = false;
                if (solutionList.Count > 0)
                {
                    foreach (var list in solutionList)
                    {
                        var serviceId = _db.SolutionServices.Where(x => x.SolutionId == list.Id).Select(x => x.ServicesId).FirstOrDefault();
                        var Servicename = _db.Services.Where(x => x.Id == serviceId).Select(x => x.ServicesName).FirstOrDefault();
                        if (model.UserId != "")
                        {
                            var SavedProjectData = _db.SavedProjects.Where(x => x.SolutionId == list.Id && x.UserId == model.UserId).FirstOrDefault();
                            if (SavedProjectData != null)
                            {
                                IsSavedProject = true;
                            }
                            else
                            {
                                IsSavedProject = false;
                            }
                        }
                        if (model.UserId != "" && CheckType != "Client")
                        {
                            industryIdlist = await _db.SolutionIndustryDetails.Where(x => x.SolutionId == list.Id && x.IsActiveForFreelancer == true).Select(x => x.IndustryId).ToListAsync();
                        }
                        else
                        {
                            industryIdlist = await _db.SolutionIndustryDetails.Where(x => x.SolutionId == list.Id && x.IsActiveForClient == true).Select(x => x.IndustryId).ToListAsync();
                        }

                        if (industryIdlist.Count > 0)
                        {
                            foreach (var industryId in industryIdlist)
                            {
                                var industryname = _db.Industries.Where(x => x.Id == industryId).Select(x => x.IndustryName).FirstOrDefault();
                                industrylist.Add(industryname);
                            }
                            SolutionsModel dataStore = new SolutionsModel();
                            dataStore.IsProjectSaved = IsSavedProject;
                            dataStore.Services = Servicename;
                            dataStore.solutionServices = serviceId;
                            dataStore.Industries = string.Join(",", industrylist);
                            dataStore.Id = list.Id;
                            dataStore.Description = list.Description;
                            dataStore.ImagePath = list.ImagePath;
                            dataStore.ImageUrlWithSas = list.ImageUrlWithSas;
                            dataStore.Title = list.Title;
                            dataStore.SubTitle = list.SubTitle;
                            solutionsModel.Add(dataStore);
                            industrylist.Clear();
                        }

                    }
                }
                return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                {
                    StatusCode = StatusCodes.Status200OK,
                    Message = "Success",
                    Result = solutionsModel
                });

            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                {
                    StatusCode = StatusCodes.Status403Forbidden,
                    Message = ex.Message + ex.InnerException

                });
            }
        }


        [HttpPost]
        [Route("GetSolutionDetailsInProject")]
        public async Task<IActionResult> GetSolutionDetailsInProject([FromBody] MileStoneDetailsViewModel model)
        {
            if (model != null)
            {
                try
                {
                    SolutionFund fundProgress = new SolutionFund();
                    if (model.UserId != "")
                    {
                        fundProgress = await _db.SolutionFund.Where(x => x.SolutionId == model.SolutionId && x.IndustryId == model.IndustryId && x.ClientId == model.UserId && x.ProjectType == model.ProjectType).FirstOrDefaultAsync();
                    }
                    if (fundProgress != null && fundProgress.ProjectType != null)
                    {
                        model.ProjectType = fundProgress.ProjectType;
                    }

                    var data = _db.SolutionIndustryDetails.Where(x => x.IndustryId == model.IndustryId && x.SolutionId == model.SolutionId).FirstOrDefault();
                    var solutionDefine = _db.SolutionDefine.Where(x => x.SolutionIndustryDetailsId == data.Id && x.ProjectType == model.ProjectType).FirstOrDefault();
                    var milestoneData = await _db.SolutionMilestone.Where(x => x.IndustryId == model.IndustryId && x.SolutionId == model.SolutionId && x.ProjectType == model.ProjectType).ToListAsync();
                    var pointsData = await _db.SolutionPoints.Where(x => x.SolutionId == model.SolutionId && x.IndustryId == model.IndustryId && x.ProjectType == model.ProjectType).ToListAsync();
                    var topProfessionalData = await _db.SolutionTopProfessionals.Where(x => x.SolutionId == model.SolutionId && x.IndustryId == model.IndustryId).ToListAsync();
                    List<SolutionTopProfessionalModel> professionalData = new List<SolutionTopProfessionalModel>();
                    if (topProfessionalData.Count > 0)
                    {
                        foreach (var topdata in topProfessionalData)
                        {
                            SolutionTopProfessionalModel solutionTop = new SolutionTopProfessionalModel();
                            solutionTop.ID = topdata.FreelancerId;
                            solutionTop.Description = topdata.Description;
                            solutionTop.Title = topdata.TopProfessionalTitle;
                            var fullname = _db.Users.Where(x => x.Id == topdata.FreelancerId).Select(x => new { x.FirstName, x.LastName }).FirstOrDefault();

                            solutionTop.FreelancerId = fullname.FirstName + " " + fullname.LastName;
                            solutionTop.ImagePath = _db.FreelancerDetails.Where(x => x.UserId == topdata.FreelancerId).Select(x => x.ImagePath).FirstOrDefault();
                            solutionTop.Rate = topdata.Rate;
                            professionalData.Add(solutionTop);
                        }
                    }

                    var successfullprojectData = await _db.SolutionSuccessfullProject.Where(x => x.IndustryId == model.IndustryId && x.SolutionId == model.SolutionId).Where(x => x.IsActive == true).ToListAsync();
                    List<SuccessfullProjectModel> successfullProjectList = new List<SuccessfullProjectModel>();
                    if (successfullprojectData.Count > 0)
                    {
                        foreach (var projectData in successfullprojectData)
                        {
                            var resultData = _db.SolutionSuccessfullProjectResult.Where(x => x.SolutionSuccessfullProjectId == projectData.Id).ToList();
                            SuccessfullProjectModel succesfulldata = new SuccessfullProjectModel();
                            if (resultData.Count > 0)
                            {
                                succesfulldata.projectResultList = resultData;
                            }
                            succesfulldata.Title = projectData.Title;
                            succesfulldata.Description = projectData.Description;
                            successfullProjectList.Add(succesfulldata);

                        }
                    }

                    List<SolutionMilestone> mileStoneToTalDays = new List<SolutionMilestone>();
                    var freelancerList = _db.FreelancerDetails.Where(x => x.HourlyRate != null && x.HourlyRate != "").ToList();
                    var MilestoneTotalDaysByProjectType = _db.SolutionMilestone.Where(x => x.SolutionId == model.SolutionId && x.IndustryId == model.IndustryId).ToList();
                    if (MilestoneTotalDaysByProjectType.Count > 0)
                    {
                        mileStoneToTalDays = MilestoneTotalDaysByProjectType
                       .GroupBy(l => l.ProjectType)
                       .Select(cl => new SolutionMilestone
                       {
                           ProjectType = cl.First().ProjectType,
                           Days = cl.Sum(c => c.Days),
                       }).ToList();
                    }

                    var solutionFeedback = _db.ProjectReview.Where(x => x.SolutionId == model.SolutionId && x.IndustryId == model.IndustryId).ToList();
                    List<ProjectReview> projectReviewList = new List<ProjectReview>();
                    if (solutionFeedback.Count > 0)
                    {
                        foreach (var feedbackdata in solutionFeedback)
                        {
                            ProjectReview projectReview = new ProjectReview();
                            var clientname = _db.Users.Where(x => x.Id == feedbackdata.ClientId).Select(x => new { x.FirstName, x.LastName }).FirstOrDefault();
                            projectReview.ClientId = clientname.FirstName + " " + clientname.LastName;
                            projectReview.AdherenceToBudget = feedbackdata.AdherenceToBudget;
                            projectReview.WellDefinedProjectScope = feedbackdata.WellDefinedProjectScope;
                            projectReview.AdherenceToProjectScope = feedbackdata.AdherenceToProjectScope;
                            projectReview.DeliverablesQuality = feedbackdata.DeliverablesQuality;
                            projectReview.MeetingTimeliness = feedbackdata.MeetingTimeliness;
                            projectReview.Clientsatisfaction = feedbackdata.Clientsatisfaction;
                            projectReview.Feedback_Message = feedbackdata.Feedback_Message;
                            projectReview.ID = feedbackdata.ID;
                            projectReviewList.Add(projectReview);


                        }


                    }

                    var currency = await ConvertToCurrencySign(model.ClientPreferredCurrency);

                    var solutionindustryData = _db.SolutionIndustryDetails.Where(x => x.SolutionId == model.SolutionId && x.IndustryId == model.IndustryId).FirstOrDefault();
                    List<CustomProjectDetials>? singleFreelancerCustomProject = null;
                    if (solutionindustryData != null)
                    {
                        var solutionDefineData = _db.SolutionDefine.Where(x => x.SolutionIndustryDetailsId == solutionindustryData.Id && x.ProjectType == "custom").FirstOrDefault();
                        if (solutionDefineData != null)
                        {
                            singleFreelancerCustomProject = _db.CustomProjectDetials.Where(x => x.SolutionDefineId == solutionDefineData.Id && x.IsSingleFreelancer).ToList();
                        }
                    }

                    return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                    {
                        StatusCode = StatusCodes.Status200OK,
                        Message = "Success",
                        Result = new
                        {
                            SolutionDefine = solutionDefine,
                            MileStone = milestoneData,
                            PointsData = pointsData,
                            TopProfessional = professionalData,
                            SuccessfullProjects = successfullProjectList,
                            SolutionFund = fundProgress,
                            FreelancerHourlyList = freelancerList,
                            MileStoneToTalDays = mileStoneToTalDays,
                            SolutionFeedback = projectReviewList,
                            PreferredCurrency = currency,
                            SingleFreelancerCustomData = singleFreelancerCustomProject
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
                StatusCode = StatusCodes.Status403Forbidden,
                Message = "Something Wennt Wrong"

            });

        }


        [HttpPost]
        [Route("GetActiveSolutionDetailsInProject")]
        public async Task<IActionResult> GetActiveSolutionDetailsInProject([FromBody] MileStoneDetailsViewModel model)
        {
            if (model != null)
            {
                try
                {

                    SolutionFund fundProgress = new SolutionFund();
                    SolutionMilestone solutionMilesData = new SolutionMilestone();
                    bool fundCompleted = false;
                    bool fundStopByClient = false;
                    decimal projectFinalPrice = 0;

                    if (model.UserId != "")
                    {
                        fundProgress = await _db.SolutionFund.Where(x => x.Id == model.SolutionFundId).FirstOrDefaultAsync();
                        projectFinalPrice = Convert.ToDecimal(fundProgress.ProjectPrice);
                        var checkfundCompleted = await _db.Contract.Where(x => x.SolutionFundId == model.SolutionFundId).Select(x => x.PaymentStatus).FirstOrDefaultAsync();
                        if (checkfundCompleted == Contract.PaymentStatuses.Splitted)
                        {
                            fundCompleted = true;
                        }
                        if (fundProgress.IsStoppedProject)
                        {
                            fundStopByClient = true;
                        }
                    }
                    if (fundProgress != null && fundProgress.ProjectType != null)
                    {
                        model.ProjectType = fundProgress.ProjectType;
                        solutionMilesData = _db.SolutionMilestone.Where(x => x.Id == fundProgress.MileStoneId).FirstOrDefault();
                    }

                    var CheckInCompeleteFund = _db.SolutionFund.Where(x => x.ClientId == model.UserId && x.IndustryId == model.IndustryId && x.SolutionId == model.SolutionId && x.Id == model.SolutionFundId && x.ProjectStatus == "INPROGRESS").FirstOrDefault();
                    if (CheckInCompeleteFund != null)
                    {
                        //var solutionFundData = _db.SolutionFund.Where(x => x.Id == CheckInCompeleteFund.SolutionFundId).FirstOrDefault();
                        //if (solutionFundData != null)
                        //{
                        CheckInCompeleteFund.ProjectStatus = "INITIATED";
                        var milestoneFundCompleted = _db.SolutionFund.Where(x => x.SolutionId == model.SolutionId && x.IndustryId == model.IndustryId && x.ProjectType == model.ProjectType && x.ClientId == model.UserId && x.IsCheckOutDone == true).Count();
                        if (milestoneFundCompleted == 0)
                        {
                            CheckInCompeleteFund.FundType = SolutionFund.FundTypes.ProjectFund;
                        }

                        _db.SaveChanges();
                        //}
                    }
                    if (fundProgress.ProjectStatus == "INITIATED")
                    {
                        if (solutionMilesData != null && fundProgress.FundType == SolutionFund.FundTypes.MilestoneFund)
                        {
                            var MilestoneTotalDaysByProjectType = _db.SolutionMilestone.Where(x => x.SolutionId == solutionMilesData.SolutionId && x.IndustryId == solutionMilesData.IndustryId && x.ProjectType == solutionMilesData.ProjectType).ToList();
                            decimal calculateProjectPrice = 0;
                            if (MilestoneTotalDaysByProjectType.Count > 0)
                            {
                                SolutionMilestone mileStoneToTalDays = MilestoneTotalDaysByProjectType
                               .GroupBy(l => l.ProjectType)
                               .Select(cl => new SolutionMilestone
                               {
                                   ProjectType = cl.First().ProjectType,
                                   Days = cl.Sum(c => c.Days),
                               }).FirstOrDefault();

                                if (mileStoneToTalDays.Days > 0)
                                {
                                    calculateProjectPrice = (Convert.ToDecimal(fundProgress.ProjectPrice) / mileStoneToTalDays.Days) * solutionMilesData.Days;
                                    fundProgress.ProjectPrice = calculateProjectPrice.ToString();
                                }
                            }
                        }
                        else
                        {
                            if (!fundProgress.IsProjectPriceAlreadyCount)
                            {
                                var finalPrice = await CountFinalProjectPricing(fundProgress);
                                projectFinalPrice = finalPrice;
                                fundProgress.ProjectPrice = finalPrice.ToString();
                                fundProgress.IsProjectPriceAlreadyCount = true;
                                _db.SaveChanges();
                            }
                        }
                    }
                    else
                    {
                        var contractAmount = _db.Contract.Where(x => x.SolutionFundId == model.SolutionFundId).Select(x => x.Amount).FirstOrDefault();
                        fundProgress.ProjectPrice = contractAmount;
                    }


                    var data = _db.SolutionIndustryDetails.Where(x => x.IndustryId == model.IndustryId && x.SolutionId == model.SolutionId).FirstOrDefault();
                    var solutionDefine = _db.SolutionDefine.Where(x => x.SolutionIndustryDetailsId == data.Id && x.ProjectType == model.ProjectType).FirstOrDefault();
                    var milestoneData = await _db.SolutionMilestone.Where(x => x.IndustryId == model.IndustryId && x.SolutionId == model.SolutionId && x.ProjectType == model.ProjectType).ToListAsync();
                    List<MileStoneModel> milestoneList = new List<MileStoneModel>();
                    if (milestoneData.Count > 0)
                    {
                        // SolutionMilestone mileStoneToTalDays = milestoneData
                        //.GroupBy(l => l.ProjectType)
                        //.Select(cl => new SolutionMilestone
                        //{
                        //    ProjectType = cl.First().ProjectType,
                        //    Days = cl.Sum(c => c.Days),
                        //}).FirstOrDefault();
                        var mileStoneToTalDays = milestoneData.Sum(x => x.Days);

                        foreach (var stonedata in milestoneData)
                        {
                            MileStoneModel milestonData = new MileStoneModel();
                            milestonData.Id = stonedata.Id;
                            milestonData.Days = stonedata.Days;
                            milestonData.Title = stonedata.Title;
                            milestonData.Description = stonedata.Description;
                            milestonData.MilestoneStatus = _db.ActiveSolutionMilestoneStatus.Where(x => x.MilestoneId == stonedata.Id && x.UserId == model.UserId).Select(x => x.MilestoneStatus).FirstOrDefault();
                            if (mileStoneToTalDays > 0)
                            {
                                milestonData.MilestonePrice = (projectFinalPrice / mileStoneToTalDays) * stonedata.Days;
                            }
                            milestoneList.Add(milestonData);
                        }
                    }
                    var pointsData = await _db.SolutionPoints.Where(x => x.SolutionId == model.SolutionId && x.IndustryId == model.IndustryId && x.ProjectType == model.ProjectType).ToListAsync();
                    var solutionTeamData = await _db.SolutionTeam.Where(x => x.SolutionFundId == model.SolutionFundId).ToListAsync();
                    List<SolutionTeamViewModel> solutionteamList = new List<SolutionTeamViewModel>();
                    if (solutionTeamData.Count > 0)
                    {
                        foreach (var soltiondata in solutionTeamData)
                        {
                            SolutionTeamViewModel solutionTeam = new SolutionTeamViewModel();
                            //if (soltiondata.IsProjectManager)
                            //{
                            //    solutionTeam.ProjectManagerPlatformFees = soltiondata.PlatformFees;
                            //}
                            var solutionFunddata = _db.SolutionFund.Where(x => x.Id == soltiondata.SolutionFundId).FirstOrDefault();
                            solutionTeam.SolutionId = solutionFunddata.SolutionId;
                            solutionTeam.IndustryId = solutionFunddata.IndustryId;
                            solutionTeam.ClientId = solutionFunddata.ClientId;
                            var fullname = _db.Users.Where(x => x.Id == soltiondata.FreelancerId).Select(x => new { x.FirstName, x.LastName }).FirstOrDefault();
                            solutionTeam.FreelancerId = soltiondata.FreelancerId;
                            solutionTeam.FreelancerName = fullname.FirstName + " " + fullname.LastName;
                            var freelancerDetails = _db.FreelancerDetails.Where(x => x.UserId == soltiondata.FreelancerId).FirstOrDefault();
                            if (freelancerDetails != null)
                            {
                                solutionTeam.FreelancerLevel = freelancerDetails.FreelancerLevel;
                                solutionTeam.ImagePath = freelancerDetails.ImagePath;
                                solutionTeam.ImageUrlWithSas = freelancerDetails.ImageUrlWithSas;
                            }
                            solutionteamList.Add(solutionTeam);
                        }
                    }

                    var successfullprojectData = await _db.SolutionSuccessfullProject.Where(x => x.IndustryId == model.IndustryId && x.SolutionId == model.SolutionId).Where(x => x.IsActive == true).ToListAsync();
                    List<SuccessfullProjectModel> successfullProjectList = new List<SuccessfullProjectModel>();
                    if (successfullprojectData.Count > 0)
                    {
                        foreach (var projectData in successfullprojectData)
                        {
                            var resultData = _db.SolutionSuccessfullProjectResult.Where(x => x.SolutionSuccessfullProjectId == projectData.Id).ToList();
                            SuccessfullProjectModel succesfulldata = new SuccessfullProjectModel();
                            if (resultData.Count > 0)
                            {
                                succesfulldata.projectResultList = resultData;
                            }
                            succesfulldata.Title = projectData.Title;
                            succesfulldata.Description = projectData.Description;
                            successfullProjectList.Add(succesfulldata);

                        }
                    }
                    var Funddecided = _db.SolutionFund.Where(x => x.SolutionId == model.SolutionId && x.IndustryId == model.IndustryId && x.ProjectType == model.ProjectType && x.ClientId == model.UserId && x.IsCheckOutDone == true).Count();

                    var contractData = _db.Contract.Where(x => x.SolutionFundId == model.SolutionFundId).FirstOrDefault();
                    bool IsProjectstop = false;
                    if (contractData != null)
                    {
                        var solutionStopData = _db.SolutionStopPayment.Where(x => x.ContractId == contractData.Id).FirstOrDefault();
                        if (solutionStopData != null)
                        {
                            IsProjectstop = true;
                        }
                    }

                    var DocumentList = _db.ActiveProjectDocuments.Where(x => x.SolutionFundId == model.SolutionFundId).ToList();
                    var freelancerList = _db.FreelancerDetails.Where(x => x.HourlyRate != null && x.HourlyRate != "").ToList();

                    var Currency = await ConvertToCurrencySign(model.ClientPreferredCurrency);

                    return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                    {
                        StatusCode = StatusCodes.Status200OK,
                        Message = "Success",
                        Result = new
                        {
                            SolutionDefine = solutionDefine,
                            MileStone = milestoneList,
                            PointsData = pointsData,
                            SuccessfullProjects = successfullProjectList,
                            SolutionFund = fundProgress,
                            SolutionTeam = solutionteamList,
                            MileStoneProgressData = solutionMilesData,
                            FundDecided = Funddecided,
                            FundCompleted = fundCompleted,
                            IsProjectStop = IsProjectstop,
                            DocumentDataList = DocumentList,
                            FreelancerList = freelancerList,
                            FundStopByClient = fundStopByClient,
                            ContractData = contractData,
                            ClientCurrency = Currency
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
                StatusCode = StatusCodes.Status403Forbidden,
                Message = "Something Wennt Wrong"

            });

        }


        [HttpPost]
        [Route("GetProjectDetails")]
        public async Task<IActionResult> GetProjectDetails([FromBody] MileStoneDetailsViewModel model)
        {
            if (model != null)
            {
                try
                {
                    var industryname = await _db.Industries.Where(x => x.Id == model.IndustryId).Select(x => x.IndustryName).FirstOrDefaultAsync();
                    var solutionName = _db.Solutions.Where(x => x.Id == model.SolutionId).Select(x => x.Title).FirstOrDefault();
                    var services = _db.SolutionServices.Where(x => x.SolutionId == model.SolutionId).Select(x => x.ServicesId).FirstOrDefault();
                    var serviceName = _db.Services.Where(x => x.Id == services).Select(x => x.ServicesName).FirstOrDefault();

                    var data = _db.SolutionIndustryDetails.Where(x => x.IndustryId == model.IndustryId && x.SolutionId == model.SolutionId).FirstOrDefault();

                    return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                    {
                        StatusCode = StatusCodes.Status200OK,
                        Message = "Success",
                        Result = new
                        {
                            ProjectData = data,
                            ServiceName = serviceName,
                            IndustryName = industryname,
                            SolutionName = solutionName,
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
                StatusCode = StatusCodes.Status403Forbidden,
                Message = "Something Wennt Wrong"

            });

        }


        [HttpPost]
        [Route("GetSolutionListBasedonType")]
        public async Task<IActionResult> GetSolutionListBasedonType([FromBody] GetUserProfileRequestModel model)
        {

            try
            {
                var CheckType = _db.Users.Where(x => x.Id == model.UserId).Select(x => x.UserType).FirstOrDefault();
                List<SolutionIndustryDetails> industryIdlist = new List<SolutionIndustryDetails>();
                List<Solutions> solutions = new List<Solutions>();

                if (model.UserId != "" && CheckType != "Client")
                {
                    industryIdlist = await _db.SolutionIndustryDetails.Where(x => x.IsActiveForFreelancer == true).ToListAsync();
                }
                else
                {
                    industryIdlist = await _db.SolutionIndustryDetails.Where(x => x.IsActiveForClient == true).ToListAsync();
                }
                if (industryIdlist.Count > 0)
                {
                    foreach (var data in industryIdlist)
                    {
                        Solutions solutionModal = new Solutions();
                        solutionModal = _db.Solutions.Where(x => x.Id == data.SolutionId).FirstOrDefault();
                        solutions.Add(solutionModal);
                    }
                }
                return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                {
                    StatusCode = StatusCodes.Status200OK,
                    Message = "Success",
                    Result = solutions.Distinct().ToList()
                });

            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                {
                    StatusCode = StatusCodes.Status403Forbidden,
                    Message = ex.Message + ex.InnerException

                });
            }

        }


        [HttpPost]
        [Route("IndustriesListBasedonUserType")]
        public async Task<IActionResult> IndustriesListBasedonUserType([FromBody] GetUserProfileRequestModel model)
        {

            try
            {
                var CheckType = _db.Users.Where(x => x.Id == model.UserId).Select(x => x.UserType).FirstOrDefault();
                List<int> industryIdlist = new List<int>();
                List<Industries> IndustryList = new List<Industries>();

                if (model.UserId != "" && CheckType != "Client")
                {
                    industryIdlist = await _db.SolutionIndustryDetails.Where(x => x.IsActiveForFreelancer == true).Select(x => x.IndustryId).ToListAsync();
                }
                else
                {
                    industryIdlist = await _db.SolutionIndustryDetails.Where(x => x.IsActiveForClient == true).Select(x => x.IndustryId).ToListAsync();
                }
                if (industryIdlist.Count > 0)
                {
                    foreach (var data in industryIdlist)
                    {
                        Industries industriesModal = new Industries();
                        var industryDetail = _db.Industries.Where(x => x.Id == data).FirstOrDefault();
                        IndustryList.Add(industryDetail);
                    }
                }
                return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                {
                    StatusCode = StatusCodes.Status200OK,
                    Message = "Success",
                    Result = IndustryList.Distinct().ToList()
                });

            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                {
                    StatusCode = StatusCodes.Status403Forbidden,
                    Message = ex.Message + ex.InnerException

                });
            }

        }

        [HttpPost]
        [Route("GetTopThreePopularSolutionsList")]
        public async Task<IActionResult> GetTopThreePopularSolutionsList(GetUserProfileRequestModel model)
        {
            try
            {
                var CheckType = _db.Users.Where(x => x.Id == model.UserId).Select(x => x.UserType).FirstOrDefault();
                List<Solutions> solutionList = _db.Solutions.ToList();
                //List<Solutions> solutionList = listDetails.Take(3).ToList();
                List<SolutionsModel> solutionsModel = new List<SolutionsModel>();
                List<string> industrylist = new List<string>();
                List<int> industryIdlist = new List<int>();
                bool IsSavedProject = false;

                if (solutionList.Count > 0)
                {
                    foreach (var list in solutionList)
                    {
                        var serviceId = _db.SolutionServices.Where(x => x.SolutionId == list.Id).Select(x => x.ServicesId).FirstOrDefault();
                        var Servicename = _db.Services.Where(x => x.Id == serviceId).Select(x => x.ServicesName).FirstOrDefault();
                        if (model.UserId != "")
                        {
                            var SavedProjectData = _db.SavedProjects.Where(x => x.SolutionId == list.Id && x.UserId == model.UserId).FirstOrDefault();
                            if (SavedProjectData != null)
                            {
                                IsSavedProject = true;
                            }
                            else
                            {
                                IsSavedProject = false;
                            }
                        }


                        if (model.UserId != "" && CheckType != "Client")
                        {
                            industryIdlist = await _db.SolutionIndustryDetails.Where(x => x.SolutionId == list.Id && x.IsActiveForFreelancer == true).Select(x => x.IndustryId).ToListAsync();
                        }
                        else
                        {
                            industryIdlist = await _db.SolutionIndustryDetails.Where(x => x.SolutionId == list.Id && x.IsActiveForClient == true).Select(x => x.IndustryId).ToListAsync();
                        }

                        if (industryIdlist.Count > 0)
                        {
                            foreach (var industryId in industryIdlist)
                            {
                                var industryname = _db.Industries.Where(x => x.Id == industryId).Select(x => x.IndustryName).FirstOrDefault();
                                industrylist.Add(industryname);
                            }
                            SolutionsModel dataStore = new SolutionsModel();
                            dataStore.IsProjectSaved = IsSavedProject;
                            dataStore.Services = Servicename;
                            dataStore.solutionServices = serviceId;
                            dataStore.Industries = string.Join(",", industrylist);
                            dataStore.Id = list.Id;
                            dataStore.Description = list.Description;
                            dataStore.ImagePath = list.ImagePath;
                            dataStore.ImageUrlWithSas = list.ImageUrlWithSas;
                            dataStore.Title = list.Title;
                            dataStore.SubTitle = list.SubTitle;
                            solutionsModel.Add(dataStore);
                            industrylist.Clear();
                        }

                    }
                }

                float totalSolutions = solutionsModel.Count();
                double pagesCount = 0;
                if (totalSolutions > 0)
                {
                    double val = Convert.ToDouble((float)totalSolutions / 6);
                    pagesCount = Math.Ceiling(val);

                }
                List<SolutionsModel> mainlist = solutionsModel.Take(6).ToList();
                return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                {
                    StatusCode = StatusCodes.Status200OK,
                    Message = "Success",
                    Result = new
                    {
                        SolutionData = mainlist,
                        TotalCount = solutionsModel.Count(),
                        PageCount = pagesCount
                    }
                });

            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                {
                    StatusCode = StatusCodes.Status403Forbidden,
                    Message = ex.Message + ex.InnerException

                });
            }
        }

        [HttpPost]
        [Route("changeSolutionListByPagination")]
        public async Task<IActionResult> changeSolutionListByPagination([FromBody] MileStoneIdViewModel model)
        {
            try
            {
                var CheckType = _db.Users.Where(x => x.Id == model.UserId).Select(x => x.UserType).FirstOrDefault();
                List<Solutions> solutionList = _db.Solutions.ToList();
                //List<Solutions> solutionList = listDetails.Take(3).ToList();
                List<SolutionsModel> solutionsModel = new List<SolutionsModel>();
                List<string> industrylist = new List<string>();
                List<int> industryIdlist = new List<int>();
                bool IsSavedProject = false;

                if (solutionList.Count > 0)
                {
                    foreach (var list in solutionList)
                    {
                        var serviceId = _db.SolutionServices.Where(x => x.SolutionId == list.Id).Select(x => x.ServicesId).FirstOrDefault();
                        var Servicename = _db.Services.Where(x => x.Id == serviceId).Select(x => x.ServicesName).FirstOrDefault();
                        if (model.UserId != "")
                        {
                            var SavedProjectData = _db.SavedProjects.Where(x => x.SolutionId == list.Id && x.UserId == model.UserId).FirstOrDefault();
                            if (SavedProjectData != null)
                            {
                                IsSavedProject = true;
                            }
                            else
                            {
                                IsSavedProject = false;
                            }
                        }


                        if (model.UserId != "" && CheckType != "Client")
                        {
                            industryIdlist = await _db.SolutionIndustryDetails.Where(x => x.SolutionId == list.Id && x.IsActiveForFreelancer == true).Select(x => x.IndustryId).ToListAsync();
                        }
                        else
                        {
                            industryIdlist = await _db.SolutionIndustryDetails.Where(x => x.SolutionId == list.Id && x.IsActiveForClient == true).Select(x => x.IndustryId).ToListAsync();
                        }

                        if (industryIdlist.Count > 0)
                        {
                            foreach (var industryId in industryIdlist)
                            {
                                var industryname = _db.Industries.Where(x => x.Id == industryId).Select(x => x.IndustryName).FirstOrDefault();
                                industrylist.Add(industryname);
                            }
                            SolutionsModel dataStore = new SolutionsModel();
                            dataStore.IsProjectSaved = IsSavedProject;
                            dataStore.Services = Servicename;
                            dataStore.solutionServices = serviceId;
                            dataStore.Industries = string.Join(",", industrylist);
                            dataStore.Id = list.Id;
                            dataStore.Description = list.Description;
                            dataStore.ImagePath = list.ImagePath;
                            dataStore.ImageUrlWithSas = list.ImageUrlWithSas;
                            dataStore.Title = list.Title;
                            dataStore.SubTitle = list.SubTitle;
                            solutionsModel.Add(dataStore);
                            industrylist.Clear();
                        }

                    }
                }

                float totalSolutions = solutionsModel.Count();
                double pagesCount = 0;
                if (totalSolutions > 0)
                {
                    double val = Convert.ToDouble((float)totalSolutions / 6);
                    pagesCount = Math.Ceiling(val);

                }
                List<SolutionsModel> mainlist = solutionsModel.Take(6).ToList();
                if (model.pageNumber > 1 && model.pageNumber != null)
                {
                    var prevPage = (int)model.pageNumber - 1;
                    var current = (int)model.pageNumber;
                    int start = (prevPage * 6) - 1;
                    int end = (current * 6) - 1;
                    int indexOfLastElement = (solutionsModel.Count) - 1;
                    mainlist = solutionsModel.Where((value, index) => index > start && index <= end).ToList();
                }
                return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                {
                    StatusCode = StatusCodes.Status200OK,
                    Message = "Success",
                    Result = new
                    {
                        SolutionData = mainlist,
                        TotalCount = solutionsModel.Count(),
                        PageCount = pagesCount
                    }
                });

            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                {
                    StatusCode = StatusCodes.Status403Forbidden,
                    Message = ex.Message + ex.InnerException

                });
            }
        }

        [HttpPost]
        [Route("GetTopThreePopularSolutionBasedOnServices")]
        public async Task<IActionResult> GetTopThreePopularSolutionBasedOnServices([FromBody] MileStoneIdViewModel model)
        {
            if (model.Id != 0)
            {
                try
                {
                    var CheckType = _db.Users.Where(x => x.Id == model.UserId).Select(x => x.UserType).FirstOrDefault();
                    List<SolutionsModel> solutionsModel = new List<SolutionsModel>();
                    List<Industries> industrylistDetails = new List<Industries>();
                    List<string> industrylist = new List<string>();
                    List<int> industryList = new List<int>();
                    var serviceData = await _db.SolutionServices.Where(x => x.ServicesId == model.Id).Select(x => x.SolutionId).ToListAsync();
                    bool IsSavedProject = false;

                    if (serviceData.Count > 0)
                    {
                        foreach (var data in serviceData)
                        {
                            var solutiondata = _db.Solutions.Where(x => x.Id == data).FirstOrDefault();
                            if (model.UserId != "")
                            {
                                var SavedProjectData = _db.SavedProjects.Where(x => x.SolutionId == solutiondata.Id && x.UserId == model.UserId).FirstOrDefault();
                                if (SavedProjectData != null)
                                {
                                    IsSavedProject = true;
                                }
                                else
                                {
                                    IsSavedProject = false;
                                }
                            }
                            if (model.UserId != "" && CheckType != "Client")
                            {
                                industryList = _db.SolutionIndustryDetails.Where(x => x.SolutionId == data && x.IsActiveForFreelancer == true).Select(x => x.IndustryId).ToList();
                            }
                            else
                            {
                                industryList = _db.SolutionIndustryDetails.Where(x => x.SolutionId == data && x.IsActiveForClient == true).Select(x => x.IndustryId).ToList();
                            }
                            //var industryList = _db.SolutionIndustryDetails.Where(x => x.SolutionId == data && x.IsActiveForClient == true).Select(x => x.IndustryId).ToList();
                            if (industryList.Count > 0)
                            {
                                foreach (var industryId in industryList)
                                {
                                    var industry = _db.Industries.Where(x => x.Id == industryId).FirstOrDefault();
                                    industrylistDetails.Add(industry);
                                    var industryname = _db.Industries.Where(x => x.Id == industryId).Select(x => x.IndustryName).FirstOrDefault();
                                    industrylist.Add(industryname);
                                }
                                SolutionsModel dataStore = new SolutionsModel();
                                dataStore.solutionServices = model.Id;
                                dataStore.IsProjectSaved = IsSavedProject;
                                dataStore.Industries = string.Join(",", industrylist);
                                //dataStore.solutionIndustriesList = industrylistDetails.Distinct().ToList();
                                dataStore.Id = solutiondata.Id;
                                dataStore.Description = solutiondata.Description;
                                dataStore.ImagePath = solutiondata.ImagePath;
                                dataStore.ImageUrlWithSas = solutiondata.ImageUrlWithSas;
                                dataStore.Title = solutiondata.Title;
                                solutionsModel.Add(dataStore);
                                industrylist.Clear();
                            }

                        }
                    }

                    float totalSolutions = solutionsModel.Count();
                    double pagesCount = 0;
                    if (totalSolutions > 0)
                    {
                        double val = Convert.ToDouble((float)totalSolutions / 6);
                        pagesCount = Math.Ceiling(val);

                    }
                    List<SolutionsModel> mainlist = solutionsModel.Take(6).ToList();
                    if (model.pageNumber > 1 && model.pageNumber != null)
                    {
                        var prevPage = (int)model.pageNumber - 1;
                        var current = (int)model.pageNumber;
                        int start = (prevPage * 6) - 1;
                        int end = (current * 6) - 1;
                        int indexOfLastElement = (solutionsModel.Count) - 1;
                        mainlist = solutionsModel.Where((value, index) => index > start && index <= end).ToList();
                    }
                    return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                    {
                        StatusCode = StatusCodes.Status200OK,
                        Message = "Success",
                        Result = new
                        {
                            SolutionData = mainlist,
                            SolutionBindData = solutionsModel,
                            IndustriesData = industrylistDetails.Distinct(),
                            TotalCount = solutionsModel.Count(),
                            PageCount = pagesCount
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

            else
            {
                try
                {
                    var CheckType = _db.Users.Where(x => x.Id == model.UserId).Select(x => x.UserType).FirstOrDefault();
                    List<Solutions> solutionList = _db.Solutions.ToList();
                    List<SolutionsModel> solutionsModel = new List<SolutionsModel>();
                    List<Industries> industrylistDetails = new List<Industries>();
                    List<string> industrylist = new List<string>();
                    List<int> industryIdlist = new List<int>();
                    bool IsSavedProject = false;

                    if (solutionList.Count > 0)
                    {
                        foreach (var list in solutionList)
                        {
                            var serviceId = _db.SolutionServices.Where(x => x.SolutionId == list.Id).Select(x => x.ServicesId).FirstOrDefault();
                            var Servicename = _db.Services.Where(x => x.Id == serviceId).Select(x => x.ServicesName).FirstOrDefault();
                            if (model.UserId != "")
                            {
                                var SavedProjectData = _db.SavedProjects.Where(x => x.SolutionId == list.Id && x.UserId == model.UserId).FirstOrDefault();
                                if (SavedProjectData != null)
                                {
                                    IsSavedProject = true;
                                }
                                else
                                {
                                    IsSavedProject = false;
                                }
                            }
                            if (model.UserId != "" && CheckType != "Client")
                            {
                                industryIdlist = _db.SolutionIndustryDetails.Where(x => x.SolutionId == list.Id && x.IsActiveForFreelancer == true).Select(x => x.IndustryId).ToList();
                            }
                            else
                            {
                                industryIdlist = _db.SolutionIndustryDetails.Where(x => x.SolutionId == list.Id && x.IsActiveForClient == true).Select(x => x.IndustryId).ToList();
                            }
                            //var industryIdlist = _db.SolutionIndustryDetails.Where(x => x.SolutionId == list.Id && x.IsActiveForClient == true).Select(x => x.IndustryId).ToList();
                            if (industryIdlist.Count > 0)
                            {
                                foreach (var industryId in industryIdlist)
                                {
                                    var industry = _db.Industries.Where(x => x.Id == industryId).FirstOrDefault();
                                    industrylistDetails.Add(industry);
                                    var industryname = _db.Industries.Where(x => x.Id == industryId).Select(x => x.IndustryName).FirstOrDefault();
                                    industrylist.Add(industryname);
                                }
                                SolutionsModel dataStore = new SolutionsModel();
                                dataStore.IsProjectSaved = IsSavedProject;
                                dataStore.Services = Servicename;
                                dataStore.solutionServices = serviceId;
                                dataStore.Industries = string.Join(",", industrylist);
                                dataStore.Id = list.Id;
                                dataStore.Description = list.Description;
                                dataStore.ImagePath = list.ImagePath;
                                dataStore.ImageUrlWithSas = list.ImageUrlWithSas;
                                dataStore.Title = list.Title;
                                dataStore.SubTitle = list.SubTitle;
                                solutionsModel.Add(dataStore);
                                industrylist.Clear();
                            }

                        }
                    }
                    float totalSolutions = solutionsModel.Count();
                    double pagesCount = 0;
                    if (totalSolutions > 0)
                    {
                        double val = Convert.ToDouble((float)totalSolutions / 6);
                        pagesCount = Math.Ceiling(val);

                    }
                    List<SolutionsModel> mainlist = solutionsModel.Take(6).ToList();
                    if (model.pageNumber > 1 && model.pageNumber != null)
                    {
                        var prevPage = (int)model.pageNumber - 1;
                        var current = (int)model.pageNumber;
                        int start = (prevPage * 6) - 1;
                        int end = (current * 6) - 1;
                        int indexOfLastElement = (solutionsModel.Count) - 1;
                        mainlist = solutionsModel.Where((value, index) => index > start && index <= end).ToList();
                    }
                    return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                    {
                        StatusCode = StatusCodes.Status200OK,
                        Message = "Success",
                        Result = new
                        {
                            SolutionData = mainlist,
                            SolutionBindData = solutionsModel,
                            IndustriesData = industrylistDetails.Distinct(),
                            TotalCount = solutionsModel.Count(),
                            PageCount = pagesCount
                        }
                    });

                }
                catch (Exception ex)
                {
                    return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                    {
                        StatusCode = StatusCodes.Status403Forbidden,
                        Message = ex.Message + ex.InnerException

                    });
                }
            }
        }

        [HttpGet]
        [Route("GetSuccessfullProjectList")]
        public async Task<IActionResult> GetSuccessfullProjectList()
        {
            try
            {
                var successfullprojectData = await _db.SolutionSuccessfullProject.Where(x => x.IsActive == true).Take(3).ToListAsync();
                List<SuccessfullProjectModel> successfullProjectList = new List<SuccessfullProjectModel>();
                if (successfullprojectData.Count > 0)
                {
                    foreach (var projectData in successfullprojectData)
                    {
                        var resultData = _db.SolutionSuccessfullProjectResult.Where(x => x.SolutionSuccessfullProjectId == projectData.Id).ToList();
                        SuccessfullProjectModel succesfulldata = new SuccessfullProjectModel();
                        if (resultData.Count > 0)
                        {
                            succesfulldata.projectResultList = resultData;
                        }
                        succesfulldata.Title = projectData.Title;
                        succesfulldata.Description = projectData.Description;
                        successfullProjectList.Add(succesfulldata);

                    }
                }

                return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                {
                    StatusCode = StatusCodes.Status200OK,
                    Message = "Success",
                    Result = successfullProjectList
                });

            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                {
                    StatusCode = StatusCodes.Status403Forbidden,
                    Message = ex.Message + ex.InnerException

                });
            }
        }


        [HttpGet]
        [Route("GetTopProfessionalDetails")]
        public async Task<IActionResult> GetTopProfessionalDetails()
        {
            try
            {
                var topProfessionalData = await _db.SolutionTopProfessionals.Where(t => t.IsVisibleOnLandingPage == true).ToListAsync();
                List<SolutionTopProfessionalModel> professionalData = new List<SolutionTopProfessionalModel>();
                if (topProfessionalData.Count > 0)
                {
                    foreach (var topdata in topProfessionalData)
                    {
                        int? sumofReview = 0;
                        double finalRate = 0.0;
                        SolutionTopProfessionalModel solutionTop = new SolutionTopProfessionalModel();
                        solutionTop.Description = topdata.Description;
                        solutionTop.Title = topdata.TopProfessionalTitle;
                        var fullname = _db.Users.Where(x => x.Id == topdata.FreelancerId).Select(x => new { x.FirstName, x.LastName }).FirstOrDefault();

                        solutionTop.FreelancerName = fullname.FirstName + " " + fullname.LastName;
                        solutionTop.FreelancerId = topdata.FreelancerId;
                        solutionTop.ImagePath = _db.FreelancerDetails.Where(x => x.UserId == topdata.FreelancerId).Select(x => x.ImagePath).FirstOrDefault();

                        var freelancerReviewByclient = _db.FreelancerReview.Where(x => x.FreelancerId == topdata.FreelancerId).ToList();
                        if(freelancerReviewByclient.Count > 0)
                        {
                            foreach(var freelancerReview in freelancerReviewByclient)
                            {
                                sumofReview += freelancerReview.CommunicationRating + freelancerReview.CollaborationRating + freelancerReview.ProfessionalismRating + freelancerReview.TechnicalRating + freelancerReview.SatisfactionRating + freelancerReview.ResponsivenessRating + freelancerReview.LikeToWorkRating;
                            }
                            finalRate = (double)sumofReview / freelancerReviewByclient.Count() / 10;
                        }
                        solutionTop.Rate = finalRate.ToString();
                        professionalData.Add(solutionTop);
                    }
                }
                return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                {
                    StatusCode = StatusCodes.Status200OK,
                    Message = "Success",
                    Result = professionalData
                });

            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                {
                    StatusCode = StatusCodes.Status403Forbidden,
                    Message = ex.Message + ex.InnerException

                });
            }
        }


        //CreateUserStripeAccount
        //[HttpPost]
        //[Route("CreateUserStripeAccount")]
        //public async Task<IActionResult> CreateUserStripeAccount([FromBody] MileStoneIdViewModel model)
        //{
        //    var userDetails = await _db.Users.Where(x => x.Id == model.UserId).FirstOrDefaultAsync();
        //    if (userDetails != null)
        //    {
        //        StripeConfiguration.ApiKey = "sk_test_51NaxGxLHv0zYK8g4ZEh9KncjP5T6hbERI8VIn5bKUZvuY36xCSfp99bdrH5Td65cXkJ5FgDdMFVbmAao6xfm8Wje00pAJrWOjf";
        //        // Connected Account creation.
        //        if (userDetails.StripeAccountStatus != ApplicationUser.StripeAccountStatuses.Complete)
        //        {
        //            userDetails.StripeConnectedId = _stripeAccountService.CreateStripeAccount(StripeConfiguration.ApiKey);

        //            if (!string.IsNullOrEmpty(userDetails.StripeConnectedId))
        //            {
        //                userDetails.StripeAccountStatus = ApplicationUser.StripeAccountStatuses.Initiated;
        //                _db.Users.Update(userDetails);
        //                _db.SaveChanges();

        //                return StatusCode(StatusCodes.Status200OK, new APIResponseModel
        //                {
        //                    StatusCode = StatusCodes.Status200OK,
        //                    Message = "User Stripe Account Created",
        //                    Result = userDetails
        //                });
        //            }
        //            else
        //            {
        //                return StatusCode(StatusCodes.Status200OK, new APIResponseModel
        //                {
        //                    StatusCode = StatusCodes.Status200OK,
        //                    Message = "Something Went Wrong",
        //                });
        //            }
        //        }
        //    }

        //    return StatusCode(StatusCodes.Status200OK, new APIResponseModel
        //    {
        //        StatusCode = StatusCodes.Status200OK,
        //        Message = "Something Went Wrong",
        //    });
        //}

        ////GetUserStripeDetails
        //[HttpPost]
        //[Route("GetUserStripeDetails")]
        //public async Task<IActionResult> GetUserStripeDetails([FromBody] MileStoneIdViewModel model)
        //{
        //    var userDetails = await _db.Users.Where(x => x.Id == model.UserId).FirstOrDefaultAsync();
        //    if (userDetails != null)
        //    {

        //        StripeConfiguration.ApiKey = "sk_test_51NaxGxLHv0zYK8g4ZEh9KncjP5T6hbERI8VIn5bKUZvuY36xCSfp99bdrH5Td65cXkJ5FgDdMFVbmAao6xfm8Wje00pAJrWOjf";

        //        // checking Status of the account
        //        if (_stripeAccountService.IsComplete(userDetails.StripeConnectedId))
        //        {
        //            userDetails.StripeAccountStatus = ApplicationUser.StripeAccountStatuses.Complete;
        //            _db.Users.Update(userDetails);
        //            _db.SaveChanges();
        //            return StatusCode(StatusCodes.Status200OK, new APIResponseModel
        //            {
        //                StatusCode = StatusCodes.Status200OK,
        //                Message = "User Updated Successsfully",
        //                Result = new
        //                {
        //                    UserDetails = userDetails,
        //                    IsCompleted = true
        //                }
        //            });
        //        }
        //        // incase account is not complete 

        //        // check if accidently landed user has not created the account.
        //        if (userDetails.StripeAccountStatus == ApplicationUser.StripeAccountStatuses.NotCreated)
        //        {
        //            userDetails.StripeConnectedId = _stripeAccountService.CreateStripeAccount(StripeConfiguration.ApiKey);
        //            userDetails.StripeAccountStatus = ApplicationUser.StripeAccountStatuses.Initiated;
        //            _db.Users.Update(userDetails);
        //            _db.SaveChanges();

        //            return StatusCode(StatusCodes.Status200OK, new APIResponseModel
        //            {
        //                StatusCode = StatusCodes.Status200OK,
        //                Message = "User Created Successsfully",
        //                Result = new
        //                {
        //                    UserDetails = userDetails,
        //                    IsCompleted = false
        //                }
        //            });
        //        }
        //    }

        //    return StatusCode(StatusCodes.Status200OK, new APIResponseModel
        //    {
        //        StatusCode = StatusCodes.Status200OK,
        //        Message = "Something Went Wrong",
        //    });
        //}

        //[HttpPost]
        //[Route("CheckOut")]
        //public async Task<IActionResult> CheckOut([FromBody] solutionFundViewModel model)
        //{
        //    try
        //    {
        //        var mileStone = _db.SolutionMilestone.Where(x => x.Id == model.Id).FirstOrDefault();
        //        var contractData = await _db.Contract.Where(x => x.MilestoneDataId == model.Id && x.ClientUserId == model.UserId).FirstOrDefaultAsync();

        //        if (contractData == null)
        //        {
        //            if (model.Id != 0)
        //            {
        //                Contract? contractSave;
        //                var trimmedPrice = model.ProjectPrice.Replace("€", "");
        //                var ProjectPrice = Convert.ToInt64(trimmedPrice);
        //                if (model.MileStoneCheckout)
        //                {
        //                    var MilestoneTotalDaysByProjectType = _db.SolutionMilestone.Where(x => x.SolutionId == model.SolutionId && x.IndustryId == model.IndustryId && x.ProjectType == mileStone.ProjectType).ToList();
        //                    if (MilestoneTotalDaysByProjectType.Count > 0)
        //                    {
        //                        SolutionMilestone mileStoneToTalDays = MilestoneTotalDaysByProjectType
        //                       .GroupBy(l => l.ProjectType)
        //                       .Select(cl => new SolutionMilestone
        //                       {
        //                           ProjectType = cl.First().ProjectType,
        //                           Days = cl.Sum(c => c.Days),
        //                       }).FirstOrDefault();

        //                        if (mileStoneToTalDays.Days > 0)
        //                        {
        //                            var calculateProjectPrice = (ProjectPrice / mileStoneToTalDays.Days) * mileStone.Days;
        //                            model.ProjectPrice = calculateProjectPrice.ToString();
        //                        }
        //                    }

        //                    contractSave = new Contract()
        //                    {
        //                        ClientUserId = model.UserId,
        //                        MilestoneDataId = mileStone.Id,
        //                        //MileStone = mileStone,
        //                        PaymentStatus = Contract.PaymentStatuses.ContractCreated,
        //                        PaymentIntentId = string.Empty,
        //                        SolutionFundId = model.SolutionFundId,
        //                        SolutionId = model.SolutionId,
        //                        IndustryId = model.IndustryId,
        //                        CreatedDateTime = DateTime.Now,
        //                        Amount = model.ProjectPrice,

        //                    };
        //                    _db.Contract.Add(contractSave);
        //                    _db.SaveChanges();

        //                    List<ContractUser> contractUsers = new List<ContractUser>();
        //                    var fl = _db.Users.Where(x => x.UserType == "Freelancer" && x.StripeAccountStatus == StripeAccountStatuses.Complete
        //                    && !string.IsNullOrEmpty(x.StripeConnectedId)).ToList();
        //                    foreach (var item in fl)
        //                    {
        //                        contractUsers.Add(new ContractUser()
        //                        {
        //                            Percentage = 10,
        //                            StripeTranferId = string.Empty,
        //                            IsTransfered = false,
        //                            ApplicationUserId = item.Id,
        //                            ContractId = contractSave.Id
        //                        });
        //                    }
        //                    _db.ContractUser.AddRange(contractUsers);
        //                    _db.SaveChanges();
        //                }
        //                else
        //                {
        //                    var contractDetails = _db.Contract.Where(x => x.SolutionId == model.SolutionId && x.IndustryId == model.IndustryId && x.ClientUserId == model.UserId).FirstOrDefault();
        //                    if (contractDetails == null)
        //                    {
        //                        contractSave = new Contract()
        //                        {
        //                            ClientUserId = model.UserId,
        //                            SolutionId = model.SolutionId,
        //                            IndustryId = model.IndustryId,
        //                            //MileStone = mileStone,
        //                            PaymentStatus = Contract.PaymentStatuses.ContractCreated,
        //                            PaymentIntentId = string.Empty,
        //                            SolutionFundId = model.SolutionFundId,
        //                            CreatedDateTime = DateTime.Now,
        //                            Amount = ProjectPrice.ToString(),

        //                        };
        //                        _db.Contract.Add(contractSave);
        //                        _db.SaveChanges();

        //                        List<ContractUser> contractUsers = new List<ContractUser>();
        //                        var fl = _db.Users.Where(x => x.UserType == "Freelancer" && x.StripeAccountStatus == StripeAccountStatuses.Complete
        //                        && !string.IsNullOrEmpty(x.StripeConnectedId)).ToList();
        //                        foreach (var item in fl)
        //                        {
        //                            contractUsers.Add(new ContractUser()
        //                            {
        //                                Percentage = 10,
        //                                StripeTranferId = string.Empty,
        //                                IsTransfered = false,
        //                                ApplicationUserId = item.Id,
        //                                ContractId = contractSave.Id
        //                            });


        //                        }
        //                        _db.ContractUser.AddRange(contractUsers);
        //                        _db.SaveChanges();
        //                    }
        //                    else
        //                    {
        //                        var solutionFundData = _db.SolutionFund.Where(x => x.Id == model.SolutionFundId).FirstOrDefault();
        //                        if (solutionFundData != null)
        //                        {
        //                            if (solutionFundData.ProjectStatus == "COMPLETED")
        //                            {
        //                                return StatusCode(StatusCodes.Status200OK, new APIResponseModel
        //                                {
        //                                    StatusCode = StatusCodes.Status200OK,
        //                                    Message = "Payment is already done for this project",
        //                                });
        //                            }
        //                        }
        //                    }

        //                }

        //            }

        //        }
        //        //var contract = _db.Contract.Include("ContractUsers").FirstOrDefault(x => x.MileStone.Id == model.Id);
        //        var contract = new Contract();
        //        if (model.MileStoneCheckout)
        //        {
        //            contract = _db.Contract.Include("ContractUsers").FirstOrDefault(x => x.MilestoneDataId == model.Id && x.ClientUserId == model.UserId);
        //        }
        //        else
        //        {
        //            contract = _db.Contract.Include("ContractUsers").FirstOrDefault(x => x.SolutionId == model.SolutionId && x.IndustryId == model.IndustryId && x.ClientUserId == model.UserId);
        //        }


        //        var domain = _configuration.GetValue<string>("DomainUrl:Domain");
        //        var successUrl = string.Format("{0}/LandingPage/CheckoutSuccess?cntId={1}", domain, contract.Id);
        //        var cancelUrl = string.Format("{0}/LandingPage/CheckoutCancel?cntId={1}", domain, contract.Id);

        //        Contract.PaymentStatuses payment = contract.PaymentStatus;
        //        if (payment == Contract.PaymentStatuses.ContractCreated)
        //        {
        //            Session session = new Session();
        //            if (model.MileStoneCheckout)
        //            {
        //                session = _stripeAccountService.CreateCheckoutSession(mileStone, model.ProjectPrice, successUrl, cancelUrl);
        //            }
        //            else
        //            {
        //                session = _stripeAccountService.CreateProjectCheckoutSession(model.ProjectPrice, successUrl, cancelUrl);
        //            }

        //            if (session == null || string.IsNullOrEmpty(session.Id))
        //            {
        //                //Response.Headers.Add("Location", domain + "/LandingPage/Project");
        //                //return new StatusCodeResult(303);
        //                var emptyStringUrl = domain + "/LandingPage/Project";
        //                return StatusCode(StatusCodes.Status200OK, new APIResponseModel
        //                {
        //                    StatusCode = StatusCodes.Status200OK,
        //                    Result = emptyStringUrl
        //                });
        //            }

        //            //checkout initiated successful
        //            contract.SessionId = session.Id;
        //            contract.SessionExpiry = session.ExpiresAt;
        //            contract.SessionStatus = _stripeAccountService.GetSesssionStatus(session);
        //            contract.PaymentStatus = _stripeAccountService.GetPaymentStatus(session);

        //            _db.Update(contract);
        //            _db.SaveChanges();

        //            //Response.Headers.Add("Location", session.Url);
        //            //return new StatusCodeResult(303);
        //            return StatusCode(StatusCodes.Status200OK, new APIResponseModel
        //            {
        //                StatusCode = StatusCodes.Status200OK,
        //                Message = "success",
        //                Result = session.Url
        //            });
        //        }
        //        else if (contract.PaymentStatus == Contract.PaymentStatuses.UnPaid)
        //        {
        //            var checkoutSession = _stripeAccountService.GetCheckOutSesssion(contract.SessionId);

        //            if (checkoutSession != null)
        //            {
        //                contract.SessionStatus = _stripeAccountService.GetSesssionStatus(checkoutSession);

        //                if (contract.SessionStatus == Contract.SessionStatuses.Open)
        //                {
        //                    //Response.Headers.Add("Location", checkoutSession.Url);
        //                    //return new StatusCodeResult(303);
        //                    return StatusCode(StatusCodes.Status200OK, new APIResponseModel
        //                    {
        //                        StatusCode = StatusCodes.Status200OK,
        //                        Message = "success",
        //                        Result = checkoutSession.Url
        //                    });
        //                }
        //                else
        //                {
        //                    Session session = _stripeAccountService.CreateCheckoutSession(mileStone, model.ProjectPrice, successUrl, cancelUrl);

        //                    if (session == null || string.IsNullOrEmpty(session.Id))
        //                    {
        //                        //Response.Headers.Add("Location", domain + "/Checkout");
        //                        //return new StatusCodeResult(303);
        //                        return StatusCode(StatusCodes.Status200OK, new APIResponseModel
        //                        {
        //                            StatusCode = StatusCodes.Status200OK,
        //                            Message = "success",
        //                            Result = domain + "/LandingPage/Project"
        //                        });
        //                    }

        //                    //checkout initiated successful
        //                    contract.SessionId = session.Id;
        //                    contract.SessionExpiry = session.ExpiresAt;
        //                    contract.SessionStatus = _stripeAccountService.GetSesssionStatus(session);
        //                    contract.PaymentStatus = _stripeAccountService.GetPaymentStatus(session);

        //                    _db.Update(contract);
        //                    _db.SaveChanges();

        //                    //Response.Headers.Add("Location", session.Url);
        //                    //return new StatusCodeResult(303);
        //                    return StatusCode(StatusCodes.Status200OK, new APIResponseModel
        //                    {
        //                        StatusCode = StatusCodes.Status200OK,
        //                        Message = "success",
        //                        Result = session.Url
        //                    });
        //                }
        //            }
        //            else
        //            {
        //                //Response.Headers.Add("Location", successUrl);
        //                //return new StatusCodeResult(303);
        //                return StatusCode(StatusCodes.Status200OK, new APIResponseModel
        //                {
        //                    StatusCode = StatusCodes.Status200OK,
        //                    Message = "success",
        //                    Result = successUrl
        //                });
        //            }
        //        }
        //        else
        //        {
        //            //Response.Headers.Add("Location", successUrl);
        //            //return new StatusCodeResult(303);
        //            return StatusCode(StatusCodes.Status200OK, new APIResponseModel
        //            {
        //                StatusCode = StatusCodes.Status200OK,
        //                Message = "success",
        //                Result = successUrl
        //            });
        //        }
        //    }
        //    catch (Exception ex)
        //    {

        //    }

        //    return StatusCode(StatusCodes.Status200OK, new APIResponseModel
        //    {
        //        StatusCode = StatusCodes.Status200OK,
        //        Message = "Something Went Wrong",
        //    });
        //}

        //GetCheckoutMileStoneData
        [HttpPost]
        [Route("GetCheckoutMileStoneData")]
        public async Task<IActionResult> GetCheckoutMileStoneData([FromBody] MileStoneDetailsViewModel model)
        {
            try
            {
                var milestoneData = await _db.SolutionMilestone.Where(x => x.SolutionId == model.SolutionId && x.IndustryId == model.IndustryId).FirstOrDefaultAsync();
                if (milestoneData != null)
                {
                    return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                    {
                        StatusCode = StatusCodes.Status200OK,
                        Message = "success",
                        Result = milestoneData
                    });
                }
            }
            catch (Exception ex)
            {

            }
            return StatusCode(StatusCodes.Status200OK, new APIResponseModel
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "No Data Found",
            });

        }


        //GetUserSuccessCheckoutDetails
        [HttpPost]
        [Route("GetUserSuccessCheckoutDetails")]
        public async Task<IActionResult> GetUserSuccessCheckoutDetails([FromBody] RevoultCheckOutModel model)
        {
            try
            {
                var solutionFundData = await _db.SolutionFund.Where(x => x.Id == model.SolutionFundId).FirstOrDefaultAsync();
                if (solutionFundData != null)
                {
                    var ContractProjectPrice = string.Empty;
                    decimal ContractclientFees = 0;
                    if (solutionFundData.FundType == SolutionFund.FundTypes.MilestoneFund)
                    {
                        var MileStoneData = _db.SolutionMilestone.Where(x => x.Id == solutionFundData.MileStoneId).FirstOrDefault();
                        var MilestoneTotalDaysByProjectType = _db.SolutionMilestone.Where(x => x.SolutionId == solutionFundData.SolutionId && x.IndustryId == solutionFundData.IndustryId && x.ProjectType == solutionFundData.ProjectType).ToList();
                        if (MilestoneTotalDaysByProjectType.Count > 0)
                        {
                            SolutionMilestone mileStoneToTalDays = MilestoneTotalDaysByProjectType
                           .GroupBy(l => l.ProjectType)
                           .Select(cl => new SolutionMilestone
                           {
                               ProjectType = cl.First().ProjectType,
                               Days = cl.Sum(c => c.Days),
                           }).FirstOrDefault();

                            if (mileStoneToTalDays.Days > 0)
                            {
                                //var finalPrice = await CountFinalProjectPricing(solutionFundData);
                                var calculateProjectPrice = (Convert.ToDecimal(solutionFundData.ProjectPrice) / mileStoneToTalDays.Days) * MileStoneData.Days;
                                ContractProjectPrice = calculateProjectPrice.ToString();
                            }
                            else
                            {
                                ContractProjectPrice = solutionFundData.ProjectPrice;
                            }
                        }
                    }
                    else
                    {
                        //var finalPrice = await CountFinalProjectPricing(solutionFundData);
                        ContractProjectPrice = solutionFundData.ProjectPrice.ToString();
                    }

                    // Save to contract Table
                    var contractSave = new Contract()
                    {
                        ClientUserId = model.UserId,
                        SolutionId = solutionFundData.SolutionId,
                        IndustryId = solutionFundData.IndustryId,
                        PaymentStatus = Contract.PaymentStatuses.ContractCreated,
                        PaymentIntentId = model.RevoultOrderId,
                        SolutionFundId = model.SolutionFundId,
                        CreatedDateTime = DateTime.Now,
                        MilestoneDataId = solutionFundData.MileStoneId,
                        Amount = ContractProjectPrice.ToString(),
                        RevolutFee = 150 //############
                    };
                    _db.Contract.Add(contractSave);
                    _db.SaveChanges();


                    // save to contract user table
                    List<ContractUser> contractUsers = new List<ContractUser>();
                    List<FreelancerDetails> freelancerList = new List<FreelancerDetails>();
                    bool associateDetailsAdded = false;
                    bool projectManagerAdded = false;
                    bool expertDetailsAdded = false;
                    var exprtcount = 0;
                    var associatecount = 0;
                    var projectmanagercount = 0;
                    var experttotal = 0;
                    var assosiatetotal = 0;
                    var projectmanagertotal = 0;

                    CustomProjectDetials? teamData = null;
                    var Userslist = _db.Users.Where(x => x.UserType == "Freelancer" && x.RevolutStatus == true && !string.IsNullOrEmpty(x.RevolutConnectId)).ToList();
                    if (solutionFundData.ProjectType == AppConst.ProjectType.CUSTOM_PROJECT)
                    {
                        var solutionIndustryData = _db.SolutionIndustryDetails.Where(x => x.SolutionId == solutionFundData.SolutionId && x.IndustryId == solutionFundData.IndustryId).FirstOrDefault();
                        if (solutionIndustryData != null)
                        {
                            var solutionDefineData = _db.SolutionDefine.Where(x => x.SolutionIndustryDetailsId == solutionIndustryData.Id && x.ProjectType == solutionFundData.ProjectType).FirstOrDefault();
                            if (solutionDefineData != null)
                            {
                                teamData = _db.CustomProjectDetials.Where(x => x.SolutionDefineId == solutionDefineData.Id).FirstOrDefault();
                            }
                        }
                        experttotal = Convert.ToInt32(teamData.Expert);
                        assosiatetotal = Convert.ToInt32(teamData.Associate);
                        projectmanagertotal = Convert.ToInt32(teamData.ProjectManager);
                    }


                    if (Userslist.Count > 0)
                    {
                        foreach (var data in Userslist)
                        {
                            if (solutionFundData.ProjectType == AppConst.ProjectType.SMALL_PROJECT)
                            {
                                //var teamSize = "1 Project Manager + 1 Associate";
                                if (!associateDetailsAdded)
                                {
                                    var associateDetails = _db.FreelancerDetails.Where(x => x.FreelancerLevel == "Associate" && x.UserId == data.Id).FirstOrDefault();
                                    if (associateDetails != null)
                                    {
                                        freelancerList.Add(associateDetails);
                                        associateDetailsAdded = true;
                                    }
                                }

                                if (!projectManagerAdded)
                                {
                                    var projectManagerDetails = _db.FreelancerDetails.Where(x => x.FreelancerLevel == "Project Manager" && x.UserId == data.Id).FirstOrDefault();
                                    if (projectManagerDetails != null)
                                    {
                                        freelancerList.Add(projectManagerDetails);
                                        projectManagerAdded = true;
                                    }
                                }

                            }
                            if (solutionFundData.ProjectType == AppConst.ProjectType.MEDIUM_PROJECT)
                            {
                                //var teamsize = "1 Project Manager + 1 Expert + 1 Associate";
                                if (!expertDetailsAdded)
                                {
                                    var expertDetails = _db.FreelancerDetails.Where(x => x.FreelancerLevel == "Expert" && x.UserId == data.Id).FirstOrDefault();
                                    if (expertDetails != null)
                                    {
                                        freelancerList.Add(expertDetails);
                                        expertDetailsAdded = true;
                                    }
                                }

                                if (!associateDetailsAdded)
                                {
                                    var associateDetails = _db.FreelancerDetails.Where(x => x.FreelancerLevel == "Associate" && x.UserId == data.Id).FirstOrDefault();
                                    if (associateDetails != null)
                                    {
                                        freelancerList.Add(associateDetails);
                                        associateDetailsAdded = true;
                                    }
                                }

                                if (!projectManagerAdded)
                                {
                                    var projectManager = _db.FreelancerDetails.Where(x => x.FreelancerLevel == "Project Manager" && x.UserId == data.Id).FirstOrDefault();
                                    if (projectManager != null)
                                    {
                                        freelancerList.Add(projectManager);
                                        projectManagerAdded = true;
                                    }
                                }

                            }
                            if (solutionFundData.ProjectType == AppConst.ProjectType.LARGE_PROJECT)
                            {
                                //var teamsize = "1 Project Manager + 2 Experts + 2 Associates";
                                if (exprtcount <= 1)
                                {
                                    var expertsDetails = _db.FreelancerDetails.Where(x => x.FreelancerLevel == "Expert" && x.UserId == data.Id).FirstOrDefault();
                                    if (expertsDetails != null)
                                    {
                                        freelancerList.Add(expertsDetails);
                                        exprtcount++;
                                    }
                                }

                                if (associatecount <= 1)
                                {
                                    var associateDetails = _db.FreelancerDetails.Where(x => x.FreelancerLevel == "Associate" && x.UserId == data.Id).FirstOrDefault();
                                    if (associateDetails != null)
                                    {
                                        freelancerList.Add(associateDetails);
                                        associatecount++;
                                    }
                                }

                                if (!projectManagerAdded)
                                {
                                    var projectManager = _db.FreelancerDetails.Where(x => x.FreelancerLevel == "Project Manager" && x.UserId == data.Id).FirstOrDefault();
                                    if (projectManager != null)
                                    {
                                        freelancerList.Add(projectManager);
                                        projectManagerAdded = true;
                                    }
                                }

                            }
                            if (solutionFundData.ProjectType == AppConst.ProjectType.CUSTOM_PROJECT)
                            {
                                //var teamsize = "1 Project Manager + 2 Experts + 2 Associates";

                                if (!expertDetailsAdded)
                                {
                                    if (exprtcount < experttotal)
                                    {
                                        var expertsDetails = _db.FreelancerDetails.Where(x => x.FreelancerLevel == "Expert" && x.UserId == data.Id).FirstOrDefault();
                                        if (expertsDetails != null)
                                        {
                                            freelancerList.Add(expertsDetails);
                                            exprtcount++;
                                        }
                                    }
                                    if (exprtcount == experttotal)
                                    {
                                        expertDetailsAdded = true;
                                    }
                                }


                                if (!associateDetailsAdded)
                                {
                                    if (associatecount < assosiatetotal)
                                    {
                                        var associateDetails = _db.FreelancerDetails.Where(x => x.FreelancerLevel == "Associate" && x.UserId == data.Id).FirstOrDefault();
                                        if (associateDetails != null)
                                        {
                                            freelancerList.Add(associateDetails);
                                            associatecount++;
                                        }
                                    }
                                    if (associatecount == assosiatetotal)
                                    {
                                        associateDetailsAdded = true;
                                    }
                                }


                                if (!projectManagerAdded)
                                {
                                    if (projectmanagercount < projectmanagertotal)
                                    {
                                        var projectManager = _db.FreelancerDetails.Where(x => x.FreelancerLevel == "Project Manager" && x.UserId == data.Id).FirstOrDefault();
                                        if (projectManager != null)
                                        {
                                            freelancerList.Add(projectManager);
                                            //
                                            projectmanagercount++;
                                        }
                                    }
                                    if (projectmanagercount == projectmanagertotal)
                                    {
                                        projectManagerAdded = true;
                                    }

                                }

                            }
                        }
                    }


                    foreach (var item in freelancerList)
                    {
                        contractUsers.Add(new ContractUser()
                        {
                            Percentage = 0, // this field is not in use
                            StripeTranferId = string.Empty,
                            IsTransfered = false,
                            ApplicationUserId = item.UserId,
                            ContractId = contractSave.Id
                        });
                    }
                    //var projectManager = _db.FreelancerDetails.Where(x => x.FreelancerLevel == "Project Manager" ).FirstOrDefault();
                    //if (projectManager != null)
                    //{
                    //    contractUsers.Add(new ContractUser() { Percentage = 0, StripeTranferId = string.Empty, IsTransfered = false, ApplicationUserId = projectManager.UserId, ContractId = contractSave.Id });
                    //}
                    _db.ContractUser.AddRange(contractUsers);
                    _db.SaveChanges();


                    // Save to SolutionFund Table
                    solutionFundData.IsCheckOutDone = true;
                    solutionFundData.ProjectStatus = "COMPLETED";
                    solutionFundData.IsArchived = true;
                    _db.SaveChanges();

                    // Save to MilestoneStatus Table
                    if (solutionFundData.FundType == SolutionFund.FundTypes.MilestoneFund)
                    {
                        var storeMilestonestatus = _db.ActiveSolutionMilestoneStatus.Where(x => x.MilestoneId == solutionFundData.MileStoneId && x.UserId == model.UserId).FirstOrDefault();
                        if (storeMilestonestatus == null)
                        {
                            var milestoneStatus = new ActiveSolutionMilestoneStatus()
                            {
                                MilestoneId = solutionFundData.MileStoneId,
                                UserId = model.UserId,
                                MilestoneStatus = "Fund Completed Pay Inprogress"
                            };
                            _db.ActiveSolutionMilestoneStatus.Add(milestoneStatus);
                            _db.SaveChanges();
                        }
                    }

                    // Invoice Generate
                    var SolutionTitle = string.Empty;
                    if (solutionFundData.FundType == SolutionFund.FundTypes.MilestoneFund)
                    {
                        SolutionTitle = _db.SolutionMilestone.Where(x => x.Id == solutionFundData.MileStoneId).Select(x => x.Title).FirstOrDefault();
                    }
                    else
                    {
                        SolutionTitle = _db.Solutions.Where(x => x.Id == solutionFundData.SolutionId).Select(x => x.Title).FirstOrDefault();
                    }
                    string invoiceErr = "";
                    try
                    {
                        #region Invoice 1 Funding for Milestone or Project
                        // Invoice 1 - Funding for Milestone or Project

                        decimal clientAndFLfees = 0;
                        decimal flFees = 0;
                        decimal clientFees = 0;

                        var teamList = _db.SolutionTeam.Where(x => x.SolutionFundId == model.SolutionFundId).ToList(); //######
                        flFees = teamList.Sum(y => y.PlatformFees); // need to add client platform fee

                        if (solutionFundData.ProjectType == AppConst.ProjectType.LARGE_PROJECT)
                        {
                            clientFees = (teamList.Sum(y => y.Amount) * AppConst.Commission.PLATFORM_COMM_FROM_CLIENT_LARGE) / 100;
                        }
                        else if (solutionFundData.ProjectType == AppConst.ProjectType.SMALL_PROJECT)
                        {
                            clientFees = (teamList.Sum(y => y.Amount) * AppConst.Commission.PLATFORM_COMM_FROM_CLIENT_SMALL) / 100;
                        }
                        else if (solutionFundData.ProjectType == AppConst.ProjectType.MEDIUM_PROJECT)
                        {
                            clientFees = (teamList.Sum(y => y.Amount) * AppConst.Commission.PLATFORM_COMM_FROM_CLIENT_MEDIUM) / 100;
                        }
                        else if (solutionFundData.ProjectType == AppConst.ProjectType.MEDIUM_PROJECT)
                        {
                            if (teamList.Count <= 3)
                            {
                                clientFees = (teamList.Sum(y => y.Amount) * AppConst.Commission.PLATFORM_COMM_FROM_CLIENT_CUSTOM_LESS_THAN_THREE_GIGS) / 100;
                            }
                            else
                            {
                                clientFees = (teamList.Sum(y => y.Amount) * AppConst.Commission.PLATFORM_COMM_FROM_CLIENT_CUSTOM) / 100;
                            }

                        }
                        clientAndFLfees = flFees + clientFees;

                        var invoiceFunding = new InvoiceList();
                        invoiceFunding.BillToClientId = model.UserId;
                        // invoiceFunding.InvoiceNumber = "INV-00101"; // ######
                        invoiceFunding.InvoiceDate = DateTime.Now;
                        invoiceFunding.TransactionType = AppConst.InvoiceTransactionType.INVOICE1_PORTAL_TO_CLIENT;
                        invoiceFunding.TotalAmount = Convert.ToString((Convert.ToDecimal(contractSave.Amount) - clientAndFLfees));
                        invoiceFunding.InvoiceType = "Invoice";
                        invoiceFunding.ContractId = contractSave.Id;
                        _db.InvoiceList.Add(invoiceFunding);
                        _db.SaveChanges();

                        await UpdateInvoiceNumberAsync(invoiceFunding.Id, "INV");

                        // Invoice 1 (Details 1) - Funding for Milestone or Project
                        var invoiceFundingDetail = new InvoiceListDetails();
                        invoiceFundingDetail.InvoiceListId = invoiceFunding.Id;
                        invoiceFundingDetail.Amount = invoiceFunding.TotalAmount;
                        invoiceFundingDetail.Description = "Funding for \"" + SolutionTitle + "\""; // ###### - this will be either project or milestone tile
                        _db.InvoiceListDetails.Add(invoiceFundingDetail);
                        _db.SaveChanges();

                        // Invoice 1 (Details 2) - Funding for Milestone or Project
                        var invoiceFundingDetail_vat = new InvoiceListDetails();
                        invoiceFundingDetail_vat.InvoiceListId = invoiceFunding.Id;
                        invoiceFundingDetail_vat.Amount = "0";
                        invoiceFundingDetail_vat.Description = "VAT (0%)";
                        _db.InvoiceListDetails.Add(invoiceFundingDetail_vat);
                        _db.SaveChanges();

                        // Invoice 1 (Details 3) - Funding for Milestone or Project
                        var invoiceFundingDetail_total = new InvoiceListDetails();
                        invoiceFundingDetail_total.InvoiceListId = invoiceFunding.Id;
                        invoiceFundingDetail_total.Amount = invoiceFundingDetail.Amount;
                        invoiceFundingDetail_total.Description = "Total amount";
                        _db.InvoiceListDetails.Add(invoiceFundingDetail_total);
                        _db.SaveChanges();
                        #endregion

                        #region Payment Receipt
                        // Payment Receipt
                        var invoicePaymentReceipt = new InvoiceList();
                        invoicePaymentReceipt.BillToClientId = model.UserId;
                        //invoicePaymentReceipt.InvoiceNumber = "INV-00102"; // ######
                        invoicePaymentReceipt.InvoiceDate = DateTime.Now;
                        invoicePaymentReceipt.TransactionType = AppConst.InvoiceTransactionType.PAYMENT_RECEIPT_AMOUNT_DUE;
                        invoicePaymentReceipt.TotalAmount = Convert.ToString(contractSave.Amount);
                        invoicePaymentReceipt.InvoiceType = "Payment Receipt";
                        invoicePaymentReceipt.ContractId = contractSave.Id;
                        _db.InvoiceList.Add(invoicePaymentReceipt);
                        _db.SaveChanges();
                        await UpdateInvoiceNumberAsync(invoicePaymentReceipt.Id, "REC");

                        // Payment Receipt (Details 1) - 
                        var invoicePaymentReceiptDetail_amtDue = new InvoiceListDetails();
                        invoicePaymentReceiptDetail_amtDue.InvoiceListId = invoicePaymentReceipt.Id;
                        invoicePaymentReceiptDetail_amtDue.Amount = invoicePaymentReceipt.TotalAmount;
                        invoicePaymentReceiptDetail_amtDue.Description = "Amount due";
                        _db.InvoiceListDetails.Add(invoicePaymentReceiptDetail_amtDue);
                        _db.SaveChanges();

                        // Payment Receipt (Details 2) - 
                        var invoicePaymentReceiptDetail_ttlAmt = new InvoiceListDetails();
                        invoicePaymentReceiptDetail_ttlAmt.InvoiceListId = invoicePaymentReceipt.Id;
                        invoicePaymentReceiptDetail_ttlAmt.Amount = invoicePaymentReceipt.TotalAmount;
                        invoicePaymentReceiptDetail_ttlAmt.Description = "Total amount";
                        _db.InvoiceListDetails.Add(invoicePaymentReceiptDetail_ttlAmt);
                        _db.SaveChanges();
                        #endregion

                        #region Invoice 3 Total platform fees
                        // Invoice 3 - Total platform fees for Milestone or Project
                        var invoiceTotalPlatformFees = new InvoiceList();
                        invoiceTotalPlatformFees.BillToClientId = model.UserId;
                        //invoiceTotalPlatformFees.InvoiceNumber = "INV-00103"; // ######
                        invoiceTotalPlatformFees.InvoiceDate = DateTime.Now;
                        invoiceTotalPlatformFees.TransactionType = AppConst.InvoiceTransactionType.INVOICE3_TOTAL_PLATFORM_FEES;
                        invoiceTotalPlatformFees.TotalAmount = Convert.ToString(clientAndFLfees);
                        invoiceTotalPlatformFees.InvoiceType = "Invoice";
                        invoiceTotalPlatformFees.ContractId = contractSave.Id;
                        _db.InvoiceList.Add(invoiceTotalPlatformFees);
                        _db.SaveChanges();

                        await UpdateInvoiceNumberAsync(invoiceTotalPlatformFees.Id, "PINV");

                        // Invoice 3 (Details 1) - Total platform fees for Milestone or Project
                        var invoiceTotalPlatformFeesDetail_total = new InvoiceListDetails();
                        invoiceTotalPlatformFeesDetail_total.InvoiceListId = invoiceTotalPlatformFees.Id;
                        invoiceTotalPlatformFeesDetail_total.Amount = invoiceTotalPlatformFees.TotalAmount;
                        invoiceTotalPlatformFeesDetail_total.Description = "Total platform fees for \"" + SolutionTitle + "\""; //######
                        _db.InvoiceListDetails.Add(invoiceTotalPlatformFeesDetail_total);
                        _db.SaveChanges();

                        // Invoice 3 (Details 2) - Total platform fees for Milestone or Project
                        var invoiceTotalPlatformFeesDetail_vat = new InvoiceListDetails();
                        invoiceTotalPlatformFeesDetail_vat.InvoiceListId = invoiceTotalPlatformFees.Id;
                        invoiceTotalPlatformFeesDetail_vat.Amount = "0";
                        invoiceTotalPlatformFeesDetail_vat.Description = "VAT (0%)";
                        _db.InvoiceListDetails.Add(invoiceTotalPlatformFeesDetail_vat);
                        _db.SaveChanges();
                        #endregion
                    }
                    catch (Exception ex)
                    {
                        invoiceErr = ex.Message;
                    }



                    return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                    {
                        StatusCode = StatusCodes.Status200OK,
                        Message = "Your payment has been received and securely held in escrow. Upon your approval of the deliverable, the funds will be disbursed to the Freelancers, with a designated commission retained by the Platform.Relavant Invoices will be generated after 2 days. " + invoiceErr,
                    });
                }

                return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                {
                    StatusCode = StatusCodes.Status200OK,
                    Message = "Data Not Found",
                });

            }
            catch (Exception ex)
            {

            }




            return StatusCode(StatusCodes.Status200OK, new APIResponseModel
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "User Not Found",
            });
        }

        //GetUserCancelCheckoutDetails
        [HttpPost]
        [Route("GetUserCancelCheckoutDetails")]
        public async Task<IActionResult> GetUserCancelCheckoutDetails([FromBody] MileStoneIdViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user != null)
            {
                //var contract = _context.Contracts.FirstOrDefault(x => x.Id.ToString() == cntId && x.ClientUserId == user.Id);
                var contract = _db.Contract.FirstOrDefault(x => x.Id == model.Id && x.ClientUserId == user.Id);

                if (contract != null && contract.PaymentStatus == Contract.PaymentStatuses.UnPaid)
                {
                    // If user decides to cancel payment and return to our site website

                    contract.PaymentStatus = Contract.PaymentStatuses.Cancelled;

                    return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                    {
                        StatusCode = StatusCodes.Status200OK,
                        Message = "Payment Cancel",
                    });
                }
            }

            return StatusCode(StatusCodes.Status200OK, new APIResponseModel
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "User Not Found",
            });
        }


        [HttpPost]
        [Route("SaveProjectInitiated")]
        public async Task<IActionResult> SaveProjectInitiated([FromBody] solutionFundViewModel model)
        {
            //FreelancerFinderHelper helper = new FreelancerFinderHelper();
            //await helper.FindFreelancersAsync(_db, model.ClientId, model.ProjectType, model.SolutionId, model.IndustryId, 0, 0, 0);
            try
            {
                if (model != null)
                {
                    if (model.GetNextMileStoneData)
                    {
                        var mileStoneData = await _db.SolutionMilestone.Where(x => x.SolutionId == model.SolutionId && x.IndustryId == model.IndustryId && x.ProjectType == model.ProjectType && x.Id > model.MileStoneId).FirstOrDefaultAsync();
                        var milestoneData = await _db.SolutionMilestone.Where(x => x.IndustryId == model.IndustryId && x.SolutionId == model.SolutionId && x.ProjectType == model.ProjectType).ToListAsync();
                        var checkType = _db.SolutionFund.Where(x => x.MileStoneId == model.MileStoneId).FirstOrDefault();
                        var Currency = await ConvertToCurrencySign(model.ClientPreferredCurrency);
                        List<MileStoneModel> milestoneList = new List<MileStoneModel>();
                        if (milestoneData.Count > 0)
                        {
                            var mileStoneToTalDays = milestoneData.Sum(x => x.Days);
                            foreach (var stonedata in milestoneData)
                            {
                                MileStoneModel milestonData = new MileStoneModel();
                                milestonData.Id = stonedata.Id;
                                milestonData.Days = stonedata.Days;
                                milestonData.Title = stonedata.Title;
                                milestonData.Description = stonedata.Description;
                                milestonData.MilestoneStatus = _db.ActiveSolutionMilestoneStatus.Where(x => x.MilestoneId == stonedata.Id && x.UserId == model.ClientId).Select(x => x.MilestoneStatus).FirstOrDefault();
                                if (mileStoneToTalDays > 0)
                                {
                                    milestonData.MilestonePrice = (Convert.ToDecimal(checkType.ProjectPrice) / mileStoneToTalDays) * stonedata.Days;
                                }
                                milestoneList.Add(milestonData);
                            }
                        }
                        if (mileStoneData != null)
                        {
                            var solutionfund = new SolutionFund()
                            {
                                SolutionId = model.SolutionId,
                                IndustryId = model.IndustryId,
                                ClientId = model.ClientId,
                                ProjectType = model.ProjectType.ToLower(),
                                ProjectPrice = checkType.ProjectPrice,
                                ProjectStatus = "INITIATED",
                                FundType = checkType.FundType,
                                MileStoneId = mileStoneData.Id
                            };
                            _db.SolutionFund.Add(solutionfund);
                            _db.SaveChanges();

                            List<SolutionTeam> solutionTeam = new List<SolutionTeam>();

                            var projectType = model.ProjectType.ToLower();
                            try
                            {
                                var Userslist = _db.Users.Where(x => x.UserType == "Freelancer" && x.RevolutStatus == true && !string.IsNullOrEmpty(x.RevolutConnectId)).ToList();
                                if (Userslist.Count > 0)
                                {
                                    if (projectType == AppConst.ProjectType.CUSTOM_PROJECT)
                                    {
                                        CustomProjectDetials? teamData = null;
                                        var solutionIndustryData = _db.SolutionIndustryDetails.Where(x => x.SolutionId == solutionfund.SolutionId && x.IndustryId == solutionfund.IndustryId).FirstOrDefault();
                                        if (solutionIndustryData != null)
                                        {
                                            var solutionDefineData = _db.SolutionDefine.Where(x => x.SolutionIndustryDetailsId == solutionIndustryData.Id && x.ProjectType == projectType).FirstOrDefault();
                                            if (solutionDefineData != null)
                                            {
                                                teamData = _db.CustomProjectDetials.Where(x => x.SolutionDefineId == solutionDefineData.Id).FirstOrDefault();
                                            }
                                        }
                                        var experttotal = Convert.ToInt32(teamData.Expert);
                                        var assosiatetotal = Convert.ToInt32(teamData.Associate);
                                        var projectmanagertotal = Convert.ToInt32(teamData.ProjectManager);
                                        await SaveSolutionTeamData(solutionfund, assosiatetotal, experttotal, projectmanagertotal);
                                    }

                                    else
                                    {
                                        await SaveSolutionTeamData(solutionfund, 0, 0, 0);
                                    }

                                }
                            }
                            catch (Exception ex)
                            {

                            }


                            var MilestoneTotalDaysByProjectType = _db.SolutionMilestone.Where(x => x.SolutionId == model.SolutionId && x.IndustryId == model.IndustryId && x.ProjectType == model.ProjectType).ToList();
                            decimal calculateProjectPrice = 0;
                            if (MilestoneTotalDaysByProjectType.Count > 0)
                            {
                                SolutionMilestone mileStoneToTalDays = MilestoneTotalDaysByProjectType
                               .GroupBy(l => l.ProjectType)
                               .Select(cl => new SolutionMilestone
                               {
                                   ProjectType = cl.First().ProjectType,
                                   Days = cl.Sum(c => c.Days),
                               }).FirstOrDefault();

                                if (mileStoneToTalDays.Days > 0)
                                {

                                    //var finalPrice = await CountFinalProjectPricing(solutionfund);
                                    calculateProjectPrice = (Convert.ToDecimal(solutionfund.ProjectPrice) / mileStoneToTalDays.Days) * mileStoneData.Days;
                                }
                            }

                            var data = _db.SolutionFund.Where(x => x.SolutionId == model.SolutionId && x.IndustryId == model.IndustryId && x.ProjectType == model.ProjectType && x.ClientId == model.ClientId && x.MileStoneId == mileStoneData.Id).FirstOrDefault();
                            var mileStone = _db.SolutionMilestone.Where(x => x.Id == mileStoneData.Id).FirstOrDefault();
                            var Funddecided = _db.SolutionFund.Where(x => x.SolutionId == model.SolutionId && x.IndustryId == model.IndustryId && x.ProjectType == model.ProjectType && x.ClientId == model.ClientId && x.IsCheckOutDone == true).Count();

                            return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                            {
                                StatusCode = StatusCodes.Status200OK,
                                Message = "projectDetails",
                                Result = new
                                {
                                    ProjectDetails = data,
                                    MileStoneData = mileStone,
                                    FundDecided = Funddecided,
                                    NextMileStonePrice = calculateProjectPrice,
                                    MilestoneList = milestoneList,
                                    Currency = Currency
                                }
                            });
                        }
                        else
                        {
                            return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                            {
                                StatusCode = StatusCodes.Status200OK,
                                Message = "NoFurtherMileStone",
                                Result = new
                                {
                                    MilestoneList = milestoneList,
                                    PreferredCurrency = Currency
                                }
                            });
                        }
                    }
                    if (model.Id == 0)
                    {
                        var mileStoneData = await _db.SolutionMilestone.Where(x => x.SolutionId == model.SolutionId && x.IndustryId == model.IndustryId && x.ProjectType == model.ProjectType).FirstOrDefaultAsync();

                        if (mileStoneData != null)
                        {
                            if (model.MileStoneCheckout)
                            {
                                model.FundType = SolutionFund.FundTypes.MilestoneFund;
                            }
                            else
                            {
                                model.FundType = SolutionFund.FundTypes.ProjectFund;
                            }

                            var solutionfund = new SolutionFund()
                            {
                                SolutionId = model.SolutionId,
                                IndustryId = model.IndustryId,
                                ClientId = model.ClientId,
                                ProjectType = model.ProjectType.ToLower(),
                                ProjectPrice = model.ProjectPrice,
                                ProjectStatus = "INITIATED",
                                FundType = model.FundType
                            };
                            _db.SolutionFund.Add(solutionfund);
                            _db.SaveChanges();


                            await SaveSolutionTeamData(solutionfund, 0, 0, 0);

                            var clientDetails = _db.Users.Where(x => x.Id == model.ClientId).FirstOrDefault();
                            if (clientDetails != null)
                            {
                                if (clientDetails.UserType == "Client")
                                {
                                    clientDetails.StartHours = DateTime.Parse(model.StartHour);
                                    clientDetails.EndHours = DateTime.Parse(model.EndHour);
                                    clientDetails.onMonday = model.onMonday;
                                    clientDetails.onThursday = model.onTuesday;
                                    clientDetails.onWednesday = model.onWednesday;
                                    clientDetails.onThursday = model.onThursday;
                                    clientDetails.onFriday = model.onFriday;
                                    clientDetails.onSaturday = model.onSaturday;
                                    clientDetails.onSunday = model.onSunday;
                                    _db.SaveChanges();
                                }
                            }

                            return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                            {
                                StatusCode = StatusCodes.Status200OK,
                                Message = "Project Initiated Successfully !",
                            });
                        }

                        else
                        {
                            return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                            {
                                StatusCode = StatusCodes.Status200OK,
                                Message = "EmptyMilestoneData",
                            });
                        }
                    }
                    else
                    {
                        var data = _db.SolutionFund.Where(x => x.SolutionId == model.SolutionId && x.IndustryId == model.IndustryId && x.ProjectType == model.ProjectType && x.ClientId == model.ClientId && x.ProjectStatus != "COMPLETED").FirstOrDefault();
                        if (data != null)
                        {
                            bool mileStoneCheckout = false;
                            if (model.MileStoneCheckout)
                            {
                                model.FundType = SolutionFund.FundTypes.MilestoneFund;
                                mileStoneCheckout = true;
                            }
                            else
                            {
                                model.FundType = SolutionFund.FundTypes.ProjectFund;
                            }

                            if (mileStoneCheckout)
                            {
                                List<SolutionTeam> solutionTeamsList = new List<SolutionTeam>();
                                var solutionTeamData = _db.SolutionTeam.Where(x => x.SolutionFundId == data.Id).ToList();
                                //
                                var clientPreferedCurrency = _db.Users.Where(x => x.Id == model.ClientId).FirstOrDefault().PreferredCurrency;
                                if (string.IsNullOrEmpty(clientPreferedCurrency))
                                {
                                    clientPreferedCurrency = "EUR";
                                }
                                //
                                if (solutionTeamData.Count > 0)
                                {
                                    if (data.ProjectType == AppConst.ProjectType.SMALL_PROJECT)
                                    {
                                        foreach (var item in solutionTeamData)
                                        {

                                            var singlemilestoneDay = _db.SolutionMilestone.Where(x => x.Id == model.MileStoneId).Select(x => x.Days).FirstOrDefault();
                                            var freelancerDetails = _db.FreelancerDetails.Where(x => x.UserId == item.FreelancerId).FirstOrDefault();
                                            // ######
                                            var freelancerPreferedCurrency = _db.Users.Where(x => x.Id == item.FreelancerId).FirstOrDefault().PreferredCurrency;
                                            if (string.IsNullOrEmpty(freelancerPreferedCurrency))
                                            {
                                                freelancerPreferedCurrency = "EUR";
                                            }
                                            var exchangeRate = _db.ExchangeRates.Where(x => x.FromCurrency == freelancerPreferedCurrency
                                                    && x.ToCurrency == clientPreferedCurrency).FirstOrDefault();
                                            var hourlyRate = Convert.ToDecimal(freelancerDetails.HourlyRate);
                                            decimal ExchangeHourlyRate = hourlyRate;
                                            if (exchangeRate != null)
                                            {
                                                ExchangeHourlyRate = Convert.ToDecimal((decimal)(hourlyRate * exchangeRate.Rate));
                                            }
                                            // ######
                                            decimal contractAmount = (singlemilestoneDay * 8 * ExchangeHourlyRate);
                                            var Platformfees = (contractAmount * AppConst.Commission.PLATFORM_COMM_FROM_FREELANCER_SMALL) / 100;
                                            item.Amount = contractAmount;
                                            item.PlatformFees = Platformfees;
                                            _db.SaveChanges();
                                        }


                                    }
                                    if (data.ProjectType == AppConst.ProjectType.MEDIUM_PROJECT)
                                    {
                                        if (solutionTeamData.Count > 0)
                                        {
                                            var singlemilestoneDay = _db.SolutionMilestone.Where(x => x.Id == model.MileStoneId).Select(x => x.Days).FirstOrDefault();
                                            foreach (var teamData in solutionTeamData)
                                            {
                                                var freelancerDetails = _db.FreelancerDetails.Where(x => x.UserId == teamData.FreelancerId).FirstOrDefault();
                                                // ######
                                                var freelancerPreferedCurrency = _db.Users.Where(x => x.Id == teamData.FreelancerId).FirstOrDefault().PreferredCurrency;
                                                if (string.IsNullOrEmpty(freelancerPreferedCurrency))
                                                {
                                                    freelancerPreferedCurrency = "EUR";
                                                }
                                                var exchangeRate = _db.ExchangeRates.Where(x => x.FromCurrency == freelancerPreferedCurrency
                                                        && x.ToCurrency == clientPreferedCurrency).FirstOrDefault();
                                                var hourlyRate = Convert.ToDecimal(freelancerDetails.HourlyRate);
                                                decimal ExchangeHourlyRate = hourlyRate;
                                                if (exchangeRate != null)
                                                {
                                                    ExchangeHourlyRate = Convert.ToDecimal((decimal)(hourlyRate * exchangeRate.Rate));
                                                }
                                                // ######
                                                decimal contractAmount = (singlemilestoneDay * 8 * ExchangeHourlyRate);
                                                var Platformfees = (contractAmount * AppConst.Commission.PLATFORM_COMM_FROM_FREELANCER_MEDIUM) / 100;
                                                teamData.Amount = contractAmount;
                                                teamData.PlatformFees = Platformfees;
                                                _db.SaveChanges();
                                            }
                                        }
                                    }
                                    if (data.ProjectType == AppConst.ProjectType.LARGE_PROJECT)
                                    {
                                        if (solutionTeamData.Count > 0)
                                        {
                                            var singlemilestoneDay = _db.SolutionMilestone.Where(x => x.Id == model.MileStoneId).Select(x => x.Days).FirstOrDefault();
                                            foreach (var teamData in solutionTeamData)
                                            {
                                                var freelancerDetails = _db.FreelancerDetails.Where(x => x.UserId == teamData.FreelancerId).FirstOrDefault();
                                                // ######
                                                var freelancerPreferedCurrency = _db.Users.Where(x => x.Id == teamData.FreelancerId).FirstOrDefault().PreferredCurrency;
                                                if (string.IsNullOrEmpty(freelancerPreferedCurrency))
                                                {
                                                    freelancerPreferedCurrency = "EUR";
                                                }
                                                var exchangeRate = _db.ExchangeRates.Where(x => x.FromCurrency == freelancerPreferedCurrency
                                                    && x.ToCurrency == clientPreferedCurrency).FirstOrDefault();
                                                var hourlyRate = Convert.ToDecimal(freelancerDetails.HourlyRate);
                                                decimal ExchangeHourlyRate = hourlyRate;
                                                if (exchangeRate != null)
                                                {
                                                    ExchangeHourlyRate = Convert.ToDecimal((decimal)(hourlyRate * exchangeRate.Rate));
                                                }
                                                // ######
                                                var contractAmount = singlemilestoneDay * 8 * ExchangeHourlyRate;
                                                var PlatformFees = (contractAmount * AppConst.Commission.PLATFORM_COMM_FROM_FREELANCER_LARGE) / 100;
                                                teamData.Amount = contractAmount;
                                                teamData.PlatformFees = PlatformFees;
                                                _db.SaveChanges();
                                            }
                                        }
                                    }
                                    if (data.ProjectType == AppConst.ProjectType.CUSTOM_PROJECT)
                                    {
                                        if (solutionTeamData.Count > 0)
                                        {
                                            var singlemilestoneDay = _db.SolutionMilestone.Where(x => x.Id == model.MileStoneId).Select(x => x.Days).FirstOrDefault();
                                            foreach (var teamData in solutionTeamData)
                                            {
                                                var freelancerDetails = _db.FreelancerDetails.Where(x => x.UserId == teamData.FreelancerId).FirstOrDefault();
                                                // ######
                                                var freelancerPreferedCurrency = _db.Users.Where(x => x.Id == teamData.FreelancerId).FirstOrDefault().PreferredCurrency;
                                                if (string.IsNullOrEmpty(freelancerPreferedCurrency))
                                                {
                                                    freelancerPreferedCurrency = "EUR";
                                                }
                                                var exchangeRate = _db.ExchangeRates.Where(x => x.FromCurrency == freelancerPreferedCurrency
                                                        && x.ToCurrency == clientPreferedCurrency).FirstOrDefault();
                                                var hourlyRate = Convert.ToDecimal(freelancerDetails.HourlyRate);
                                                decimal ExchangeHourlyRate = hourlyRate;
                                                if (exchangeRate != null)
                                                {
                                                    ExchangeHourlyRate = Convert.ToDecimal((decimal)(hourlyRate * exchangeRate.Rate));
                                                }
                                                // ######
                                                decimal contractAmount = (singlemilestoneDay * 8 * ExchangeHourlyRate);
                                                var Platformfees = (contractAmount * AppConst.Commission.PLATFORM_COMM_FROM_FREELANCER_CUSTOM) / 100;
                                                teamData.Amount = contractAmount;
                                                teamData.PlatformFees = Platformfees;
                                                _db.SaveChanges();
                                            }
                                        }
                                    }
                                }
                            }

                            if (data.ProjectStatus == "INITIATED")
                            {
                                data.ProjectStatus = "INPROGRESS";
                                data.MileStoneId = model.MileStoneId;
                                data.FundType = model.FundType;
                                _db.SaveChanges();

                                var mileStoneData = _db.SolutionMilestone.Where(x => x.Id == model.MileStoneId).FirstOrDefault();

                                var getRevoultToken = await CheckOutUsingRevoult(data);
                                return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                                {
                                    StatusCode = StatusCodes.Status200OK,
                                    Message = "CompleteProcess",
                                    Result = new
                                    {
                                        ProjectDetails = data,
                                        MileStoneData = mileStoneData,
                                        RevoultToken = getRevoultToken,
                                        SolutionFundId = data.Id
                                    }
                                });
                            }
                            if (data.ProjectStatus == "INPROGRESS")
                            {
                                //data.ProjectStatus = "COMPLETED";
                                //_db.SaveChanges();
                                var getRevoultToken = await CheckOutUsingRevoult(data);
                                var mileStoneData = _db.SolutionMilestone.Where(x => x.Id == model.MileStoneId).FirstOrDefault();
                                return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                                {
                                    StatusCode = StatusCodes.Status200OK,
                                    Message = "CompleteProcess",
                                    Result = new
                                    {
                                        ProjectDetails = data,
                                        MileStoneData = mileStoneData,
                                        RevoultToken = getRevoultToken,
                                        SolutionFundId = data.Id
                                    }
                                });
                            }
                        }

                        var completedData = _db.SolutionFund.Where(x => x.Id == model.Id && x.ProjectStatus == "COMPLETED").FirstOrDefault();
                        if (completedData != null)
                        {
                            if (completedData.ProjectStatus == "COMPLETED")
                            {
                                var SolutionTitle = string.Empty;
                                var contract = _db.Contract.Where(x => x.SolutionFundId == model.Id).Include("ContractUsers").FirstOrDefault();

                                if (completedData.FundType == SolutionFund.FundTypes.MilestoneFund)
                                {
                                    SolutionTitle = _db.SolutionMilestone.Where(x => x.Id == completedData.MileStoneId).Select(x => x.Title).FirstOrDefault();
                                }
                                else
                                {
                                    SolutionTitle = _db.Solutions.Where(x => x.Id == completedData.SolutionId).Select(x => x.Title).FirstOrDefault();
                                }

                                if (contract != null)
                                {
                                    var mileStone = _db.SolutionMilestone.FirstOrDefault(x => x.Id == contract.MilestoneDataId);
                                    //var checkoutSession = _stripeAccountService.GetCheckOutSesssion(contract.SessionId);

                                    var allAccounts = await _revoultService.RetrieveAllAccounts();
                                    double totalAmt = 0;
                                    foreach (var contractUser in contract.ContractUsers.Where(x => !x.IsTransfered))
                                    {
                                        var user = _db.Users.FirstOrDefault(x => x.Id == contractUser.ApplicationUserId);

                                        if (user != null && user.RevolutStatus == true)
                                        {
                                            //var paymentIntent = _stripeAccountService.GetPaymentIntent(contract.PaymentIntentId);
                                            if (user.PreferredCurrency == null)
                                            {
                                                user.PreferredCurrency = "EUR";
                                            }
                                            double priceToTransfer = 0;
                                            var teamMember = _db.SolutionTeam.Where(m => m.FreelancerId == contractUser.ApplicationUserId && m.SolutionFundId == model.Id).FirstOrDefault();
                                            //var priceToTransfer = (long)(Convert.ToDecimal(contractUser.Percentage) / 100 * 200 * 100);
                                            priceToTransfer = (long)(Convert.ToDecimal(teamMember?.Amount));
                                            //var getExchangeRate = _db.ExchangeRates.Where(q => q.FromCurrency == model.ClientPreferredCurrency && q.ToCurrency == user.PreferredCurrency).FirstOrDefault();
                                            //if (getExchangeRate == null)
                                            //{
                                            //    priceToTransfer = (long)(Convert.ToDecimal(teamMember?.Amount));
                                            //}
                                            //else
                                            //{
                                            //    priceToTransfer = (Convert.ToDecimal(teamMember?.Amount)) * (Convert.ToDecimal(getExchangeRate.Rate));
                                            //}
                                            var getCounterParties = await _revoultService.GetCounterparties();
                                            // When Pay Amount button click
                                            totalAmt += (double)priceToTransfer;

                                            CreatePaymentReq createPaymentReq = new CreatePaymentReq
                                            {
                                                AccountId = allAccounts.Where(x => x.Currency == model.ClientPreferredCurrency
                                                && x.Balance >= (double)totalAmt).Select(x => x.Id).FirstOrDefault(), // ##### Freelancer PreferredCurrency
                                                RequestId = Guid.NewGuid().ToString(),
                                                Amount = (double)priceToTransfer, // ##### actual amount - solutionteam.transferamount 
                                                Currency = model.ClientPreferredCurrency,
                                                Reference = "Payment For- " + SolutionTitle,
                                                Receiver = new CreatePaymentReq.ReceiverData()
                                                {
                                                    CounterpartyId = user.RevolutConnectId, // freelancer RevolutConnectId
                                                    AccountId = user.RevolutAccountId // freelancer RevolutAccountId
                                                }
                                            };

                                            var CreatePaymentRsp = await _revoultService.CreatePayment(createPaymentReq);
                                            //CreatePaymentRsp.State = "completed";
                                            //Possible values: [created, pending, completed, declined, failed, reverted]
                                            if (CreatePaymentRsp.State == "completed" || CreatePaymentRsp.State == "pending")
                                            {
                                                var TranscationFeesDetails = await _revoultService.GetTranscationFeesDetails(CreatePaymentRsp.Id);
                                                if (TranscationFeesDetails.IsSuccessStatusCode)
                                                {
                                                    decimal paymentFee = 0;
                                                    var content = TranscationFeesDetails.Content;
                                                    dynamic parseContent = JObject.Parse(content);
                                                    JArray legsArray = (JArray)parseContent["legs"];
                                                    foreach (JObject item in legsArray)
                                                    {
                                                        paymentFee = (decimal)item["fee"];
                                                    }
                                                    contractUser.PaymentFees = paymentFee.ToString();
                                                }
                                                contractUser.StripeTranferId = CreatePaymentRsp.Id;
                                                contractUser.IsTransfered = true;
                                                contractUser.PaymentAmount = priceToTransfer.ToString();
                                                contractUser.TransferDateTime = DateTime.Now;
                                                _db.ContractUser.Update(contractUser);
                                                _db.SaveChanges();




                                            }
                                            else
                                            {
                                                //Please check the status and place logic accordingly.
                                            }

                                            //if (teamMember.IsProjectManager)
                                            //{
                                            //    // ########## same logic as above - but it will update PaymentAmountProjectMgr and PaymentFeesProjectMgr in Contractuser table

                                            //    decimal pmpriceToTransfer = 0;
                                            //    var pmExchangeRate = _db.ExchangeRates.Where(q => q.FromCurrency == model.ClientPreferredCurrency && q.ToCurrency == user.PreferredCurrency).FirstOrDefault();
                                            //    if (pmExchangeRate == null)
                                            //    {
                                            //        pmpriceToTransfer = (Convert.ToDecimal(teamMember?.ProjectManagerPlatformFees));
                                            //    }
                                            //    else
                                            //    {
                                            //        pmpriceToTransfer = (Convert.ToDecimal(teamMember?.ProjectManagerPlatformFees)) * (Convert.ToDecimal(pmExchangeRate.Rate));
                                            //    }
                                            //    var getpmCounterParties = await _revoultService.GetCounterparties();

                                            //    CreatePaymentReq createpmPaymentReq = new CreatePaymentReq
                                            //    {
                                            //        AccountId = allAccounts.Where(x => x.Currency == user.PreferredCurrency).Select(x => x.Id).FirstOrDefault(), // ##### Freelancer PreferredCurrency
                                            //        RequestId = Guid.NewGuid().ToString(),
                                            //        Amount = 2, // ##### actual amount  Solutionteam.ProjectManagerPlatformFees
                                            //        Currency = user.PreferredCurrency,
                                            //        Reference = "Payment For- " + SolutionTitle,
                                            //        Receiver = new CreatePaymentReq.ReceiverData()
                                            //        {
                                            //            CounterpartyId = user.RevolutConnectId, // freelancer RevolutConnectId
                                            //            AccountId = user.RevolutAccountId // freelancer RevolutAccountId
                                            //        }
                                            //    };

                                            //    var CreatepmPaymentRsp = await _revoultService.CreatePayment(createpmPaymentReq);
                                            //    //Possible values: [created, pending, completed, declined, failed, reverted]
                                            //    if (CreatepmPaymentRsp.State == "completed" || CreatepmPaymentRsp.State == "pending")
                                            //    {
                                            //        var TranscationFeesDetails = await _revoultService.GetTranscationFeesDetails(CreatePaymentRsp.Id);
                                            //        if (TranscationFeesDetails.IsSuccessStatusCode)
                                            //        {
                                            //            decimal paymentFee = 0;
                                            //            var content = TranscationFeesDetails.Content;
                                            //            dynamic parseContent = JObject.Parse(content);
                                            //            JArray legsArray = (JArray)parseContent["legs"];
                                            //            foreach (JObject item in legsArray)
                                            //            {
                                            //                paymentFee = (decimal)item["fee"];
                                            //            }
                                            //            contractUser.PaymentFeesProjectMgr = paymentFee;
                                            //        }
                                            //        contractUser.PaymentAmountProjectMgr = pmpriceToTransfer;
                                            //        _db.ContractUser.Update(contractUser);
                                            //        _db.SaveChanges();
                                            //    }


                                            //    ////
                                            //}
                                        }
                                        else
                                        {
                                            // this User(freelabncer or architect) has not completed his Stripe account please 
                                        }
                                    }

                                    // Ephylinklog section
                                    var ephylinkaccountDetails = _db.EphylinkRevolutAccount.FirstOrDefault();
                                    if (ephylinkaccountDetails != null)
                                    {
                                        if (ephylinkaccountDetails.IsEnable)
                                        {
                                            var fullTeam = await _db.SolutionTeam.Where(x => x.SolutionFundId == contract.SolutionFundId).ToListAsync();
                                            decimal totalamountTotransfer = 0;
                                            decimal clientFees = 0;
                                            decimal revolutFees = contract.RevolutFee;
                                            decimal platfomrFromClient = 0; // I8 - J8
                                                                            // I8 = clientFees = fullTeam.Sum(y => y.Amount) * const / 100;
                                                                            // J8 = (revolutFees * clientFees) / solutionTeamSum;
                                            if (completedData.ProjectType == AppConst.ProjectType.LARGE_PROJECT)
                                            {
                                                clientFees = (fullTeam.Sum(y => y.Amount) * AppConst.Commission.PLATFORM_COMM_FROM_CLIENT_LARGE) / 100;
                                            }
                                            else if (completedData.ProjectType == AppConst.ProjectType.SMALL_PROJECT)
                                            {
                                                clientFees = (fullTeam.Sum(y => y.Amount) * AppConst.Commission.PLATFORM_COMM_FROM_CLIENT_SMALL) / 100;
                                            }
                                            else if (completedData.ProjectType == AppConst.ProjectType.MEDIUM_PROJECT)
                                            {
                                                clientFees = (fullTeam.Sum(y => y.Amount) * AppConst.Commission.PLATFORM_COMM_FROM_CLIENT_MEDIUM) / 100;
                                            }
                                            else if (completedData.ProjectType == AppConst.ProjectType.CUSTOM_PROJECT)
                                            {
                                                if (fullTeam.Count <= 3)
                                                {
                                                    clientFees = (fullTeam.Sum(y => y.Amount) * AppConst.Commission.PLATFORM_COMM_FROM_CLIENT_CUSTOM_LESS_THAN_THREE_GIGS) / 100;
                                                }
                                                else
                                                {
                                                    clientFees = (fullTeam.Sum(y => y.Amount) * AppConst.Commission.PLATFORM_COMM_FROM_CLIENT_CUSTOM) / 100;
                                                }
                                            }
                                            var solutionTeamSum = fullTeam.Sum(x => x.Amount)
                                                                //+ fullTeam.Sum(y => y.PlatformFees) // not include because x.Amount already includes this
                                                                + clientFees;
                                            decimal j8 = (revolutFees * clientFees) / solutionTeamSum;
                                            platfomrFromClient = clientFees - j8;
                                            totalamountTotransfer += platfomrFromClient;
                                            foreach (var teamMember in fullTeam)
                                            {
                                                decimal checkoutRevolutFees = (revolutFees * teamMember.PlatformFees) / solutionTeamSum;
                                                decimal amountToBeTransfered = teamMember.PlatformFees - checkoutRevolutFees;
                                                totalamountTotransfer += amountToBeTransfered;
                                            }
                                            CreatePaymentReq createephylinkPaymentReq = new CreatePaymentReq
                                            {
                                                AccountId = allAccounts.Where(x => x.Currency == ephylinkaccountDetails.Currency
                                                && x.Balance >= (double)totalamountTotransfer).Select(x => x.Id).FirstOrDefault(),
                                                RequestId = Guid.NewGuid().ToString(),
                                                Amount = (double)totalamountTotransfer,
                                                Currency = ephylinkaccountDetails.Currency,
                                                Reference = "Payment to Ephylink",
                                                Receiver = new CreatePaymentReq.ReceiverData()
                                                {
                                                    CounterpartyId = ephylinkaccountDetails.RevolutConnectId,
                                                    AccountId = ephylinkaccountDetails.RevolutAccountId
                                                }
                                            };

                                            var CreateephylinkPaymentRsp = await _revoultService.CreatePayment(createephylinkPaymentReq);
                                            if (CreateephylinkPaymentRsp.State == "completed" || CreateephylinkPaymentRsp.State == "pending")
                                            {
                                                var ephylinkFeesDetails = await _revoultService.GetTranscationFeesDetails(CreateephylinkPaymentRsp.Id);
                                                if (ephylinkFeesDetails.IsSuccessStatusCode)
                                                {
                                                    decimal paymentFee = 0;
                                                    var content = ephylinkFeesDetails.Content;
                                                    dynamic parseContent = JObject.Parse(content);
                                                    JArray legsArray = (JArray)parseContent["legs"];
                                                    foreach (JObject item in legsArray)
                                                    {
                                                        paymentFee = (decimal)item["fee"];
                                                    }

                                                    var accountLog = new EphylinkRevolutAccountTransferLog()
                                                    {
                                                        ContractId = contract.Id,
                                                        TransferAmount = totalamountTotransfer,
                                                        RevoultFee = paymentFee,
                                                        TransferDateTime = DateTime.Now
                                                    };
                                                    _db.EphylinkRevolutAccountTransferLog.Add(accountLog);
                                                    _db.SaveChanges();

                                                }
                                            }

                                        }
                                    }


                                    if (completedData.FundType == SolutionFund.FundTypes.MilestoneFund)
                                    {
                                        var updatemilestonestatus = _db.ActiveSolutionMilestoneStatus.Where(x => x.MilestoneId == contract.MilestoneDataId && x.UserId == completedData.ClientId).FirstOrDefault();
                                        if (updatemilestonestatus != null)
                                        {
                                            updatemilestonestatus.MilestoneStatus = "Milestone Completed";
                                            _db.SaveChanges();
                                        }
                                    }

                                    // 4 Invoice Generate 
                                    try
                                    {
                                        await GenerateInvoiceAfterPayAmount(contract, SolutionTitle, completedData.ProjectType);
                                    }
                                    catch (Exception exInvoice)
                                    {

                                        //#####
                                    }


                                    var transferredcount = contract.ContractUsers.Where(x => x.IsTransfered).Count();
                                    var transfertoFreelancer = false;
                                    if (transferredcount == contract.ContractUsers.Count())
                                    {
                                        contract.PaymentStatus = Contract.PaymentStatuses.Splitted;
                                        _db.Contract.Update(contract);
                                        _db.SaveChanges();
                                        var message = string.Format("Amount is transferred to all {0} users(freelancers) and its status is splitted Now.", transferredcount);
                                        transfertoFreelancer = true;
                                        return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                                        {
                                            StatusCode = StatusCodes.Status200OK,
                                            Message = message,
                                            Result = new
                                            {
                                                IsTransfer = transfertoFreelancer,
                                                FundType = completedData.FundType
                                            }

                                        });
                                    }
                                    else if (transferredcount > 0)
                                    {
                                        contract.PaymentStatus = Contract.PaymentStatuses.PartiallySplitted;
                                        _db.Contract.Update(contract);
                                        _db.SaveChanges();
                                        var message = string.Format("Amount is transferred to {0} users(freelancers) and its status is Partially Splitted Now. Please press transfer again and make sure all users are onboard(revoult)", transferredcount);
                                        transfertoFreelancer = true;
                                        return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                                        {
                                            StatusCode = StatusCodes.Status200OK,
                                            Message = message,
                                            Result = new
                                            {
                                                IsTransfer = transfertoFreelancer
                                            }

                                        });
                                    }
                                    else
                                    {
                                        var message = string.Format("Amount is transferred to {0} users(freelancers) and its status is not changed from previous. Please press transfer again and make sure all users(freelancers) are onboard(revoult)", transferredcount);
                                        return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                                        {
                                            StatusCode = StatusCodes.Status200OK,
                                            Message = message,
                                            Result = new
                                            {
                                                IsTransfer = transfertoFreelancer
                                            }

                                        });
                                    }



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
            }
            catch(Exception ex)
            {
                return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                {
                    StatusCode = StatusCodes.Status200OK,
                    Message = ex.Message + ex.InnerException
                });
            }
            

            return StatusCode(StatusCodes.Status200OK, new APIResponseModel
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "Data Not Found",
            });
        }

        [HttpPost]
        [Route("saveClientAvailabilityData")]
        public async Task<IActionResult> saveClientAvailabilityData([FromBody] ClientAvailabilityModel model)
        {
            if (model != null)
            {
                if (!string.IsNullOrEmpty(model.ClientId))
                {
                    var checkUserExistance = _db.CustomSolutions.Where(x => x.ClientId == model.ClientId && x.SolutionId == model.SolutionId && x.IndustryId == model.IndustryId).FirstOrDefault();
                    if (checkUserExistance != null)
                    {
                        return StatusCode(StatusCodes.Status200OK, new APIResponseModel { StatusCode = StatusCodes.Status403Forbidden, Message = "Already Exists !!" });
                    }
                    DateTime[] holidays = model.Holidays;
                    DateTime firstDay = Convert.ToDateTime(model.StartDate);
                    DateTime lastDay = Convert.ToDateTime(model.EndDate);
                    var list = new List<ClientAvailability>();
                    int daysCount = 0;
                    if (model.isExcludeWeekends == true)
                    {
                        for (DateTime date = firstDay; date <= lastDay; date = date.AddDays(1))
                        {
                            if (firstDay.DayOfWeek != DayOfWeek.Saturday && firstDay.DayOfWeek != DayOfWeek.Sunday && !holidays.Contains(date)) // && !bankHolidays.Contains(date)
                            {
                                ClientAvailability obj = new ClientAvailability
                                {
                                    ClientId = model.ClientId,
                                    AvailableDate = firstDay,
                                    SolutionId = model.SolutionId,
                                    IndustryId = model.IndustryId
                                };
                                list.Add(obj);
                                daysCount++;
                            }
                            firstDay = firstDay.AddDays(1);
                        }
                        if (!list.IsNullOrEmpty())
                        {
                            await _db.ClientAvailability.AddRangeAsync(list);
                            await _db.SaveChangesAsync();
                        }
                        return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                        {
                            StatusCode = StatusCodes.Status200OK,
                            Message = "Data saved successfully",
                            Result = daysCount
                        });
                    }
                    else
                    {
                        for (DateTime date = firstDay; date <= lastDay; date = date.AddDays(1))
                        {
                            if (!holidays.Contains(date)) // && !bankHolidays.Contains(date)
                            {
                                ClientAvailability obj = new ClientAvailability
                                {
                                    ClientId = model.ClientId,
                                    AvailableDate = firstDay,
                                    SolutionId = model.SolutionId,
                                    IndustryId = model.IndustryId
                                };
                                list.Add(obj);
                                daysCount++;
                            }
                            firstDay = firstDay.AddDays(1);
                        }
                        if (!list.IsNullOrEmpty())
                        {
                            await _db.ClientAvailability.AddRangeAsync(list);
                            await _db.SaveChangesAsync();
                        }
                        return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                        {
                            StatusCode = StatusCodes.Status200OK,
                            Message = "Data saved successfully",
                            Result = daysCount
                        });
                    }
                }
                else
                {
                    return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                    {
                        StatusCode = StatusCodes.Status200OK,
                        Message = "Please signUp as 'Client' !!"
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
        [Route("SaveRequestedProposal")]
        public async Task<IActionResult> SaveRequestedProposal([FromBody] CustomSolutionsModel model)
        {
            try
            {
                if (model.ClientId == null || model.ClientId == "")
                {
                    return StatusCode(StatusCodes.Status200OK, new APIResponseModel { StatusCode = StatusCodes.Status403Forbidden, Message = "Please signUp as 'Client' !!" });
                }
                var checkUserExistance = _db.CustomSolutions.Where(x => x.ClientId == model.ClientId && x.SolutionId == model.SolutionId && x.ServiceId == model.ServiceId && x.IndustryId == model.IndustryId).FirstOrDefault();
                if (checkUserExistance != null)
                {
                    return StatusCode(StatusCodes.Status200OK, new APIResponseModel { StatusCode = StatusCodes.Status403Forbidden, Message = "You already have submitted custom project request !" });
                }
                var customSolution_data = new CustomSolutions()
                {
                    ClientId = model.ClientId,
                    ServiceId = model.ServiceId,
                    SolutionId = model.SolutionId,
                    IndustryId = model.IndustryId,
                    SolutionTitle = model.SolutionTitle,
                    SoultionDescription = model.SoultionDescription,
                    DeliveryTime = model.DeliveryTime,
                    Budget = Convert.ToDecimal(model.Budget),
                    StartDate = model.StartDate,
                    EndDate = model.EndDate
                };

                await _db.CustomSolutions.AddAsync(customSolution_data);
                var result = _db.SaveChanges();
                if (result != 0)
                {
                    return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                    {
                        StatusCode = StatusCodes.Status200OK,
                        Message = "Submitted Successfully",
                        Result = customSolution_data.ID
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
        [Route("UpdateSolutionDocument")]
        public async Task<IActionResult> UpdateSolutionDocument([FromBody] CustomSolutionDocument model)
        {
            if (model.AlreadyExistDocument)
            {
                var applicantsDetails = _db.CustomSolutions.Where(x => x.ID == model.ID).FirstOrDefault();
                if (applicantsDetails != null)
                    return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                    {
                        StatusCode = StatusCodes.Status200OK,
                        Message = "Your request submitted Successfully !"
                    });
            }
            else
            {
                if (model.ID != 0)
                {
                    var UpdateDocument = _db.CustomSolutions.Where(x => x.ID == model.ID).FirstOrDefault();
                    if (UpdateDocument != null)
                    {

                        UpdateDocument.BlobStorageBaseUrl = model.BlobStorageBaseUrl;
                        UpdateDocument.DocumentPath = model.DocumentPath;
                        UpdateDocument.DocumentUrlWithSas = model.DocumentUrlWithSas;
                        _db.SaveChanges();

                        return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                        {
                            StatusCode = StatusCodes.Status200OK,
                            Message = "Your request submitted Successfully !"
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
        [Route("RaiseDispute")]
        public async Task<IActionResult> RaiseDispute([FromBody] solutionFundViewModel model)
        {
            if (model != null)
            {
                try
                {
                    var solutionfundId = await _db.SolutionFund.Where(x => x.SolutionId == model.SolutionId && x.IndustryId == model.IndustryId && x.ClientId == model.ClientId && x.ProjectType == model.ProjectType).FirstOrDefaultAsync();
                    if (solutionfundId.Id != 0)
                    {
                        var contractId = await _db.Contract.Where(x => x.SolutionFundId == solutionfundId.Id).Select(x => x.Id).FirstOrDefaultAsync();
                        if (contractId != 0)
                        {
                            var checkDisputeData = _db.SolutionDispute.Where(x => x.ContractId == contractId).FirstOrDefault();
                            if (checkDisputeData != null)
                            {
                                return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                                {
                                    StatusCode = StatusCodes.Status200OK,
                                    Message = "Dispute is already Raised !!",
                                });
                            }
                            var data = new SolutionDispute()
                            {
                                ContractId = contractId,
                                Status = "NOT RESOLVED",
                                CreatedDateTime = DateTime.Now
                            };

                            _db.SolutionDispute.Add(data);
                            _db.SaveChanges();

                            SolutionDisputeViewModel solutionDisputeView = new SolutionDisputeViewModel();
                            solutionDisputeView.ContractId = contractId;
                            solutionDisputeView.SolutionName = _db.Solutions.Where(x => x.Id == solutionfundId.SolutionId).Select(x => x.Title).FirstOrDefault();
                            solutionDisputeView.IndustryName = _db.Industries.Where(x => x.Id == solutionfundId.IndustryId).Select(x => x.IndustryName).FirstOrDefault();
                            var fullname = _db.Users.Where(x => x.Id == model.ClientId).Select(x => new { x.FirstName, x.LastName }).FirstOrDefault();
                            solutionDisputeView.ClientName = fullname.FirstName + " " + fullname.LastName;
                            solutionDisputeView.AdminEmailId = _db.Users.Where(x => x.UserType == "Admin").Select(x => x.UserName).FirstOrDefault();

                            solutionfundId.IsDispute = true;
                            _db.SaveChanges();

                            return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                            {
                                StatusCode = StatusCodes.Status200OK,
                                Message = "Dispute Raised",
                                Result = solutionDisputeView
                            });
                        }
                    }
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
        [Route("SaveActiveProjectDocumentsDetails")]
        public async Task<IActionResult> SaveActiveProjectDocumentsDetails([FromBody] ActiveProjectDocuments model)
        {
            if (model != null)
            {
                try
                {
                    if (model.Id == 0)
                    {
                        var DocumentData = new ActiveProjectDocuments()
                        {
                            SolutionFundId = model.SolutionFundId,
                            Description = model.Description,
                            ClientId = model.ClientId,
                            CreatedDateTime = DateTime.Now
                        };
                        await _db.ActiveProjectDocuments.AddAsync(DocumentData);
                        await _db.SaveChangesAsync();

                        return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                        {
                            StatusCode = StatusCodes.Status200OK,
                            Message = "Save Successfully!",
                            Result = DocumentData.Id
                        });
                    }
                    else
                    {
                        var DocumentDetails = await _db.ActiveProjectDocuments.Where(x => x.Id == model.Id).FirstOrDefaultAsync();
                        if (DocumentDetails != null)
                        {
                            DocumentDetails.DocumentName = model.DocumentName;
                            DocumentDetails.DocumentBlobStorageBaseUrl = model.DocumentBlobStorageBaseUrl;
                            DocumentDetails.DocumentPath = model.DocumentPath;
                            DocumentDetails.DocumentUrlWithSas = model.DocumentUrlWithSas;
                            _db.SaveChanges();
                            return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                            {
                                StatusCode = StatusCodes.Status200OK,
                                Message = "File Uploaded Successfully!",
                            });
                        }
                    }
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
        [Route("filterSolutionBySolutionName")]
        public async Task<IActionResult> filterSolutionBySolutionName([FromBody] MileStoneIdViewModel model)
        {
            if (model.SolutionName != "" || model.SolutionName != null)
            {
                try
                {
                    var CheckType = _db.Users.Where(x => x.Id == model.UserId).Select(x => x.UserType).FirstOrDefault();
                    List<Solutions> solutionList = _db.Solutions.ToList();
                    List<SolutionsModel> solutionsModel = new List<SolutionsModel>();
                    List<Industries> industrylistDetails = new List<Industries>();
                    List<string> industrylist = new List<string>();
                    List<int> industryIdlist = new List<int>();
                    bool IsSavedProject = false;

                    var filteredSolutions = _db.Solutions.Where(s => s.Title.Contains(model.SolutionName) || s.SubTitle.Contains(model.SolutionName) || s.Description.Contains(model.SolutionName)).ToList();

                    if (filteredSolutions.Count > 0)
                    {
                        foreach (var list in filteredSolutions)
                        {
                            var serviceId = _db.SolutionServices.Where(x => x.SolutionId == list.Id).Select(x => x.ServicesId).FirstOrDefault();
                            var Servicename = _db.Services.Where(x => x.Id == serviceId).Select(x => x.ServicesName).FirstOrDefault();
                            if (model.UserId != "")
                            {
                                var SavedProjectData = _db.SavedProjects.Where(x => x.SolutionId == list.Id && x.UserId == model.UserId).FirstOrDefault();
                                if (SavedProjectData != null)
                                {
                                    IsSavedProject = true;
                                }
                                else
                                {
                                    IsSavedProject = false;
                                }
                            }
                            if (model.UserId != "" && CheckType != "Client")
                            {
                                industryIdlist = _db.SolutionIndustryDetails.Where(x => x.SolutionId == list.Id && x.IsActiveForFreelancer == true).Select(x => x.IndustryId).ToList();
                            }
                            else
                            {
                                industryIdlist = _db.SolutionIndustryDetails.Where(x => x.SolutionId == list.Id && x.IsActiveForClient == true).Select(x => x.IndustryId).ToList();
                            }
                            //var industryIdlist = _db.SolutionIndustryDetails.Where(x => x.SolutionId == list.Id && x.IsActiveForClient == true).Select(x => x.IndustryId).ToList();
                            if (industryIdlist.Count > 0)
                            {
                                foreach (var industryId in industryIdlist)
                                {
                                    var industry = _db.Industries.Where(x => x.Id == industryId).FirstOrDefault();
                                    industrylistDetails.Add(industry);
                                    var industryname = _db.Industries.Where(x => x.Id == industryId).Select(x => x.IndustryName).FirstOrDefault();
                                    industrylist.Add(industryname);
                                }
                                SolutionsModel dataStore = new SolutionsModel();
                                dataStore.IsProjectSaved = IsSavedProject;
                                dataStore.Services = Servicename;
                                dataStore.solutionServices = serviceId;
                                dataStore.Industries = string.Join(",", industrylist);
                                dataStore.Id = list.Id;
                                dataStore.Description = list.Description;
                                dataStore.ImagePath = list.ImagePath;
                                dataStore.ImageUrlWithSas = list.ImageUrlWithSas;
                                dataStore.Title = list.Title;
                                dataStore.SubTitle = list.SubTitle;
                                solutionsModel.Add(dataStore);
                                industrylist.Clear();
                            }

                        }
                    }
                    float totalSolutions = solutionsModel.Count();
                    double pagesCount = 0;
                    if (totalSolutions > 0)
                    {
                        double val = Convert.ToDouble((float)totalSolutions / 6);
                        pagesCount = Math.Ceiling(val);

                    }
                    List<SolutionsModel> mainlist = solutionsModel.Take(6).ToList();
                    if (model.pageNumber > 1 && model.pageNumber != null)
                    {
                        var prevPage = (int)model.pageNumber - 1;
                        var current = (int)model.pageNumber;
                        int start = (prevPage * 6) - 1;
                        int end = (current * 6) - 1;
                        int indexOfLastElement = (solutionsModel.Count) - 1;
                        mainlist = solutionsModel.Where((value, index) => index > start && index <= end).ToList();
                    }
                    return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                    {
                        StatusCode = StatusCodes.Status200OK,
                        Message = "Success",
                        Result = new
                        {
                            SolutionData = mainlist,
                            SolutionBindData = solutionsModel,
                            IndustriesData = industrylistDetails.Distinct(),
                            TotalCount = solutionsModel.Count(),
                            PageCount = pagesCount
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
                StatusCode = StatusCodes.Status403Forbidden,
                Message = "Something Went Wrong !!"

            });

        }

        //GetSolutionIndustry
        [HttpPost]
        [Route("GetSolutionIndustry")]
        public async Task<IActionResult> GetSolutionIndustry([FromBody] MileStoneDetailsViewModel model)
        {
            if (model != null)
            {
                try
                {
                    List<SolutionsModel> industryList = new List<SolutionsModel>();
                    var IndustryList = await _db.SolutionIndustryDetails.Where(x => x.SolutionId == model.SolutionId && x.IsActiveForClient == true).ToListAsync();
                    if (IndustryList.Count > 0)
                    {
                        foreach (var data in IndustryList)
                        {
                            SolutionsModel solutionsModel = new SolutionsModel();
                            solutionsModel.IndustryId = data.IndustryId;
                            solutionsModel.Industries = _db.Industries.Where(x => x.Id == data.IndustryId).Select(x => x.IndustryName).FirstOrDefault();
                            industryList.Add(solutionsModel);
                        }


                    }
                    return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                    {
                        StatusCode = StatusCodes.Status200OK,
                        Message = "Success",
                        Result = new
                        {
                            IndustriesData = industryList,
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
                StatusCode = StatusCodes.Status403Forbidden,
                Message = "Something Went Wrong !!"

            });

        }

        //DeleteActiveSolutionDocument
        [HttpPost]
        [Route("DeleteActiveSolutionDocument")]
        public async Task<IActionResult> DeleteActiveSolutionDocument([FromBody] ActiveProjectDocuments model)
        {
            if (model != null)
            {
                try
                {
                    if (model.Id != 0)
                    {
                        var documnetData = await _db.ActiveProjectDocuments.Where(x => x.Id == model.Id).FirstOrDefaultAsync();
                        if (documnetData != null)
                        {
                            _db.ActiveProjectDocuments.Remove(documnetData);
                            _db.SaveChanges();

                            return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                            {
                                StatusCode = StatusCodes.Status200OK,
                                Message = "Data Delete Successfully!",
                            });
                        }
                        return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                        {
                            StatusCode = StatusCodes.Status200OK,
                            Message = "Data Not Found!",
                        });
                    }
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

        //StopActiveProject
        [HttpPost]
        [Route("StopActiveProject")]
        public async Task<IActionResult> StopActiveProject([FromBody] solutionFundViewModel model)
        {
            if (model != null)
            {
                try
                {
                    var solutionfundData = await _db.SolutionFund.Where(x => x.SolutionId == model.SolutionId && x.IndustryId == model.IndustryId && x.ClientId == model.ClientId && x.ProjectType == model.ProjectType).FirstOrDefaultAsync();
                    if (solutionfundData != null)
                    {
                        if (solutionfundData.FundType == SolutionFund.FundTypes.ProjectFund)
                        {
                            solutionfundData.IsStoppedProject = true;
                            solutionfundData.StoppedProjectDateTime = DateTime.Now;
                            _db.SaveChanges();
                        }
                        else
                        {
                            var solutionmilestonefundData = await _db.SolutionFund.Where(x => x.SolutionId == model.SolutionId && x.IndustryId == model.IndustryId && x.ClientId == model.ClientId && x.ProjectType == model.ProjectType && x.MileStoneId == model.MileStoneId).FirstOrDefaultAsync();
                            solutionmilestonefundData.IsStoppedProject = true;
                            solutionmilestonefundData.StoppedProjectDateTime = DateTime.Now;
                            _db.SaveChanges();
                        }

                        return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                        {
                            StatusCode = StatusCodes.Status200OK,
                            Message = "Project Stopped"
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

        //GetAllFeedbacks
        [HttpPost]
        [Route("GetAllFeedbacks")]
        public async Task<IActionResult> GetAllFeedbacks([FromBody] solutionFundViewModel model)
        {
            if (model != null)
            {
                try
                {
                    var feedbackData = await _db.ProjectReview.Where(x => x.SolutionId == model.SolutionId && x.IndustryId == model.IndustryId).ToListAsync();
                    List<ProjectReview> projectReviewList = new List<ProjectReview>();
                    if (feedbackData.Count > 0)
                    {
                        foreach (var feedbackdata in feedbackData)
                        {
                            ProjectReview projectReview = new ProjectReview();
                            var clientname = _db.Users.Where(x => x.Id == feedbackdata.ClientId).Select(x => new { x.FirstName, x.LastName }).FirstOrDefault();
                            projectReview.ClientId = clientname.FirstName + " " + clientname.LastName;
                            projectReview.Feedback_Message = feedbackdata.Feedback_Message;
                            projectReview.ID = feedbackdata.ID;
                            projectReviewList.Add(projectReview);
                        }
                    }
                    return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                    {
                        StatusCode = StatusCodes.Status200OK,
                        Message = "success",
                        Result = projectReviewList
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

        //CheckOutUsingRevoult
        [HttpPost]
        [Route("CheckOutUsingRevoult")]
        public async Task<string> CheckOutUsingRevoult([FromBody] SolutionFund model)
        {

            try
            {
                var clientpreferredCurrency = _db.Users.Where(x => x.Id == model.ClientId).Select(x => x.PreferredCurrency).FirstOrDefault();
                if (clientpreferredCurrency == null)
                {
                    clientpreferredCurrency = "EUR";
                }
                string clientpreferredCurrencyQuote = '"' + clientpreferredCurrency + '"';


                var ProjectPrice = Convert.ToDecimal(model.ProjectPrice);
                if (model.FundType == SolutionFund.FundTypes.MilestoneFund)
                {
                    var MileStoneData = _db.SolutionMilestone.Where(x => x.Id == model.MileStoneId).FirstOrDefault();
                    var MilestoneTotalDaysByProjectType = _db.SolutionMilestone.Where(x => x.SolutionId == model.SolutionId && x.IndustryId == model.IndustryId && x.ProjectType == model.ProjectType).ToList();
                    if (MilestoneTotalDaysByProjectType.Count > 0)
                    {
                        SolutionMilestone mileStoneToTalDays = MilestoneTotalDaysByProjectType
                       .GroupBy(l => l.ProjectType)
                       .Select(cl => new SolutionMilestone
                       {
                           ProjectType = cl.First().ProjectType,
                           Days = cl.Sum(c => c.Days),
                       }).FirstOrDefault();

                        if (mileStoneToTalDays.Days > 0)
                        {
                            // var finalPrice = await CountFinalProjectPricing(model);
                            var calculateProjectPrice = ((ProjectPrice / mileStoneToTalDays.Days) * MileStoneData.Days) * 100;
                            model.ProjectPrice = calculateProjectPrice.ToString();
                        }
                    }
                }
                else
                {
                    //var finalPrice = await CountFinalProjectPricing(model);
                    model.ProjectPrice = (ProjectPrice * 100).ToString();
                }

                var options = new RestClientOptions("https://sandbox-merchant.revolut.com/")
                {
                    MaxTimeout = -1,
                };
                var client = new RestClient(options);
                var request = new RestRequest("https://sandbox-merchant.revolut.com/api/orders", Method.Post);
                request.AddHeader("Content-Type", "application/json");
                request.AddHeader("Accept", "application/json");
                request.AddHeader("Authorization", "Bearer sk_u8VvFPDvr2eor1R-Ti_4fXa1J2G7jeVEyB8AXndKu7yaT20UkLlLsBDM3naKRzY4");
                request.AddHeader("Revolut-Api-Version", "2023-09-01");
                var body = @"{" + "\n" +
                @"  ""amount"": " + model.ProjectPrice + "," + "\n" +
                @"  ""currency"": " + clientpreferredCurrencyQuote + "" + "\n" +
                @"}";
                request.AddStringBody(body, DataFormat.Json);
                RestResponse response = await client.ExecuteAsync(request);
                var responseDto = JsonConvert.DeserializeObject<ResponseDto>(response.Content);
                //ViewData["Token"] = responseDto.token;
                //return View();
                var tokenandOrerId = responseDto.token + "|" + responseDto.id;

                //await _revoultService.GetOrderDetails("65294f62-be93-a6e5-b590-d55366e4fb28");

                return tokenandOrerId;
            }
            catch (Exception ex)
            {
                return ex.Message + ex.InnerException;
            }

        }

        //SaveSolutionTeamData
        [HttpPost]
        [Route("SaveSolutionTeamData")]
        public async Task<string> SaveSolutionTeamData([FromBody] SolutionFund model, int assosiate, int expert, int projectmanager)
        {
            List<SolutionTeam> solutionTeam = new List<SolutionTeam>();

            var projectType = model.ProjectType.ToLower();
            var totalMileStoneData = await _db.SolutionMilestone.Where(x => x.SolutionId == model.SolutionId && x.IndustryId == model.IndustryId && x.ProjectType == projectType).ToListAsync();
            var totalMilestoneDays = totalMileStoneData.Sum(x => x.Days);
            bool associateDetailsAdded = false;
            bool projectManagerAdded = false;
            bool expertDetailsAdded = false;
            var exprtcount = 0;
            var associatecount = 0;
            var projectmanagercount = 0;

            try
            {
                var Userslist = _db.Users.Where(x => x.UserType == "Freelancer" && x.RevolutStatus == true && !string.IsNullOrEmpty(x.RevolutConnectId)).ToList();
                if (Userslist.Count > 0)
                {
                    List<FreelancerDetails> freelancerList = new List<FreelancerDetails>();
                    foreach (var data in Userslist)
                    {
                        if (projectType == AppConst.ProjectType.SMALL_PROJECT)
                        {
                            //var teamSize = "1 Project Manager + 1 Associate";
                            if (!associateDetailsAdded)
                            {
                                var associateDetails = _db.FreelancerDetails.Where(x => x.FreelancerLevel == "Associate" && x.UserId == data.Id).FirstOrDefault();
                                if (associateDetails != null)
                                {
                                    freelancerList.Add(associateDetails);
                                    associateDetailsAdded = true;
                                }
                            }

                            if (!projectManagerAdded)
                            {
                                var projectManagerDetails = _db.FreelancerDetails.Where(x => x.FreelancerLevel == "Project Manager" && x.UserId == data.Id).FirstOrDefault();
                                if (projectManagerDetails != null)
                                {
                                    freelancerList.Add(projectManagerDetails);
                                    projectManagerAdded = true;
                                }
                            }

                        }
                        if (projectType == AppConst.ProjectType.MEDIUM_PROJECT)
                        {
                            //var teamsize = "1 Project Manager + 1 Expert + 1 Associate";
                            if (!expertDetailsAdded)
                            {
                                var expertDetails = _db.FreelancerDetails.Where(x => x.FreelancerLevel == "Expert" && x.UserId == data.Id).FirstOrDefault();
                                if (expertDetails != null)
                                {
                                    freelancerList.Add(expertDetails);
                                    expertDetailsAdded = true;
                                }
                            }

                            if (!associateDetailsAdded)
                            {
                                var associateDetails = _db.FreelancerDetails.Where(x => x.FreelancerLevel == "Associate" && x.UserId == data.Id).FirstOrDefault();
                                if (associateDetails != null)
                                {
                                    freelancerList.Add(associateDetails);
                                    associateDetailsAdded = true;
                                }
                            }

                            if (!projectManagerAdded)
                            {
                                var projectManager = _db.FreelancerDetails.Where(x => x.FreelancerLevel == "Project Manager" && x.UserId == data.Id).FirstOrDefault();
                                if (projectManager != null)
                                {
                                    freelancerList.Add(projectManager);
                                    projectManagerAdded = true;
                                }
                            }

                        }
                        if (projectType == AppConst.ProjectType.LARGE_PROJECT)
                        {
                            //var teamsize = "1 Project Manager + 2 Experts + 2 Associates";

                            if (exprtcount <= 1)
                            {
                                var expertsDetails = _db.FreelancerDetails.Where(x => x.FreelancerLevel == "Expert" && x.UserId == data.Id).FirstOrDefault();
                                if (expertsDetails != null)
                                {
                                    //if(exprtcount == 1)
                                    //{
                                    //    expertDetailsAdded = true;
                                    //}
                                    freelancerList.Add(expertsDetails);
                                    exprtcount++;
                                }
                            }

                            if (associatecount <= 1)
                            {
                                var associateDetails = _db.FreelancerDetails.Where(x => x.FreelancerLevel == "Associate" && x.UserId == data.Id).FirstOrDefault();
                                if (associateDetails != null)
                                {
                                    freelancerList.Add(associateDetails);
                                    associatecount++;
                                }
                            }

                            if (!projectManagerAdded)
                            {
                                var projectManager = _db.FreelancerDetails.Where(x => x.FreelancerLevel == "Project Manager" && x.UserId == data.Id).FirstOrDefault();
                                if (projectManager != null)
                                {
                                    freelancerList.Add(projectManager);
                                    projectManagerAdded = true;
                                }
                            }

                        }
                        if (projectType == AppConst.ProjectType.CUSTOM_PROJECT)
                        {
                            if (!expertDetailsAdded)
                            {
                                if (exprtcount < expert)
                                {
                                    var expertsDetails = _db.FreelancerDetails.Where(x => x.FreelancerLevel == "Expert" && x.UserId == data.Id).FirstOrDefault();
                                    if (expertsDetails != null)
                                    {
                                        freelancerList.Add(expertsDetails);
                                        exprtcount++;
                                    }
                                }
                                if (exprtcount == expert)
                                {
                                    expertDetailsAdded = true;
                                }
                            }


                            if (!associateDetailsAdded)
                            {
                                if (associatecount < assosiate)
                                {
                                    var associateDetails = _db.FreelancerDetails.Where(x => x.FreelancerLevel == "Associate" && x.UserId == data.Id).FirstOrDefault();
                                    if (associateDetails != null)
                                    {
                                        freelancerList.Add(associateDetails);
                                        associatecount++;
                                    }
                                }
                                if (associatecount == assosiate)
                                {
                                    associateDetailsAdded = true;
                                }
                            }


                            if (!projectManagerAdded)
                            {
                                if (projectmanagercount < projectmanager)
                                {
                                    var projectManager = _db.FreelancerDetails.Where(x => x.FreelancerLevel == "Project Manager" && x.UserId == data.Id).FirstOrDefault();
                                    if (projectManager != null)
                                    {
                                        freelancerList.Add(projectManager);
                                        //
                                        projectmanagercount++;
                                    }
                                }
                                if (projectmanagercount == projectmanager)
                                {
                                    projectManagerAdded = true;
                                }

                            }

                        }
                    }



                    if (freelancerList.Count > 0)
                    {
                        var clientId = _db.SolutionFund.Where(x => x.Id == model.Id).FirstOrDefault().ClientId;
                        var clientPreferedCurrency = _db.Users.Where(x => x.Id == clientId).FirstOrDefault().PreferredCurrency;
                        if (string.IsNullOrEmpty(clientPreferedCurrency))
                        {
                            clientPreferedCurrency = "EUR";
                        }
                        foreach (var data in freelancerList)
                        {
                            var freelancerPreferedCurrency = _db.Users.Where(x => x.Id == data.UserId).FirstOrDefault().PreferredCurrency;
                            if (string.IsNullOrEmpty(freelancerPreferedCurrency))
                            {
                                freelancerPreferedCurrency = "EUR";
                            }
                            var exchangeRate = _db.ExchangeRates.Where(x => x.FromCurrency == freelancerPreferedCurrency
                            && x.ToCurrency == clientPreferedCurrency).FirstOrDefault();
                            var HourlyRate = Convert.ToDecimal(data.HourlyRate);
                            decimal ExchangeHourlyRate = HourlyRate;
                            if (exchangeRate != null)
                            {
                                ExchangeHourlyRate = Convert.ToDecimal((decimal)(HourlyRate * exchangeRate.Rate));
                            }

                            //var projectManager = false;
                            decimal contractAmount = contractAmount = (totalMilestoneDays * 8 * ExchangeHourlyRate);
                            decimal Platformfees = 0;
                            if (projectType == AppConst.ProjectType.SMALL_PROJECT)
                            {
                                Platformfees = (contractAmount * AppConst.Commission.PLATFORM_COMM_FROM_FREELANCER_SMALL) / 100;
                            }
                            if (projectType == AppConst.ProjectType.MEDIUM_PROJECT)
                            {
                                Platformfees = (contractAmount * AppConst.Commission.PLATFORM_COMM_FROM_FREELANCER_MEDIUM) / 100;
                            }
                            if (projectType == AppConst.ProjectType.LARGE_PROJECT)
                            {
                                Platformfees = (contractAmount * AppConst.Commission.PLATFORM_COMM_FROM_FREELANCER_LARGE) / 100;
                            }
                            if (projectType == AppConst.ProjectType.CUSTOM_PROJECT)
                            {
                                Platformfees = (contractAmount * AppConst.Commission.PLATFORM_COMM_FROM_FREELANCER_CUSTOM) / 100;
                            }


                            //if (data.FreelancerLevel == "Project Manager")
                            //{
                            //    projectManager = true;
                            //}
                            //else
                            //{
                            //    projectManager = false;

                            //}
                            solutionTeam.Add(new SolutionTeam()
                            {
                                FreelancerId = data.UserId,
                                SolutionFundId = model.Id,
                                //IsProjectManager = projectManager,
                                IsProjectManager = false,
                                Amount = contractAmount,
                                PlatformFees = Platformfees
                            });
                        }
                        _db.SolutionTeam.AddRange(solutionTeam);
                        _db.SaveChanges();
                        //var soluionTeamData = _db.SolutionTeam.Where(x => x.SolutionFundId == model.Id).ToList();
                        //if (soluionTeamData.Count > 0)
                        //{
                        //    var projectTypeFee = 0;
                        //    if (projectType == AppConst.ProjectType.SMALL_PROJECT)
                        //    {
                        //        projectTypeFee = AppConst.Commission.PROJECT_MANAGER_SMALL;
                        //    }
                        //    if (projectType == AppConst.ProjectType.MEDIUM_PROJECT)
                        //    {
                        //        projectTypeFee = AppConst.Commission.PROJECT_MANAGER_MEDIUM;
                        //    }
                        //    if (projectType == AppConst.ProjectType.LARGE_PROJECT)
                        //    {
                        //        projectTypeFee = AppConst.Commission.PROJECT_MANAGER_LARGE;
                        //    }
                        //    var projectmanagerPlatformFees = (soluionTeamData.Sum(x => x.Amount)) * projectTypeFee / 100;
                        //    var projectManagerData = soluionTeamData.Where(x => x.IsProjectManager).FirstOrDefault();
                        //    projectManagerData.ProjectManagerPlatformFees = projectmanagerPlatformFees;
                        //    _db.SaveChanges();
                        //}
                    }
                }
                return "success";

            }
            catch (Exception ex)
            {

            }

            return "Something went wrong";
        }

        [HttpGet]
        [Route("CheckRevolutOrderStatus")]
        public async Task<IActionResult> CheckRevolutOrderStatus()
        {
            try
            {
                //OrderResponse = "SKIP", "DONE", ""
                var contractList = await _db.Contract.Where(x =>
                x.OrderResponse != "SKIP" &&
                x.OrderResponse != "DONE").ToListAsync();

                foreach (var item in contractList)
                {
                    try
                    {
                        var content = await _revoultService.GetOrderDetails(item.PaymentIntentId);
                        dynamic parseContent = JObject.Parse(content);
                        JArray payments = (JArray)parseContent["payments"];
                        if (payments != null && payments.Count > 0)
                        {
                            JArray fees = (JArray)payments[0]["fees"];
                            if (fees != null && fees.Count > 0)
                            {
                                decimal totalFee = 0;
                                foreach (JObject fee in fees)
                                {
                                    totalFee += (decimal)fee["amount"];
                                }
                                item.RevolutFee = totalFee;
                                item.OrderResponse = "DONE";
                                _db.Contract.Update(item);
                                _db.SaveChanges();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        item.OrderResponseRemarks = ex.Message;
                        _db.Contract.Update(item);
                        _db.SaveChanges();
                    }

                }

                // Mark "SKIP" for Where code will not be trying to communicate with revolut for "skip" records
                var skipRecords = await _db.Contract.Where(x => (DateTime.Now - x.CreatedDateTime).TotalDays >= 8).ToListAsync();
                foreach (var contract in skipRecords)
                {
                    contract.OrderResponse = "SKIP";
                    _db.Contract.Update(contract);
                    _db.SaveChanges();
                }
                return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                {
                    StatusCode = StatusCodes.Status200OK,
                    Message = "success"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                {
                    StatusCode = StatusCodes.Status200OK,
                    Message = ex.Message + " || " + ex.StackTrace
                });

            }
        }

        //CountFinalProjectPricing
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
                //decimal projectManagerFees = 0;
                //var projectFreelancers = await _db.SolutionTeam.Where(x => x.SolutionFundId == model.Id).ToListAsync();
                //var projectManagerPlatformFees = projectFreelancers.Where(x => x.IsProjectManager).Select(x => x.ProjectManagerPlatformFees).FirstOrDefault();
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

        [HttpPost]
        [Route("GetExchangeRate")]
        public async Task<IActionResult> GetExchangeRate()
        {

            try
            {
                var UsdtoGbpexchangeDetails = await _revoultService.ExchangeCurrency("USD", "GBP", "1");
                if (UsdtoGbpexchangeDetails.StatusCode == HttpStatusCode.OK)
                {
                    var content = UsdtoGbpexchangeDetails.Content;
                    dynamic parseContent = JObject.Parse(content);
                    var rate = (decimal)parseContent["rate"];
                    var fromCurrency = (string)parseContent["from"]["currency"];
                    var toCurrency = (string)parseContent["to"]["currency"];

                    var checkdataExists = _db.ExchangeRates.Where(x => x.FromCurrency == fromCurrency && x.ToCurrency == toCurrency).FirstOrDefault();
                    if (checkdataExists == null)
                    {
                        var exchangeData = new ExchangeRates()
                        {
                            FromCurrency = fromCurrency,
                            ToCurrency = toCurrency,
                            Rate = rate,
                            ExchangeRateDateTime = DateTime.Now
                        };
                        _db.ExchangeRates.Add(exchangeData);
                        _db.SaveChanges();
                    }
                    else
                    {
                        checkdataExists.Rate = rate;
                        checkdataExists.ExchangeRateDateTime = DateTime.Now;
                        _db.SaveChanges();
                    }
                }

                var UsdtoEurexchangeDetails = await _revoultService.ExchangeCurrency("USD", "EUR", "1");
                if (UsdtoEurexchangeDetails.StatusCode == HttpStatusCode.OK)
                {
                    var content = UsdtoEurexchangeDetails.Content;
                    dynamic parseContent = JObject.Parse(content);
                    var rate = (decimal)parseContent["rate"];
                    var fromCurrency = (string)parseContent["from"]["currency"];
                    var toCurrency = (string)parseContent["to"]["currency"];

                    var checkdataExists = _db.ExchangeRates.Where(x => x.FromCurrency == fromCurrency && x.ToCurrency == toCurrency).FirstOrDefault();
                    if (checkdataExists == null)
                    {
                        var exchangeData = new ExchangeRates()
                        {
                            FromCurrency = fromCurrency,
                            ToCurrency = toCurrency,
                            Rate = rate,
                            ExchangeRateDateTime = DateTime.Now
                        };
                        _db.ExchangeRates.Add(exchangeData);
                        _db.SaveChanges();
                    }
                    else
                    {
                        checkdataExists.Rate = rate;
                        checkdataExists.ExchangeRateDateTime = DateTime.Now;
                        _db.SaveChanges();
                    }
                }

                var EurToUsdexchangeDetails = await _revoultService.ExchangeCurrency("EUR", "USD", "1");
                if (EurToUsdexchangeDetails.StatusCode == HttpStatusCode.OK)
                {
                    var content = EurToUsdexchangeDetails.Content;
                    dynamic parseContent = JObject.Parse(content);
                    var rate = (decimal)parseContent["rate"];
                    var fromCurrency = (string)parseContent["from"]["currency"];
                    var toCurrency = (string)parseContent["to"]["currency"];

                    var checkdataExists = _db.ExchangeRates.Where(x => x.FromCurrency == fromCurrency && x.ToCurrency == toCurrency).FirstOrDefault();
                    if (checkdataExists == null)
                    {
                        var exchangeData = new ExchangeRates()
                        {
                            FromCurrency = fromCurrency,
                            ToCurrency = toCurrency,
                            Rate = rate,
                            ExchangeRateDateTime = DateTime.Now
                        };
                        _db.ExchangeRates.Add(exchangeData);
                        _db.SaveChanges();
                    }
                    else
                    {
                        checkdataExists.Rate = rate;
                        checkdataExists.ExchangeRateDateTime = DateTime.Now;
                        _db.SaveChanges();
                    }
                }

                var EurToGbpexchangeDetails = await _revoultService.ExchangeCurrency("EUR", "GBP", "1");
                if (EurToGbpexchangeDetails.StatusCode == HttpStatusCode.OK)
                {
                    var content = EurToGbpexchangeDetails.Content;
                    dynamic parseContent = JObject.Parse(content);
                    var rate = (decimal)parseContent["rate"];
                    var fromCurrency = (string)parseContent["from"]["currency"];
                    var toCurrency = (string)parseContent["to"]["currency"];

                    var checkdataExists = _db.ExchangeRates.Where(x => x.FromCurrency == fromCurrency && x.ToCurrency == toCurrency).FirstOrDefault();
                    if (checkdataExists == null)
                    {
                        var exchangeData = new ExchangeRates()
                        {
                            FromCurrency = fromCurrency,
                            ToCurrency = toCurrency,
                            Rate = rate,
                            ExchangeRateDateTime = DateTime.Now
                        };
                        _db.ExchangeRates.Add(exchangeData);
                        _db.SaveChanges();
                    }
                    else
                    {
                        checkdataExists.Rate = rate;
                        checkdataExists.ExchangeRateDateTime = DateTime.Now;
                        _db.SaveChanges();
                    }
                }

                var GbpToUsdexchangeDetails = await _revoultService.ExchangeCurrency("GBP", "USD", "1");
                if (GbpToUsdexchangeDetails.StatusCode == HttpStatusCode.OK)
                {
                    var content = GbpToUsdexchangeDetails.Content;
                    dynamic parseContent = JObject.Parse(content);
                    var rate = (decimal)parseContent["rate"];
                    var fromCurrency = (string)parseContent["from"]["currency"];
                    var toCurrency = (string)parseContent["to"]["currency"];

                    var checkdataExists = _db.ExchangeRates.Where(x => x.FromCurrency == fromCurrency && x.ToCurrency == toCurrency).FirstOrDefault();
                    if (checkdataExists == null)
                    {
                        var exchangeData = new ExchangeRates()
                        {
                            FromCurrency = fromCurrency,
                            ToCurrency = toCurrency,
                            Rate = rate,
                            ExchangeRateDateTime = DateTime.Now
                        };
                        _db.ExchangeRates.Add(exchangeData);
                        _db.SaveChanges();
                    }
                    else
                    {
                        checkdataExists.Rate = rate;
                        checkdataExists.ExchangeRateDateTime = DateTime.Now;
                        _db.SaveChanges();
                    }
                }

                var GbpToEurexchangeDetails = await _revoultService.ExchangeCurrency("GBP", "EUR", "1");
                if (GbpToEurexchangeDetails.StatusCode == HttpStatusCode.OK)
                {
                    var content = GbpToEurexchangeDetails.Content;
                    dynamic parseContent = JObject.Parse(content);
                    var rate = (decimal)parseContent["rate"];
                    var fromCurrency = (string)parseContent["from"]["currency"];
                    var toCurrency = (string)parseContent["to"]["currency"];

                    var checkdataExists = _db.ExchangeRates.Where(x => x.FromCurrency == fromCurrency && x.ToCurrency == toCurrency).FirstOrDefault();
                    if (checkdataExists == null)
                    {
                        var exchangeData = new ExchangeRates()
                        {
                            FromCurrency = fromCurrency,
                            ToCurrency = toCurrency,
                            Rate = rate,
                            ExchangeRateDateTime = DateTime.Now
                        };
                        _db.ExchangeRates.Add(exchangeData);
                        _db.SaveChanges();
                    }
                    else
                    {
                        checkdataExists.Rate = rate;
                        checkdataExists.ExchangeRateDateTime = DateTime.Now;
                        _db.SaveChanges();
                    }
                }
                else
                {
                    return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                    {
                        StatusCode = StatusCodes.Status200OK,
                        Message = GbpToEurexchangeDetails.Content
                    });
                }

                return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                {
                    StatusCode = StatusCodes.Status200OK,
                    Message = "Rate Saved Successfully !"
                });

            }
            catch (Exception ex)
            {

            }

            return StatusCode(StatusCodes.Status200OK, new APIResponseModel
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "Something Went Wrong"
            });
        }

        //GetFreelancerInvoiceTypeDetails
        [HttpPost]
        [Route("GetFreelancerInvoiceTypeDetails")]
        public async Task<IActionResult> GetFreelancerInvoiceTypeDetails([FromBody] SolutionsModel model)
        {

            try
            {
                var invoiceListData = await _db.InvoiceList.Where(x => x.ContractId == model.ContractId && x.FreelancerId == model.ClientId).ToListAsync();
                List<dynamic> list = new List<dynamic>();
                foreach (var item in invoiceListData)
                {
                    string name = "";
                    if (item.FreelancerId != null)
                    {
                        var freelancer = await _db.Users.FirstOrDefaultAsync(x => x.Id == item.FreelancerId);
                        if (freelancer != null)
                        {
                            name = freelancer.FirstName + "_" + freelancer.LastName + "_" + item.InvoiceNumber;
                        }
                    }
                    else
                    {
                        name = item.InvoiceType + "_" + item.InvoiceNumber;
                    }

                    list.Add(
                    new
                    {
                        InvoiceName = name,
                        Id = item.Id
                    });
                }
                return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                {
                    StatusCode = StatusCodes.Status200OK,
                    Message = "success",
                    Result = list
                });
            }
            catch (Exception ex)
            {

            }

            return StatusCode(StatusCodes.Status200OK, new APIResponseModel
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "Something Went Wrong"
            });
        }

        //GetInvoiceTranscationTypeDetails
        [HttpPost]
        [Route("GetInvoiceTranscationTypeDetails")]
        public async Task<IActionResult> GetInvoiceTranscationTypeDetails([FromBody] SolutionsModel model)
        {

            try
            {
                var invoiceListData = await _db.InvoiceList.Where(x => x.ContractId == model.ContractId && x.BillToClientId == model.ClientId).ToListAsync();
                List<dynamic> list = new List<dynamic>();
                foreach (var item in invoiceListData)
                {
                    string name = "";
                    if (item.FreelancerId != null)
                    {
                        var freelancer = await _db.Users.FirstOrDefaultAsync(x => x.Id == item.FreelancerId);
                        if (freelancer != null)
                        {
                            name = freelancer.FirstName + "_" + freelancer.LastName + "_" + item.InvoiceNumber;
                        }
                    }
                    else
                    {
                        name = item.InvoiceType + "_" + item.InvoiceNumber;
                    }

                    list.Add(
                    new
                    {
                        InvoiceName = name,
                        Id = item.Id
                    });
                }
                return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                {
                    StatusCode = StatusCodes.Status200OK,
                    Message = "success",
                    Result = list
                });
            }
            catch (Exception ex)
            {

            }

            return StatusCode(StatusCodes.Status200OK, new APIResponseModel
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "Something Went Wrong"
            });
        }

        [HttpPost]
        [Route("ConvertToCurrencySign")]
        public async Task<string> ConvertToCurrencySign(string Currency)
        {

            try
            {
                if (Currency != null)
                {
                    if (Currency == "USD")
                    {
                        Currency = "$";
                    }
                    if (Currency == "EUR")
                    {
                        Currency = "€";
                    }
                    if (Currency == "GBP")
                    {
                        Currency = "£";
                    }
                }
                else
                {
                    Currency = "€";
                }

                return Currency;
            }
            catch (Exception ex)
            {
                return ex.Message + ex.InnerException;

            }
        }


        [HttpPost]
        public async Task GenerateInvoiceAfterPayAmount(Contract contract, string solutionTitle, string projectType)
        {
            #region Invoice - Credit Memo
            var inv1 = _db.InvoiceList.Where(x => x.ContractId == contract.Id
            && x.TransactionType == AppConst.InvoiceTransactionType.INVOICE1_PORTAL_TO_CLIENT).FirstOrDefault();
            var invoiceCreditMemo = new InvoiceList();
            invoiceCreditMemo.BillToClientId = inv1.BillToClientId;
            //invoiceCreditMemo.InvoiceNumber = "INV-00104"; // ######
            invoiceCreditMemo.InvoiceDate = DateTime.Now;
            invoiceCreditMemo.TransactionType = AppConst.InvoiceTransactionType.CREDIT_MEMO;
            invoiceCreditMemo.TotalAmount = Convert.ToString((Convert.ToDecimal(inv1.TotalAmount) * -1));
            invoiceCreditMemo.InvoiceType = "Credit Memo";
            invoiceCreditMemo.ContractId = contract.Id;
            _db.InvoiceList.Add(invoiceCreditMemo);
            _db.SaveChanges();

            await UpdateInvoiceNumberAsync(invoiceCreditMemo.Id, "CM");

            // Invoice - Credit Memo (Details 1) 
            var invoiceCreditMemoDetail_escrow = new InvoiceListDetails();
            invoiceCreditMemoDetail_escrow.InvoiceListId = invoiceCreditMemo.Id;
            invoiceCreditMemoDetail_escrow.Amount = "(" + inv1.TotalAmount + ")";
            invoiceCreditMemoDetail_escrow.Description = "Paid from escrow for \"" + solutionTitle + "\""; // ###### - this will be either project or milestone tile
            _db.InvoiceListDetails.Add(invoiceCreditMemoDetail_escrow);
            _db.SaveChanges();

            // Invoice - Credit Memo (Details 2) 
            var invoiceCreditMemoDetail_vat = new InvoiceListDetails();
            invoiceCreditMemoDetail_vat.InvoiceListId = invoiceCreditMemo.Id;
            invoiceCreditMemoDetail_vat.Amount = "";
            invoiceCreditMemoDetail_vat.Description = "VAT (0%)";
            _db.InvoiceListDetails.Add(invoiceCreditMemoDetail_vat);
            _db.SaveChanges();

            // Invoice - Credit Memo (Details 3)
            var invoiceCreditMemoDetail_total = new InvoiceListDetails();
            invoiceCreditMemoDetail_total.InvoiceListId = invoiceCreditMemo.Id;
            invoiceCreditMemoDetail_total.Amount = invoiceCreditMemo.TotalAmount;
            invoiceCreditMemoDetail_total.Description = "Total amount";
            _db.InvoiceListDetails.Add(invoiceCreditMemoDetail_total);
            _db.SaveChanges();
            #endregion

            #region Invoices - Freelancers
            //For all Freelancer
            var allFreelancers = await _db.ContractUser.Where(x => x.ContractId == contract.Id).ToListAsync();
            var fullTeam = await _db.SolutionTeam.Where(x => x.SolutionFundId == contract.SolutionFundId).ToListAsync();
            decimal clientFees = 0;
            decimal freelancerFees = 0;
            //decimal pmPlatformfees = 0;
            if (projectType == AppConst.ProjectType.LARGE_PROJECT)
            {
                clientFees = (fullTeam.Sum(y => y.Amount) * AppConst.Commission.PLATFORM_COMM_FROM_CLIENT_LARGE) / 100;
                freelancerFees = (fullTeam.Sum(y => y.Amount) * AppConst.Commission.PLATFORM_COMM_FROM_FREELANCER_LARGE) / 100;
                //pmPlatformfees = (fullTeam.Sum(y => y.Amount) * AppConst.Commission.PLATFORM_COMM_FROM_FREELANCER_LARGE) / 100;
            }
            else if (projectType == AppConst.ProjectType.SMALL_PROJECT)
            {
                clientFees = (fullTeam.Sum(y => y.Amount) * AppConst.Commission.PLATFORM_COMM_FROM_CLIENT_SMALL) / 100;
                freelancerFees = (fullTeam.Sum(y => y.Amount) * AppConst.Commission.PLATFORM_COMM_FROM_FREELANCER_SMALL) / 100;
            }
            else if (projectType == AppConst.ProjectType.MEDIUM_PROJECT)
            {
                clientFees = (fullTeam.Sum(y => y.Amount) * AppConst.Commission.PLATFORM_COMM_FROM_CLIENT_MEDIUM) / 100;
                freelancerFees = (fullTeam.Sum(y => y.Amount) * AppConst.Commission.PLATFORM_COMM_FROM_FREELANCER_MEDIUM) / 100;
            }
            else if (projectType == AppConst.ProjectType.CUSTOM_PROJECT)
            {
                if (fullTeam.Count <= 3)
                {
                    clientFees = (fullTeam.Sum(y => y.Amount) * AppConst.Commission.PLATFORM_COMM_FROM_CLIENT_CUSTOM_LESS_THAN_THREE_GIGS) / 100;
                }
                else
                {
                    clientFees = (fullTeam.Sum(y => y.Amount) * AppConst.Commission.PLATFORM_COMM_FROM_CLIENT_CUSTOM) / 100;
                }

                freelancerFees = (fullTeam.Sum(y => y.Amount) * AppConst.Commission.PLATFORM_COMM_FROM_FREELANCER_CUSTOM) / 100;
            }
            var revolutFees = contract.RevolutFee;
            var solutionTeamSum = fullTeam.Sum(x => x.Amount)
                    //+ fullTeam.Sum(y => y.PlatformFees) // not include because x.Amount already includes this
                    //+ fullTeam.Sum(p => p.ProjectManagerPlatformFees)
                    + clientFees;
            //var count = 1;
            foreach (var freelancer in allFreelancers)
            {

                var solutionTeam = fullTeam.Where(x => x.FreelancerId == freelancer.ApplicationUserId
                    && x.SolutionFundId == contract.SolutionFundId).FirstOrDefault();

                // freelancer initial total amount - platform fee (i.e solutionTeam.Amount - solutionTeam.PlatformFees)
                decimal totalAmount = solutionTeam.Amount - solutionTeam.PlatformFees;
                var invoiceFreelancer = new InvoiceList();
                invoiceFreelancer.BillToClientId = contract.ClientUserId;
                //invoiceFreelancer.InvoiceNumber = "INV-00104_" + count; // ######
                invoiceFreelancer.InvoiceDate = DateTime.Now;
                invoiceFreelancer.TransactionType = AppConst.InvoiceTransactionType.INVOICE_FREELANCER;
                invoiceFreelancer.TotalAmount = Convert.ToString(totalAmount);
                invoiceFreelancer.InvoiceType = "Invoice";
                invoiceFreelancer.ContractId = contract.Id;
                invoiceFreelancer.FreelancerId = freelancer.ApplicationUserId;
                _db.InvoiceList.Add(invoiceFreelancer);
                _db.SaveChanges();

                await UpdateInvoiceNumberAsync(invoiceFreelancer.Id, "INV");

                //



                var checkoutRevoluteFeesForFreelancer = ((revolutFees * totalAmount) / solutionTeamSum);

                // Invoice - Freelancer (Details 1)
                var invoiceFreelancerDetail_InvFor = new InvoiceListDetails();
                invoiceFreelancerDetail_InvFor.InvoiceListId = invoiceFreelancer.Id;
                invoiceFreelancerDetail_InvFor.Amount = Convert.ToString((totalAmount
                    - (checkoutRevoluteFeesForFreelancer + Convert.ToDecimal(freelancer.PaymentFees))
                    - (solutionTeam.PlatformFees * -1)));
                invoiceFreelancerDetail_InvFor.Description = "Invoice for  \"" + solutionTitle + "\""; // ###### - this will be either project or milestone tile
                _db.InvoiceListDetails.Add(invoiceFreelancerDetail_InvFor);
                _db.SaveChanges();

                // Invoice - Freelancer (Details 2)
                var invoiceFreelancerDetail_platformComm = new InvoiceListDetails();
                invoiceFreelancerDetail_platformComm.InvoiceListId = invoiceFreelancer.Id;
                invoiceFreelancerDetail_platformComm.Amount = Convert.ToString((solutionTeam.PlatformFees * -1));
                invoiceFreelancerDetail_platformComm.Description = "Platform comission";
                _db.InvoiceListDetails.Add(invoiceFreelancerDetail_platformComm);
                _db.SaveChanges();

                // Invoice - Freelancer (Details 3)
                var invoiceFreelancerDetail_vat = new InvoiceListDetails();
                invoiceFreelancerDetail_vat.InvoiceListId = invoiceFreelancer.Id;
                invoiceFreelancerDetail_vat.Amount = "";
                invoiceFreelancerDetail_vat.Description = "VAT(0%)";
                _db.InvoiceListDetails.Add(invoiceFreelancerDetail_vat);
                _db.SaveChanges();


                // Invoice - Freelancer (Details 4)
                var invoiceFreelancerDetail_OtherFees = new InvoiceListDetails();
                invoiceFreelancerDetail_OtherFees.InvoiceListId = invoiceFreelancer.Id;
                invoiceFreelancerDetail_OtherFees.Amount = Convert.ToString((checkoutRevoluteFeesForFreelancer + Convert.ToDecimal(freelancer.PaymentFees)));
                invoiceFreelancerDetail_OtherFees.Description = "Other fees (Payment processing fees)";
                _db.InvoiceListDetails.Add(invoiceFreelancerDetail_OtherFees);
                _db.SaveChanges();

                // Invoice - Freelancer (Details 5)
                var invoiceFreelancerDetail_totalAmt = new InvoiceListDetails();
                invoiceFreelancerDetail_totalAmt.InvoiceListId = invoiceFreelancer.Id;
                invoiceFreelancerDetail_totalAmt.Amount = Convert.ToString(totalAmount);
                invoiceFreelancerDetail_totalAmt.Description = "Total amount";
                _db.InvoiceListDetails.Add(invoiceFreelancerDetail_totalAmt);
                _db.SaveChanges();

                //  count++;
            }
            #endregion


            //#region Invoice - Project Manager Fee
            //var invoicePM = new InvoiceList();
            //invoicePM.BillToClientId = contract.ClientUserId;
            //invoicePM.InvoiceNumber = "INV-00106"; // ######
            //invoicePM.InvoiceDate = DateTime.Now;
            //invoicePM.TransactionType = AppConst.InvoiceTransactionType.INVOICE_PA;
            //invoicePM.TotalAmount = Convert.ToString(fullTeam.Sum(p => p.ProjectManagerPlatformFees));
            //invoicePM.InvoiceType = "Invoice";
            //invoicePM.ContractId = contract.Id;
            //_db.InvoiceList.Add(invoicePM);
            //_db.SaveChanges();

            //// J11 = C15 
            //var checkoutRevoluteFeesForFreelancerPM = ((revolutFees * fullTeam.Sum(p => p.ProjectManagerPlatformFees)) / solutionTeamSum);

            //var paymentFeesProjectMgr = _db.ContractUser.Where(x => x.ContractId == contract.Id).Sum(y => y.PaymentFeesProjectMgr);
            //// Invoice  - Project Manager (Details 1) 
            //var invoicePMDetail_invFor = new InvoiceListDetails();
            //invoicePMDetail_invFor.InvoiceListId = invoicePM.Id;
            //invoicePMDetail_invFor.Amount = Convert.ToString(Convert.ToDecimal(invoicePM.TotalAmount)
            //    - (checkoutRevoluteFeesForFreelancerPM + paymentFeesProjectMgr));
            //invoicePMDetail_invFor.Description = "Invoice for \"" + solutionTitle + "\""; // ###### - this will be either project or milestone tile
            //_db.InvoiceListDetails.Add(invoicePMDetail_invFor);
            //_db.SaveChanges();

            //// Invoice - Project Manager (Details 2) 
            //var invoicePMDetail_vat = new InvoiceListDetails();
            //invoicePMDetail_vat.InvoiceListId = invoicePM.Id;
            //invoicePMDetail_vat.Amount = "";
            //invoicePMDetail_vat.Description = "VAT (0%)";
            //_db.InvoiceListDetails.Add(invoicePMDetail_vat);
            //_db.SaveChanges();

            //// Invoice - Project Manager (Details 3)
            //var invoicePMDetail_OtherFee = new InvoiceListDetails();
            //invoicePMDetail_OtherFee.InvoiceListId = invoicePM.Id;
            //invoicePMDetail_OtherFee.Amount = Convert.ToString((checkoutRevoluteFeesForFreelancerPM + paymentFeesProjectMgr));
            //invoicePMDetail_OtherFee.Description = "Other fees (Payment processing fees)";
            //_db.InvoiceListDetails.Add(invoicePMDetail_OtherFee);
            //_db.SaveChanges();

            //// Invoice - Project Manager (Details 4)
            //var invoicePMDetail_total = new InvoiceListDetails();
            //invoicePMDetail_total.InvoiceListId = invoicePM.Id;
            //invoicePMDetail_total.Amount = invoicePM.TotalAmount;
            //invoicePMDetail_total.Description = "Total amount";
            //_db.InvoiceListDetails.Add(invoicePMDetail_total);
            //_db.SaveChanges();
            //#endregion

            #region Invoice - Invoice Commission

            var adminRevolute = _db.EphylinkRevolutAccount.FirstOrDefault();

            decimal total_detail1 = 0; // B11 - C11 - E11
            // B11 = clientFees
            // C11 = J8 = (revolutFees * clientFees)/ solutionTeamSum
            // E11 = L8 = if admin flag is false then 0 else revolutetransferfee
            decimal b11 = clientFees;
            decimal c11 = (revolutFees * clientFees) / solutionTeamSum;
            decimal e11 = 0;
            if (adminRevolute != null)
            {
                if (adminRevolute.IsEnable)
                {
                    var totalRevolutFeePortal = _db.EphylinkRevolutAccountTransferLog.Where(x => x.ContractId == contract.Id).FirstOrDefault();
                    if (totalRevolutFeePortal != null)
                    {
                        e11 = (totalRevolutFeePortal.RevoultFee / (fullTeam.Count + 1)); // Because we do only one transfer with sum of client + freelancers
                    }
                }
                else
                {
                    e11 = 0;
                }
            }
            total_detail1 = b11 - c11 - e11;

            decimal total_detail2 = 0; //=B12+B13-C12-C13-E12-E13
            // B12+B13 = sum(solutionTeam.platformfees)
            decimal b12b13 = fullTeam.Sum(x => x.PlatformFees);

            decimal total_detail4 = 0; // =C11+C12+C13+E11+E12+E13
            // C11 = J8 = (revolutFees * clietFees)/ solutionTeamSum
            total_detail4 += c11;
            total_detail4 += e11;
            foreach (var fl in fullTeam)
            {
                // C12 = J9 = (revolutFees * f1.platformfees)/ solutionTeamSum
                // C13 = J10 = (revolutFees * f2.platformfees)/ solutionTeamSum
                decimal c12 = (revolutFees * fl.PlatformFees) / solutionTeamSum;
                b12b13 -= c12;

                total_detail4 += c12;
                // E12 = L9 = if admin flag is false then 0 else revolutetransferfee
                // E13 = L10 = if admin flag is false then 0 else revolutetransferfee    
                decimal e12 = 0;
                if (adminRevolute != null)
                {
                    if (adminRevolute.IsEnable)
                    {
                        e12 = e11; // Because we do only one transfer with sum of client + freelancers
                    }
                    else
                    {
                        e12 = 0;
                    }
                }
                b12b13 -= e12;

                total_detail4 += e12;

            }
            total_detail2 = b12b13;

            decimal total_detail3 = 0; // VAT 0%


            var invoiceCommission = new InvoiceList();
            invoiceCommission.BillToClientId = inv1.BillToClientId;
            //invoiceCommission.InvoiceNumber = "INV-00106"; // ######
            invoiceCommission.InvoiceDate = DateTime.Now;
            invoiceCommission.TransactionType = AppConst.InvoiceTransactionType.INVOICE_COMMISIONS;
            invoiceCommission.TotalAmount = Convert.ToString((total_detail1 + total_detail2 + total_detail3 + total_detail4));
            invoiceCommission.InvoiceType = "Invoice Commisions";
            invoiceCommission.ContractId = contract.Id;
            _db.InvoiceList.Add(invoiceCommission);
            _db.SaveChanges();

            await UpdateInvoiceNumberAsync(invoiceCommission.Id, "INV");

            // Invoice - Commisions (Details 1) 
            var invoiceplatformFees_Client = new InvoiceListDetails();
            invoiceplatformFees_Client.InvoiceListId = invoiceCommission.Id;
            invoiceplatformFees_Client.Amount = Convert.ToString(total_detail1);
            invoiceplatformFees_Client.Description = "Platform fees to client for  \"" + solutionTitle + "\""; // ###### - this will be either project or milestone tile
            _db.InvoiceListDetails.Add(invoiceplatformFees_Client);
            _db.SaveChanges();

            // Invoice - Commisions (Details 2) 
            var invoiceplatformFees_Freelancer = new InvoiceListDetails();
            invoiceplatformFees_Freelancer.InvoiceListId = invoiceCommission.Id;
            invoiceplatformFees_Freelancer.Amount = Convert.ToString(total_detail2);
            invoiceplatformFees_Freelancer.Description = "Platform fees to freelancers for  \"" + solutionTitle + "\""; // ###### - this will be either project or milestone tile
            _db.InvoiceListDetails.Add(invoiceplatformFees_Freelancer);
            _db.SaveChanges();

            // Invoice - Commisions (Details 3)
            var invoiceVat = new InvoiceListDetails();
            invoiceVat.InvoiceListId = invoiceCommission.Id;
            invoiceVat.Amount = "";
            invoiceVat.Description = "VAT (0%)";
            _db.InvoiceListDetails.Add(invoiceVat);
            _db.SaveChanges();

            // Invoice - Commisions (Details 4)
            var invoiceCommision_OtherFees = new InvoiceListDetails();
            invoiceCommision_OtherFees.InvoiceListId = invoiceCommission.Id;
            invoiceCommision_OtherFees.Amount = Convert.ToString(total_detail4);
            invoiceCommision_OtherFees.Description = "Other fees (Payment processing fees)";
            _db.InvoiceListDetails.Add(invoiceCommision_OtherFees);
            _db.SaveChanges();

            // Invoice - Commisions (Details 5)
            var invoiceCommision_Totalamount = new InvoiceListDetails();
            invoiceCommision_Totalamount.InvoiceListId = invoiceCommission.Id;
            invoiceCommision_Totalamount.Amount = invoiceCommission.TotalAmount;
            invoiceCommision_Totalamount.Description = "Total amount";
            _db.InvoiceListDetails.Add(invoiceCommision_Totalamount);
            _db.SaveChanges();
            #endregion
        }

        //SaveCustomSolutionData
        [HttpPost]
        [Route("SaveCustomSolutionData")]
        public async Task<IActionResult> SaveCustomSolutionData([FromBody] CustomSolutionModel model)
        {

            try
            {
                var solutionIndustryDetails = await _db.SolutionIndustryDetails.Where(x => x.IndustryId == model.IndustryId
                                              && x.SolutionId == model.SolutionId).FirstOrDefaultAsync();
                if (solutionIndustryDetails != null)
                {
                    if (model.IsSingleFreelancer)
                    {
                        if (model.SingleFreelancer == "Expert")
                        {
                            model.TotalExpert = "1";
                        }
                        if (model.SingleFreelancer == "Associate")
                        {
                            model.TotalAssociate = "1";
                        }
                        if (model.SingleFreelancer == "ProjectManager")
                        {
                            model.TotalProjectManager = "1";
                        }
                    }

                    //FreelancerFinderHelper helper = new FreelancerFinderHelper();
                    //await helper.FindFreelancersAsync(_db, model.UserId, model.ProjectType, model.SolutionId, model.IndustryId, Convert.ToInt32(model.TotalProjectManager), Convert.ToInt32(model.TotalExpert), Convert.ToInt32(model.TotalAssociate));


                    var teamSize = Convert.ToInt16(model.TotalAssociate) + Convert.ToInt16(model.TotalExpert) + Convert.ToInt16(model.TotalProjectManager);
                    var solutionDefine = new SolutionDefine()
                    {
                        SolutionIndustryDetailsId = solutionIndustryDetails.Id,
                        ProjectOutline = model.CustomProjectOutline,
                        ProjectDetails = model.CustomProjectDetail,
                        ProjectType = model.ProjectType,
                        IsActive = true,
                        CreatedDateTime = DateTime.Now,
                        TeamSize = Convert.ToInt16(teamSize),
                        Duration = model.CustomProjectDuration
                    };

                    _db.SolutionDefine.Add(solutionDefine);
                    _db.SaveChanges();

                    var customsolution = new CustomProjectDetials()
                    {
                        SolutionDefineId = solutionDefine.Id,
                        EstimatedPrice = model.CustomPrice,
                        StartDate = DateTime.Parse(model.CustomStartDate),
                        EndDate = DateTime.Parse(model.CustomEndDate),
                        StartHour = DateTime.Parse(model.CustomStartHour),
                        EndHour = DateTime.Parse(model.CustomEndHour),
                        ClientId = model.UserId,
                        Associate = model.TotalAssociate.ToString(),
                        Expert = model.TotalExpert.ToString(),
                        ProjectManager = model.TotalProjectManager.ToString(),
                        IsSingleFreelancer = model.IsSingleFreelancer,
                        ProjectDuration = model.CustomProjectDuration,
                        IsExcludeWeekend = model.CustomExcludeWeekend,
                        OtherHolidays = model.CustomOtherHolidayList,

                    };

                    var customDetail = _db.CustomProjectDetials.Add(customsolution);
                    _db.SaveChanges();

                    var solutionfund = new SolutionFund()
                    {
                        SolutionId = model.SolutionId,
                        IndustryId = model.IndustryId,
                        ClientId = model.UserId,
                        ProjectType = model.ProjectType.ToLower(),
                        ProjectPrice = model.CustomPrice.ToString(),
                        ProjectStatus = "INITIATED",
                        FundType = SolutionFund.FundTypes.ProjectFund,
                        CustomProjectDetialsId = customsolution.Id
                    };
                    _db.SolutionFund.Add(solutionfund);
                    _db.SaveChanges();

                    var solutionstatus = await SaveSolutionTeamData(solutionfund, Convert.ToInt32(model.TotalAssociate), Convert.ToInt32(model.TotalExpert), Convert.ToInt32(model.TotalProjectManager));
                    if (solutionstatus == "success")
                    {
                        return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                        {
                            StatusCode = StatusCodes.Status200OK,
                            Message = "Project Initated Successfully!",
                            Result = customDetail.Entity.Id
                        });
                    }
                }
                return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                {
                    StatusCode = StatusCodes.Status200OK,
                    Message = "Something Went Wrong While inititaing Project Please try again later!"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                {
                    StatusCode = StatusCodes.Status200OK,
                    Message = ex.Message + ex.InnerException
                });

            }
        }

        private async Task UpdateInvoiceNumberAsync(int id, string invoicePrefix)
        {
            if (id > 0)
            {
                string invoiceNumber = invoicePrefix + "-" + id.ToString("D5");

                var invoiceModel = await _db.InvoiceList.FirstOrDefaultAsync(x => x.Id == id);
                if (invoiceModel != null)
                {
                    invoiceModel.InvoiceNumber = invoiceNumber;
                }
                _db.InvoiceList.Update(invoiceModel);
                await _db.SaveChangesAsync();
            }
        }


        //GetClientWorkingHours
        [HttpPost]
        [Route("GetClientWorkingHours")]
        public async Task<IActionResult> GetClientWorkingHours([FromBody] UserIdModel model)
        {

            try
            {
                if (model.UserId != null)
                {
                    var clientData = await _db.Users.Where(x => x.Id == model.UserId).FirstOrDefaultAsync();
                    if (clientData != null)
                    {
                        UserDetailsModel clientDetails = new UserDetailsModel();
                        clientDetails.StartHour = clientData.StartHours;
                        clientDetails.EndHour = clientData.EndHours;
                        clientDetails.onSunday = clientData.onSunday;
                        clientDetails.onMonday = clientData.onMonday;
                        clientDetails.onTuesday = clientData.onTuesday;
                        clientDetails.onWednesday = clientData.onWednesday;
                        clientDetails.onThursday = clientData.onThursday;
                        clientDetails.onFriday = clientData.onFriday;
                        clientDetails.onSaturday = clientData.onSaturday;

                        return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                        {
                            StatusCode = StatusCodes.Status200OK,
                            Message = "success",
                            Result = clientDetails
                        });
                    }
                }
                return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                {
                    StatusCode = StatusCodes.Status200OK,
                    Message = "User not Found"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                {
                    StatusCode = StatusCodes.Status200OK,
                    Message = ex.Message + ex.InnerException
                });

            }
        }

        //GetPopularAiSolution
        [HttpGet]
        [Route("GetPopularAiSolution")]
        public async Task<IActionResult> GetPopularAiSolution()
        {

            try
            {
                List<Solutions> FinalSolutionList = new List<Solutions>();
                var solutionList = await _db.SolutionFund.ToListAsync();
                if(solutionList.Count > 0)
                {
                    
                    var solutionGroupBy = solutionList.GroupBy(x => new { x.SolutionId, x.IndustryId }).Select(cl => new SolutionFund { SolutionId = cl.Key.SolutionId, IndustryId = cl.Key.IndustryId });
                    foreach(var solutions in solutionGroupBy)
                    {
                        var solutionData = _db.Solutions.Where(x => x.Id == solutions.SolutionId).FirstOrDefault();
                        if(solutionData != null )
                        {
                            FinalSolutionList.Add(solutionData);
                        }
                    }
                    return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                    {
                        StatusCode = StatusCodes.Status200OK,
                        Message = "success",
                        Result = FinalSolutionList
                    });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                {
                    StatusCode = StatusCodes.Status200OK,
                    Message = ex.Message + ex.InnerException
                });

            }
            return StatusCode(StatusCodes.Status200OK, new APIResponseModel
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "No data found"
            });
        }

    }
}