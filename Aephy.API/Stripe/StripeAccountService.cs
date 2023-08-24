using Stripe;

namespace Aephy.API.Stripe
{
    public class StripeAccountService : IStripeAccountService
    {
        public string CreateStripeAccount(string apiKey)
        {
            StripeConfiguration.ApiKey = apiKey;
            var accountOptions = new AccountCreateOptions
            {
                Type = "express",
                Capabilities = new AccountCapabilitiesOptions
                {
                    CardPayments = new AccountCapabilitiesCardPaymentsOptions
                    {
                        Requested = true
                    },
                    Transfers = new AccountCapabilitiesTransfersOptions
                    {
                        Requested = true
                    }
                }
            };

            try
            {
                var accountService = new AccountService();
                return accountService.Create(accountOptions).Id;
            }
            catch (Exception ex)
            {
                return string.Empty;
            }
        }

        public bool IsComplete(string connectedAccountId)
        {
            try
            {
                var service = new AccountService();
                var response = service.Get(connectedAccountId);

                if (response.Capabilities.Transfers == "active")
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}
