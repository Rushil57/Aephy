﻿using Aephy.API.DBHelper;
using Microsoft.AspNetCore.Mvc;
using RestSharp;
using RevolutAPI.Models.BusinessApi.Account;
using RevolutAPI.Models.BusinessApi.Counterparties;
using RevolutAPI.Models.BusinessApi.Payment;
using static Aephy.API.Models.AdminViewModel;
//using static Aephy.API.Models.AdminViewModel;

namespace Aephy.API.Revoult
{
    public interface IRevoultService
    {
        public Task<AddNonRevolutCounterpartyResp> AddInternationalCounterParty(AddNonRevolutCounterpartyReq addCounterpartyReq);

        public Task<List<GetCounterpartyResp>> GetCounterparties();

        public Task<CreatePaymentResp> CreatePayment(CreatePaymentReq createPaymentReq);
        public Task<List<GetAccountResp>> RetrieveAllAccounts();
        public Task<string> GetOrderDetails(string orderId);
        public Task<RefundPaymentRequest> RefundToClient(RefundPaymentRequest model);

        public Task<RestResponse> GetTranscationFeesDetails(string transferId);

        public Task<RestResponse> ExchangeCurrency(string fromCurrency, string toCurrency, string amount);
        
    }
}
