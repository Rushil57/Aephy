using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Aephy.API.DBHelper
{
    public class SolutionFund
    {

        public enum FundTypes
        {
            MilestoneFund,
            ProjectFund
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int SolutionId { get; set; }

        public int IndustryId { get; set; }

        public int MileStoneId { get; set; }

        public string? ClientId { get; set; }

        public string? ProjectType { get; set; }

        public string? ProjectPrice { get; set; }

        public bool IsDispute { get; set; }

        public string? DisputeReason { get; set; }
        public string? ProjectStatus { get; set; }

        public bool IsArchived { get; set; }

        public FundTypes FundType { get; set;}

        public bool IsCheckOutDone { get; set; }

        public bool IsStoppedProject { get; set; }

        public DateTime? StoppedProjectDateTime { get; set; }

        public bool IsProjectPriceAlreadyCount { get; set; }

        public int CustomProjectDetialsId { get; set; }
    }

    public class SolutionDispute
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int ContractId { get; set; }

        public string? DisputeReason { get; set; }

        public string? Status { get; set; }

        public DateTime CreatedDateTime { get; set; }
    }

    public class SolutionTeam
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public string? FreelancerId { get; set; }

        public int SolutionFundId { get; set; }

        public decimal Amount {  get; set; }

        public bool IsProjectManager {  get; set; }

        public decimal PlatformFees { get; set; }

        public decimal TransferAmount { get; set; }

        public decimal ProjectManagerPlatformFees { get; set; }
        
    }

    public class ActiveProjectDocuments
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int SolutionFundId { get; set; }

        public string? Description { get; set; }

        public string? ClientId { get; set; }

        public string? DocumentName { get; set; }

        public string? DocumentPath { get; set; }
        public string? DocumentBlobStorageBaseUrl { get; set; }
        public string? DocumentUrlWithSas { get; set; }

        public DateTime? CreatedDateTime { get; set;  }
    }

    public class ActiveSolutionMilestoneStatus
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int MilestoneId {  get; set; }

        public string? UserId {  get; set; }

        public string? MilestoneStatus { get; set; }
    }

    public class ExchangeRates
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public string? FromCurrency { get; set; }

        public string? ToCurrency { get; set; }

        public decimal? Rate { get; set; }

        public DateTime? ExchangeRateDateTime { get; set; }

    }

    public class SolutionLeave
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int SolutionFundId {  get; set; }

        public string? FreelancerId {  get; set; }

        public DateTime? LeaveDateTime {  get; set; }
    }
}

