using Newtonsoft.Json;
using System.Net.Security;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using Aephy.WEB.Provider;

namespace Aephy.WEB.Repository
{
    public class ApiRepository : IApiRepository
    {
        private readonly IConfiguration _configuration;
        public ApiRepository(IConfiguration configuration)
        {
            _configuration= configuration;
        }
        public async Task<string> MakeApiCallAsync(string endPoint, HttpMethod httpMethod, dynamic payload = null)
        {
            try
            {
                ServicePointManager.ServerCertificateValidationCallback =
          delegate (object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
          {
              return true;
          };
                var baseApiUrl = _configuration.GetValue<string>("BaseApiUrl:ApiUrl");
                HttpContent content = null;
                if (payload != null)
                {
                    content = new StringContent(
                            JsonConvert.SerializeObject(payload),
                            System.Text.Encoding.UTF8,
                            "application/json"
                        );
                }
                var httpRequest = new HttpRequestMessage
                {
                    RequestUri = new Uri(baseApiUrl + endPoint, UriKind.Absolute),
                    Method = httpMethod,
                    Content = content
                };
                HttpClient httpClient = new HttpClient();
                httpClient.Timeout = TimeSpan.FromMinutes(240);
                var httpResponseMessage = await httpClient.SendAsync(httpRequest);
                if (httpResponseMessage.IsSuccessStatusCode)
                {
                    var response = await httpResponseMessage.Content.ReadAsStringAsync();
                    return response;
                }
                else
                {
                    var response = await httpResponseMessage.Content.ReadAsStringAsync();
                    //throw new Exception("API ["+ endPoint + "] Error: " + response.ToString());
                    throw new Exception("API [" + endPoint + "] Error: " + response);
                }
            }
            catch (Exception ex)
            {
            }
            return "";


        }
    }
}
