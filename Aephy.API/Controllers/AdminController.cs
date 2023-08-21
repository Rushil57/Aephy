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
using Stripe.Checkout;
using Stripe;
using System.Collections;
using System.Collections.Generic;
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
                        StatusCode = StatusCodes.Status200OK,
                        Message = "Success",
                        Result = servicesData
                    });
                }
                else
                {
                    return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                    {
                        StatusCode = StatusCodes.Status200OK,
                        Message = "Id not found"
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
                    return StatusCode(StatusCodes.Status200OK, new APIResponseModel { StatusCode = StatusCodes.Status200OK, Message = ex.Message + ex.InnerException });
                }
            }

            return StatusCode(StatusCodes.Status200OK, new APIResponseModel { StatusCode = StatusCodes.Status200OK, Message = "Id Not Found" });
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
                        return StatusCode(StatusCodes.Status200OK, new APIResponseModel { StatusCode = StatusCodes.Status403Forbidden, Message = "Something Went Wrong." });
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
                                    if (checkIndustry == null)
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
                        return StatusCode(StatusCodes.Status200OK, new APIResponseModel { StatusCode = StatusCodes.Status403Forbidden, Message = ex.Message + ex.InnerException });
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
                var freelancerPoolList = _db.FreelancerPool.ToList();
                var SolutionList = _db.Solutions.ToList();
                List<UserViewModel> users = new List<UserViewModel>();
                List<dynamic> userlist = new List<dynamic>();
                if (list.Count > 0)
                {
                    foreach (var data in list)
                    {
                        Solutions solData = new Solutions();
                        if (data.UserType == "Freelancer")
                        {
                            var Solution = freelancerPoolList.Where(x => x.FreelancerID == data.Id).FirstOrDefault();
                            if (Solution != null)
                            {
                                solData = _db.Solutions.Where(s => s.Id == Solution.SolutionID).FirstOrDefault();
                            }
                        }
                        dynamic obj = new
                        {
                            Id = data.Id,
                            FirstName = data.FirstName,
                            LastName = data.LastName,
                            UserRole = data.UserType,
                            EmailAddress = data.UserName,
                            FreelancerLevel = _db.FreelancerDetails.Where(x => x.UserId == data.Id).Select(x => x.FreelancerLevel).FirstOrDefault(),
                            SolutionID = solData?.Id ?? 0,
                            SolutionName = solData?.Title ?? "",
                        };
                        userlist.Add(obj);
                    }
                }

                return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                {
                    StatusCode = StatusCodes.Status200OK,
                    Message = "Success",
                    Result = userlist
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
        [Route("getUserByID")]
        public async Task<IActionResult> getUserByID([FromBody] UserIdModel userdata)
        {
            try
            {
                var list = await _userManager.Users.Where(x => x.IsDeleted == false && x.UserType != "Admin").ToListAsync();
                var data = list.Where(u => u.Id == userdata.Id).FirstOrDefault();
                var freelancerPoolList = _db.FreelancerPool.ToList();
                dynamic obj = new
                {
                    Id = data.Id,
                    FirstName = data.FirstName,
                    LastName = data.LastName,
                    UserRole = data.UserType,
                    EmailAddress = data.UserName,
                    FreelancerLevel = _db.FreelancerDetails.Where(x => x.UserId == data.Id).Select(x => x.FreelancerLevel).FirstOrDefault(),
                };

                return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                {
                    StatusCode = StatusCodes.Status200OK,
                    Message = "Success",
                    Result = obj
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
        [Route("InvitedUserList")]
        public async Task<IActionResult> InvitedUserList()
        {
            try
            {
                var list = await _userManager.Users.Where(x => x.IsDeleted == false && x.UserType != "Admin" && x.IsInvited == true).ToListAsync();
                List<UserViewModel> users = new List<UserViewModel>();
                if (list.Count > 0)
                {
                    foreach (var data in list)
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
        [Route("getFreelancerByLevel")]
        public async Task<IActionResult> getFreelancerByLevel()
        {
            try
            {
                var list = await _userManager.Users.Where(x => x.IsDeleted == false && x.UserType == "Freelancer").ToListAsync();
                List<UserViewModel> users = new List<UserViewModel>();
                if (list.Count > 0)
                {
                    foreach (var data in list)
                    {
                        UserViewModel userdataStore = new UserViewModel();
                        userdataStore.Id = data.Id;
                        userdataStore.FirstName = data.FirstName;
                        userdataStore.LastName = data.LastName;
                        userdataStore.UserRole = data.UserType;
                        userdataStore.EmailAddress = data.UserName;
                        userdataStore.FreelancerLevel = _db.FreelancerDetails.Where(x => x.UserId == data.Id).Select(x => x.FreelancerLevel).FirstOrDefault();
                        if (userdataStore.FreelancerLevel == "Project Manager/Project Architecture")
                        {
                            users.Add(userdataStore);
                        }
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
                        x.isActive,
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

        [HttpGet]
        [Route("ActiveRolesList")]
        public async Task<IActionResult> ActiveRolesList()
        {
            try
            {
                var listDB = _db.GigOpenRoles.Where(x => x.isActive == true).ToList();
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
                        x.isActive,
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
                            IndustryId = model.IndustryId,
                            isActive = model.isActive
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
                            openRolesdata.isActive = model.isActive;
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
        public async Task<IActionResult> RolesDataById([FromBody] OpenGigRolesModel model)
        {
            try
            {
                GigOpenRoles openRoles = _db.GigOpenRoles.Where(x => x.ID == model.ID).FirstOrDefault();
                FreelancerDetails freelancerDetail = _db.FreelancerDetails.Where(x => x.UserId == model.FreelancerID).FirstOrDefault();
                if (openRoles != null)
                {
                    return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                    {
                        StatusCode = StatusCodes.Status200OK,
                        Message = "Success",
                        Result = new
                        {
                            OpenRoles = openRoles,
                            FreelancerDetail = freelancerDetail
                        }
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

                    var solutionServiceList = _db.SolutionServices.ToList();
                    var serviceList = _db.Services.ToList();
                    var industriesList = _db.Industries.ToList();
                    var solutionsList = _db.Solutions.ToList();

                    foreach (var list in solutionIndustries)
                    {
                        var serviceId = solutionServiceList.Where(x => x.SolutionId == list.SolutionId).Select(x => x.ServicesId).FirstOrDefault();
                        var Servicename = serviceList.Where(x => x.Id == serviceId).Select(x => x.ServicesName).FirstOrDefault();

                        SolutionsModel dataStore = new SolutionsModel();
                        dataStore.Industries = industriesList.Where(x => x.Id == list.IndustryId).Select(x => x.IndustryName).FirstOrDefault();
                        dataStore.Id = list.Id;
                        dataStore.Description = list.Description;
                        dataStore.ImagePath = list.ImagePath;
                        dataStore.ImageUrlWithSas = list.ImageUrlWithSas;
                        dataStore.Title = solutionsList.Where(x => x.Id == list.SolutionId).Select(x => x.Title).FirstOrDefault();
                        dataStore.SubTitle = solutionsList.Where(x => x.Id == list.SolutionId).Select(x => x.SubTitle).FirstOrDefault();
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
                var list = await _userManager.Users.Where(x => x.IsDeleted == false && x.UserType == "Freelancer").ToListAsync();
                var milestoneDetails = _db.SolutionMilestone.Where(x => x.SolutionId == solutionIndustry.SolutionId &&
                x.IndustryId == solutionIndustry.IndustryId && x.ProjectType == "SMALL").ToList();

                var pointsDetails = _db.SolutionPoints.Where(x => x.SolutionId == solutionIndustry.SolutionId &&
                x.IndustryId == solutionIndustry.IndustryId && x.ProjectType == "SMALL").ToList();

                var freeLancerPoolIds = _db.FreelancerPool.Where(x => x.SolutionID == solutionIndustry.SolutionId && x.IndustryId == solutionIndustry.IndustryId).Select(x => x.FreelancerID).ToList();
                var architectIds = _db.FreelancerPool.Where(x => x.SolutionID == solutionIndustry.SolutionId && x.IndustryId == solutionIndustry.IndustryId && x.IsProjectArchitect == true).Select(x => x.FreelancerID).ToList();

                var solutionDefine = _db.SolutionDefine.Where(x => x.SolutionIndustryDetailsId == solutionIndustry.Id && x.ProjectType == "SMALL").FirstOrDefault();

                return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                {
                    StatusCode = StatusCodes.Status200OK,
                    Message = "success",
                    Result = new
                    {
                        Solution = solution,
                        SolutionIndustryDetails = solutionIndustry,
                        Industryname = industries,
                        Services = solutionServices,
                        MilestoneDetails = milestoneDetails,
                        FreeLancerPoolIds = freeLancerPoolIds,
                        ArchitectIds = architectIds,
                        PointsDetails = pointsDetails,
                        SolutionDefine = solutionDefine
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
                if (model.Id == 0)
                {
                    var solution = new SolutionIndustryDetails()
                    {
                        SolutionId = model.SolutionId,
                        IndustryId = model.IndustryId,
                        Description = model.Description,
                        //AssignedFreelancerId = model.AssignedFreelancerId,
                        IsActiveByAdmin = Convert.ToInt32(model.ActiveByAdmin),
                        IsActiveForFreelancer = model.IsActiveForClient,
                        IsActiveForClient = model.IsActiveForClient
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
                        //data.AssignedFreelancerId = model.AssignedFreelancerId;
                        data.IsActiveByAdmin = Convert.ToInt32(model.ActiveByAdmin);
                        data.IsActiveForClient = model.IsActiveForClient;
                        data.IsActiveForFreelancer = model.IsActiveForFreelancer;
                        //_db.SaveChanges();
                    }

                    var freelancerPoolData = _db.FreelancerPool.Where(x => x.SolutionID == model.SolutionId && x.IndustryId == model.IndustryId).ToList();
                    _db.FreelancerPool.RemoveRange(freelancerPoolData);
                    //await _db.SaveChangesAsync();

                    var freeLancerPoolListData = new List<FreelancerPool>();
                    foreach (var item in model.AssignedFreelancerIds)
                    {
                        var freeLancerPool = new FreelancerPool
                        {
                            FreelancerID = item,
                            SolutionID = model.SolutionId,
                            IsProjectArchitect = model.IsArchitectIds.Contains(item),
                            IndustryId = model.IndustryId,
                        };

                        freeLancerPoolListData.Add(freeLancerPool);
                    }

                    await _db.FreelancerPool.AddRangeAsync(freeLancerPoolListData);
                    await _db.SaveChangesAsync();

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
        [Route("ActionByAdminOnSolution")]
        public async Task<IActionResult> ActionByAdminOnSolution(string solutionIndustryDetailsId, string action)
        {
            try
            {
                var solutionDetail = _db.SolutionIndustryDetails.Where(x => x.Id == Convert.ToInt32(solutionIndustryDetailsId)).FirstOrDefault();
                solutionDetail.ActionOn = DateTime.Now;
                solutionDetail.IsApproved = Convert.ToInt32(action);
                _db.SaveChanges();
                return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                {
                    StatusCode = StatusCodes.Status200OK,
                    Message = "Data Updated Succesfully!",
                    Result = ""
                });
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
                if (model.Id != null)
                {
                    //if (userDetails != null)
                    //{
                    //    userDetails.IsDeleted = true;
                    //    _db.SaveChanges();

                    //    return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                    //    {
                    //        StatusCode = StatusCodes.Status200OK,
                    //        Message = "User Deleted Succesfully!"
                    //    });
                    //}

                    var userDetails = _db.Users.Where(x => x.Id == model.Id).FirstOrDefault();
                    _db.Users.Remove(userDetails);
                    int status = await _db.SaveChangesAsync();
                    if (status > 0)
                    {
                        return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                        {
                            StatusCode = StatusCodes.Status200OK,
                            Message = "User Deleted Succesfully!"
                        });
                    }
                    else
                    {
                        return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                        {
                            StatusCode = StatusCodes.Status200OK,
                            Message = "Something Went Wrong"
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

        [HttpPost]
        [Route("GetFreelancerSolutionList")]
        public async Task<IActionResult> GetFreelancerSolutionList([FromBody] UserIdModel model)
        {
            try
            {
                if (model.Id != null)
                {
                    var userDetails = _db.Users.Where(x => x.Id == model.Id).FirstOrDefault();

                    if (userDetails != null)
                    {
                        List<dynamic> solutions = new List<dynamic>();
                        var solutionsList = await _db.FreelancerPool.Where(x => x.FreelancerID == model.Id).ToListAsync();
                        if (solutionsList.Count > 0)
                        {
                            solutionsList.ForEach(mdl =>
                            {
                                var sln = _db.Solutions.Where(s => s.Id == mdl.SolutionID).FirstOrDefault();
                                var ind = _db.Industries.Where(x => x.Id == mdl.IndustryId).FirstOrDefault();
                                solutions.Add(
                                    new
                                    {
                                        sln?.Id,
                                        sln?.Title,
                                        ind?.IndustryName
                                    }
                                );
                            });
                        }

                        return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                        {
                            StatusCode = StatusCodes.Status200OK,
                            Message = "Success",
                            Result = solutions
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

        //Application Section
        [HttpGet]
        [Route("GetApplicationList")]
        public async Task<IActionResult> GetApplicationList()
        {
            try
            {
                var rolesApplications = await _db.OpenGigRolesApplications.ToListAsync();
                var gigOpenRole = await _db.GigOpenRoles.ToListAsync();
                var solutions = await _db.Solutions.ToListAsync();
                var industries = await _db.Industries.ToListAsync();
                var users = await _db.Users.ToListAsync();

                List<AdminViewModel.GigOpenRolesModel> roles = new List<AdminViewModel.GigOpenRolesModel>();
                if (rolesApplications.Count > 0)
                {
                    foreach (var data in rolesApplications)
                    {
                        AdminViewModel.GigOpenRolesModel gigRolesStore = new AdminViewModel.GigOpenRolesModel();
                        gigRolesStore.ID = data.ID;
                        gigRolesStore.Description = data.Description;
                        gigRolesStore.CreatedDateTime = data.CreatedDateTime.ToString("yyyy-MM-dd");

                        //gigRolesStore.Name = _db.Users.Where(x => x.Id == data.FreelancerID).Select(x => x.FirstName).FirstOrDefault();

                        var userModel = users.Where(x => x.Id == data.FreelancerID).FirstOrDefault();
                        if (userModel != null)
                        {

                            gigRolesStore.Name = userModel.FirstName;
                        }

                        var gigOpenRoleModel = gigOpenRole.Where(x => x.ID == data.GigOpenRoleId).FirstOrDefault();
                        if (gigOpenRoleModel != null)
                        {
                            gigRolesStore.SolutionName = solutions.Where(x => x.Id == gigOpenRoleModel.SolutionId)
                                                        .Select(x => x.Title).FirstOrDefault();
                            gigRolesStore.IndustriesName = industries.Where(x => x.Id == gigOpenRoleModel.IndustryId)
                                                            .Select(x => x.IndustryName).FirstOrDefault();

                            gigRolesStore.FreeLancerLavel = gigOpenRoleModel.Level;
                        }
                        gigRolesStore.ApproveOrReject = data.IsApproved ? "Approve" : data.IsRejected ? "Reject" : "No Action";
                        roles.Add(gigRolesStore);
                    }

                }
                return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                {
                    StatusCode = StatusCodes.Status200OK,
                    Message = "Success",
                    Result = roles
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
        [Route("GetApplicantsdataById")]
        public async Task<IActionResult> GetApplicantsdataById([FromBody] AdminViewModel.GigOpenRolesModel solutionsModel)
        {
            try
            {
                OpenGigRolesApplications openGigRoles = _db.OpenGigRolesApplications.Where(x => x.ID == solutionsModel.ID).FirstOrDefault();
                GigOpenRoles gigOpenRoles = new GigOpenRoles();

                var freelancerName = string.Empty;
                if (openGigRoles != null)
                {
                    freelancerName = _db.Users.Where(x => x.Id == openGigRoles.FreelancerID).Select(x => x.FirstName).FirstOrDefault();
                    gigOpenRoles = _db.GigOpenRoles.Where(x => x.ID == openGigRoles.GigOpenRoleId).FirstOrDefault();
                }

                return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                {
                    StatusCode = StatusCodes.Status200OK,
                    Message = "success",
                    Result = new
                    {
                        OpenGigRoles = openGigRoles,
                        FreelancerName = freelancerName,
                        GigOpenRoles = gigOpenRoles,
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new APIResponseModel { StatusCode = StatusCodes.Status403Forbidden, Message = ex.Message + ex.InnerException });
            }
        }

        [HttpPost]
        [Route("DeleteUserApplication")]
        public async Task<IActionResult> DeleteUserApplication([FromBody] AdminViewModel.GigOpenRolesModel ApplucationModel)
        {
            if (ApplucationModel.ID != 0)
            {
                try
                {
                    OpenGigRolesApplications? openGigRoles = await _db.OpenGigRolesApplications.Where(x => x.ID == ApplucationModel.ID).FirstOrDefaultAsync();
                    if (openGigRoles != null)
                    {
                        GigOpenRoles gigopenrole = await _db.GigOpenRoles.Where(g => g.ID == openGigRoles.GigOpenRoleId).FirstOrDefaultAsync();
                        if (gigopenrole != null)
                        {
                            FreelancerPool freelanerPool = await _db.FreelancerPool.Where(f => f.FreelancerID == openGigRoles.FreelancerID 
                            && f.SolutionID == gigopenrole.SolutionId && f.IndustryId == gigopenrole.IndustryId).FirstOrDefaultAsync();
                            if (freelanerPool != null)
                            {
                                _db.FreelancerPool.Remove(freelanerPool);
                                await _db.SaveChangesAsync();
                            }
                        }
                        _db.OpenGigRolesApplications.Remove(openGigRoles);
                        await _db.SaveChangesAsync();
                    }
                    return StatusCode(StatusCodes.Status200OK, new APIResponseModel { StatusCode = StatusCodes.Status200OK, Message = "Delete Succesfully !" });
                }
                catch (Exception ex)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, new APIResponseModel { StatusCode = StatusCodes.Status403Forbidden, Message = ex.Message + ex.InnerException });
                }
            }
            return StatusCode(StatusCodes.Status200OK, new APIResponseModel { StatusCode = StatusCodes.Status200OK, Message = "Something Went Wrong." });

        }


        [HttpPost]
        [Route("ApproveOrRejectFreelancer")]
        public async Task<IActionResult> ApproveOrRejectFreelancer([FromBody] AdminViewModel.GigOpenRolesModel solutionsModel)
        {
            try
            {
                var freelancerData = new OpenGigRolesApplications();
                if (solutionsModel.ID != 0)
                {
                    freelancerData = _db.OpenGigRolesApplications.Where(x => x.ID == solutionsModel.ID).FirstOrDefault();
                    if (freelancerData != null)
                    {
                        if (solutionsModel.ApproveOrReject == "Approve".Trim())
                        {
                            freelancerData.IsApproved = true;
                            freelancerData.IsRejected = false;

                            var data = _db.FreelancerPool.Where(x => x.FreelancerID == solutionsModel.FreelancerId && x.SolutionID == solutionsModel.SolutionId && x.IndustryId == solutionsModel.IndustryId).FirstOrDefault();
                            if (data == null)
                            {
                                var dbModel = new FreelancerPool
                                {
                                    FreelancerID = freelancerData.FreelancerID,
                                    IndustryId = solutionsModel.IndustryId,
                                    SolutionID = solutionsModel.SolutionId
                                };

                                await _db.FreelancerPool.AddAsync(dbModel);
                                _db.SaveChanges();
                            }

                        }
                        if (solutionsModel.ApproveOrReject == "Reject".Trim())
                        {
                            freelancerData.IsApproved = false;
                            freelancerData.IsRejected = true;
                            _db.SaveChanges();

                            var data = _db.FreelancerPool.Where(x => x.FreelancerID == solutionsModel.FreelancerId && x.SolutionID == solutionsModel.SolutionId && x.IndustryId == solutionsModel.IndustryId).FirstOrDefault();
                            if (data != null)
                            {
                                _db.FreelancerPool.Remove(data);
                                _db.SaveChanges();
                            }
                        }
                    }

                }

                var userData = await _userManager.FindByIdAsync(freelancerData.FreelancerID);

                return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                {
                    StatusCode = StatusCodes.Status200OK,
                    Message = solutionsModel.ApproveOrReject + " Successfully!",
                    Result = userData
                });

            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                {
                    StatusCode = StatusCodes.Status200OK,
                    Message = "success",
                    Result = ex.Message + ex.InnerException
                });
            }

        }

        [HttpPost]
        [Route("GetSolutionDefineData")]
        public async Task<IActionResult> GetSolutionDefineData([FromBody] SolutionDefineRequestViewModel model)
        {
            try
            {
                int solutionIndustryDetailsId = await _db.SolutionIndustryDetails.Where(x => x.IndustryId == model.IndustryId
                && x.SolutionId == model.SolutionId).Select(x => x.Id).FirstOrDefaultAsync();

                if (solutionIndustryDetailsId > 0)
                {
                    var solutionDefineMpdel = await _db.SolutionDefine.Where(x => x.SolutionIndustryDetailsId == solutionIndustryDetailsId
                                                && x.ProjectType == model.ProjectType).FirstOrDefaultAsync();

                    var solutionMilestone = await _db.SolutionMilestone.Where(x => x.SolutionId == model.SolutionId
                                                    && x.IndustryId == model.IndustryId
                                                    && x.ProjectType == model.ProjectType).ToListAsync();

                    var solutionPoints = await _db.SolutionPoints.Where(x => x.SolutionId == model.SolutionId
                                                    && x.IndustryId == model.IndustryId
                                                    && x.ProjectType == model.ProjectType).ToListAsync();

                    return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                    {
                        StatusCode = StatusCodes.Status200OK,
                        Message = "Success",
                        Result = new
                        {
                            SolutionDefine = solutionDefineMpdel,
                            SolutionMilestone = solutionMilestone,
                            SolutionPoints = solutionPoints
                        }
                    });

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


        //[HttpGet]
        //[Route("checkOut")]
        //public async Task<IActionResult> checkOut()
        //{
        //    try
        //    {
        //        StripeConfiguration.ApiKey = "sk_test_51NcndQSEtmOn47Zj9wmGZXP6MxXu66bdakmxiFLTqmI3lUliPyEBKsW1WLfGuPe7jVcy4XTYIDgDZOV5szLT8S7X00SjdYZtWp";
        //        var options = new SessionCreateOptions
        //        {
        //            SuccessUrl = "https://example.com/success",
        //            CancelUrl = "https://example.com/success",
        //            LineItems = new List<SessionLineItemOptions>
        //            {
        //                new SessionLineItemOptions
        //                {
        //                    PriceData = new SessionLineItemPriceDataOptions
        //                    {
        //                        UnitAmount = (long)(100),
        //                        Currency = "usd",
        //                        ProductData = new SessionLineItemPriceDataProductDataOptions
        //                        {
        //                            Name = "AI Roadmap Development",
        //                            Description = "Enable your organization to make informed decisions about AI investments and implement AI solutions that align with your business objectives."
        //                        }
        //                    },
        //                    Quantity = 1,
        //                },
        //            },
        //            Mode = "payment",
        //        };
        //        var service = new SessionService();
        //        Session session = service.Create(options);

        //        Response.Headers.Add("Location", session.Url);

        //        return StatusCode(StatusCodes.Status200OK, new APIResponseModel
        //        {
        //            StatusCode = StatusCodes.Status403Forbidden,
        //            Message = "Payment Success..",
        //            Result = session.Url
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(StatusCodes.Status200OK, new APIResponseModel
        //        {
        //            StatusCode = StatusCodes.Status403Forbidden,
        //            Message = ex.Message + ex.InnerException

        //        });
        //    }
        //}

        [HttpPost]
        [Route("checkOut")]
        public async Task<IActionResult> checkOut([FromBody] MileStoneDetailsViewModel model)
        {
            try
            {
                StripeConfiguration.ApiKey = "sk_test_51NcndQSEtmOn47Zj9wmGZXP6MxXu66bdakmxiFLTqmI3lUliPyEBKsW1WLfGuPe7jVcy4XTYIDgDZOV5szLT8S7X00SjdYZtWp";

                // For showing solutions data dynamically into the checkout page
                /*var usermail = string.Empty;
                var sessionLineItems = new SessionLineItemOptions();
                var solutionName = string.Empty;
                var solutionDescription = string.Empty;
                var solutionsList = _db.Solutions.ToList();
                var industruCheck = _db.SolutionIndustry.Where(i => i.IndustryId == model.IndustryId && i.SolutionId == model.SolutionId).FirstOrDefault();
                if (industruCheck != null)
                {
                    var SolutionData = solutionsList.Where(s => s.Id == model.SolutionId).FirstOrDefault();
                    if (SolutionData != null)
                    {
                        solutionName = SolutionData.Title;
                        solutionDescription = SolutionData.Description;
                        sessionLineItems = new SessionLineItemOptions
                        {
                            PriceData = new SessionLineItemPriceDataOptions
                            {
                                UnitAmount = Convert.ToInt64(200 * 100),
                                Currency = "usd",
                                ProductData = new SessionLineItemPriceDataProductDataOptions
                                {
                                    Name = "AI Roadmap Development",
                                    Description = "Enable your organization to make informed decisions about AI investments and implement AI solutions that align with your business objectives."
                                }
                            },
                            Quantity = 1,
                        };
                    }
                }

                var userDetails = _db.Users.Where(user => user.Id == model.FreelancerId).FirstOrDefault();
                if (userDetails != null)
                {
                    usermail = userDetails.Email;
                }*/

                var options = new SessionCreateOptions
                {
                    SuccessUrl = "https://example.com/success",
                    CancelUrl = "https://example.com/success",
                    PaymentMethodTypes = new List<string>
                    {
                        "card"
                    },
                    LineItems = new List<SessionLineItemOptions>
                    {
                        new SessionLineItemOptions
                        {
                            PriceData = new SessionLineItemPriceDataOptions
                            {
                                UnitAmount = Convert.ToInt64(200 * 100),
                                Currency = "usd",
                                ProductData = new SessionLineItemPriceDataProductDataOptions
                                {
                                    Name = "AI Roadmap Development",
                                    Description = "Enable your organization to make informed decisions about AI investments and implement AI solutions that align with your business objectives."
                                }
                            },
                            Quantity = 1,
                        },
                    },
                    Mode = "payment",
                    //Add Privacy policy to checkout page : https://dashboard.stripe.com/settings/public
                    //ConsentCollection = new SessionConsentCollectionOptions { TermsOfService = "required" }
                };
                var service = new SessionService();
                Session session = service.Create(options);

                return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                {
                    StatusCode = StatusCodes.Status200OK,
                    Message = "Success",
                    Result = new
                    {
                        checkoutLink = session.Url
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
        [Route("SaveSolutionAssignedFreelancer")]
        public async Task<IActionResult> SaveSolutionAssignedFreelancer([FromBody] SolutionDefineRequestViewModel model)
        {
            try
            {
                var data = _db.FreelancerPool.Where(x => x.FreelancerID == model.FreelancerId && x.SolutionID == model.SolutionId && x.IndustryId == model.IndustryId).FirstOrDefault();
                if (data == null)
                {
                    var dbModel = new FreelancerPool
                    {
                        FreelancerID = model.FreelancerId,
                        IndustryId = model.IndustryId,
                        SolutionID = model.SolutionId,
                        IsProjectArchitect = model.ProjectArchitect
                    };

                    await _db.FreelancerPool.AddAsync(dbModel);
                    _db.SaveChanges();

                    return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                    {
                        StatusCode = StatusCodes.Status200OK,
                        Message = "Asigned Successfully!",
                    });
                }
                else
                {
                    return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                    {
                        StatusCode = StatusCodes.Status200OK,
                        Message = "Already Assigned!",
                    });
                }


            }
            catch (Exception ex)
            {

            }

            return StatusCode(StatusCodes.Status200OK, new APIResponseModel
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "Something went Wrong!",
            });
        }



        [HttpGet]
        [Route("GetFreelancersNameList")]
        public async Task<IActionResult> GetFreelancersNameList()
        {
            try
            {
                var list = await _userManager.Users.Where(x => x.IsDeleted == false && x.UserType == "Freelancer").ToListAsync();
                return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                {
                    StatusCode = StatusCodes.Status200OK,
                    Message = "Success",
                    Result = list
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
        [Route("AddTopProfessionalData")]
        public async Task<IActionResult> AddTopProfessionalData([FromBody] SolutionTopProfessionals model)
        {
            try
            {
                if (model.Id == 0)
                {
                    var dbModel = new SolutionTopProfessionals
                    {
                        FreelancerId = model.FreelancerId,
                        IndustryId = model.IndustryId,
                        SolutionId = model.SolutionId,
                        TopProfessionalTitle = model.TopProfessionalTitle,
                        Description = model.Description,
                        Rate = model.Rate,
                    };

                    await _db.SolutionTopProfessionals.AddAsync(dbModel);
                    _db.SaveChanges();
                    return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                    {
                        StatusCode = StatusCodes.Status200OK,
                        Message = "Data Save Successfully!",
                    });
                }
                else
                {
                    var topProfessionalData = _db.SolutionTopProfessionals.Where(x => x.Id == model.Id).FirstOrDefault();
                    if(topProfessionalData != null)
                    {
                        topProfessionalData.TopProfessionalTitle = model.TopProfessionalTitle;
                        topProfessionalData.Rate = model.Rate;
                        topProfessionalData.Description = model.Description;
                        _db.SaveChanges();

                        return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                        {
                            StatusCode = StatusCodes.Status200OK,
                            Message = "Data Updated Successfully!",
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

            return StatusCode(StatusCodes.Status200OK, new APIResponseModel
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "Something went Wrong!",
            });
        }


        [HttpPost]
        [Route("GetProfessionalList")]
        public async Task<IActionResult> GetProfessionalList(MileStoneDetailsViewModel model)
        {
            try
            {
                var list = await _db.SolutionTopProfessionals.Where(x => x.SolutionId == model.SolutionId && x.IndustryId == model.IndustryId).ToListAsync();
                return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                {
                    StatusCode = StatusCodes.Status200OK,
                    Message = "Success",
                    Result = list
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
        [Route("DeleteTopProfessionalData")]
        public async Task<IActionResult> DeleteTopProfessionalData([FromBody] MileStoneIdViewModel model)
        {
            try
            {
                if (model.Id != 0)
                {
                    var data = _db.SolutionTopProfessionals.Where(x => x.Id == model.Id).FirstOrDefault();
                    if (data != null)
                    {
                        _db.SolutionTopProfessionals.Remove(data);
                        _db.SaveChanges();

                        return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                        {
                            StatusCode = StatusCodes.Status200OK,
                            Message = "Delete Successfully!",
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

            return StatusCode(StatusCodes.Status200OK, new APIResponseModel
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "Something went Wrong!",
            });
        }

      
        [HttpPost]
        [Route("UpdateUserProfileImage")]
        public async Task<IActionResult> UpdateUserProfileImage([FromBody] SolutionImage solutionImage)
        {
            try
            {
                var UpdateImage = _db.FreelancerDetails.Where(x => x.UserId == solutionImage.FreelancerId).FirstOrDefault();
                if (UpdateImage != null)
                {

                    UpdateImage.ImageBlobStorageBaseUrl = solutionImage.BlobStorageBaseUrl;
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
            return StatusCode(StatusCodes.Status200OK, new APIResponseModel
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "Something Went Wrong"
            });
        }

        [HttpPost]
        [Route("RemoveUserFromInviteList")]
        public async Task<IActionResult> RemoveUserFromInviteList([FromBody] UserIdModel model)
        {
            try
            {
                if (model.Id != null)
                {
                    var userDetails = _db.Users.Where(x => x.Id == model.Id).FirstOrDefault();
                    if (userDetails != null && userDetails.IsInvited != false)
                    {
                        userDetails.IsInvited = false;
                        _db.SaveChanges();

                        return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                        {
                            StatusCode = StatusCodes.Status200OK,
                            Message = "Removed Successfully !"
                        });
                    }
                    else
                    {
                        return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                        {
                            StatusCode = StatusCodes.Status200OK,
                            Message = "Something Went Wrong"
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

       
        [HttpPost]
        [Route("SaveSuccessfullProjectResult")]
        public async Task<IActionResult> SaveSuccessfullProjectResult([FromBody] SolutionSuccessfullProjectResult model)
        {
            if(model.Id == 0)
            {
                var dbModel = new SolutionSuccessfullProjectResult
                {
                   SolutionSuccessfullProjectId = model.SolutionSuccessfullProjectId,
                    ResultKey= model.ResultKey,
                    ResultValue= model.ResultValue
                };

                await _db.SolutionSuccessfullProjectResult.AddAsync(dbModel);
                _db.SaveChanges();
                return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                {
                    StatusCode = StatusCodes.Status200OK,
                    Message = "Data Save Successfully!"
                });
            }
            else
            {
                var resultData = _db.SolutionSuccessfullProjectResult.Where(x => x.Id == model.Id).FirstOrDefault();
                if(resultData != null)
                {
                    resultData.ResultKey = model.ResultKey;
                    resultData.ResultValue = model.ResultValue;
                    _db.SaveChanges();
                    return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                    {
                        StatusCode = StatusCodes.Status200OK,
                        Message = "Data Updated Successfully!"
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
        [Route("SaveSuccessfullProject")]
        public async Task<IActionResult> SaveSuccessfullProject([FromBody] SolutionSuccessfullProject model)
        {
            try
            {
                if(model.Id == 0)
                {
                    var dbModel = new SolutionSuccessfullProject
                    {
                        IndustryId = model.IndustryId,
                        SolutionId = model.SolutionId,
                        Title = model.Title,
                        Description = model.Description,
                        IsActive = model.IsActive
                    };

                    await _db.SolutionSuccessfullProject.AddAsync(dbModel);
                    _db.SaveChanges();
                    return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                    {
                        StatusCode = StatusCodes.Status200OK,
                        Message = "Data Save Successfully!",
                        Result = dbModel.Id
                    });
                }
                else
                {
                    var projectData = _db.SolutionSuccessfullProject.Where(x => x.Id == model.Id).FirstOrDefault();
                    if(projectData != null)
                    {
                        projectData.Title = model.Title;
                        projectData.Description = model.Description;
                        projectData.IsActive = model.IsActive;
                        _db.SaveChanges();

                        return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                        {
                            StatusCode = StatusCodes.Status200OK,
                            Message = "Data Update Successfully!"
                        });
                    }
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
                Message = "Something Went Wrong"
            });
        }

        [HttpPost]
        [Route("GetSuccessfullProjectList")]
        public async Task<IActionResult> GetSuccessfullProjectList(MileStoneDetailsViewModel model)
        {
            try
            {
                var list = await _db.SolutionSuccessfullProject.Where(x => x.SolutionId == model.SolutionId && x.IndustryId == model.IndustryId).ToListAsync();
                return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                {
                    StatusCode = StatusCodes.Status200OK,
                    Message = "Success",
                    Result = list
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
        [Route("GetProjectResultList")]
        public async Task<IActionResult> GetProjectResultList(MileStoneIdViewModel model)
        {
            try
            {
                //var list = await _db.SolutionSuccessfullProject.Where(x => x.SolutionId == model.SolutionId && x.IndustryId == model.IndustryId).ToListAsync();
                //List<SolutionSuccessfullProjectResult> ResultList = new List<SolutionSuccessfullProjectResult>();
                //if(list.Count > 0){
                //    foreach(var data in list)
                //    {
                //        var resultData = _db.SolutionSuccessfullProjectResult.Where(x => x.SolutionSuccessfullProjectId == data.Id).ToList();
                //        if(resultData.Count > 0)
                //        {
                //            foreach(var result in resultData)
                //            {
                //                ResultList.Add(result);
                //            }
                //        }
                //    }
                //}
                var resultList = _db.SolutionSuccessfullProjectResult.Where(x => x.SolutionSuccessfullProjectId == model.Id).ToList();
                return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                {
                    StatusCode = StatusCodes.Status200OK,
                    Message = "Success",
                    Result = resultList
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
        [Route("DeleteProjectResultData")]
        public async Task<IActionResult> DeleteProjectResultData([FromBody] MileStoneIdViewModel model)
        {
            try
            {
                if (model.Id != 0)
                {
                    var data = _db.SolutionSuccessfullProjectResult.Where(x => x.Id == model.Id).FirstOrDefault();
                    if (data != null)
                    {
                        _db.SolutionSuccessfullProjectResult.Remove(data);
                        _db.SaveChanges();

                        return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                        {
                            StatusCode = StatusCodes.Status200OK,
                            Message = "Delete Successfully!",
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

            return StatusCode(StatusCodes.Status200OK, new APIResponseModel
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "Something went Wrong!",
            });
        }

        [HttpPost]
        [Route("DeleteSuccessfullProjectData")]
        public async Task<IActionResult> DeleteSuccessfullProjectData([FromBody] MileStoneIdViewModel model)
        {
            try
            {
                if (model.Id != 0)
                {
                    var data = _db.SolutionSuccessfullProjectResult.Where(x => x.SolutionSuccessfullProjectId == model.Id).ToList();
                    if (data.Count > 0)
                    {
                        _db.SolutionSuccessfullProjectResult.RemoveRange(data);
                        _db.SaveChanges();
                    }
                    var projectData = _db.SolutionSuccessfullProject.Where(x => x.Id == model.Id).FirstOrDefault();
                    if(projectData != null)
                    {
                        _db.SolutionSuccessfullProject.Remove(projectData);
                        _db.SaveChanges();
                    }

                    return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                    {
                        StatusCode = StatusCodes.Status200OK,
                        Message = "Delete Successfully!",
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

            return StatusCode(StatusCodes.Status200OK, new APIResponseModel
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "Something went Wrong!",
            });
        }

        [HttpPost]
        [Route("GetSuccessfullProjectDetailsById")]
        public async Task<IActionResult> GetSuccessfullProjectDetailsById([FromBody] MileStoneIdViewModel model)
        {
            try
            {
                if (model.Id != 0)
                {
                    var projectData = _db.SolutionSuccessfullProject.Where(x => x.Id == model.Id).FirstOrDefault();
                    if (projectData != null)
                    {
                        return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                        {
                            StatusCode = StatusCodes.Status200OK,
                            Message = "Success",
                            Result = projectData,
                        });
                    }

                    return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                    {
                        StatusCode = StatusCodes.Status200OK,
                        Message = "Data not found!",
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

            return StatusCode(StatusCodes.Status200OK, new APIResponseModel
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "Something went Wrong!",
            });
        }

        
        [HttpPost]
        [Route("GetProjectDetailsById")]
        public async Task<IActionResult> GetProjectDetailsById([FromBody] MileStoneIdViewModel model)
        {
            try
            {
                if (model.Id != 0)
                {
                    var resultData = _db.SolutionSuccessfullProjectResult.Where(x => x.Id == model.Id).FirstOrDefault();
                    if (resultData != null)
                    {
                        return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                        {
                            StatusCode = StatusCodes.Status200OK,
                            Message = "Success",
                            Result = resultData,
                        });
                    }

                    return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                    {
                        StatusCode = StatusCodes.Status200OK,
                        Message = "Data not found!",
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

            return StatusCode(StatusCodes.Status200OK, new APIResponseModel
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "Something went Wrong!",
            });
        }

        [HttpPost]
        [Route("SaveLevelData")]
        public async Task<IActionResult> SaveLevelData([FromBody] List<LevelRange> levelList)
        {
            try
            {
                if(levelList.Count > 0)
                {
                    foreach(var level in levelList)
                    {
                        if (level.ID == 0)
                        {
                            var dbModel = new LevelRange
                            {
                                Level = level.Level,
                                minLevel = level.minLevel,
                                maxLevel = level.maxLevel
                            };

                            await _db.LevelRanges.AddAsync(dbModel);
                            _db.SaveChanges();
                        }
                        else
                        {
                            var LevelData = _db.LevelRanges.Where(x => x.ID == level.ID).FirstOrDefault();
                            if (LevelData != null)
                            {
                                LevelData.Level = level.Level;
                                LevelData.minLevel = level.minLevel;
                                LevelData.maxLevel = level.maxLevel;
                                _db.SaveChanges();
                            }
                        }
                    }
                    return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                    {
                        StatusCode = StatusCodes.Status200OK,
                        Message = "Data Saved Successfully!"
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
                Message = "Something Went Wrong"
            });
        }

        [HttpGet]
        [Route("GetSavedLevelsList")]
        public async Task<IActionResult> GetSavedLevelsList()
        {
            try
            {
                var list = await _db.LevelRanges.ToListAsync();
                return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                {
                    StatusCode = StatusCodes.Status200OK,
                    Message = "Success",
                    Result = list
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
        [Route("GetTopProfessionalDetailsById")]
        public async Task<IActionResult> GetTopProfessionalDetailsById([FromBody] MileStoneIdViewModel model)
        {
            try
            {
                if (model.Id != 0)
                {
                    var topprofessionalData = _db.SolutionTopProfessionals.Where(x => x.Id == model.Id).FirstOrDefault();
                    if (topprofessionalData != null)
                    {
                        var freelancerdata = _db.FreelancerDetails.Where(x => x.UserId == topprofessionalData.FreelancerId).FirstOrDefault();
                        if(freelancerdata != null)
                        {
                            return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                            {
                                StatusCode = StatusCodes.Status200OK,
                                Message = "Success",
                                Result = new
                                {
                                    TopProfessionalData = topprofessionalData,
                                    FreelancerData = freelancerdata
                                }
                            });
                        }
                       
                    }

                    return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                    {
                        StatusCode = StatusCodes.Status200OK,
                        Message = "Data not found!",
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

            return StatusCode(StatusCodes.Status200OK, new APIResponseModel
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "Something went Wrong!",
            });
        }
    }
}
