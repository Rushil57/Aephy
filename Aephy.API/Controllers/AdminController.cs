using Aephy.API.DBHelper;
using Aephy.API.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Azure;
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
        [Route("AddServices")]
        public async Task<IActionResult> AddServices([FromBody] ServicesModel model)
        {
            if (model.Id == 0)
            {
                try
                {
                    var CheckServices = _db.Services.Where(x => x.ServicesName == model.ServiceName).FirstOrDefault();
                    if (CheckServices != null)
                    {
                        return StatusCode(StatusCodes.Status200OK, new APIResponseModel { StatusCode = StatusCodes.Status403Forbidden, Message = "Services Already Added!" });
                    }
                    var services = new Services()
                    {
                        ServicesName = model.ServiceName,
                        Active = model.Active
                    };
                    _db.Services.Add(services);
                    _db.SaveChanges();

                    return StatusCode(StatusCodes.Status200OK, new APIResponseModel { StatusCode = StatusCodes.Status200OK, Message = "Data Saved Succesfully!." });
                }
                catch (Exception ex)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, new APIResponseModel { StatusCode = StatusCodes.Status403Forbidden, Message = "Something Went Wrong." });
                }
            }
            else
            {
                try
                {
                    var servicesDetails = _db.Services.Where(x => x.Id == model.Id).FirstOrDefault();
                    if (servicesDetails != null)
                    {
                        servicesDetails.ServicesName = model.ServiceName;
                        servicesDetails.Active = model.Active;
                        _db.SaveChanges();
                    }
                    return StatusCode(StatusCodes.Status200OK, new APIResponseModel { StatusCode = StatusCodes.Status200OK, Message = "Data Updated Successfully." });
                }
                catch (Exception ex)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, new APIResponseModel { StatusCode = StatusCodes.Status403Forbidden, Message = "Something Went Wrong." });
                }
            }

        }


        [HttpGet]
        [Route("ServiceList")]
        public async Task<IActionResult> ServiceList()
        {
            var list = _db.Services.ToList();
            return StatusCode(StatusCodes.Status200OK, new APIResponseModel
            {
                StatusCode = StatusCodes.Status403Forbidden,
                Message = "Success",
                Result = list
            });

        }

        [HttpGet]
        [Route("GetServicesById")]
        public async Task<IActionResult> GetServicesById(int Id)
        {
            var servicesData = _db.Services.Where(x => x.Id == Id).FirstOrDefault();
            return StatusCode(StatusCodes.Status200OK, new APIResponseModel
            {
                StatusCode = StatusCodes.Status403Forbidden,
                Message = "Success",
                Result = servicesData
            });

        }



        [HttpPost]
        [Route("DeleteServicesById")]
        public async Task<IActionResult> DeleteServicesById([FromBody] ServicesModel model)
        {
            if (model.Id != 0)
            {
                try
                {
                    Services services = _db.Services.Find(model.Id);
                    if (services != null)
                    {
                        _db.Services.Remove(services);
                        _db.SaveChanges();
                    }
                    return StatusCode(StatusCodes.Status200OK, new APIResponseModel { StatusCode = StatusCodes.Status200OK, Message = "Delete Succesfully !" });
                }
                catch (Exception ex)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, new APIResponseModel { StatusCode = StatusCodes.Status403Forbidden, Message = "Something Went Wrong." });
                }
            }
            return StatusCode(StatusCodes.Status500InternalServerError, new APIResponseModel { StatusCode = StatusCodes.Status403Forbidden, Message = "Something Went Wrong." });

        }


        [HttpPost]
        [Route("AddorEditSolutionData")]
        public async Task<IActionResult> AddorEditSolutionData(SolutionsModel model)
        {
            if (model.Id == 0)
            {
                try
                {
                    var solution = new Solutions()
                    {
                        Title = model.Title,
                        SubTitle = model.SubTitle,
                        Description = model.Description
                    };
                    _db.Solutions.Add(solution);
                    _db.SaveChanges();

                    if (model.solutionIndustries.Count > 0)
                    {
                        foreach (var industry in model.solutionIndustries)
                        {
                            var solutionindustry = new SolutionIndustry()
                            {
                                SolutionId = solution.Id,
                                IndustryId = industry
                            };
                            _db.SolutionIndustry.Add(solutionindustry);
                            _db.SaveChanges();
                        }
                    }

                    if (model.solutionServices != null)
                    {

                        var solutionservices = new SolutionServices()
                        {
                            SolutionId = solution.Id,
                            ServicesId = model.solutionServices
                        };
                        _db.SolutionServices.Add(solutionservices);
                        _db.SaveChanges();

                    }
                    return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                    {
                        StatusCode = StatusCodes.Status200OK,
                        Message = "Data Saved Succesfully!.",
                        Result = solution.Id
                    });
                }
                catch (Exception ex)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, new APIResponseModel { StatusCode = StatusCodes.Status403Forbidden, Message = "Something Went Wrong." });
                }
            }
            else
            {
                try
                {
                    var solutiondata = _db.Solutions.Where(x => x.Id == model.Id).FirstOrDefault();
                    if (solutiondata != null)
                    {
                        solutiondata.Title = model.Title;
                        solutiondata.SubTitle = model.SubTitle;
                        solutiondata.Description = model.Description;
                        _db.SaveChanges();
                    }

                    var solutionindustrydata = _db.SolutionIndustry.Where(x => x.SolutionId == model.Id).ToList();
                    if (solutionindustrydata.Count > 0)
                    {
                        _db.SolutionIndustry.RemoveRange(solutionindustrydata);
                        _db.SaveChanges();

                        if (model.solutionIndustries.Count > 0)
                        {
                            foreach (var industry in model.solutionIndustries)
                            {
                                var solutionindustry = new SolutionIndustry()
                                {
                                    SolutionId = model.Id,
                                    IndustryId = industry
                                };
                                _db.SolutionIndustry.Add(solutionindustry);
                                _db.SaveChanges();
                            }
                        }
                    }

                    var solutionservices = _db.SolutionServices.Where(x => x.SolutionId == model.Id).ToList();
                    if (solutionservices.Count > 0)
                    {
                        _db.SolutionServices.RemoveRange(solutionservices);
                        _db.SaveChanges();

                        if (model.solutionServices != null)
                        {

                            var solutionindustry = new SolutionServices()
                            {
                                SolutionId = model.Id,
                                ServicesId = model.solutionServices
                            };
                            _db.SolutionServices.Add(solutionindustry);
                            _db.SaveChanges();

                        }
                    }

                    return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                    {
                        StatusCode = StatusCodes.Status200OK,
                        Message = "Data Saved Succesfully!.",
                        Result = solutiondata.ImagePath
                    });
                }
                catch (Exception ex)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, new APIResponseModel { StatusCode = StatusCodes.Status403Forbidden, Message = "Something Went Wrong." });
                }
            }
        }




        [HttpGet]
        [Route("SolutionList")]
        public async Task<IActionResult> SolutionList()
        {
            try
            {
                List<Solutions> solution = _db.Solutions.ToList();
                List<SolutionServices> solutionservice = _db.SolutionServices.ToList();
                List<SolutionIndustry> solutionindustry = _db.SolutionIndustry.ToList();
                List<Services> services = _db.Services.ToList();

                var data = from e in solution
                           select new SolutionsModel
                           {
                               Id = e.Id,
                               Title = e.Title,
                               SubTitle = e.SubTitle,
                               Description = e.Description,
                               ImageUrlWithSas = e.ImageUrlWithSas,
                               ImagePath = e.ImagePath,
                               Industries = string.Join(",", solutionindustry.FindAll(x => x.SolutionId == e.Id).Select(p => p.IndustryId)),
                               Services = string.Join(",", solutionservice.Where(x => x.SolutionId == e.Id).Select(c => c.ServicesId)),
                           };

                List<SolutionsModel> solutionsModel = new List<SolutionsModel>();
                List<string> industrylist = new List<string>();
                List<string> Serviceslist = new List<string>();

                foreach (var list in data)
                {
                    if (list.Industries != null)
                    {
                        var industries = list.Industries.Split(",");
                        foreach (var indus in industries)
                        {
                            var industryname = _db.Industries.Where(x => x.Id.ToString() == indus).Select(x => x.IndustryName).FirstOrDefault();

                            industrylist.Add(industryname);
                        }
                    }
                    if (list.Services != null)
                    {
                        var servicesdata = list.Services.Split(",");
                        foreach (var datas in servicesdata)
                        {
                            var servicename = _db.Services.Where(x => x.Id.ToString() == datas).Select(x => x.ServicesName).FirstOrDefault();
                            Serviceslist.Add(servicename);
                        }
                    }
                    SolutionsModel dataStore = new SolutionsModel();
                    dataStore.Services = string.Join(",", Serviceslist);
                    dataStore.Industries = string.Join(",", industrylist);
                    dataStore.Id = list.Id;
                    dataStore.Description = list.Description;
                    dataStore.ImagePath = list.ImagePath;
                    dataStore.ImageUrlWithSas = list.ImageUrlWithSas;
                    dataStore.Title = list.Title;
                    dataStore.SubTitle = list.SubTitle;
                    solutionsModel.Add(dataStore);
                    Serviceslist.Clear();
                    industrylist.Clear();
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

            return StatusCode(StatusCodes.Status200OK, new APIResponseModel
            {
                StatusCode = StatusCodes.Status403Forbidden,
                Message = "Something Went Wrong!"

            });
        }



        [HttpPost]
        [Route("DeleteSolutionById")]
        public async Task<IActionResult> DeleteSolutionById([FromBody] SolutionIdModel solutionsModel)
        {
            if (solutionsModel.Id != 0)
            {
                try
                {
                    var solutionindustryList = _db.SolutionIndustry.Where(x => x.SolutionId == solutionsModel.Id).ToList();
                    if (solutionindustryList.Count > 0)
                    {
                        _db.SolutionIndustry.RemoveRange(solutionindustryList);
                        _db.SaveChanges();
                    }

                    var solutionserviceslist = _db.SolutionServices.Where(x => x.SolutionId == solutionsModel.Id).ToList();
                    if (solutionserviceslist.Count > 0)
                    {
                        _db.SolutionServices.RemoveRange(solutionserviceslist);
                        _db.SaveChanges();
                    }
                    Solutions solutions = _db.Solutions.Find(solutionsModel.Id);
                    if (solutions != null)
                    {
                        _db.Solutions.Remove(solutions);
                        _db.SaveChanges();
                    }
                    return StatusCode(StatusCodes.Status200OK, new APIResponseModel { StatusCode = StatusCodes.Status200OK, Message = "Delete Succesfully !" });
                }
                catch (Exception ex)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, new APIResponseModel { StatusCode = StatusCodes.Status403Forbidden, Message = "Something Went Wrong." });
                }
            }
            return StatusCode(StatusCodes.Status500InternalServerError, new APIResponseModel { StatusCode = StatusCodes.Status403Forbidden, Message = "Something Went Wrong." });

        }

        [HttpPost]
        [Route("AddIndustries")]
        public async Task<IActionResult> AddIndustries([FromBody] Industries model)
        {
            if (model.Id == 0)
            {
                try
                {
                    var checkIndustries = _db.Industries.Where(x => x.IndustryName == model.IndustryName).FirstOrDefault();
                    if (checkIndustries != null)
                    {
                        return StatusCode(StatusCodes.Status200OK, new APIResponseModel { StatusCode = StatusCodes.Status403Forbidden, Message = "Industry Already Exists!" });
                    }
                    await _db.Industries.AddAsync(model);
                    var result = _db.SaveChanges();
                    if (result != 0)
                    {
                        return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                        {
                            StatusCode = StatusCodes.Status200OK,
                            Message = "Industry Created Successfully"
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
            else
            {
                try
                {
                    var IndusrtyRecord = _db.Industries.Where(x => x.Id == model.Id).FirstOrDefault();
                    if (IndusrtyRecord != null)
                    {
                        IndusrtyRecord.IndustryName = model.IndustryName;
                        IndusrtyRecord.Active = model.Active;
                        _db.Entry(IndusrtyRecord).State = EntityState.Modified;
                        var result = _db.SaveChanges();
                        return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                        {
                            StatusCode = StatusCodes.Status200OK,
                            Message = "Industry Updated Successfully"
                        });
                    }
                }
                catch (Exception ex)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, new APIResponseModel { StatusCode = StatusCodes.Status403Forbidden, Message = "Something Went Wrong." });
                }
            }
            return StatusCode(StatusCodes.Status500InternalServerError, new APIResponseModel { StatusCode = StatusCodes.Status403Forbidden, Message = "Something Went Wrong." });
        }



        [HttpPost]
        [Route("IndustriesList")]
        public async Task<IActionResult> IndustriesList()
        {
            try
            {
                List<Industries> industryList = _db.Industries.ToList();
                if (industryList != null)
                {
                    return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                    {
                        StatusCode = StatusCodes.Status200OK,
                        Message = "Success",
                        Result = industryList
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
        [Route("GetIndustryById")]
        public async Task<IActionResult> GetIndustryById([FromBody] Industries model)
        {
            try
            {
                Industries industryRecord = _db.Industries.Where(x => x.Id == model.Id).FirstOrDefault();
                if (industryRecord != null)
                {
                    return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                    {
                        StatusCode = StatusCodes.Status200OK,
                        Message = "Success",
                        Result = industryRecord
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
        [Route("DeleteIndustryById")]
        public async Task<IActionResult> DeleteIndustryById([FromBody] Industries IndustryData)
        {
            try
            {
                Industries industryRecord = _db.Industries.Where(x => x.Id == IndustryData.Id).FirstOrDefault();
                if (industryRecord != null)
                {
                    _db.Industries.Remove(industryRecord);
                    var result = _db.SaveChanges();
                    if (result != 0)
                    {
                        return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                        {
                            StatusCode = StatusCodes.Status200OK,
                            Message = "Deleted Successfully"
                        });
                    }
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
            return StatusCode(StatusCodes.Status500InternalServerError, new APIResponseModel { StatusCode = StatusCodes.Status403Forbidden, Message = "Something Went Wrong." });
        }

        [HttpPost]
        [Route("SolutionDataById")]
        public async Task<IActionResult> SolutionDataById([FromBody] SolutionIdModel solutionsModel)
        {
            try
            {
                Solutions solution = _db.Solutions.Where(x => x.Id == solutionsModel.Id).FirstOrDefault();
                List<SolutionServices> solutionservice = _db.SolutionServices.Where(x => x.SolutionId == solutionsModel.Id).ToList();
                List<SolutionIndustry> solutionindustry = _db.SolutionIndustry.Where(x => x.SolutionId == solutionsModel.Id).ToList();

                return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                {
                    StatusCode = StatusCodes.Status200OK,
                    Message = "success",
                    Result = solution,
                    IndustryResult = solutionindustry,
                    ServiceResult = solutionservice
                });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new APIResponseModel { StatusCode = StatusCodes.Status403Forbidden, Message = "Something Went Wrong." });
            }
        }

        [HttpPost]
        [Route("UpdateImage")]
        public async Task<IActionResult> UpdateImage([FromBody] SolutionImage solutionImage)
        {
            if (solutionImage.Id != 0)
            {
                try
                {
                    var UpdateImage = _db.Solutions.Where(x => x.Id == solutionImage.Id).FirstOrDefault();
                    if (UpdateImage != null)
                    {

                        UpdateImage.BlobStorageBaseUrl = solutionImage.BlobStorageBaseUrl;
                        UpdateImage.ImagePath = solutionImage.ImagePath;
                        UpdateImage.ImageUrlWithSas = solutionImage.ImageUrlWithSas;

                        _db.SaveChanges();

                        return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                        {
                            StatusCode = StatusCodes.Status200OK,
                            Message = "Data Save Suucessfully !"
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
            }
            return StatusCode(StatusCodes.Status200OK, new APIResponseModel
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "Something Went Wrong"
            });
        }



        [HttpPost]
        [Route("UpdateImageById")]
        public async Task<IActionResult> UpdateImageById([FromBody] SolutionImage solutionImage)
        {

            if (solutionImage.Id != 0)
            {
                var UpdateImage = _db.Solutions.Where(x => x.Id == solutionImage.Id).FirstOrDefault();
                if (UpdateImage != null)
                {
                    UpdateImage.ImagePath = solutionImage.ImagePath;
                    _db.SaveChanges();

                    return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                    {
                        StatusCode = StatusCodes.Status200OK,
                        Message = "Data Save Suucessfully !"
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
