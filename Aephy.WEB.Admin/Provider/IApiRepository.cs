namespace Aephy.WEB.Provider
{
    public interface IApiRepository
    {
        Task<string> MakeApiCallAsync(string endPoint, HttpMethod httpMethod, dynamic payload = null);
    }
}
