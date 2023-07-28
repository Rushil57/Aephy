using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Aephy.API.DBHelper
{
    public class SolutionIndustryDetails
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int SolutionId { get; set; }

        public int IndustryId { get; set; }

        public string? AssignedFreelancerId { get; set; }

        public string? ImagePath { get; set; }

        public string? BlobStorageBaseUrl { get; set; }

        public string? ImageUrlWithSas { get; set; }

        public string? Description { get; set; }
    }
}
