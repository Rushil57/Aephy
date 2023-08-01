using Aephy.API.DBHelper;
using Aephy.API.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Aephy.API.Controllers
{
    [Route("/api/Authenticate/")]
    [ApiController]
    public class AuthenticateController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;
        private Microsoft.AspNetCore.Hosting.IHostingEnvironment _hostingEnv;
        CommonMethod common;
        private readonly AephyAppDbContext _db;

        public AuthenticateController(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IConfiguration configuration,
            Microsoft.AspNetCore.Hosting.IHostingEnvironment env, AephyAppDbContext dbContext)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration;
            _hostingEnv = env;
            _db = dbContext;
            common = new CommonMethod(dbContext);
        }


        /// <summary>
        /// Validate User’s Credentials and Return Token.
        /// </summary>
        /// <remarks>
        /// Sample request:
        /// 
        ///     {        
        ///       "Username": "user.avidclan@gmail.com",
        ///       "Password": "Test@123",
        ///       "FCMToken": "ZmNXw9mT1KfKb2gS1G15f:APA91bHF15E2I3f80U4Ry21lswfgfnIByeRTxgLINBJCBqBQTFnmtU46QAdbZDDuNBJQ6rbjAw69gb3tS0icM0-tm5rgcXO-_79MffPpGUDTs8abAeSff-JiZ86_8SRYfVVT_ol1Hf6R",
        ///       "Device": "A"
        ///     }
        /// </remarks>
        [HttpPost]
        [Route("Login")]
        public async Task<IActionResult> Login(LoginModel model)
        {
            UserDetail userDetail = new UserDetail();
            string strError = "";
            try
            {
                var user = await _userManager.FindByNameAsync(model.Username);
                if (user != null)
                {
                    if (await _userManager.CheckPasswordAsync(user, model.Password))
                    {
                        if (user.IsDeleted == false)
                        {
                            //Code for check user are active or not
                            if (user.IsActive)
                            {
                                FreelancerDetails freelancer = new FreelancerDetails();
                                if (user.UserType == "Freelancer")
                                {
                                    freelancer = _db.FreelancerDetails.Where(x => x.UserId == user.Id).FirstOrDefault();
                                }
                                return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                                {
                                    StatusCode = StatusCodes.Status200OK,
                                    Message = "Login Success",
                                    Result = new
                                    {
                                        UserId = user.Id,
                                        FirstName = user.FirstName,
                                        LastName = user.LastName,
                                        Role = user.UserType,
                                        Level = !String.IsNullOrEmpty(freelancer.FreelancerLevel) ? freelancer.FreelancerLevel : "none"
                                    }
                                });
                            }
                            else
                            {
                                strError = "Your account not active please active once";
                            }
                        }
                        else
                        {
                            strError = "User does not exists with this email address";
                        }  
                    }
                    else
                    {
                        strError = "Please enter valid credentials";
                    }
                }
                else
                {
                    strError = "User does not exists with this email address";
                }
                return StatusCode(StatusCodes.Status200OK, new APIResponseModel { StatusCode = StatusCodes.Status403Forbidden, Message = !string.IsNullOrEmpty(strError) ? strError : "Something Went Wrong" });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new APIResponseModel { StatusCode = StatusCodes.Status403Forbidden, Message = "Something Went Wrong" });
            }
        }

        /// <summary>
        /// New User Registration.
        /// </summary>
        /// <remarks>
        /// Sample request:
        /// 
        ///     Email:user.avidclan@gmail.com
        ///     FirstName:userfirstname 
        ///     LastName:test
        ///     Password:Test@123
        ///     
        /// </remarks>
        [HttpPost]
        [Route("Register")]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            try
            {
                var userExists = await _userManager.FindByNameAsync(model.Email);
                if (userExists != null)
                    return StatusCode(StatusCodes.Status200OK, new APIResponseModel { StatusCode = StatusCodes.Status403Forbidden, Message = "User already exists with this email address" });
                var refreshToken = GenerateRefreshToken();
                ApplicationUser user = new()
                {
                    Email = model.Email,
                    SecurityStamp = Guid.NewGuid().ToString(),
                    UserName = model.Email,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    RefreshToken = refreshToken,
                    UserType = model.UserType,
                    CreatedDateTime = DateTime.UtcNow,
                    IsDeleted = false,
                    IsInvited = Convert.ToBoolean(model.IsInvited),
                    IsActive = model.UserType == "Admin" ? true : false,
                };
                var result = await _userManager.CreateAsync(user, model.Password);
                if (!result.Succeeded)
                {
                    string msg = "Password should follow below criteria:\n";
                    var strings = result.Errors.ToList().Select(x => x.Description).ToList();
                    return StatusCode(StatusCodes.Status200OK, new APIResponseModel { StatusCode = StatusCodes.Status403Forbidden, Message = msg + string.Join("\n", strings) });
                }
                else
                {
                    var getUserDetail = await _userManager.FindByEmailAsync(model.Email);
                    if (getUserDetail != null)
                    {
                        if (getUserDetail.UserType == "Client")
                        {
                            ClientDetails client = new()
                            {
                                UserId = getUserDetail.Id.ToString()
                            };
                            _db.ClientDetails.Add(client);
                            _db.SaveChanges();
                        }
                        else
                        {
                            FreelancerDetails freelancer = new()
                            {
                                UserId = getUserDetail.Id.ToString(),
                                FreelancerLevel = model.FreelancerLevel
                            };
                            _db.FreelancerDetails.Add(freelancer);
                            _db.SaveChanges();
                        }

                    }

                    return StatusCode(StatusCodes.Status200OK, new APIResponseModel { StatusCode = StatusCodes.Status200OK, Message = "User created successfully", Result = getUserDetail });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new APIResponseModel { StatusCode = StatusCodes.Status403Forbidden, Message = ex.Message + "  ||  " + ex.StackTrace });
            }
        }

        private JwtSecurityToken GetToken(List<Claim> authClaims)
        {
            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]));

            var token = new JwtSecurityToken(
                issuer: _configuration["JWT:ValidIssuer"],
                audience: _configuration["JWT:ValidAudience"],
                expires: DateTime.UtcNow.AddHours(24),
                claims: authClaims,
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
                );

            return token;
        }


        private static string GenerateRefreshToken()
        {
            var randomNumber = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        private ClaimsPrincipal? GetPrincipalFromExpiredToken(string? token)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"])),
                ValidateLifetime = false
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);
            if (securityToken is not JwtSecurityToken jwtSecurityToken || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                throw new SecurityTokenException("Invalid token");

            return principal;
        }

        /// <summary>
        /// Regenerate Password of User and Send it to an existing Email Id
        /// </summary>
        /// <remarks>
        /// Sample request:
        /// 
        ///     {
        ///         "UserEmail": "user.avidclan@gmail.com"
        ///     }
        ///     
        /// </remarks>
        [HttpPost]
        [Route("ForgotPassword")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequestModel model)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(model.UserEmail.Trim());
                if (user != null)
                {
                    //var UserLoggedIn = user.ToString();
                    string newPassword = common.GenerateRandomPassword();
                    var resetPasswordToken = await _userManager.GeneratePasswordResetTokenAsync(user);
                    var UpdatedPassword = await _userManager.ResetPasswordAsync(user, resetPasswordToken, newPassword);
                    if (UpdatedPassword.Succeeded)
                    {
                        //var folderPath = Path.Combine(_hostingEnv.ContentRootPath, "Images");
                        //var filePath = Path.Combine(folderPath, " Logo.png");

                        var mailSend = common.SendMail(user.ToString(), newPassword, user.FirstName);
                        if (mailSend.Result == "Mail Sent Successfully")
                        {
                            return StatusCode(StatusCodes.Status200OK, new APIResponseModel { StatusCode = StatusCodes.Status200OK, Message = "We have sent an updated password to your registered email address. You can use that password and we recommend updating the password after login from the mobile application" });
                        }
                        else
                        {
                            return StatusCode(StatusCodes.Status200OK, new APIResponseModel { StatusCode = StatusCodes.Status403Forbidden, Message = "Mail not sent" });
                        }
                    }
                    else
                    {
                        return StatusCode(StatusCodes.Status200OK, new APIResponseModel { StatusCode = StatusCodes.Status403Forbidden, Message = "Password does not updated" });
                    }
                }
                else
                {
                    return StatusCode(StatusCodes.Status200OK, new APIResponseModel { StatusCode = StatusCodes.Status403Forbidden, Message = "Email address does not exists" });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new APIResponseModel { StatusCode = StatusCodes.Status200OK, Message = "Something Went Wrong" });
            }

        }

        [HttpPost]
        [Route("VerifyAccount")]
        public async Task<IActionResult> VerifyAccount([FromBody] string userId)
        {
            try
            {
                var dbData = await _userManager.FindByIdAsync(userId);
                if (dbData != null)
                {

                    dbData.IsActive = true;
                    var status = await _userManager.UpdateAsync(dbData);
                    if (status.Succeeded)
                    {
                        return StatusCode(StatusCodes.Status200OK, new APIResponseModel { StatusCode = StatusCodes.Status200OK, Message = "Successfully Activated" });
                    }
                    else
                    {
                        return StatusCode(StatusCodes.Status200OK, new APIResponseModel { StatusCode = StatusCodes.Status403Forbidden, Message = "Verification Faild." });
                    }
                }
                else
                {
                    return StatusCode(StatusCodes.Status200OK, new APIResponseModel { StatusCode = StatusCodes.Status403Forbidden, Message = "Invalid User" });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new APIResponseModel { StatusCode = StatusCodes.Status200OK, Message = "Something Went Wrong" });
            }
        }
    }
}
