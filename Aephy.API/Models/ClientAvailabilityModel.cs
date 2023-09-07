using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace Aephy.API.Models
{
    public class ClientAvailabilityModel
    {
        public int Id { get; set; }
        public string? ClientId { get; set; }
        public DateTime? AvailableDate { get; set; }
        public int? SolutionId { get; set; }
        public int? IndustryId { get; set; }

        [NotMapped]
        public DateTime? StartDate { get; set; }
        [NotMapped]
        public DateTime? EndDate { get; set; }
        [NotMapped]
        public DateTime[]? Holidays { get; set; }

        [NotMapped]
        public string? HolidaysList { get; set; }

        [NotMapped]
        public bool? isExcludeWeekends { get; set; }
    }
}
