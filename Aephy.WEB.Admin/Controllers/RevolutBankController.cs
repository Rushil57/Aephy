using Aephy.WEB.Admin.Models;
using Aephy.WEB.Provider;
using Microsoft.AspNetCore.Mvc;

namespace Aephy.WEB.Admin.Controllers
{
    public class RevolutBankController : Controller
    {
        private readonly IApiRepository _apiRepository;

        public RevolutBankController(IApiRepository apiRepository)
        {
            _apiRepository = apiRepository;
        }

        public ActionResult RevolutBank()
        {
            return View();
        }

        //OnboardAdminRevoultAccount
        [HttpPost]
        public async Task<string> OnboardAdminRevoultAccount([FromBody] AddNonRevolutCounterpartyReq addNonRevolutCounterpartyReq)
        {
            if (addNonRevolutCounterpartyReq != null)
            {
                try
                {
                    var userId = HttpContext.Session.GetString("LoggedAdmin");
                    addNonRevolutCounterpartyReq.UserId = userId;
                    var data = await _apiRepository.MakeApiCallAsync("api/Revoult/OnboardAdminRevoultAccount", HttpMethod.Post, addNonRevolutCounterpartyReq);
                    return data;
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
            return "";
        }

        //GetEphylinkRevoultDetails
        [HttpGet]
        public async Task<string> GetEphylinkRevoultDetails()
        {

            try
            {
                var data = await _apiRepository.MakeApiCallAsync("api/Revoult/GetEphylinkRevoultDetails", HttpMethod.Get);
                return data;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
