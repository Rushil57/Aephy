using Aephy.Helper.Helpers;
using Aephy.WEB.Admin.Models;
using Microsoft.AspNetCore.Mvc;
using Aephy.WEB.Provider;

namespace Aephy.WEB.Admin.Controllers;

public class InviteUserController : Controller
{
    private readonly string _rootPath;
    private readonly IConfiguration _configuration;
    private readonly IApiRepository _apiRepository;

    public InviteUserController(IWebHostEnvironment hostEnvironment, IConfiguration configuration, IApiRepository apiRepository)
    {
        _rootPath = hostEnvironment.WebRootPath;
        _configuration = configuration;
        _apiRepository = apiRepository;
    }

    public IActionResult Invite()
    {
        return View();
    }

    [HttpPost]
    public JsonResult AddInviteForm([FromBody] InviteUserViewModel InviteData)
    {
        if (InviteData != null)
        {

            #region SendInvite Email

            string body = System.IO.File.ReadAllText(_rootPath + "/EmailTemplates/InviteTemplate.html");
            string signUpUrl = _configuration.GetValue<string>("InviteURL:Url").Replace("{{InviteFlag}}", "true");

            body = body.Replace("{{ user_name }}", InviteData.FirstName + " " + InviteData.LastName);
            body = body.Replace("{{ url }}", signUpUrl);

            bool send = SendEmailHelper.SendEmail(InviteData.EmailAddress, "Welcome to Ephey", body);

            if (!send)
            {
                return Json(new { message = "Invitation sending failed." });
            }
            #endregion

            return Json(new { message = "Invitation sending success." });

        }
        else
        {
            return Json(new { message = "Invitation sending failed. null data found." });
        }
    }

    [HttpGet]
    public async Task<string> GetInvitedUsers()
    {
        var userList = await _apiRepository.MakeApiCallAsync("api/Admin/InvitedUserList", HttpMethod.Get);
        return userList;
    }
}
