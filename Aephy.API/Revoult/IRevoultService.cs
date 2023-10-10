using static Aephy.API.Models.AdminViewModel;

namespace Aephy.API.Revoult
{
    public interface IRevoultService
    {
        public Task<AddNonRevolutCounterpartyResp> AddInternationalCounterParty(AddNonRevolutCounterpartyReq addCounterpartyReq);
    }
}
