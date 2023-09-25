using Aephy.API.DBHelper;
using Stripe;
using Stripe.Checkout;

namespace Aephy.API.Stripe
{
    public interface IStripeAccountService
    {
        public string CreateStripeAccount(string apiKey);
        public bool IsComplete(string connectedAccountId);

        public Contract.SessionStatuses GetSesssionStatus(Session session);
        public Contract.PaymentStatuses GetPaymentStatus(Session session);
        public Session GetCheckOutSesssion(string sessionId);
        public PaymentIntent GetPaymentIntent(string paymentIntentId);
        public Session CreateCheckoutSession(SolutionMilestone milestone,string projectPrice, string successUrl, string cancelUrl);

        public Session CreateProjectCheckoutSession(string projectPrice, string successUrl, string cancelUrl);

        public string CreateTransferonCharge(long amount, string currency, string destination, string sourceTransaction, string transferGroup);

        public string RefundAmountToClient(string lastChargeId, long amount, string currency);

        public Session GetTaxDetails(string sessionId);
    }
}
