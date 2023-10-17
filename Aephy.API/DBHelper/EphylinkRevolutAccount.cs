using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Aephy.API.DBHelper
{
    public class EphylinkRevolutAccount
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public string? RevolutConnectId { get; set; }

        public string? RevolutAccountId { get; set; }

        public bool RevolutStatus { get; set; }

        public string? BankCountry { get; set; }

        public string? Currency { get; set; }

        public string? Iban { get; set; }

        public string? Bic { get; set; }

        public string? Address { get; set; }

        public string? City { get; set; }

        public string? Country { get; set; }

        public string? PostCode { get; set; }

        public bool IsEnable { get; set; }

    }
}
