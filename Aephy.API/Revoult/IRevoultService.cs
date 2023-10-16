﻿using RevolutAPI.Models.BusinessApi.Account;
using RevolutAPI.Models.BusinessApi.Counterparties;
using RevolutAPI.Models.BusinessApi.Payment;
//using static Aephy.API.Models.AdminViewModel;

namespace Aephy.API.Revoult
{
    public interface IRevoultService
    {
        public Task<AddNonRevolutCounterpartyResp> AddInternationalCounterParty(AddNonRevolutCounterpartyReq addCounterpartyReq);

        public Task<List<GetCounterpartyResp>> GetCounterparties();

        public Task<CreatePaymentResp> CreatePayment(CreatePaymentReq createPaymentReq);
        public Task<List<GetAccountResp>> RetrieveAllAccounts();
        public Task GetOrderDetails(string id);
        public Task RefundToClient(string orderId, string amount, string description);
    }
}
