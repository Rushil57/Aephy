using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Aephy.API.DBHelper
{
    public class Invoices
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int SolutionId { get; set; }

        public int IndustryId { get; set; }

        public int SolutionFundId { get; set; }

        public string? InvoiceNumber { get; set; }

        public DateTime? Date { get; set; }

        public DateTime? DueDate { get; set; }

        public string? TotalAmount { get; set; }

        public string? DueAmount { get; set; }

        public string? ClientId { get; set; }

        public string? ClientAddress { get; set; }

        public string? VatId { get; set;}

        public string? TaxId { get; set; }

        public string? Title { get; set; }

        public string? Amount { get; set; }

        public string? VatPercentage { get; set; }

        public string? VatAmount { get; set; }

        public string? ClientName { get; set; }

    }

    public class InvoiceList
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public string? InvoiceType { get; set; }

        public string? InvoiceNumber { get; set; }

        public DateTime? InvoiceDate { get; set; }

        public string? TotalAmount { get; set; }

        public string? BillToClientId { get; set; }

        public string? TransactionType { get; set; }

        public int ContractId { get; set; }

        public string? FreelancerId { get; set; }
    }

    public class InvoiceListDetails
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int InvoiceListId {  get; set; }

        public string? Description { get; set;}

        public string? Amount { get; set; }
    }
}
