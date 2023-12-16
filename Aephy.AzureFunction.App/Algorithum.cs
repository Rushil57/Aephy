using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace Aephy.AzureFunction.App
{
    public class Algorithum
    {
        [FunctionName("Algorithum")]
        public async Task Run([TimerTrigger("0 */5 * * * *",
            #if DEBUG
                RunOnStartup= true
            #endif
            )]TimerInfo myTimer, ILogger log)
        {
            log.LogInformation($"Algorithum Timer trigger function executed at: {DateTime.Now}");
        }
    }
}
