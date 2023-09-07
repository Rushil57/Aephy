﻿using Aephy.API.DBHelper;
using Aephy.API.Models;
using Aephy.API.Stripe;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Stripe;
using Stripe.Checkout;
using Stripe.Identity;
using System.Collections.Generic;
using System.Xml.Schema;
using static Aephy.API.DBHelper.ApplicationUser;
using static Aephy.API.Models.AdminViewModel;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Aephy.API.Controllers
{
    [Route("api/Client/")]
    [ApiController]
    public class ClientController : ControllerBase
    {
        private readonly AephyAppDbContext _db;
        private readonly IStripeAccountService _stripeAccountService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;
        public ClientController(AephyAppDbContext dbContext, IStripeAccountService stripeAccountService, UserManager<ApplicationUser> userManager, IConfiguration configuration)
        {
            _db = dbContext;
            _configuration = configuration;
            _stripeAccountService = stripeAccountService;
            _userManager = userManager;
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
                        fundProgress = await _db.SolutionFund.Where(x => x.SolutionId == model.SolutionId && x.IndustryId == model.IndustryId && x.ClientId == model.UserId).FirstOrDefaultAsync();
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

                    //SolutionFund fundProgress = new SolutionFund();
                    //if (model.UserId != "")
                    //{
                    //    fundProgress = _db.SolutionFund.Where(x => x.SolutionId == model.SolutionId && x.IndustryId == model.IndustryId && x.ClientId == model.UserId).FirstOrDefault();
                    //}


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
                            SolutionFund = fundProgress
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
                    var industryname = _db.Industries.Where(x => x.Id == model.IndustryId).Select(x => x.IndustryName).FirstOrDefault();
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
                        SolutionTopProfessionalModel solutionTop = new SolutionTopProfessionalModel();
                        solutionTop.Description = topdata.Description;
                        solutionTop.Title = topdata.TopProfessionalTitle;
                        var fullname = _db.Users.Where(x => x.Id == topdata.FreelancerId).Select(x => new { x.FirstName, x.LastName }).FirstOrDefault();

                        solutionTop.FreelancerId = fullname.FirstName + " " + fullname.LastName;
                        solutionTop.ImagePath = _db.FreelancerDetails.Where(x => x.UserId == topdata.FreelancerId).Select(x => x.ImagePath).FirstOrDefault();
                        solutionTop.Rate = topdata.Rate;
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
        [HttpPost]
        [Route("CreateUserStripeAccount")]
        public async Task<IActionResult> CreateUserStripeAccount([FromBody] MileStoneIdViewModel model)
        {
            var userDetails = _db.Users.Where(x => x.Id == model.UserId).FirstOrDefault();
            if (userDetails != null)
            {
                StripeConfiguration.ApiKey = "sk_test_51NaxGxLHv0zYK8g4ZEh9KncjP5T6hbERI8VIn5bKUZvuY36xCSfp99bdrH5Td65cXkJ5FgDdMFVbmAao6xfm8Wje00pAJrWOjf";
                // Connected Account creation.
                if (userDetails.StripeAccountStatus == ApplicationUser.StripeAccountStatuses.NotCreated)
                {
                    userDetails.StripeConnectedId = _stripeAccountService.CreateStripeAccount(StripeConfiguration.ApiKey);

                    if (!string.IsNullOrEmpty(userDetails.StripeConnectedId))
                    {
                        userDetails.StripeAccountStatus = ApplicationUser.StripeAccountStatuses.Initiated;
                        _db.Users.Update(userDetails);
                        _db.SaveChanges();

                        return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                        {
                            StatusCode = StatusCodes.Status200OK,
                            Message = "User Stripe Account Created",
                            Result = userDetails
                        });
                    }
                    else
                    {
                        return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                        {
                            StatusCode = StatusCodes.Status200OK,
                            Message = "Something Went Wrong",
                        });
                    }
                }
            }

            return StatusCode(StatusCodes.Status200OK, new APIResponseModel
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "Something Went Wrong",
            });
        }

        //GetUserStripeDetails
        [HttpPost]
        [Route("GetUserStripeDetails")]
        public async Task<IActionResult> GetUserStripeDetails([FromBody] MileStoneIdViewModel model)
        {
            var userDetails = await _db.Users.Where(x => x.Id == model.UserId).FirstOrDefaultAsync();
            if (userDetails != null)
            {

                StripeConfiguration.ApiKey = "sk_test_51NaxGxLHv0zYK8g4ZEh9KncjP5T6hbERI8VIn5bKUZvuY36xCSfp99bdrH5Td65cXkJ5FgDdMFVbmAao6xfm8Wje00pAJrWOjf";

                // checking Status of the account
                if (_stripeAccountService.IsComplete(userDetails.StripeConnectedId))
                {
                    userDetails.StripeAccountStatus = ApplicationUser.StripeAccountStatuses.Complete;
                    _db.Users.Update(userDetails);
                    _db.SaveChanges();
                    return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                    {
                        StatusCode = StatusCodes.Status200OK,
                        Message = "User Updated Successsfully",
                        Result = new
                        {
                            UserDetails = userDetails,
                            IsCompleted = true
                        }
                    });
                }
                // incase account is not complete 

                // check if accidently landed user has not created the account.
                if (userDetails.StripeAccountStatus == ApplicationUser.StripeAccountStatuses.NotCreated)
                {
                    userDetails.StripeConnectedId = _stripeAccountService.CreateStripeAccount(StripeConfiguration.ApiKey);
                    userDetails.StripeAccountStatus = ApplicationUser.StripeAccountStatuses.Initiated;
                    _db.Users.Update(userDetails);
                    _db.SaveChanges();

                    return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                    {
                        StatusCode = StatusCodes.Status200OK,
                        Message = "User Created Successsfully",
                        Result = new
                        {
                            UserDetails = userDetails,
                            IsCompleted = false
                        }
                    });
                }
            }

            return StatusCode(StatusCodes.Status200OK, new APIResponseModel
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "Something Went Wrong",
            });
        }

        [HttpPost]
        [Route("CheckOut")]
        public async Task<IActionResult> CheckOut([FromBody] SolutionIndustryDetailsModel model)
        {
            try
            {
                var mileStone = _db.SolutionMilestone.Where(x => x.Id == model.Id).FirstOrDefault();
                var contractData = await _db.Contract.Where(x => x.MilestoneDataId == model.Id).FirstOrDefaultAsync();

                if (contractData == null)
                {
                    if (model.Id != 0)
                    {
                        Contract? contractSave;
                        if (model.MileStoneCheckout)
                        {
                            contractSave = new Contract()
                            {
                                ClientUserId = model.UserId,
                                MilestoneDataId = mileStone.Id,
                                //MileStone = mileStone,
                                PaymentStatus = Contract.PaymentStatuses.ContractCreated,
                                PaymentIntentId = string.Empty,
                                SolutionFundId = model.SolutionFundId,
                                SolutionId = model.SolutionId,
                                IndustryId = model.IndustryId,

                            };
                            _db.Contract.Add(contractSave);
                            _db.SaveChanges();

                            List<ContractUser> contractUsers = new List<ContractUser>();
                            var fl = _db.Users.Where(x => x.UserType == "Freelancer" && x.StripeAccountStatus == StripeAccountStatuses.Complete
                            && !string.IsNullOrEmpty(x.StripeConnectedId)).ToList();
                            foreach (var item in fl)
                            {
                                contractUsers.Add(new ContractUser()
                                {
                                    Percentage = 10,
                                    StripeTranferId = string.Empty,
                                    IsTransfered = false,
                                    ApplicationUserId = item.Id,
                                    ContractId = contractSave.Id
                                });
                            }
                            _db.ContractUser.AddRange(contractUsers);
                            _db.SaveChanges();
                        }
                        else
                        {
                            var contractDetails = _db.Contract.Where(x => x.SolutionId == model.SolutionId && x.IndustryId == model.IndustryId && x.ClientUserId == model.UserId).FirstOrDefault();
                            if(contractDetails == null)
                            {
                                contractSave = new Contract()
                                {
                                    ClientUserId = model.UserId,
                                    SolutionId = model.SolutionId,
                                    IndustryId = model.IndustryId,
                                    //MileStone = mileStone,
                                    PaymentStatus = Contract.PaymentStatuses.ContractCreated,
                                    PaymentIntentId = string.Empty,
                                    SolutionFundId = model.SolutionFundId,

                                };
                                _db.Contract.Add(contractSave);
                                _db.SaveChanges();

                                List<ContractUser> contractUsers = new List<ContractUser>();
                                var fl = _db.Users.Where(x => x.UserType == "Freelancer" && x.StripeAccountStatus == StripeAccountStatuses.Complete
                                && !string.IsNullOrEmpty(x.StripeConnectedId)).ToList();
                                foreach (var item in fl)
                                {
                                    contractUsers.Add(new ContractUser()
                                    {
                                        Percentage = 10,
                                        StripeTranferId = string.Empty,
                                        IsTransfered = false,
                                        ApplicationUserId = item.Id,
                                        ContractId = contractSave.Id
                                    });
                                }
                                _db.ContractUser.AddRange(contractUsers);
                                _db.SaveChanges();
                            }
                            else
                            {
                                var solutionFundData = _db.SolutionFund.Where(x => x.Id == contractDetails.SolutionFundId).FirstOrDefault();
                                if(solutionFundData != null)
                                {
                                    if(solutionFundData.ProjectStatus == "COMPLETED")
                                    {
                                        return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                                        {
                                            StatusCode = StatusCodes.Status200OK,
                                            Message = "Payment is already done for this project",
                                        });
                                    }
                                }
                            }
                           
                        }
                       
                    }

                }
                //var contract = _db.Contract.Include("ContractUsers").FirstOrDefault(x => x.MileStone.Id == model.Id);
                var contract = new Contract();
                if (model.MileStoneCheckout)
                {
                    contract = _db.Contract.Include("ContractUsers").FirstOrDefault(x => x.MilestoneDataId == model.Id);
                }
                else
                {
                    contract = _db.Contract.Include("ContractUsers").FirstOrDefault(x => x.SolutionId == model.SolutionId && x.IndustryId == model.IndustryId);
                }
                

                var domain = _configuration.GetValue<string>("DomainUrl:Domain");
                var successUrl = string.Format("{0}/LandingPage/CheckoutSuccess?cntId={1}", domain, contract.Id);
                var cancelUrl = string.Format("{0}/LandingPage/CheckoutCancel?cntId={1}", domain, contract.Id);

                Contract.PaymentStatuses payment = contract.PaymentStatus;
                if (payment == Contract.PaymentStatuses.ContractCreated)
                {
                    Session session = new Session();
                    if (model.MileStoneCheckout)
                    {
                        session = _stripeAccountService.CreateCheckoutSession(mileStone, successUrl, cancelUrl);
                    }
                    else
                    {
                        session = _stripeAccountService.CreateProjectCheckoutSession(successUrl, cancelUrl);
                    }

                    if (session == null || string.IsNullOrEmpty(session.Id))
                    {
                        //Response.Headers.Add("Location", domain + "/LandingPage/Project");
                        //return new StatusCodeResult(303);
                        var emptyStringUrl = domain + "/LandingPage/Project";
                        return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                        {
                            StatusCode = StatusCodes.Status200OK,
                            Result = emptyStringUrl
                        });
                    }

                    //checkout initiated successful
                    contract.SessionId = session.Id;
                    contract.SessionExpiry = session.ExpiresAt;
                    contract.SessionStatus = _stripeAccountService.GetSesssionStatus(session);
                    contract.PaymentStatus = _stripeAccountService.GetPaymentStatus(session);

                    _db.Update(contract);
                    _db.SaveChanges();

                    //Response.Headers.Add("Location", session.Url);
                    //return new StatusCodeResult(303);
                    return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                    {
                        StatusCode = StatusCodes.Status200OK,
                        Message = "success",
                        Result = session.Url
                    });
                }
                else if (contract.PaymentStatus == Contract.PaymentStatuses.UnPaid)
                {
                    var checkoutSession = _stripeAccountService.GetCheckOutSesssion(contract.SessionId);

                    if (checkoutSession != null)
                    {
                        contract.SessionStatus = _stripeAccountService.GetSesssionStatus(checkoutSession);

                        if (contract.SessionStatus == Contract.SessionStatuses.Open)
                        {
                            //Response.Headers.Add("Location", checkoutSession.Url);
                            //return new StatusCodeResult(303);
                            return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                            {
                                StatusCode = StatusCodes.Status200OK,
                                Message = "success",
                                Result = checkoutSession.Url
                            });
                        }
                        else
                        {
                            Session session = _stripeAccountService.CreateCheckoutSession(mileStone, successUrl, cancelUrl);

                            if (session == null || string.IsNullOrEmpty(session.Id))
                            {
                                //Response.Headers.Add("Location", domain + "/Checkout");
                                //return new StatusCodeResult(303);
                                return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                                {
                                    StatusCode = StatusCodes.Status200OK,
                                    Message = "success",
                                    Result = domain + "/LandingPage/Project"
                                });
                            }

                            //checkout initiated successful
                            contract.SessionId = session.Id;
                            contract.SessionExpiry = session.ExpiresAt;
                            contract.SessionStatus = _stripeAccountService.GetSesssionStatus(session);
                            contract.PaymentStatus = _stripeAccountService.GetPaymentStatus(session);

                            _db.Update(contract);
                            _db.SaveChanges();

                            //Response.Headers.Add("Location", session.Url);
                            //return new StatusCodeResult(303);
                            return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                            {
                                StatusCode = StatusCodes.Status200OK,
                                Message = "success",
                                Result = session.Url
                            });
                        }
                    }
                    else
                    {
                        //Response.Headers.Add("Location", successUrl);
                        //return new StatusCodeResult(303);
                        return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                        {
                            StatusCode = StatusCodes.Status200OK,
                            Message = "success",
                            Result = successUrl
                        });
                    }
                }
                else
                {
                    //Response.Headers.Add("Location", successUrl);
                    //return new StatusCodeResult(303);
                    return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                    {
                        StatusCode = StatusCodes.Status200OK,
                        Message = "success",
                        Result = successUrl
                    });
                }
            }
            catch (Exception ex)
            {

            }

            return StatusCode(StatusCodes.Status200OK, new APIResponseModel
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "Something Went Wrong",
            });
        }

        //GetCheckoutMileStoneData
        [HttpPost]
        [Route("GetCheckoutMileStoneData")]
        public async Task<IActionResult> GetCheckoutMileStoneData([FromBody] MileStoneDetailsViewModel model)
        {
            try
            {
                var milestoneData = _db.SolutionMilestone.Where(x => x.SolutionId == model.SolutionId && x.IndustryId == model.IndustryId).FirstOrDefault();
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
        public async Task<IActionResult> GetUserSuccessCheckoutDetails([FromBody] MileStoneIdViewModel model)
        {
            var user = await _db.Users.Where(x => x.Id == model.UserId).FirstOrDefaultAsync();

            if (user != null)
            {
                // Please note that this logic is verification of the encrypted string that is set for specific contract and logged in user is owner of the contract.
                var contract = _db.Contract.FirstOrDefault(x => x.Id == model.Id && x.ClientUserId == user.Id);


                if (contract != null)
                {
                    var checkoutSession = _stripeAccountService.GetCheckOutSesssion(contract.SessionId);

                    if (checkoutSession != null)
                    {
                        contract.SessionStatus = _stripeAccountService.GetSesssionStatus(checkoutSession);
                        contract.PaymentStatus = _stripeAccountService.GetPaymentStatus(checkoutSession);

                        if (contract.PaymentStatus == Contract.PaymentStatuses.Paid && contract.SessionStatus == Contract.SessionStatuses.Complete)
                        {
                            contract.PaymentIntentId = checkoutSession.PaymentIntentId;

                            var paymentIntent = _stripeAccountService.GetPaymentIntent(contract.PaymentIntentId);

                            if (paymentIntent != null && !string.IsNullOrEmpty(paymentIntent.LatestChargeId))
                            {
                                contract.LatestChargeId = paymentIntent.LatestChargeId;
                                contract.PaymentStatus = Contract.PaymentStatuses.Paid;
                                _db.SaveChanges();

                                //var data = _db.SolutionFund.Where(x => x.MileStoneId == contract.MileStoneId).FirstOrDefault();
                                var data = _db.SolutionFund.Where(x => x.MileStoneId == contract.MilestoneDataId).FirstOrDefault();
                                if (data != null)
                                {
                                    data.IsCheckOutDone = true;
                                    data.ProjectStatus = "COMPLETED";
                                    data.IsArchived = true;
                                    _db.SaveChanges();
                                }
                            }

                            return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                            {
                                StatusCode = StatusCodes.Status200OK,
                                Message = "Your payment is compeleted successfully and in escrow. Incase of approved successfully it will be transfered to all stakeholders(Freelances, Architects and Platfom)",
                            });
                        }
                        else if (contract.PaymentStatus == Contract.PaymentStatuses.NoPaymentRequired)
                        {
                            return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                            {
                                StatusCode = StatusCodes.Status200OK,
                                Message = "Your payment is in no payment required state. The payment is delayed to a future date, or the Checkout Session is in setup mode and doesn’t require a payment at this time.",
                            });
                            //ViewData["Message"] = "Your payment is in no payment required state. The payment is delayed to a future date, or the Checkout Session is in setup mode and doesn’t require a payment at this time.";
                        }
                        else if (contract.PaymentStatus == Contract.PaymentStatuses.UnPaid)
                        {
                            return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                            {
                                StatusCode = StatusCodes.Status200OK,
                                Message = "Your payment is in upaid state yet.We have not received the payment.",
                            });
                            //ViewData["Message"] = "Your payment is in upaid state yet.We have not received the payment.";
                        }
                    }

                    _db.Contract.Update(contract);
                    _db.SaveChanges();
                }
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


        //SaveProjectInitiated
        [HttpPost]
        [Route("SaveProjectInitiated")]
        public async Task<IActionResult> SaveProjectInitiated([FromBody] solutionFundViewModel model)
        {
            if (model != null)
            {
                if (model.Id == 0)
                {
                    var mileStoneData = _db.SolutionMilestone.Where(x => x.SolutionId == model.SolutionId && x.IndustryId == model.IndustryId && x.ProjectType == model.ProjectType).FirstOrDefault();
                    //var projectDetails = _db.SolutionFund.Where(x => x.SolutionId == model.SolutionId && x.IndustryId == model.IndustryId && x.ProjectType == model.ProjectType).FirstOrDefault();
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
                            ProjectType = model.ProjectType,
                            ProjectPrice = model.ProjectPrice,
                            ProjectStatus = "INITIATED",
                            FundType = model.FundType
                        };
                        _db.SolutionFund.Add(solutionfund);
                        _db.SaveChanges();

                        var projectDetails = _db.SolutionFund.Where(x => x.SolutionId == model.SolutionId && x.IndustryId == model.IndustryId && x.ProjectType == model.ProjectType).FirstOrDefault();

                        return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                        {
                            StatusCode = StatusCodes.Status200OK,
                            Message = "projectDetails",
                            Result = new
                            {
                                ProjectDetails = projectDetails,
                                MileStoneData = mileStoneData
                            }
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
                    var data = _db.SolutionFund.Where(x => x.SolutionId == model.SolutionId && x.IndustryId == model.IndustryId && x.ProjectType == model.ProjectType && x.ClientId == model.ClientId).FirstOrDefault();
                    if (data != null)
                    {
                        if (model.MileStoneCheckout)
                        {
                            model.FundType = SolutionFund.FundTypes.MilestoneFund;
                        }
                        else
                        {
                            model.FundType = SolutionFund.FundTypes.ProjectFund;
                        }

                        if (data.ProjectStatus == "INITIATED")
                        {
                            data.ProjectStatus = "INPROGRESS";
                            data.MileStoneId = model.MileStoneId;
                            data.FundType = model.FundType;
                            _db.SaveChanges();

                            var mileStoneData = _db.SolutionMilestone.Where(x => x.Id == model.MileStoneId).FirstOrDefault();

                            return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                            {
                                StatusCode = StatusCodes.Status200OK,
                                Message = "CompleteProcess",
                                Result = new
                                {
                                    ProjectDetails = data,
                                    MileStoneData = mileStoneData
                                }
                            });
                        }

                        //if (data.ProjectStatus == "INPROGRESS")
                        //{
                        //    //data.ProjectStatus = "COMPLETED";
                        //    //data.IsArchived = true;
                        //    //_db.SaveChanges();

                        //    //var mileStoneData = _db.SolutionMilestone.Where(x => x.Id == model.MileStoneId).FirstOrDefault();

                        //    return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                        //    {
                        //        StatusCode = StatusCodes.Status200OK,
                        //        Message = "CompleteProcess"
                        //    });
                        //}

                    }
                }

                return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                {
                    StatusCode = StatusCodes.Status200OK,
                    Message = "Data Not Found"
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
                            if(checkDisputeData != null)
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
                catch(Exception ex)
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

    }
}

