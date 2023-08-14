using Microsoft.AspNetCore.SignalR;

namespace Aephy.WEB.DashboardHubs
{
    public class DashboardHub : Hub
    {
        public DashboardHub(IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("ConnStr");
        }
        public async Task SendProducts()
        {
            //var products = productRepository.GetProducts();
            //await Clients.All.SendAsync("ChnagedOpenRoles", true);
        }

		public async Task SendApproveList()
		{
			//await Clients.All.SendAsync("ChangedApprovedList", true);
		}
	}
}
