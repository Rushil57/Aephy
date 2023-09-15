using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Aephy.API.DBHelper
{
    public class Contract
    {
        /// <summary>
        /// StripeAccountStatus
        /// </summary>
        public enum PaymentStatuses
        {
            ContractCreated,
            UnPaid,
            Paid,
            NoPaymentRequired,
            PartiallySplitted,
            Splitted,
            Cancelled
        }

        public enum SessionStatuses
        {
            NotCreated,
            Open,
            Complete,
            Expired
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        //public SolutionMilestone MileStone { get; set; }
        public List<ContractUser> ContractUsers { get; set; }
        public string? PaymentIntentId { get; set; }
        public ApplicationUser ClientUser { get; set; }

        public int MilestoneDataId { get; set; }

        public int SolutionId { get; set; }

        public int IndustryId { get; set; }

        // Stripe specific fields below
        public PaymentStatuses PaymentStatus { get; set; }
        public string? ClientUserId { get; set; }
        public string? TransactionId { get; set; }
        public string? SessionId { get; set; }
        public SessionStatuses SessionStatus { get; set; }
        public DateTime? SessionExpiry { get; set; }
        public string? LatestChargeId { get; set; }
        public int SolutionFundId { get; set; }

        public bool IsClientRefund { get; set; }

        public string? RefundAmount { get; set; }

        public DateTime RefundDateTime { get; set; }
        public DateTime CreatedDateTime { get; set; }

        // Stripe fields above

    }

    public class ContractUser
    {

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public ApplicationUser ApplicationUser { get; set; }
        public int Percentage { get; set; }
        public string? StripeTranferId { get; set; }
        public bool IsTransfered { get; set; } = false;
        public string ApplicationUserId { get; set; }
        public int ContractId { get; set; }

        public bool IsRefund { get; set; }

        public DateTime RefundDateTime { get; set; }

        public string? Amount { get; set; }
    }
}
