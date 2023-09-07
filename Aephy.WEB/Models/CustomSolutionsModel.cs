using System.ComponentModel.DataAnnotations.Schema;

namespace Aephy.WEB.Models
{
    public class CustomSolutionsModel
    {
        public int ID { get; set; }
        public string? ClientId { get; set; }
        public int ServiceId { get; set; }
        public int SolutionId { get; set; }
        public int IndustryId { get; set; }
        public string SolutionTitle { get; set; }
        public string SoultionDescription { get; set; }
        public string? DocumentPath { get; set; }
        public string? BlobStorageBaseUrl { get; set; }
        public string? DocumentUrlWithSas { get; set; }
        public DateTime DeliveryTime { get; set; }
        public decimal Budget { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        [NotMapped]
        public bool AlreadyExistDocument { get; set; }
    }
    public class CustomSolutionDocument
    {
        public int ID { get; set; }

        public string? DocumentPath { get; set; }

        public string? BlobStorageBaseUrl { get; set; }

        public string? DocumentUrlWithSas { get; set; }

        public bool AlreadyExistDocument { get; set; }

        public string? ClientId { get; set; }
    }
}
