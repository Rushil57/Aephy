using Aephy.API.DBHelper;
using Aephy.API.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.WindowsAzure.Storage.Shared.Protocol;
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
                            Active = model.Active
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
                    StatusCode = StatusCodes.Status403Forbidden,
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
                    StatusCode = StatusCodes.Status403Forbidden,
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
                if (industryList.Count > 0)
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
                    return StatusCode(StatusCodes.Status500InternalServerError, new APIResponseModel { StatusCode = StatusCodes.Status403Forbidden, Message = "Empty List" });
                }
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
                List<Industries> IndutrynameList = new List<Industries>();
                if (solutionindustry.Count > 0)
                {
                    foreach (var industryId in solutionindustry)
                    {
                        //var industryName = _db.Industries.Where(x => x.Id == industryId.IndustryId).Select(p => p.IndustryName).FirstOrDefault();
                        var industryName = _db.Industries.Where(x => x.Id == industryId.IndustryId).FirstOrDefault();
                        IndutrynameList.Add(industryName);
                    }
                }

                return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                {
                    StatusCode = StatusCodes.Status200OK,
                    Message = "success",
                    Result = new
                    {
                        Solution = solution,
                        IndustryResult = solutionindustry,
                        ServiceResult = solutionservice,
                        IndustryNameList = IndutrynameList
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
                var list = await _userManager.Users.ToListAsync();

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
        [Route("RolesList")]
        public async Task<IActionResult> RolesList()
        {
            try
            {
                var list = _db.GigOpenRoles.ToList();
                return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                {
                    StatusCode = StatusCodes.Status403Forbidden,
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
                            Level = model.Level
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


        //AddSolutionDescribedData
        [HttpPost]
        [Route("AddSolutionDescribedData")]
        public async Task<IActionResult> AddSolutionDescribedData([FromBody] List<SolutionDescribeModel> model)
        {
            try
            {
                if (model.Count > 0)
                {
                    List<int> industryIds = new List<int>();
                    foreach (var item in model)
                    {
                        if (item.Id == 0)
                        {
                            var solution = new SolutionIndustryDetails()
                            {
                                SolutionId = item.SolutionId,
                                IndustryId = item.IndustryId,
                                Description = item.Description
                            };
                            _db.SolutionIndustryDetails.Add(solution);
                            _db.SaveChanges();

                            industryIds.Add(solution.Id);   
                        }
                    }
                    return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                    {
                        StatusCode = StatusCodes.Status200OK,
                        Message = "Data Saved Succesfully!",
                        Result = industryIds
                    });
                }
                return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                {
                    StatusCode = StatusCodes.Status200OK,
                    Message = "No data found"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new APIResponseModel { StatusCode = StatusCodes.Status403Forbidden, Message = ex.Message + ex.InnerException });
            }
        }


        [HttpPost]
        [Route("SolutionDetailsDataById")]
        public async Task<IActionResult> SolutionDetailsDataById([FromBody] SolutionIdModel solutionsModel)
        {
            try
            {
                Solutions solution = _db.Solutions.Where(x => x.Id == solutionsModel.Id).FirstOrDefault();
                List<SolutionServices> solutionservice = _db.SolutionServices.Where(x => x.SolutionId == solutionsModel.Id).ToList();
                List<SolutionIndustry> solutionindustry = _db.SolutionIndustry.Where(x => x.SolutionId == solutionsModel.Id).ToList();
                List<SolutionIndustryDetails> solutionIndustryDetails = _db.SolutionIndustryDetails.Where(x => x.SolutionId == solutionsModel.Id).ToList();
                List<Industries> IndutrynameList = new List<Industries>();
                if (solutionindustry.Count > 0)
                {
                    foreach (var industryId in solutionindustry)
                    {
                        //var industryName = _db.Industries.Where(x => x.Id == industryId.IndustryId).Select(p => p.IndustryName).FirstOrDefault();
                        var industryName = _db.Industries.Where(x => x.Id == industryId.IndustryId).FirstOrDefault();
                        IndutrynameList.Add(industryName);
                    }
                }

                return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                {
                    StatusCode = StatusCodes.Status200OK,
                    Message = "success",
                    Result = new
                    {
                        Solution = solution,
                        IndustryResult = solutionindustry,
                        ServiceResult = solutionservice,
                        IndustryNameList = IndutrynameList
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new APIResponseModel { StatusCode = StatusCodes.Status403Forbidden, Message = ex.Message + ex.InnerException });
            }
        }
    }
}
