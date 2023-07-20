using Aephy.API.DBHelper;
using Aephy.API.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
        public async Task<IActionResult> AddorEditSolutionData([FromBody] SolutionsModel model)
        {
            if (model.Id == 0)
            {
                Solutions solutions = new Solutions();
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

                    if (model.solutionServices.Count > 0)
                    {
                        foreach (var services in model.solutionServices)
                        {
                            var solutionservices = new SolutionServices()
                            {
                                SolutionId = solution.Id,
                                ServicesId = services
                            };
                            _db.SolutionServices.Add(solutionservices);
                            _db.SaveChanges();
                        }
                    }
                    return StatusCode(StatusCodes.Status200OK, new APIResponseModel { StatusCode = StatusCodes.Status200OK, Message = "Data Saved Succesfully!." });
                }
                catch (Exception ex)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, new APIResponseModel { StatusCode = StatusCodes.Status403Forbidden, Message = "Something Went Wrong." });
                }
            }
            else
            {
                //try
                //{
                //    var servicesDetails = _db.Services.Where(x => x.Id == model.Id).FirstOrDefault();
                //    if (servicesDetails != null)
                //    {
                //        servicesDetails.ServicesName = model.ServiceName;
                //        servicesDetails.Active = model.Active;
                //        _db.SaveChanges();
                //    }
                //    return StatusCode(StatusCodes.Status200OK, new APIResponseModel { StatusCode = StatusCodes.Status200OK, Message = "Data Updated Successfully." });
                //}
                //catch (Exception ex)
                //{
                //    return StatusCode(StatusCodes.Status500InternalServerError, new APIResponseModel { StatusCode = StatusCodes.Status403Forbidden, Message = "Something Went Wrong." });
                //}
            }
            return StatusCode(StatusCodes.Status500InternalServerError, new APIResponseModel { StatusCode = StatusCodes.Status403Forbidden, Message = "Something Went Wrong." });

        }



        [HttpGet]
        [Route("SolutionList")]
        public async Task<IActionResult> SolutionList()
        {
            try
            {
                //var entryPoint = (from ep in _db.Solutions
                //                  join e in _db.SolutionServices on ep.Id equals e.SolutionId
                //                  join t in _db.SolutionIndustry on e.Id equals t.SolutionId
                //                  select new
                //                  {
                //                      Id = ep.Id,
                //                      Title = ep.Title,
                //                      SubTitle = ep.SubTitle,
                //                      Description = ep.Description,
                //                      Services = string.Join(", ", e.ServicesId),
                //                      Industry = string.Join(", ", t.IndustryId)
                //                  }).ToList();

                //var ok = from d in _db.Solutions
                //join c in _db.SolutionServices on d.Id equals c.SolutionId
                //join s in _db.SolutionIndustry on d.Id equals s.SolutionId
                //select new
                //{
                //    Id = d.Id,
                //    Title = d.Title,
                //    Description = d.Description,
                //    Industries = s.IndustryId,
                //    solutionServices = c.ServicesId
                //};

                List<Solutions> solution = _db.Solutions.ToList();
                List<SolutionServices> solutionservice = _db.SolutionServices.ToList();
                List<SolutionIndustry> solutionindustry = _db.SolutionIndustry.ToList();
                List<Industries> industries = _db.Industries.ToList();


                var emps = from e in solution
                           select new SolutionsModel
                           {
                               Id = e.Id,
                               Title = e.Title,
                               SubTitle = e.SubTitle,
                               Description = e.Description,
                               //solutionIndustries = string.Join(", ", emp_Depts.Where(x => x.SolutionId == e.Id).
                               //Select(c => c.In.DepartmentName))
                               //ServiceName = string.Join(", ", solutionservice.Where(x => x.SolutionId == e.Id)),
                           };
                return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                {
                    StatusCode = StatusCodes.Status200OK,
                    Message = "Success",
                    Result = emps
                });
            }
            catch (Exception ex)
            {

            }

            return StatusCode(StatusCodes.Status200OK, new APIResponseModel
            {
                StatusCode = StatusCodes.Status403Forbidden,
                Message = "Success"

            });
        }



        [HttpPost]
        [Route("DeleteSolutionById")]
        public async Task<IActionResult> DeleteSolutionById([FromBody] SolutionsModel solutionsModel)
        {
            if (solutionsModel.Id != 0)
            {
                try
                {
                    var solutionindustryList = _db.SolutionIndustry.Where(x => x.SolutionId == solutionsModel.Id).ToList();
                    if (solutionindustryList.Count > 0)
                    {
                        foreach (var solution in solutionindustryList)
                        {
                            _db.SolutionIndustry.Remove(solution);
                            _db.SaveChanges();
                        }
                    }

                    var solutionserviceslist = _db.SolutionServices.Where(x => x.SolutionId == solutionsModel.Id).ToList();
                    if (solutionserviceslist.Count > 0)
                    {
                        foreach (var solution in solutionserviceslist)
                        {
                            _db.SolutionServices.Remove(solution);
                            _db.SaveChanges();
                        }
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

        //SolutionDataById
        [HttpPost]
        [Route("SolutionDataById")]
        public async Task<IActionResult> SolutionDataById([FromBody] SolutionsModel solutionsModel)
        {
            try
            {
                var solutionRecord = _db.Solutions.Where(x => x.Id == solutionsModel.Id).FirstOrDefault();
                var solutionIndustry = _db.SolutionIndustry.Where(x => x.SolutionId == solutionsModel.Id).ToList();
                var solutionServices = _db.SolutionServices.Where(x => x.SolutionId == solutionsModel.Id).ToList();

                //var list = solutionIndustry.Concat(solutionServices);

                var categories = _db.SolutionIndustry.Select(c => new
                {
                    SolutionId = c.SolutionId,
                    IndustryId = c.IndustryId
                }).ToList();

                var services = _db.SolutionServices.Select(c => new
                {
                    SolutionId = c.SolutionId,
                    ServicesId = c.ServicesId
                }).ToList();

                var req = from r in _db.Solutions
                          from p in _db.SolutionIndustry
                          from q in _db.SolutionServices
                          where r.Id == solutionsModel.Id
                          select new
                          {
                              IndustryId = p.IndustryId,
                              ServicesId = q.ServicesId
                          };

                return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                {
                    StatusCode = StatusCodes.Status200OK,
                    Message = "success",
                    Result = solutionRecord
                });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new APIResponseModel { StatusCode = StatusCodes.Status403Forbidden, Message = "Something Went Wrong." });
            }
        }

    }
}
