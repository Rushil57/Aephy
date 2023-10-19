//using System;
//using System.Net.Http;
//using System.Net.Security;
//using System.Net;
//using System.Security.Cryptography.X509Certificates;
//using System.Threading.Tasks;
//using Microsoft.Azure.WebJobs;
//using Microsoft.Azure.WebJobs.Host;
//using Microsoft.Extensions.Configuration;
//using Microsoft.Extensions.Logging;
//using Newtonsoft.Json;

//namespace Aephy.AzureFunction.App
//{
//    public class CheckRevolutOrderStatus
//    {
//        [FunctionName("CheckRevolutOrderStatus")]
//        public async Task Run([TimerTrigger("0 */1 * * * *",
//            #if DEBUG
//                RunOnStartup= true
//            #endif
//            )]TimerInfo myTimer, ILogger log)
//        {

//            log.LogInformation($"CheckRevolutOrderStatus Timer trigger function executed at: {DateTime.Now}");
//            //var data = await MakeApiCallAsync("api/Client/CheckRevolutOrderStatus", HttpMethod.Get);
//            log.LogInformation($"CheckRevolutOrderStatus Timer trigger function stopped at: {DateTime.Now}");
//        }

//        //public async Task<string> MakeApiCallAsync(string endPoint, HttpMethod httpMethod, dynamic payload = null)
//        //{
//        //    try
//        //    {
//        //        ServicePointManager.ServerCertificateValidationCallback =
//        //  delegate (object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
//        //  {
//        //      return true;
//        //  };
//        //        //var baseApiUrl = _configuration.GetValue<string>("BaseApiUrl:ApiUrl"); #######
//        //        var baseApiUrl = "https://localhost:7118/";
//        //        HttpContent content = null;
//        //        if (payload != null)
//        //        {
//        //            content = new StringContent(
//        //                    JsonConvert.SerializeObject(payload),
//        //                    System.Text.Encoding.UTF8,
//        //                    "application/json"
//        //                );
//        //        }
//        //        var httpRequest = new HttpRequestMessage
//        //        {
//        //            RequestUri = new Uri(baseApiUrl + endPoint, UriKind.Absolute),
//        //            Method = httpMethod,
//        //            Content = content
//        //        };
//        //        HttpClient httpClient = new HttpClient();
//        //        httpClient.Timeout = TimeSpan.FromMinutes(240);
//        //        var httpResponseMessage = await httpClient.SendAsync(httpRequest);
//        //        if (httpResponseMessage.IsSuccessStatusCode)
//        //        {
//        //            var response = await httpResponseMessage.Content.ReadAsStringAsync();
//        //            return response;
//        //        }
//        //        else
//        //        {
//        //            var response = await httpResponseMessage.Content.ReadAsStringAsync();
//        //            //throw new Exception("API ["+ endPoint + "] Error: " + response.ToString());
//        //            throw new Exception("API [" + endPoint + "] Error: " + response);
//        //        }
//        //    }
//        //    catch (Exception ex)
//        //    {
//        //    }
//        //    return "";

//        //}
//    }
//}
