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
}

