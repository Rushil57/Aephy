using Aephy.API.DBHelper;
using Aephy.API.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Microsoft.WindowsAzure.Storage.Shared.Protocol;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
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
        private readonly UserManager<ApplicationUser> _userManager;
        public AdminController(AephyAppDbContext dbContext, UserManager<ApplicationUser> userManager)
        {
            _db = dbContext;
            _userManager = userManager;
        }

        [HttpPost]
        [Route("AddorEditServices")]
        public async Task<IActionResult> AddorEditServices([FromBody] ServicesModel model)
        {
            try
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
                            Active = model.Active,
                            IsActiveFreelancer = model.IsActiveFreelancer,
                            IsActiveClient = model.IsActiveClient
                        };
                        _db.Services.Add(services);
                        _db.SaveChanges();

                        return StatusCode(StatusCodes.Status200OK, new APIResponseModel { StatusCode = StatusCodes.Status200OK, Message = "Data Saved Succesfully!." });
                    }
                    catch (Exception ex)
                    {
                        return StatusCode(StatusCodes.Status500InternalServerError, new APIResponseModel { StatusCode = StatusCodes.Status403Forbidden, Message = ex.Message + ex.InnerException });
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
                            servicesDetails.IsActiveClient = model.IsActiveClient;
                            servicesDetails.IsActiveFreelancer = model.IsActiveFreelancer;
                            _db.SaveChanges();
                        }
                        return StatusCode(StatusCodes.Status200OK, new APIResponseModel { StatusCode = StatusCodes.Status200OK, Message = "Data Updated Successfully." });
                    }
                    catch (Exception ex)
                    {
                        return StatusCode(StatusCodes.Status500InternalServerError, new APIResponseModel { StatusCode = StatusCodes.Status403Forbidden, Message = ex.Message + ex.InnerException });
                    }
                }

            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new APIResponseModel
                    {
                        StatusCode = StatusCodes.Status403Forbidden,
                        Message = ex.Message + ex.InnerException
                    });
            }

        }


        [HttpGet]
        [Route("ServiceList")]
        public async Task<IActionResult> ServiceList()
        {
            try
            {
                var list = _db.Services.ToList();
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
        [Route("GetServicesById")]
        public async Task<IActionResult> GetServicesById([FromBody] ServicesModel model)
        {
            try
            {
                if (model.Id != 0)
                {
                    var servicesData = _db.Services.Where(x => x.Id == model.Id).FirstOrDefault();
                    return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                    {
                        StatusCode = StatusCodes.Status403Forbidden,
                        Message = "Success",
                        Result = servicesData
                    });
                }
                else
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, new APIResponseModel
                    {
                        StatusCode = StatusCodes.Status403Forbidden,
                        Message = "Id not found"
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
                    return StatusCode(StatusCodes.Status500InternalServerError, new APIResponseModel { StatusCode = StatusCodes.Status403Forbidden, Message = ex.Message + ex.InnerException });
                }
            }

            return StatusCode(StatusCodes.Status500InternalServerError, new APIResponseModel { StatusCode = StatusCodes.Status403Forbidden, Message = "Something Went Wrong" });
        }


        [HttpPost]
        [Route("AddorEditSolutionData")]
        public async Task<IActionResult> AddorEditSolutionData(SolutionsModel model)
        {
            try
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

                                var solutionindustryDetails = new SolutionIndustryDetails()
                                {
                                    SolutionId = solution.Id,
                                    IndustryId = industry
                                };
                                _db.SolutionIndustryDetails.Add(solutionindustryDetails);
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

                                    var checkIndustry = _db.SolutionIndustryDetails.Where(x => x.IndustryId == industry && x.SolutionId == model.Id).FirstOrDefault();
                                    if(checkIndustry == null)
                                    {
                                        var solutionindustryDetails = new SolutionIndustryDetails()
                                        {
                                            SolutionId = model.Id,
                                            IndustryId = industry
                                        };
                                        _db.SolutionIndustryDetails.Add(solutionindustryDetails);
                                        _db.SaveChanges();
                                    }

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

                        var solutionIndutryDetaillist = _db.SolutionIndustryDetails.Where(x => x.SolutionId == model.Id).Select(x => x.IndustryId).ToList();
                        var solutionIndustryList = _db.SolutionIndustry.Where(x => x.SolutionId == model.Id).Select(x => x.IndustryId).ToList();
                        var getindustry = solutionIndutryDetaillist.Except(solutionIndustryList).ToList();
                        if (getindustry.Count > 0)
                        {
                            foreach (var data in getindustry)
                            {
                                var getdata = _db.SolutionIndustryDetails.Where(x => x.IndustryId == data).FirstOrDefault();
                                _db.SolutionIndustryDetails.Remove(getdata);
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
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new APIResponseModel { StatusCode = StatusCodes.Status403Forbidden, Message = ex.Message + ex.InnerException });
            }
        }




        [HttpGet]
        [Route("SolutionList")]
        public async Task<IActionResult> SolutionList()
        {
            try
            {
                List<Solutions> solutionList = _db.Solutions.ToList();
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

        [HttpGet]
        [Route("GetSolutionList")]
        public async Task<IActionResult> GetSolutionList()
        {
            try
            {
                var list = _db.Solutions.ToList();
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

                    var solutionsIndustry = _db.SolutionIndustryDetails.Where(x => x.SolutionId == solutionsModel.Id).ToList();
                    if (solutionsIndustry.Count > 0)
                    {
                        _db.SolutionIndustryDetails.RemoveRange(solutionsIndustry);
                        _db.SaveChanges();
                    }
                    return StatusCode(StatusCodes.Status200OK, new APIResponseModel { StatusCode = StatusCodes.Status200OK, Message = "Delete Succesfully !" });
                }
                catch (Exception ex)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, new APIResponseModel { StatusCode = StatusCodes.Status403Forbidden, Message = ex.Message + ex.InnerException });
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
                            Message = "Data Created Successfully"
                        });
                    }
                    else
                    {
                        return StatusCode(StatusCodes.Status500InternalServerError, new APIResponseModel { StatusCode = StatusCodes.Status403Forbidden, Message = "Something Went Wrong." });
                    }
                }
                catch (Exception ex)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, new APIResponseModel { StatusCode = StatusCodes.Status403Forbidden, Message = ex.Message + ex.InnerException });
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
                        IndusrtyRecord.IsActiveFreelancer = model.IsActiveFreelancer;
                        IndusrtyRecord.IsActiveClient = model.IsActiveClient;
                        _db.Entry(IndusrtyRecord).State = EntityState.Modified;
                        var result = _db.SaveChanges();
                        return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                        {
                            StatusCode = StatusCodes.Status200OK,
                            Message = "Data Updated Successfully"
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

                return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                {
                    StatusCode = StatusCodes.Status200OK,
                    Message = "Success",
                    Result = industryList
                });


            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new APIResponseModel { StatusCode = StatusCodes.Status403Forbidden, Message = ex.Message + ex.InnerException });
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
                return StatusCode(StatusCodes.Status500InternalServerError, new APIResponseModel { StatusCode = StatusCodes.Status403Forbidden, Message = ex.Message + ex.InnerException });
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
                return StatusCode(StatusCodes.Status500InternalServerError, new APIResponseModel { StatusCode = StatusCodes.Status403Forbidden, Message = ex.Message + ex.InnerException });
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
                    Result = new
                    {
                        Solution = solution,
                        IndustryResult = solutionindustry,
                        ServiceResult = solutionservice,
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new APIResponseModel { StatusCode = StatusCodes.Status403Forbidden, Message = ex.Message + ex.InnerException });
            }
        }


        [HttpPost]
        [Route("UpdateImage")]
        public async Task<IActionResult> UpdateImage([FromBody] SolutionImage solutionImage)
        {
            if (solutionImage.HasImageFile)
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
                                Message = "Data Save Sucessfully !"
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
            }
            else
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
                            Message = "Data Save Sucessfully !"
                        });
                    }
                }
            }

            return StatusCode(StatusCodes.Status200OK, new APIResponseModel
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "Something Went Wrong"
            });
        }



        [HttpGet]
        [Route("UserList")]
        public async Task<IActionResult> UserList()
        {
            try
            {
                var list = await _userManager.Users.Where(x => x.IsDeleted == false && x.UserType != "Admin").ToListAsync();
                List<UserViewModel> users = new List<UserViewModel>();
                if(list.Count > 0)
                {
                    foreach(var data in list)
                    {
                        UserViewModel userdataStore = new UserViewModel();
                        userdataStore.Id = data.Id;
                        userdataStore.FirstName = data.FirstName;
                        userdataStore.LastName = data.LastName;
                        userdataStore.UserRole = data.UserType;
                        userdataStore.EmailAddress = data.UserName;
                        userdataStore.FreelancerLevel = _db.FreelancerDetails.Where(x => x.UserId == data.Id).Select(x => x.FreelancerLevel).FirstOrDefault();
                        users.Add(userdataStore);
                    }
                    

                }

                return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                {
                    StatusCode = StatusCodes.Status200OK,
                    Message = "Success",
                    Result = users
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
        [Route("RolesList")]
        public async Task<IActionResult> RolesList()
        {
            try
            {
                var listDB = _db.GigOpenRoles.ToList();
                var listSolution = _db.Solutions.ToList();
                var listServiceSol = _db.SolutionServices.ToList();
                var listService = _db.Services.ToList();
                var listIndustry = _db.Industries.ToList();
                var listIndustrySol = _db.SolutionIndustry.ToList();
                List<dynamic> finalList = new List<dynamic>();
                listDB.ForEach(x =>
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
        [Route("AddorEditRoles")]
        public async Task<IActionResult> AddorEditRoles([FromBody] GigOpenRoles model)
        {
            try
            {
                if (model.ID == 0)
                {
                    try
                    {
                        var roles = new GigOpenRoles()
                        {
                            SolutionId = model.SolutionId,
                            Title = model.Title,
                            Level = model.Level,
                            Description = model.Description,
                            IndustryId = model.IndustryId
                        };
                        _db.GigOpenRoles.Add(roles);
                        _db.SaveChanges();
                        return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                        {
                            StatusCode = StatusCodes.Status200OK,
                            Message = "Data Saved Succesfully!."
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
                        var openRolesdata = _db.GigOpenRoles.Where(x => x.ID == model.ID).FirstOrDefault();
                        if (openRolesdata != null)
                        {
                            openRolesdata.SolutionId = model.SolutionId;
                            openRolesdata.Title = model.Title;
                            openRolesdata.Level = model.Level;
                            openRolesdata.Description = model.Description;
                            openRolesdata.IndustryId = model.IndustryId;
                            _db.SaveChanges();
                        }

                        return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                        {
                            StatusCode = StatusCodes.Status200OK,
                            Message = "Data Saved Succesfully!."
                        });
                    }
                    catch (Exception ex)
                    {
                        return StatusCode(StatusCodes.Status500InternalServerError, new APIResponseModel { StatusCode = StatusCodes.Status403Forbidden, Message = "Something Went Wrong." });
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new APIResponseModel { StatusCode = StatusCodes.Status403Forbidden, Message = ex.Message + ex.InnerException });
            }
        }

        [HttpPost]
        [Route("RolesDataById")]
        public async Task<IActionResult> RolesDataById([FromBody] GigOpenRoles model)
        {
            try
            {
                GigOpenRoles openRoles = _db.GigOpenRoles.Where(x => x.ID == model.ID).FirstOrDefault();
                if (openRoles != null)
                {
                    return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                    {
                        StatusCode = StatusCodes.Status200OK,
                        Message = "Success",
                        Result = openRoles
                    });
                }
                else
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, new APIResponseModel { StatusCode = StatusCodes.Status403Forbidden, Message = "Something Went Wrong." });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new APIResponseModel { StatusCode = StatusCodes.Status403Forbidden, Message = ex.Message + ex.InnerException });
            }
        }

        [HttpPost]
        [Route("DeleteRolesById")]
        public async Task<IActionResult> DeleteRolesById([FromBody] GigOpenRoles rolesData)
        {
            try
            {
                GigOpenRoles openRolesRecord = _db.GigOpenRoles.Where(x => x.ID == rolesData.ID).FirstOrDefault();
                if (openRolesRecord != null)
                {
                    _db.GigOpenRoles.Remove(openRolesRecord);
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
                return StatusCode(StatusCodes.Status500InternalServerError, new APIResponseModel { StatusCode = StatusCodes.Status403Forbidden, Message = ex.Message + ex.InnerException });
            }
            return StatusCode(StatusCodes.Status500InternalServerError, new APIResponseModel { StatusCode = StatusCodes.Status403Forbidden, Message = "Something Went Wrong." });
        }

        [HttpGet]
        [Route("FilteredRolesList")]
        public async Task<IActionResult> FilteredRolesList(int serviceId, int solutionId, string level, int industryId)
        {
            try
            {
                var listDB = _db.GigOpenRoles.ToList();
                var listSolution = _db.Solutions.ToList();
                var listServiceSol = _db.SolutionServices.ToList();
                var listService = _db.Services.ToList();
                var listIndustry = _db.Industries.ToList();
                var listIndustrySol = _db.SolutionIndustry.ToList();
                List<dynamic> finalList = new List<dynamic>();

                listDB.ForEach(x =>
                {
                    var solSer = listServiceSol.Where(t => t.SolutionId == x.SolutionId).FirstOrDefault();
                    var service = new Services();
                    if (solSer != null)
                    {
                        service = listService.Where(s => s.Id == solSer.ServicesId).FirstOrDefault();
                    }

                    var solInd = listIndustrySol.Where(t1 => t1.SolutionId == x.SolutionId).FirstOrDefault();
                    var industry = new Industries();
                    if (solInd != null)
                    {
                        industry = listIndustry.Where(s1 => s1.Id == solInd.IndustryId).FirstOrDefault();
                    }
                    var solutionName = listSolution.Where(m => m.Id == x.SolutionId).FirstOrDefault()?.Title;
                    dynamic obj = new
                    {
                        x.Level,
                        x.Title,
                        x.CreatedDateTime,
                        x.ID,
                        x.SolutionId,
                        x.Description,
                        SolutionName = solutionName,
                        ServiceName = service?.ServicesName,
                        IndustryName = industry?.IndustryName,
                        ServiceId = service?.Id,
                        IndustryId = industry?.Id,

                    };
                    finalList.Add(obj);
                });

                if (serviceId != 0)
                    finalList = finalList.Where(x => x.ServiceId == serviceId).ToList();
                if (solutionId != 0)
                    finalList = finalList.Where(x => x.SolutionId == solutionId).ToList();
                if (level != "0")
                    finalList = finalList.Where(x => x.Level == level).ToList();
                if (industryId != 0)
                    finalList = finalList.Where(x => x.IndustryId == industryId).ToList();

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
        [Route("SolutionIndustriesUpdateImage")]
        public async Task<IActionResult> SolutionIndustriesUpdateImage([FromBody] SolutionImage solutionImage)
        {
            if (solutionImage.HasImageFile)
            {
                if (solutionImage.Id != 0)
                {
                    try
                    {
                        var UpdateImage = _db.SolutionIndustryDetails.Where(x => x.Id == solutionImage.Id).FirstOrDefault();
                        if (UpdateImage != null)
                        {

                            UpdateImage.BlobStorageBaseUrl = solutionImage.BlobStorageBaseUrl;
                            UpdateImage.ImagePath = solutionImage.ImagePath;
                            UpdateImage.ImageUrlWithSas = solutionImage.ImageUrlWithSas;

                            _db.SaveChanges();

                            return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                            {
                                StatusCode = StatusCodes.Status200OK,
                                Message = "Data Save Sucessfully !"
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
            }
            else
            {

                if (solutionImage.Id != 0)
                {
                    var UpdateImage = _db.SolutionIndustryDetails.Where(x => x.Id == solutionImage.Id).FirstOrDefault();
                    if (UpdateImage != null)
                    {
                        UpdateImage.ImagePath = solutionImage.ImagePath;
                        _db.SaveChanges();

                        return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                        {
                            StatusCode = StatusCodes.Status200OK,
                            Message = "Data Save Sucessfully !"
                        });
                    }
                }
            }

            return StatusCode(StatusCodes.Status200OK, new APIResponseModel
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "Something Went Wrong"
            });
        }



        //New

        [HttpGet]
        [Route("SolutionDetailsList")]
        public async Task<IActionResult> SolutionDetailsList()
        {
            try
            {
                List<SolutionIndustryDetails> solutionIndustries = _db.SolutionIndustryDetails.ToList();

                List<SolutionsModel> solutionsModel = new List<SolutionsModel>();
                List<string> industrylist = new List<string>();

                if (solutionIndustries.Count > 0)
                {
                    var industryname = string.Empty;
                    foreach (var list in solutionIndustries)
                    {
                        var serviceId = _db.SolutionServices.Where(x => x.SolutionId == list.SolutionId).Select(x => x.ServicesId).FirstOrDefault();
                        var Servicename = _db.Services.Where(x => x.Id == serviceId).Select(x => x.ServicesName).FirstOrDefault();

                        SolutionsModel dataStore = new SolutionsModel();
                        dataStore.Industries = _db.Industries.Where(x => x.Id == list.IndustryId).Select(x => x.IndustryName).FirstOrDefault();
                        dataStore.Id = list.Id;
                        dataStore.Description = list.Description;
                        dataStore.ImagePath = list.ImagePath;
                        dataStore.ImageUrlWithSas = list.ImageUrlWithSas;
                        dataStore.Title = _db.Solutions.Where(x => x.Id == list.SolutionId).Select(x => x.Title).FirstOrDefault();
                        dataStore.SubTitle = _db.Solutions.Where(x => x.Id == list.SolutionId).Select(x => x.SubTitle).FirstOrDefault();
                        dataStore.Services = Servicename;
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

        [HttpPost]
        [Route("SolutionDetailsDataById")]
        public async Task<IActionResult> SolutionDetailsDataById([FromBody] SolutionIdModel solutionsModel)
        {
            try
            {
                SolutionIndustryDetails solutionIndustry = _db.SolutionIndustryDetails.Where(x => x.Id == solutionsModel.Id).FirstOrDefault();
                var industries = _db.Industries.Where(x => x.Id == solutionIndustry.IndustryId).FirstOrDefault();
                Solutions solution = _db.Solutions.Where(x => x.Id == solutionIndustry.SolutionId).FirstOrDefault();
                SolutionServices solutionServices = _db.SolutionServices.Where(x => x.SolutionId == solution.Id).FirstOrDefault();

                return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                {
                    StatusCode = StatusCodes.Status200OK,
                    Message = "success",
                    Result = new
                    {
                        Solution = solution,
                        SolutionIndustryDetails = solutionIndustry,
                        Industryname = industries,
                        Services = solutionServices
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new APIResponseModel { StatusCode = StatusCodes.Status403Forbidden, Message = ex.Message + ex.InnerException });
            }
        }

        [HttpPost]
        [Route("AddSolutionDescribedData")]
        public async Task<IActionResult> AddSolutionDescribedData([FromBody] SolutionDescribeModel model)
        {
            try
            {
                if(model.Id == 0)
                {
                    var solution = new SolutionIndustryDetails()
                    {
                        SolutionId = model.SolutionId,
                        IndustryId = model.IndustryId,
                        Description = model.Description
                    };
                    _db.SolutionIndustryDetails.Add(solution);
                    _db.SaveChanges();
                    return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                    {
                        StatusCode = StatusCodes.Status200OK,
                        Message = "Data Saved Succesfully!",
                        Result = solution.Id
                    });
                }
                else
                {
                    var data = _db.SolutionIndustryDetails.Where(x => x.Id == model.Id).FirstOrDefault();
                    if (data != null)
                    {
                        data.Description = model.Description;
                        _db.SaveChanges();
                    }

                    return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                    {
                        StatusCode = StatusCodes.Status200OK,
                        Message = "Data Saved Succesfully!",
                        Result = data.ImagePath
                    });
                }
            }

            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new APIResponseModel { StatusCode = StatusCodes.Status403Forbidden, Message = ex.Message + ex.InnerException });
            }
        }


        [HttpPost]
        [Route("UserUpdateIsDelete")]
        public async Task<IActionResult> UserUpdateIsDelete([FromBody] UserIdModel model)
        {
            try
            {
                if(model.Id != null)
                {
                    var userDetails = _db.Users.Where(x => x.Id == model.Id).FirstOrDefault();
                   
                    if (userDetails != null)
                    {
                        userDetails.IsDeleted = true;
                        _db.SaveChanges();

                        return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                        {
                            StatusCode = StatusCodes.Status200OK,
                            Message = "User Deleted Succesfully!"
                        });
                    }
                }
                return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                {
                    StatusCode = StatusCodes.Status200OK,
                    Message = "Something Went Wrong"
                });
            }

            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new APIResponseModel { StatusCode = StatusCodes.Status403Forbidden, Message = ex.Message + ex.InnerException });
            }
        }
    }
}
