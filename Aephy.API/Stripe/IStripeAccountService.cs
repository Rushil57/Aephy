namespace Aephy.API.Stripe
{
    public interface IStripeAccountService
    {
        public string CreateStripeAccount(string apiKey);
        public bool IsComplete(string connectedAccountId);
    }
}
