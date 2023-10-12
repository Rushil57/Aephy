using Aephy.API.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using static Aephy.API.Models.AdminViewModel;
using RevolutAPI.Models.BusinessApi.Counterparties;
using Aephy.API.DBHelper;
using Aephy.API.Revoult;
using static RevolutAPI.Models.BusinessApi.Counterparties.AddNonRevolutCounterpartyReq;

namespace Aephy.API.Controllers
{
    [Route("api/Revoult/")]
    [ApiController]
    public class RevoultController : ControllerBase
    {
        private readonly AephyAppDbContext _db;
        private readonly IRevoultService _revoultService;
        public RevoultController(AephyAppDbContext dbContext, IRevoultService revoultService)
        {
            _db = dbContext;
            _revoultService = revoultService;
        }

        [HttpPost]
        [Route("OnboardUserRevoultAccount")]
        public async Task<IActionResult> OnboardUserRevoultAccount([FromBody] AddNonRevolutCounterpartyReq addNonRevolutCounterpartyReq)
        {
            if (addNonRevolutCounterpartyReq != null)
            {
                try
                {
                    var freelancerDetails = _db.FreelancerDetails.Where(x => x.UserId == addNonRevolutCounterpartyReq.UserId).FirstOrDefault();
                    if(freelancerDetails != null)
                    {
                        var UserDetails = _db.Users.Where(x => x.Id == addNonRevolutCounterpartyReq.UserId).FirstOrDefault();
                        var CountryCode = _db.Country.Where(x => x.Id == UserDetails.CountryId).Select(x => x.Code).FirstOrDefault();

                        if (addNonRevolutCounterpartyReq.Address is null)
                        {
                            addNonRevolutCounterpartyReq.Address = new AddressData
                            {
                                Postcode = freelancerDetails.PostCode,
                                Country = CountryCode,
                                City = freelancerDetails.City,
                                StreetLine1 = freelancerDetails.Address
                            };
                        }

                        if (addNonRevolutCounterpartyReq.IndividualName is null)
                        {
                            addNonRevolutCounterpartyReq.IndividualName = new IndividualNameData
                            {
                                FirstName = UserDetails.FirstName,
                                LastName = UserDetails.LastName
                            };
                        }

                        var resp = await _revoultService.AddInternationalCounterParty(addNonRevolutCounterpartyReq);
                        if(resp != null)
                        {
                            if (resp.State == "created")
                            {
                                var UserData = _db.Users.Where(x => x.Id == addNonRevolutCounterpartyReq.UserId).FirstOrDefault();
                                if (UserData != null)
                                {
                                    UserData.RevolutConnectId = UserData.Id;
                                    UserData.RevolutStatus = true;
                                    if(resp.Accounts != null)
                                    {
                                        UserData.RevolutAccountId = resp.Accounts[0].Id;
                                    }
                                    _db.SaveChanges();
                                }
                                return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                                {
                                    StatusCode = StatusCodes.Status200OK,
                                    Message = "Account Created",
                                });
                            }
                        }
                    }
                    return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                    {
                        StatusCode = StatusCodes.Status200OK,
                        Message = "Error Occur while registering please try later",

                    });
                }
                catch (Exception ex)
                {
                    return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                    {
                        StatusCode = StatusCodes.Status200OK,
                        Message = ex.Message + ex.InnerException,
                    });
                }

            }
            return StatusCode(StatusCodes.Status200OK, new APIResponseModel
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "Data Not Found",
            });
        }
    }
}
