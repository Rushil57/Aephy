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
}
