using Aephy.API.DBHelper;
using Aephy.API.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using static Aephy.API.Models.AdminViewModel;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Aephy.API.Controllers
{
    [Route("api/Client/")]
    [ApiController]
    public class ClientController : ControllerBase
    {
        private readonly AephyAppDbContext _db;
        public ClientController(AephyAppDbContext dbContext)
        {
            _db = dbContext;
        }

        [HttpPost]
        [Route("GetPopularSolutionBasedOnServices")]
        public async Task<IActionResult> GetPopularSolutionBasedOnServices([FromBody] MileStoneIdViewModel model)
        {
            if(model.Id != 0)
            {
                try
                {
                    List<SolutionsModel> solutionsModel = new List<SolutionsModel>();
                    List<Industries> industrylistDetails = new List<Industries>();
                    List<string> industrylist = new List<string>();
                    var serviceData = _db.SolutionServices.Where(x => x.ServicesId == model.Id).Select(x => x.SolutionId).ToList();

                    if (serviceData.Count > 0)
                    {
                        foreach (var data in serviceData)
                        {
                            var solutiondata = _db.Solutions.Where(x => x.Id == data).FirstOrDefault();
                            var industryList = _db.SolutionIndustry.Where(x => x.SolutionId == data).Select(x => x.IndustryId).ToList();
                            foreach (var industryId in industryList)
                            {
                                var industry = _db.Industries.Where(x => x.Id == industryId).FirstOrDefault();
                                industrylistDetails.Add(industry);
                                var industryname = _db.Industries.Where(x => x.Id == industryId).Select(x => x.IndustryName).FirstOrDefault();
                                industrylist.Add(industryname);
                            }
                            SolutionsModel dataStore = new SolutionsModel();
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
                    return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                    {
                        StatusCode = StatusCodes.Status200OK,
                        Message = "Success",
                        Result = new
                        {
                           SolutionData =  solutionsModel,
                           IndustriesData = industrylistDetails.Distinct()
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
                    List<Solutions> solutionList = _db.Solutions.Take(3).ToList();
                    List<SolutionsModel> solutionsModel = new List<SolutionsModel>();
                    List<Industries> industrylistDetails = new List<Industries>();
                    List<string> industrylist = new List<string>();

                    if (solutionList.Count > 0)
                    {
                        foreach (var list in solutionList)
                        {
                            var serviceId = _db.SolutionServices.Where(x => x.SolutionId == list.Id).Select(x => x.ServicesId).FirstOrDefault();
                            var Servicename = _db.Services.Where(x => x.Id == serviceId).Select(x => x.ServicesName).FirstOrDefault();

                            var industryIdlist = _db.SolutionIndustry.Where(x => x.SolutionId == list.Id).Select(x => x.IndustryId).ToList();
                            foreach (var industryId in industryIdlist)
                            {
                                var industry = _db.Industries.Where(x => x.Id == industryId).FirstOrDefault();
                                industrylistDetails.Add(industry);
                                var industryname = _db.Industries.Where(x => x.Id == industryId).Select(x => x.IndustryName).FirstOrDefault();
                                industrylist.Add(industryname);
                            }
                            SolutionsModel dataStore = new SolutionsModel();
                            dataStore.Services = Servicename;
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
                    return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                    {
                        StatusCode = StatusCodes.Status200OK,
                        Message = "Success",
                        Result = new
                        {
                            SolutionData = solutionsModel,
                            IndustriesData = industrylistDetails.Distinct()
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
                    List<SolutionsModel> solutionsModel = new List<SolutionsModel>();
                    List<Industries> industrylistDetails = new List<Industries>();
                    List<string> industrylist = new List<string>();
                    var solutiondata = _db.Solutions.Where(x => x.Id == model.Id).FirstOrDefault();
                    var industryList = _db.SolutionIndustry.Where(x => x.SolutionId == solutiondata.Id).Select(x => x.IndustryId).ToList();
                    if(industryList.Count > 0)
                    {
                        foreach (var industryId in industryList)
                        {
                            var industry = _db.Industries.Where(x => x.Id == industryId).FirstOrDefault();
                            industrylistDetails.Add(industry);
                            var industryname = _db.Industries.Where(x => x.Id == industryId).Select(x => x.IndustryName).FirstOrDefault();
                            industrylist.Add(industryname);
                        }
                    }

                    SolutionsModel dataStore = new SolutionsModel();
                    dataStore.Industries = string.Join(",", industrylist);
                   // dataStore.solutionIndustriesList = industrylistDetails;
                    dataStore.Id = solutiondata.Id;
                    dataStore.Description = solutiondata.Description;
                    dataStore.ImagePath = solutiondata.ImagePath;
                    dataStore.ImageUrlWithSas = solutiondata.ImageUrlWithSas;
                    dataStore.Title = solutiondata.Title;
                    solutionsModel.Add(dataStore);
                    return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                    {
                        StatusCode = StatusCodes.Status200OK,
                        Message = "Success",
                        Result = new
                        {
                            SolutionData = solutionsModel,
                            IndustriesData = industrylistDetails
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
                    List<Solutions> solutionList = _db.Solutions.Take(3).ToList();
                    List<SolutionsModel> solutionsModel = new List<SolutionsModel>();
                    List<string> industrylist = new List<string>();

                    if (solutionList.Count > 0)
                    {
                        foreach (var list in solutionList)
                        {
                            var serviceId = _db.SolutionServices.Where(x => x.SolutionId == list.Id).Select(x => x.ServicesId).FirstOrDefault();
                            var Servicename = _db.Services.Where(x => x.Id == serviceId).Select(x => x.ServicesName).FirstOrDefault();

                            var industryIdlist = _db.SolutionIndustry.Where(x => x.SolutionId == list.Id).Select(x => x.IndustryId).ToList();
                            foreach (var industryId in industryIdlist)
                            {
                                var industryname = _db.Industries.Where(x => x.Id == industryId).Select(x => x.IndustryName).FirstOrDefault();
                                industrylist.Add(industryname);
                            }
                            SolutionsModel dataStore = new SolutionsModel();
                            dataStore.Services = Servicename;
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
        }

        [HttpGet]
        [Route("GetPopularSolutionList")]
        public async Task<IActionResult> GetPopularSolutionList()
        {
            try
            {
                List<Solutions> solutionList = _db.Solutions.Take(3).ToList();
                List<SolutionsModel> solutionsModel = new List<SolutionsModel>();
                List<Industries> industrylistDetails = new List<Industries>();

                if (solutionList.Count > 0)
                {
                    foreach (var list in solutionList)
                    {
                        var serviceId = _db.SolutionServices.Where(x => x.SolutionId == list.Id).Select(x => x.ServicesId).FirstOrDefault();
                        var Servicename = _db.Services.Where(x => x.Id == serviceId).Select(x => x.ServicesName).FirstOrDefault();

                        var industryIdlist = _db.SolutionIndustry.Where(x => x.SolutionId == list.Id).Select(x => x.IndustryId).ToList();
                        foreach (var industryId in industryIdlist)
                        {
                            var industry = _db.Industries.Where(x => x.Id == industryId).FirstOrDefault();
                            industrylistDetails.Add(industry);
                        }
                        SolutionsModel dataStore = new SolutionsModel();
                        dataStore.Services = Servicename;
                       // dataStore.solutionIndustriesList = industrylistDetails;
                        dataStore.Id = list.Id;
                        dataStore.Description = list.Description;
                        dataStore.ImagePath = list.ImagePath;
                        dataStore.ImageUrlWithSas = list.ImageUrlWithSas;
                        dataStore.Title = list.Title;
                        dataStore.SubTitle = list.SubTitle;
                        solutionsModel.Add(dataStore);
                        industrylistDetails.Clear();
                    }
                }
                return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                {
                    StatusCode = StatusCodes.Status200OK,
                    Message = "Success",
                    Result =  solutionsModel
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
}
