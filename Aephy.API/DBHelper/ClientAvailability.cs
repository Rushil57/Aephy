using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Aephy.API.DBHelper
{
    public class ClientAvailability
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public string? ClientId { get; set; }
        public DateTime? AvailableDate { get; set; }
        public int? SolutionId { get; set; }
        public int? IndustryId { get; set; }
    }
}
