using RevolutAPI.Models.MerchantApi.Orders;

namespace Aephy.API.Models
{
    public static class AppConst
    {
        public static class Commission
        {
            //%
            public static int PLATFORM_COMM_FROM_CLIENT_SMALL = 10;
            public static int PLATFORM_COMM_FROM_CLIENT_MEDIUM = 7;
            public static int PLATFORM_COMM_FROM_CLIENT_LARGE = 7;
            public static int PLATFORM_COMM_FROM_CLIENT_CUSTOM = 7;
            public static int PLATFORM_COMM_FROM_CLIENT_CUSTOM_LESS_THAN_THREE_GIGS = 10;

            //PROJACT MANAGER FEES  (%)
            //public static int PROJECT_MANAGER_SMALL = 4;
            //public static int PROJECT_MANAGER_MEDIUM = 3;
            //public static int PROJECT_MANAGER_LARGE = 3;
            //public static int PROJECT_MANAGER_CUSTOM = 0;

            // PLATFORM FEES (%)
            public static int PLATFORM_COMM_FROM_FREELANCER_SMALL = 6;
            public static int PLATFORM_COMM_FROM_FREELANCER_MEDIUM = 6;
            public static int PLATFORM_COMM_FROM_FREELANCER_LARGE = 6;
            public static int PLATFORM_COMM_FROM_FREELANCER_CUSTOM = 6;
        }

        public static class InvoiceTransactionType
        {
            public static string INVOICE1_PORTAL_TO_CLIENT = "Invoice1 - portal to client";
            public static string PAYMENT_RECEIPT_AMOUNT_DUE = "Payment Receipt - Amount Due";
            public static string INVOICE3_TOTAL_PLATFORM_FEES = "Invoice3 - Total platform fees";
            public static string CREDIT_MEMO = "CREDIT MEMO";
            public static string INVOICE_FREELANCER = "Invoice Freelancer";
            public static string INVOICE_PA = "Invoice PA";
            public static string INVOICE_COMMISIONS = "Invoice Commision";
        }

        public static class ProjectType
        {
            public static string SMALL_PROJECT = "small";

            public static string MEDIUM_PROJECT = "medium";

            public static string LARGE_PROJECT = "large";

            public static string CUSTOM_PROJECT = "custom";
        }
    }
}
