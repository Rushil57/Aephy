using Aephy.WEB.Models;
using Aephy.WEB.DashboardHubs;
using ProductsUI.SubscribeTableDependencies;
using TableDependency.SqlClient;

namespace Aephy.WEB.SubscribeTableDependencies
{
	public class SubscribeApprovedListTableDependency : ISubscribeTableDependency
	{
		SqlTableDependency<OpenGigRolesApplications> tableDependency;
		DashboardHub dashboardHub;

		public SubscribeApprovedListTableDependency(DashboardHub dashboardHub)
		{
			this.dashboardHub = dashboardHub;
		}

		public void SubscribeTableDependency(string connectionString)
		{
			tableDependency = new SqlTableDependency<OpenGigRolesApplications>(connectionString);
			tableDependency.OnChanged += TableDependency_OnChanged;
			tableDependency.OnError += TableDependency_OnError;
			tableDependency.Start();
		}

		private void TableDependency_OnChanged(object sender, TableDependency.SqlClient.Base.EventArgs.RecordChangedEventArgs<OpenGigRolesApplications> e)
		{
			if (e.ChangeType != TableDependency.SqlClient.Base.Enums.ChangeType.None)
			{
				dashboardHub.SendApproveList();
			}
		}

		private void TableDependency_OnError(object sender, TableDependency.SqlClient.Base.EventArgs.ErrorEventArgs e)
		{
			Console.WriteLine($"{nameof(OpenGigRolesApplications)} SqlTableDependency error: {e.Error.Message}");
		}
	}
}
