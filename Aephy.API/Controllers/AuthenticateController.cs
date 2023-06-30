﻿using Aephy.API.DBHelper;
using Aephy.API.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Aephy.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticateController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;
        private Microsoft.AspNetCore.Hosting.IHostingEnvironment _hostingEnv;
        CommonMethod common;

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
        public async Task<IActionResult> Login([FromBody] LoginModel model)
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
                        if (!string.IsNullOrEmpty(model.FCMToken))
                        {
                            if (!string.IsNullOrEmpty(model.Device))
                            {
                                var userRoles = await _userManager.GetRolesAsync(user);

                                var authClaims = new List<Claim>
                            {
                                new Claim(ClaimTypes.Name, user.UserName),
                                new Claim(ClaimTypes.NameIdentifier, user.Id),
                                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                            };

                                foreach (var userRole in userRoles)
                                {
                                    authClaims.Add(new Claim(ClaimTypes.Role, userRole));
                                }

                                var token = GetToken(authClaims);
                                var refreshToken = GenerateRefreshToken();

                                int.TryParse(_configuration["JWT:RefreshTokenValidityInDays"], out int refreshTokenValidityInDays);

                                user.RefreshToken = refreshToken;
                                user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(refreshTokenValidityInDays);
                                user.FCMToken = model.FCMToken.Trim();
                                user.Device = model.Device.Trim().ToUpper();
                                await _userManager.UpdateAsync(user);
                                userDetail.UserId = user.Id;
                                userDetail.UserName = user.FirstName + " " + user.LastName;
                                userDetail.Email = user.UserName;
                                if (!string.IsNullOrEmpty(user.ProfileUrl))
                                {
                                    userDetail.ProfileImage = _configuration["BlobStorageSettings:UserImagesPath"].ToString() + user.ProfileUrl + _configuration["BlobStorageSettings:UserImagesPathToken"].ToString();
                                    //Byte[] bytes = common.GetFileFromAzure(user.ProfileUrl);
                                    //userDetail.ProfileImage = common.ImageCompressed(bytes);

                                }

                                return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                                {
                                    StatusCode = StatusCodes.Status200OK,
                                    Message = "Success",
                                    Result = new
                                    {
                                        Token = new JwtSecurityTokenHandler().WriteToken(token),
                                        RefreshToken = refreshToken,
                                        Expiration = token.ValidTo,
                                        RefreshTokenExpiration = user.RefreshTokenExpiryTime.ToUniversalTime(),
                                        Id = user.Id,
                                        UserDetail = userDetail
                                    }
                                });
                            }
                            else
                            {
                                strError = "Device is required";
                            }
                        }
                        else
                        {
                            strError = "FCMToken is required";
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
        public async Task<IActionResult> Register([FromForm] RegisterModel model)
        {
            try
            {
                var userExists = await _userManager.FindByNameAsync(model.Email);
                if (userExists != null)
                    return StatusCode(StatusCodes.Status200OK, new APIResponseModel { StatusCode = StatusCodes.Status403Forbidden, Message = "User already exists with this email address" });

                ApplicationUser user = new()
                {
                    Email = model.Email,
                    SecurityStamp = Guid.NewGuid().ToString(),
                    UserName = model.Email,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    CreatedDateTime = DateTime.UtcNow,
                };

                if (model.ProfileImage != null)
                {
                    if (model.ProfileImage.Length > 0)
                    {
                        user.ProfileUrl = await common.UploadBlobFile(model.ProfileImage, "userimages");
                        //IFormFile formFile = model.ProfileImage;

                        //var folderPath = Path.Combine(_hostingEnv.ContentRootPath, "UserImages");
                        //var folderName = "UserImages";
                        //var fileName = Path.GetRandomFileName() + Path.GetExtension(formFile.FileName).ToLowerInvariant();
                        //var filePath = Path.Combine(folderPath, fileName);

                        //if (!Directory.Exists(folderPath))
                        //    Directory.CreateDirectory(folderPath);

                        //using (var fileStream = new FileStream(filePath, FileMode.Create))
                        //{
                        //    await formFile.CopyToAsync(fileStream);
                        //    fileStream.Flush();
                        //    user.ProfileUrl = fileName;
                        //}
                    }
                }
                var result = await _userManager.CreateAsync(user, model.Password);
                if (!result.Succeeded)
                {
                    string msg = "Password should follow below criteria:\n";
                    var strings = result.Errors.ToList().Select(x => x.Description).ToList();
                    return StatusCode(StatusCodes.Status200OK, new APIResponseModel { StatusCode = StatusCodes.Status403Forbidden, Message = msg + string.Join("\n", strings) });
                }
                else
                {
                    return StatusCode(StatusCodes.Status200OK, new APIResponseModel { StatusCode = StatusCodes.Status200OK, Message = "User created successfully" });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new APIResponseModel { StatusCode = StatusCodes.Status403Forbidden, Message = "Something Went Wrong" });
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
    }
}