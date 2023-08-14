//using Aephy.WEB.Models;
//using Aephy.WEB.DashboardHubs;
//using TableDependency.SqlClient;
//using static Aephy.API.Models.AdminViewModel;

//namespace ProductsUI.SubscribeTableDependencies
//{
//    public class SubscribeProductTableDependencies: ISubscribeTableDependency
//    {
//        SqlTableDependency<GigOpenRoles> tableDependency;
//        DashboardHub dashboardHub;

//        public SubscribeProductTableDependencies(DashboardHub dashboardHub)
//        {
//            this.dashboardHub = dashboardHub;
//        }

//        public void SubscribeTableDependency(string connectionString)
//        {
//            tableDependency = new SqlTableDependency<GigOpenRoles>(connectionString);
//            tableDependency.OnChanged += TableDependency_OnChanged;
//            tableDependency.OnError += TableDependency_OnError;
//            tableDependency.Start();
//        }

//        private void TableDependency_OnChanged(object sender, TableDependency.SqlClient.Base.EventArgs.RecordChangedEventArgs<GigOpenRoles> e)
//        {
//            if (e.ChangeType != TableDependency.SqlClient.Base.Enums.ChangeType.None)
//            {
//                dashboardHub.SendProducts();
//            }
//        }

//        private void TableDependency_OnError(object sender, TableDependency.SqlClient.Base.EventArgs.ErrorEventArgs e)
//        {
//            Console.WriteLine($"{nameof(GigOpenRoles)} SqlTableDependency error: {e.Error.Message}");
//        }
//    }
//}
