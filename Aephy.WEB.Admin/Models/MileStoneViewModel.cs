namespace Aephy.WEB.Admin.Models
{
    public class SolutionFundModel 
    {
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

        public bool MileStoneCheckout { get; set; }

        public bool GetNextMileStoneData { get; set; }
        public int ContractId { get; set; }

        public int SolutionFundId { get; set; }

        public string? UserId { get; set; }

        public string? ClientPreferredCurrency { get; set; }
        public int InvoiceId {  get; set; }

    }
}
