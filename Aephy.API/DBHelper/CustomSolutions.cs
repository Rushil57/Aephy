using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Aephy.API.DBHelper
{
    public class CustomSolutions
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        public string ClientId { get; set; }
        public int ServiceId { get; set; }
        public int SolutionId { get; set; }
        public int IndustryId { get; set; }
        public string? SolutionTitle { get; set; }
        public string? SoultionDescription { get; set; }
        public string? DocumentPath { get; set; }
        public string? BlobStorageBaseUrl { get; set; }
        public string? DocumentUrlWithSas { get; set; }
        public DateTime DeliveryTime { get; set; }
        public decimal Budget { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }

    public class CustomProjectDetials
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int SolutionDefineId { get; set; }

        public string? ProjectDuration { get; set; }

        public decimal EstimatedPrice { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime? StartHour { get; set; }
        public DateTime? EndHour { get; set; }

        public DateTime? AvailableDate { get; set; }

        public string? ClientId { get; set; }

        public string? Associate { get; set; }

        public string? Expert { get; set; }

        public string? ProjectManager { get; set; }

        public bool IsSingleFreelancer { get; set; }
    }
}
