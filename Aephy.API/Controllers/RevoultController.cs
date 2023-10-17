using Aephy.API.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using static Aephy.API.Models.AdminViewModel;
using RevolutAPI.Models.BusinessApi.Counterparties;
using Aephy.API.DBHelper;
using Aephy.API.Revoult;
using static RevolutAPI.Models.BusinessApi.Counterparties.AddNonRevolutCounterpartyReq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Azure;

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
                                    UserData.RevolutConnectId = resp.Id;
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

        //OnboardAdminRevoultAccount
        [HttpPost]
        [Route("OnboardAdminRevoultAccount")]
        public async Task<IActionResult> OnboardAdminRevoultAccount([FromBody] AddNonRevolutCounterpartyReq addNonRevolutCounterpartyReq)
        {
            if (addNonRevolutCounterpartyReq != null)
            {
                try
                {
                    var userDetails = _db.Users.Where(x => x.Id == addNonRevolutCounterpartyReq.UserId).FirstOrDefault();
                    if (userDetails != null)
                    {
                        if (addNonRevolutCounterpartyReq.Address is null)
                        {
                            addNonRevolutCounterpartyReq.Address = new AddressData
                            {
                                Postcode = addNonRevolutCounterpartyReq.Address.Postcode,
                                Country = addNonRevolutCounterpartyReq.Address.Country,
                                City = addNonRevolutCounterpartyReq.Address.City,
                                StreetLine1 = addNonRevolutCounterpartyReq.Address.StreetLine1
                            };
                        }

                        if (addNonRevolutCounterpartyReq.IndividualName is null)
                        {
                            addNonRevolutCounterpartyReq.IndividualName = new IndividualNameData
                            {
                                FirstName = userDetails.FirstName,
                                LastName = userDetails.LastName
                            };
                        }

                        var resp = await _revoultService.AddInternationalCounterParty(addNonRevolutCounterpartyReq);
                        if (resp != null)
                        {
                            if (resp.State == "created")
                            {
                                if (addNonRevolutCounterpartyReq.EphylinkId == 0)
                                {
                                    var ephylinkAccount = new EphylinkRevolutAccount()
                                    {
                                        BankCountry = addNonRevolutCounterpartyReq.BankCountry,
                                        Bic = addNonRevolutCounterpartyReq.Bic,
                                        Iban = addNonRevolutCounterpartyReq.Iban,
                                        Currency = addNonRevolutCounterpartyReq.Currency,
                                        RevolutConnectId = resp.Id,
                                        RevolutStatus = true,
                                        RevolutAccountId = resp.Accounts[0].Id,
                                        IsEnable = addNonRevolutCounterpartyReq.IsEnable,
                                        Address = addNonRevolutCounterpartyReq.Address.StreetLine1,
                                        City = addNonRevolutCounterpartyReq.Address.City,
                                        Country = addNonRevolutCounterpartyReq.Address.Country,
                                        PostCode = addNonRevolutCounterpartyReq.Address.Postcode
                                    };
                                    _db.EphylinkRevolutAccount.Add(ephylinkAccount);
                                    _db.SaveChanges();

                                    return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                                    {
                                        StatusCode = StatusCodes.Status200OK,
                                        Message = "Account Created",
                                        Result = ephylinkAccount.Id
                                    });
                                }
                                else
                                {
                                    var accountDetails = await _db.EphylinkRevolutAccount.Where(x => x.Id == addNonRevolutCounterpartyReq.EphylinkId).FirstOrDefaultAsync();
                                    if(accountDetails != null)
                                    {
                                        accountDetails.BankCountry = addNonRevolutCounterpartyReq.BankCountry;
                                        accountDetails.Currency = addNonRevolutCounterpartyReq.Currency;
                                        accountDetails.Iban = addNonRevolutCounterpartyReq.Iban;
                                        accountDetails.Bic = addNonRevolutCounterpartyReq.Bic;
                                        accountDetails.RevolutConnectId = resp.Id;
                                        accountDetails.RevolutAccountId = resp.Accounts[0].Id;
                                        accountDetails.IsEnable = addNonRevolutCounterpartyReq.IsEnable;
                                        accountDetails.Address = addNonRevolutCounterpartyReq.Address.StreetLine1;
                                        accountDetails.City = addNonRevolutCounterpartyReq.Address.City;
                                        accountDetails.Country = addNonRevolutCounterpartyReq.Address.Country;
                                        accountDetails.PostCode = addNonRevolutCounterpartyReq.Address.Postcode;

                                        _db.SaveChanges();
                                        return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                                        {
                                            StatusCode = StatusCodes.Status200OK,
                                            Message = "Account Updated Successfully",
                                        });
                                    }

                                }
                               
                            }
                            return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                            {
                                StatusCode = StatusCodes.Status200OK,
                                Message = "Something went wrong while creating account",
                            });
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

        //GetEphylinkRevoultDetails
        [HttpGet]
        [Route("GetEphylinkRevoultDetails")]
        public async Task<IActionResult> GetEphylinkRevoultDetails()
        {

            try
            {
                var ephylinkbankDetails = await _db.EphylinkRevolutAccount.FirstOrDefaultAsync();
                if(ephylinkbankDetails != null)
                {
                    return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                    {
                        StatusCode = StatusCodes.Status200OK,
                        Message = "success",
                        Result = ephylinkbankDetails
                    });
                }
                else
                {
                    return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                    {
                        StatusCode = StatusCodes.Status200OK,
                        Message = "Empty Data",
                    });
                }
               
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
    }
}
