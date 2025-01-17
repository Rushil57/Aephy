﻿using Aephy.API.DBHelper;
using Aephy.API.Models;
using Aephy.API.NotificationMethod;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Aephy.API.Controllers
{
    [Route("api/User/")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        public string UserImgPath;
        private readonly IConfiguration _configuration;
        private readonly AephyAppDbContext _db;
        NotificationHelper notificationHelper = new NotificationHelper();

        CommonMethod common;
        public UserController(UserManager<ApplicationUser> userManager, IConfiguration configuration, Microsoft.AspNetCore.Hosting.IWebHostEnvironment env, AephyAppDbContext dbContext)
        {
            _userManager = userManager;
            _configuration = configuration;
            UserImgPath = Path.Combine(env.ContentRootPath, "UserImages");
            _db = dbContext;
            common = new CommonMethod(dbContext);

        }

        /// <summary>
        /// Get User Details.
        /// </summary>
        /// <remarks>
        /// Sample request:
        /// 
        ///     {        
        ///       "UserId": "565e602b-6818-406c-b470-5d162d71de77"       
        ///     }
        ///     
        /// </remarks>
        [HttpPost]
        [Route("GetUserProfile")]
        public async Task<IActionResult> GetUserProfile([FromBody] GetUserProfileRequestModel model)
        {
            try
            {
                FreelancerDetails freelancerDetails = new FreelancerDetails();
                ClientDetails clientDetails = new ClientDetails();

                var filePath = "";
                if (!string.IsNullOrEmpty(model.UserId))
                {
                    var user = await _userManager.FindByIdAsync(model.UserId.Trim());
                    var Countryname = string.Empty;
                    var Countrycode = string.Empty;
                    var IBANComplusory = false;
                    var IsIndiaField = false;
                    var IsAustraliaField = false;
                    var IsMexicanField = false;
                    var IsUsField = false;
                    if (user != null)
                    {
                        if (user.CountryId != 0)
                        {
                            var CountryData = _db.Country.Where(x => x.Id == user.CountryId).FirstOrDefault();
                            if (CountryData != null)
                            {
                                Countryname = CountryData.CountryName;
                                Countrycode = CountryData.Code;
                                IBANComplusory = CountryData.IsIBANMandatory;

                                if (Countrycode == "IN")
                                {
                                    IsIndiaField = true;
                                }
                                if (Countrycode == "AU")
                                {
                                    IsAustraliaField = true;
                                }
                                if (Countrycode == "MX")
                                {
                                    IsMexicanField = true;
                                }
                                if (Countrycode == "US")
                                {
                                    IsUsField = true;
                                }
                            }
                        }
                        if (user.UserType == "Client")
                        {
                            clientDetails = _db.ClientDetails.Where(x => x.UserId == model.UserId).FirstOrDefault();
                        }
                        else
                        {
                            freelancerDetails = _db.FreelancerDetails.Where(x => x.UserId == model.UserId).FirstOrDefault();
                        }

                        AdminViewModel.UserDetailsModel UserDetails = new AdminViewModel.UserDetailsModel();
                        UserDetails.UserId = user.Id;
                        UserDetails.FirstName = string.IsNullOrEmpty(user.FirstName) ? "" : user.FirstName;
                        UserDetails.LastName = string.IsNullOrEmpty(user.LastName) ? "" : user.LastName;
                        UserDetails.Email = user.Email;
                        UserDetails.ProfileUrl = filePath;
                        UserDetails.Role = user.UserType;
                        UserDetails.Description = clientDetails.Description;
                        UserDetails.ClientAddress = clientDetails.Address;
                        UserDetails.HourlyRate = freelancerDetails.HourlyRate;
                        UserDetails.Education = freelancerDetails.Education;
                        UserDetails.ProffessionalExperience = freelancerDetails.ProffessionalExperience;
                        UserDetails.FreelancerAddress = freelancerDetails.Address;
                        UserDetails.FreelancerLevel = freelancerDetails.FreelancerLevel;
                        UserDetails.CVPath = freelancerDetails.CVPath;
                        UserDetails.ImagePath = freelancerDetails.ImagePath;
                        UserDetails.ImageUrlWithSas = freelancerDetails.ImageUrlWithSas;
                        UserDetails.CountryId = user.CountryId;
                        UserDetails.CountryName = Countryname;
                        UserDetails.CompanyName = clientDetails.CompanyName;
                        UserDetails.BackCountry = Countrycode;
                        UserDetails.IsIBanMandantory = IBANComplusory;
                        UserDetails.RevoultStatus = user.RevolutStatus;
                        UserDetails.ShowIndiaField = IsIndiaField;
                        UserDetails.ShowAustraliaField = IsAustraliaField;
                        UserDetails.ShowMexicanField = IsMexicanField;
                        UserDetails.ShowUsField = IsUsField;
                        UserDetails.FreelancerCity = freelancerDetails.City;
                        UserDetails.FreelancerPostCode = freelancerDetails.PostCode;
                        UserDetails.ClientCity = clientDetails.City;
                        UserDetails.ClientPostCode = clientDetails.PostCode;
                        UserDetails.PreferredCurrency = user.PreferredCurrency;
                        UserDetails.TaxType = user.TaxType;
                        UserDetails.TaxNumber = user.TaxNumber;
                        UserDetails.IsRevoultBankAccount = freelancerDetails.IsRevoultBankAccount;
                        UserDetails.RevTag = freelancerDetails.RevTag;
                        UserDetails.StartHour = user.StartHours;
                        UserDetails.EndHour = user.EndHours;
                        UserDetails.StartDate = freelancerDetails.StartDate;
                        UserDetails.EndDate = freelancerDetails.EndDate;
                        UserDetails.IsWeekendExclude = freelancerDetails.IsWeekendExclude;
                        UserDetails.IsNotAvailableForNextSixMonth = freelancerDetails.IsNotAvailableForNextSixMonth;
                        UserDetails.IsWorkEarlier = freelancerDetails.IsWorkEarlier;
                        UserDetails.IsWorkLater = freelancerDetails.IsWorkLater;
                        UserDetails.StartHoursEarlier = user.StartHoursEarlier;
                        UserDetails.StartHoursLater = user.StartHoursLater;
                        UserDetails.EndHoursEarlier = user.EndHoursEarlier;
                        UserDetails.EndHoursLater = user.EndHoursLater;
                        UserDetails.onMonday = user.onMonday;
                        UserDetails.onTuesday = user.onTuesday;
                        UserDetails.onWednesday = user.onWednesday;
                        UserDetails.onThursday = user.onThursday;
                        UserDetails.onFriday = user.onFriday;
                        UserDetails.onSaturday = user.onSaturday;
                        UserDetails.onSunday = user.onSunday;
                        UserDetails.ClientImagePath = clientDetails.ImagePath;
                        UserDetails.ClientImageUrlWithSas = clientDetails.ImageUrlWithSas;


                        return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                        {
                            StatusCode = StatusCodes.Status200OK,
                            Message = "Success",
                            Result = UserDetails
                        });
                    }
                    else
                    {
                        return StatusCode(StatusCodes.Status200OK, new APIResponseModel { StatusCode = StatusCodes.Status403Forbidden, Message = "User details does not found" });
                    }
                }
                else
                {
                    return StatusCode(StatusCodes.Status200OK, new APIResponseModel { StatusCode = StatusCodes.Status403Forbidden, Message = "User Id is required" });
                }

            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new APIResponseModel { StatusCode = StatusCodes.Status403Forbidden, Message = "Something Went Wrong." });
            }
        }

        /// <summary>
        ///  Delete User's Account.
        /// </summary>
        /// <remarks>
        /// Sample request:
        /// 
        ///     {        
        ///       "UserId": "565e602b-6818-406c-b470-5d162d71de77"       
        ///     }
        ///     
        /// </remarks>
        [HttpPost]
        [Route("DeleteAccount")]
        public async Task<IActionResult> DeleteAccount([FromBody] GetUserProfileRequestModel model)
        {
            string errStr = "";
            try
            {
                if (!string.IsNullOrEmpty(model.UserId))
                {
                    var user = await _userManager.FindByIdAsync(model.UserId.Trim());
                    if (user != null)
                    {
                        var result = await _userManager.DeleteAsync(user);
                        if (!result.Succeeded)
                        {
                            errStr = "User deleted failed! Please check user details and try again.";
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(user.ProfileUrl))
                            {
                                await common.DeleteBlobFile(user.ProfileUrl, "userimages");
                            }

                            return StatusCode(StatusCodes.Status200OK, new APIResponseModel { StatusCode = StatusCodes.Status200OK, Message = "User deleted successfully" });
                        }
                    }
                    else
                    {
                        errStr = "User Details not found";
                    }
                }
                return StatusCode(StatusCodes.Status200OK, new APIResponseModel { StatusCode = StatusCodes.Status403Forbidden, Message = string.IsNullOrEmpty(errStr) ? "UserId is required" : errStr });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new APIResponseModel { StatusCode = StatusCodes.Status403Forbidden, Message = "Something Went Wrong." });
            }
        }

        /// <summary>
        ///  Update User Details.
        /// </summary>
        /// <remarks>
        /// Sample request:
        /// 
        ///     Id:0ca89f59-d98f-4b20-8876-b92ea0fb70b2 
        ///     FirstName:Riya 
        ///     LastName:Parmar    
        ///     
        /// </remarks>
        [HttpPost]
        [Route("UpdateProfile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UserModel model)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(model.Id.Trim());
                if (user != null)
                {
                    user.FirstName = model.FirstName.Trim();
                    user.LastName = model.LastName.Trim();
                    user.PreferredCurrency = model.PreferredCurrency;
                    user.TaxNumber = model.TaxNumber;
                    user.TaxType = model.TaxType;
                    user.StartHours = model.StartHour;
                    user.EndHours = model.EndHour;

                    //if(model.freelancerDetail != null)
                    //{
                    //    user.CountryId = model.freelancerDetail.CountryId;
                    //}
                    //if(model.clientDetail != null)
                    //{
                    //    user.CountryId = model.clientDetail.CountryId;
                    //}
                    var result = await _userManager.UpdateAsync(user);

                    if (model.freelancerDetail != null)
                    {
                        var freelancerDetails = _db.FreelancerDetails.Where(x => x.UserId == model.Id.Trim()).FirstOrDefault();
                        if (freelancerDetails != null)
                        {
                            if (freelancerDetails.HourlyRate != model.freelancerDetail.HourlyRate)
                            {
                                var adminDetails = _db.Users.Where(x => x.UserType == "Admin").FirstOrDefault();
                                if (adminDetails != null)
                                {
                                    List<Notifications> notificationsList = new List<Notifications>();
                                    Notifications adminNoTeamnotifications = new Notifications();
                                    adminNoTeamnotifications.ToUserId = adminDetails.Id;
                                    adminNoTeamnotifications.NotificationText = model.FirstName + "'s hourly rate now " + model.freelancerDetail.HourlyRate;
                                    adminNoTeamnotifications.NotificationTitle = "Rate Update:";
                                    adminNoTeamnotifications.NotificationTime = DateTime.Now;
                                    adminNoTeamnotifications.IsRead = false;
                                    notificationsList.Add(adminNoTeamnotifications);

                                    await notificationHelper.SaveNotificationData(_db,notificationsList);
                                }

                            }
                            freelancerDetails.HourlyRate = model.freelancerDetail.HourlyRate;
                            freelancerDetails.Address = model.freelancerDetail.FreelancerAddress;
                            freelancerDetails.Education = model.freelancerDetail.Education;
                            freelancerDetails.ProffessionalExperience = model.freelancerDetail.ProffessionalExperience;
                            freelancerDetails.City = model.freelancerDetail.City;
                            freelancerDetails.PostCode = model.freelancerDetail.PostCode;
                            freelancerDetails.IsRevoultBankAccount = model.freelancerDetail.IsRevoultBankAccount;
                            if (model.freelancerDetail.IsRevoultBankAccount)
                            {
                                freelancerDetails.RevTag = model.freelancerDetail.RevTag;
                            }
                            else
                            {
                                freelancerDetails.RevTag = "";
                            }
                            _db.SaveChanges();

                            var userData = _db.Users.Where(x => x.Id == freelancerDetails.UserId).FirstOrDefault();
                            userData.CountryId = model.freelancerDetail.CountryId;
                            _db.SaveChanges();
                        }
                    }

                    if (model.clientDetail != null)
                    {
                        var clientDetails = _db.ClientDetails.Where(x => x.UserId == model.Id.Trim()).FirstOrDefault();
                        if (clientDetails != null)
                        {
                            clientDetails.Description = model.clientDetail.Description;
                            clientDetails.Address = model.clientDetail.ClientAddress;
                            clientDetails.CompanyName = model.clientDetail.CompanyName;
                            clientDetails.City = model.clientDetail.City;
                            clientDetails.PostCode = model.clientDetail.PostCode;
                            _db.SaveChanges();

                            var userData = _db.Users.Where(x => x.Id == clientDetails.UserId).FirstOrDefault();
                            userData.CountryId = model.clientDetail.CountryId;
                            _db.SaveChanges();
                        }
                    }
                    if (!result.Succeeded)
                    {
                        return StatusCode(StatusCodes.Status200OK, new APIResponseModel { StatusCode = StatusCodes.Status403Forbidden, Message = "User Updated failed! Please check user details and try again" });
                    }
                    else
                    {
                        //=== Remove notification for reminder update profile for admin ===//
                        var notificationData = await _db.Notifications.Where(x => x.NotificationTitle == "Profile Completion Reminder" && x.NotificationText.Contains(user.Id)).FirstOrDefaultAsync();
                        if (notificationData != null)
                        {
                            _db.Remove(notificationData);
                            await _db.SaveChangesAsync();
                        }
                        return StatusCode(StatusCodes.Status200OK, new APIResponseModel { StatusCode = StatusCodes.Status200OK, Message = "User Updated successfully!" });
                    }
                }
                else
                {
                    return StatusCode(StatusCodes.Status200OK, new APIResponseModel { StatusCode = StatusCodes.Status403Forbidden, Message = "User Details not found." });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new APIResponseModel { StatusCode = StatusCodes.Status200OK, Message = "Something Went Wrong." });
            }
        }

        [HttpPost]
        [Route("UpdateCalendarData")]
        public async Task<IActionResult> UpdateCalendarData([FromBody] CalendarData model)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(model.Id.Trim());
                if (user != null)
                {
                    bool IsDataUpdated = false;
                    if (model.IsNotAvailableForNextSixMonth)
                    {
                        var freelancerData = _db.FreelancerDetails.Where(x => x.UserId == model.Id.Trim()).FirstOrDefault();
                        if (freelancerData != null)
                        {
                            freelancerData.IsNotAvailableForNextSixMonth = model.IsNotAvailableForNextSixMonth;
                            _db.FreelancerDetails.Update(freelancerData);
                            _db.SaveChanges();
                        }
                        IsDataUpdated = true;
                    }
                    else
                    {
                        user.StartHours = model.StartHour;
                        user.EndHours = model.EndHour;
                        user.StartHoursFinal = model.StartHoursFinal;
                        user.EndHoursFinal = model.EndHoursFinal;
                        if (model.IsWorkEarlier)
                        {
                            user.StartHoursEarlier = model.StartHoursEarlier;
                            user.EndHoursEarlier = model.EndHoursEarlier;
                        }
                        if (model.IsWorkLater)
                        {
                            user.StartHoursLater = model.StartHoursLater;
                            user.EndHoursLater = model.EndHoursLater;
                        }

                        user.onSunday = model.onSunday;
                        user.onMonday = model.onMonday;
                        user.onTuesday = model.onTuesday;
                        user.onWednesday = model.onWednesday;
                        user.onThursday = model.onThursday;
                        user.onFriday = model.onFriday;
                        user.onSaturday = model.onSaturday;


                        var result = await _userManager.UpdateAsync(user);
                        await _db.SaveChangesAsync();

                        var freelancerDetails = _db.FreelancerDetails.Where(x => x.UserId == model.Id.Trim()).FirstOrDefault();
                        if (freelancerDetails != null)
                        {
                            if (model.IsWeekendExclude)
                            {
                                DateTime firstDay = model.StartDate ?? DateTime.Now;
                                DateTime lastDay = model.EndDate ?? DateTime.Now;
                                var dbmodelList = new List<FreelancerExcludeDate>();
                                var freelancerExcludatesList = _db.FreelancerExcludeDate.ToList();
                                var checkStartDate = freelancerDetails.StartDate ?? null;
                                var checkEndDate = freelancerDetails.EndDate ?? null;
                                var isOldWeekendExcluded = freelancerDetails.IsWeekendExclude ?? false;

                                if ((freelancerDetails.EndDate > lastDay && checkEndDate != null) || (freelancerDetails.StartDate < firstDay && checkStartDate != null) && isOldWeekendExcluded)
                                {
                                    DateTime StartDay = freelancerDetails.StartDate ?? DateTime.Now;
                                    DateTime EndDay = freelancerDetails.EndDate ?? DateTime.Now;

                                    var RdbmodelList = new List<FreelancerExcludeDate>();
                                    var RfreelancerExcludatesList = _db.FreelancerExcludeDate.ToList();

                                    if (StartDay != null && EndDay != null)
                                    {
                                        for (DateTime date = StartDay; date <= EndDay; date = date.AddDays(1))
                                        {
                                            if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
                                            {
                                                var dataRecord = RfreelancerExcludatesList.Where(x => x.ExcludeDate == date && x.FreelancerId == freelancerDetails.UserId).FirstOrDefault();
                                                if (dataRecord != null)
                                                {
                                                    _db.FreelancerExcludeDate.Remove(dataRecord);
                                                }
                                            }
                                        }
                                        await _db.SaveChangesAsync();
                                    }
                                }

                                if (firstDay != null && lastDay != null)
                                {
                                    for (DateTime date = firstDay; date <= lastDay; date = date.AddDays(1))
                                    {
                                        if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
                                        {
                                            var duplicate = await _db.FreelancerExcludeDate.Where(x => x.ExcludeDate == date && x.FreelancerId == freelancerDetails.UserId).FirstOrDefaultAsync();
                                            if (duplicate == null)
                                            {
                                                var dbmodel = new FreelancerExcludeDate()
                                                {
                                                    FreelancerId = freelancerDetails.UserId,
                                                    ExcludeDate = date
                                                };
                                                dbmodelList.Add(dbmodel);
                                            }
                                        }
                                    }
                                }
                                await _db.FreelancerExcludeDate.AddRangeAsync(dbmodelList);
                                await _db.SaveChangesAsync();
                            }
                            else
                            {
                                DateTime firstDay = model.StartDate ?? DateTime.Now;
                                DateTime lastDay = model.EndDate ?? DateTime.Now;
                                var checkStartDate = model.StartDate ?? null;
                                var checkEndDate = model.EndDate ?? null;

                                if ((freelancerDetails.EndDate > lastDay && checkEndDate != null) || (freelancerDetails.StartDate < firstDay && checkStartDate != null))
                                {
                                    firstDay = freelancerDetails.StartDate ?? DateTime.Now;
                                    lastDay = freelancerDetails.EndDate ?? DateTime.Now;
                                }

                                var dbmodelList = new List<FreelancerExcludeDate>();
                                var freelancerExcludatesList = _db.FreelancerExcludeDate.ToList();
                                int days = 0;

                                if (firstDay != null && lastDay != null)
                                {
                                    for (DateTime date = firstDay; date <= lastDay; date = date.AddDays(1))
                                    {
                                        if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
                                        {
                                            var dataRecord = freelancerExcludatesList.Where(x => x.ExcludeDate == date && x.FreelancerId == freelancerDetails.UserId).FirstOrDefault();
                                            if (dataRecord != null)
                                            {
                                                _db.FreelancerExcludeDate.Remove(dataRecord);
                                            }
                                        }
                                    }
                                }
                            }

                            freelancerDetails.StartDate = model.StartDate;
                            freelancerDetails.EndDate = model.EndDate;
                            freelancerDetails.IsWeekendExclude = model.IsWeekendExclude;
                            freelancerDetails.IsNotAvailableForNextSixMonth = model.IsNotAvailableForNextSixMonth;
                            freelancerDetails.IsWorkEarlier = model.IsWorkEarlier;
                            freelancerDetails.IsWorkLater = model.IsWorkLater;
                            _db.FreelancerDetails.Update(freelancerDetails);
                            await _db.SaveChangesAsync();
                        }
                        if (result.Succeeded)
                            IsDataUpdated = true;
                    }

                    if (!IsDataUpdated)
                    {
                        return StatusCode(StatusCodes.Status200OK, new APIResponseModel { StatusCode = StatusCodes.Status403Forbidden, Message = "Failed to save data! Please check details and try again" });
                    }
                    else
                    {
                        return StatusCode(StatusCodes.Status200OK, new APIResponseModel { StatusCode = StatusCodes.Status200OK, Message = "Data Saved Successfully!" });
                    }
                }
                else
                {
                    return StatusCode(StatusCodes.Status200OK, new APIResponseModel { StatusCode = StatusCodes.Status403Forbidden, Message = "User Details not found." });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new APIResponseModel { StatusCode = StatusCodes.Status200OK, Message = "Something Went Wrong." });
            }
        }

        /// <summary>
        ///  Change User Password.
        /// </summary>
        /// <remarks>
        /// Sample request:
        /// 
        ///     {        
        ///       "UserId": "565e602b-6818-406c-b470-5d162d71de77"       
        ///       "CurrentPassword": "Test@123",
        ///       "NewPassword": "Testing@123"
        ///     }
        ///     
        /// </remarks>
        [HttpPost]
        [Route("ChangePassword")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordModel model)
        {
            string strMessage = "";
            try
            {
                if (!string.IsNullOrEmpty(model.Id))
                {
                    if (!string.IsNullOrEmpty(model.CurrentPassword))
                    {
                        if (!string.IsNullOrEmpty(model.NewPassword))
                        {
                            var user = await _userManager.FindByIdAsync(model.Id.Trim());
                            if (user != null)
                            {
                                if (await _userManager.CheckPasswordAsync(user, model.CurrentPassword))
                                {
                                    if (model.CurrentPassword.Trim() == model.NewPassword.Trim())
                                    {
                                        strMessage = "Current password and new password can not be same";
                                    }
                                    else
                                    {
                                        var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword.Trim(), model.NewPassword.Trim());
                                        if (!result.Succeeded)
                                        {
                                            string msg = "Password should follow below criteria:\n";
                                            var strings = result.Errors.ToList().Select(x => x.Description).ToList();
                                            strMessage = msg + string.Join("\n", strings);
                                        }
                                        else
                                        {
                                            return StatusCode(StatusCodes.Status200OK, new APIResponseModel { StatusCode = StatusCodes.Status200OK, Message = "Password updated successfully" });
                                        }
                                    }
                                }
                                else
                                {
                                    strMessage = "Current password is not valid";
                                }
                            }
                            else
                            {
                                strMessage = "User details not found";
                            }
                        }
                        else
                        {
                            strMessage = "New password is required";
                        }
                    }
                    else
                    {
                        strMessage = "Current password is required";
                    }
                }
                else
                {
                    strMessage = "Id is required";
                }
                return StatusCode(StatusCodes.Status200OK, new APIResponseModel { StatusCode = StatusCodes.Status403Forbidden, Message = string.IsNullOrEmpty(strMessage) ? "Something Went Wrong" : strMessage });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new APIResponseModel { StatusCode = StatusCodes.Status200OK, Message = "Something Went Wrong" });
            }
        }

        /// <summary>
        ///  Remove Token.
        /// </summary>
        /// <remarks>
        /// Sample request:
        /// 
        ///     {        
        ///       "UserId": "565e602b-6818-406c-b470-5d162d71de77"       
        ///     }
        ///     
        /// </remarks>
        [HttpPost]
        [Route("Logout")]
        public async Task<IActionResult> Logout([FromBody] GetUserProfileRequestModel model)
        {
            try
            {
                if (!string.IsNullOrEmpty(model.UserId))
                {
                    var user = await _userManager.FindByIdAsync(model.UserId.Trim());
                    if (user != null)
                    {
                        user.FCMToken = "";
                        await _userManager.UpdateAsync(user);
                        await _userManager.UpdateSecurityStampAsync(user);
                        var result = await _userManager.RemoveAuthenticationTokenAsync(user, "JWT", "JWT Token");
                        if (result.Succeeded)
                            return StatusCode(StatusCodes.Status200OK, new APIResponseModel { StatusCode = StatusCodes.Status200OK, Message = "Success" });
                    }
                    return StatusCode(StatusCodes.Status200OK, new APIResponseModel { StatusCode = StatusCodes.Status403Forbidden, Message = "User detail not found" });
                }
                else
                {
                    return StatusCode(StatusCodes.Status200OK, new APIResponseModel { StatusCode = StatusCodes.Status403Forbidden, Message = "User Id is required" });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new APIResponseModel { StatusCode = StatusCodes.Status403Forbidden, Message = "Something Went Wrong." });
            }
        }

        [HttpPost]
        [Route("GetFreeLancerByType")]
        public async Task<IActionResult> GetFreeLancerByType([FromBody] string freeLancerLavel)
        {
            try
            {
                var dbFreeLancerIds = await _db.FreelancerDetails.Where(x => x.FreelancerLevel == freeLancerLavel).Select(x => x.UserId).ToListAsync();
                var dbUsers = await _db.Users.Where(x => dbFreeLancerIds.Contains(x.Id)).ToListAsync();

                if (dbUsers != null)
                {
                    return StatusCode(StatusCodes.Status200OK, new APIResponseModel { StatusCode = StatusCodes.Status200OK, Message = "Data Found.", Result = dbUsers });
                }
                else
                {
                    return StatusCode(StatusCodes.Status200OK, new APIResponseModel { StatusCode = StatusCodes.Status403Forbidden, Message = "No Data Found" });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new APIResponseModel { StatusCode = StatusCodes.Status200OK, Message = "Something Went Wrong" });
            }
        }

        [HttpPost]
        [Route("GetFreeLancerByIds")]
        public async Task<IActionResult> GetFreeLancerByIds([FromBody] UserIdsModel userIdsModel)
        {
            try
            {
                List<UserWiseLavelDetail> userlevelDetails = new List<UserWiseLavelDetail>();
                foreach (var id in userIdsModel.Ids)
                {
                    UserWiseLavelDetail data = new UserWiseLavelDetail();
                    var userData = await _db.Users.Where(x => x.Id == id).FirstOrDefaultAsync();
                    if (userData != null)
                    {
                        data.Id = userData.Id;
                        data.FirstName = userData.FirstName;
                        data.LastName = userData.LastName;
                        data.Lavel = _db.FreelancerDetails.Where(x => x.UserId == id).Select(x => x.FreelancerLevel).FirstOrDefault();
                        userlevelDetails.Add(data);
                    }
                }
                //var dbUsers = await _db.Users.Where(x => userIdsModel.Ids.Contains(x.Id)).Select(x=> new UserWiseLavelDetail
                //{
                //    Id = x.Id,
                //    FirstName = x.FirstName,
                //    LastName = x.LastName,
                //    Lavel = _db.FreelancerDetails.Where(y=>y.UserId == x.Id).Select(y => y.FreelancerLevel).FirstOrDefault()
                //}).ToListAsync();

                if (userlevelDetails.Count > 0)
                {
                    return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                    {
                        StatusCode = StatusCodes.Status200OK,
                        Message = "Data Found.",
                        Result = userlevelDetails
                    });
                }
                else
                {
                    return StatusCode(StatusCodes.Status200OK, new APIResponseModel { StatusCode = StatusCodes.Status403Forbidden, Message = "No Data Found" });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new APIResponseModel { StatusCode = StatusCodes.Status200OK, Message = "Something Went Wrong" });
            }
        }
    }
}
