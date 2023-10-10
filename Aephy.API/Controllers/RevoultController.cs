using Aephy.API.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using static Aephy.API.Models.AdminViewModel.AddNonRevolutCounterpartyReq;
using static Aephy.API.Models.AdminViewModel;
using Aephy.API.DBHelper;
using Aephy.API.Revoult;

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
        [Route("OboardUserRevoultAccount")]
        public async Task<IActionResult> OboardUserRevoultAccount([FromBody] AddNonRevolutCounterpartyReq addNonRevolutCounterpartyReq)
        {
            if (addNonRevolutCounterpartyReq != null)
            {
                try
                {
                    if (addNonRevolutCounterpartyReq.Address is null)
                    {
                        addNonRevolutCounterpartyReq.Address = new AddressData
                        {
                            Postcode = "380006",
                            Country = "FR",
                            City = "Ahmedabad",
                            StreetLine1 = "Steert No 1 House 2"
                        };
                    }

                    if (addNonRevolutCounterpartyReq.IndividualName is null)
                    {
                        addNonRevolutCounterpartyReq.IndividualName = new IndividualNameData
                        {
                            FirstName = "Name",
                            LastName = "Coo"
                        };
                    }

                    var resp = await _revoultService.AddInternationalCounterParty(addNonRevolutCounterpartyReq);
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
                    return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                    {
                        StatusCode = StatusCodes.Status200OK,
                        Message = "Something Went Wrong",

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
