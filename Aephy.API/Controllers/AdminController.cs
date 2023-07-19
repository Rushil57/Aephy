﻿using Aephy.API.DBHelper;
using Aephy.API.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Security.Policy;
using static Aephy.API.Models.AdminViewModel;
using static Azure.Core.HttpHeader;

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

                    if(model.solutionIndustries.Count > 0)
                    {
                        foreach(var industry in model.solutionIndustries)
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
                var entryPoint = (from ep in _db.Solutions
                                  join e in _db.SolutionServices on ep.Id equals e.SolutionId
                                  join t in _db.SolutionIndustry on e.Id equals t.SolutionId
                                  select new
                                  {
                                      Id = ep.Id,
                                      Title = ep.Title,
                                      SubTitle = ep.SubTitle,
                                      Description = ep.Description,
                                      Services = string.Join(", ", e.ServicesId),
                                      Industry = string.Join(", ", t.IndustryId)
                                  }).ToList();

                return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                {
                    StatusCode = StatusCodes.Status403Forbidden,
                    Message = "Success",
                    Result = entryPoint
                });
            }
            catch(Exception ex)
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
                    SolutionIndustry services = _db.SolutionIndustry.Find(solutionsModel.Id);
                    if (services != null)
                    {
                        _db.SolutionIndustry.Remove(services);
                        _db.SaveChanges();
                    }
                    SolutionServices solutionServices = _db.SolutionServices.Find(solutionsModel.Id);   
                    if(solutionServices != null)
                    {
                        _db.SolutionServices.Remove(solutionServices);
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

    }
}
