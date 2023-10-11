using Aephy.API.DBHelper;
using Aephy.API.Models;
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
                        if(user.CountryId != 0)
                        {
                            var CountryData = _db.Country.Where(x => x.Id == user.CountryId).FirstOrDefault();
                            if(CountryData != null)
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
                            freelancerDetails.HourlyRate = model.freelancerDetail.HourlyRate;
                            freelancerDetails.Address = model.freelancerDetail.FreelancerAddress;
                            freelancerDetails.Education = model.freelancerDetail.Education;
                            freelancerDetails.ProffessionalExperience = model.freelancerDetail.ProffessionalExperience;
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
                    var userData = _db.Users.Where(x => x.Id == id).FirstOrDefault();
                    if (userData != null)
                    {
                        data.Id = userData.Id;
                        data.FirstName = userData.FirstName;
                        data.LastName = userData.LastName;
                        data.Lavel = _db.FreelancerDetails.Where(x => x.UserId == id).Select(x => x.FreelancerLevel).FirstOrDefault();
                        data.IsProjectArchitect = _db.FreelancerPool.Where(x => x.FreelancerID == id && x.SolutionID == userIdsModel.SolutionId && x.IndustryId == userIdsModel.IndustryId).Select(x => x.IsProjectArchitect).FirstOrDefault();
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
