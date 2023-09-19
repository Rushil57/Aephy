using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Aephy.API.DBHelper
{
    public class FreelancerReview
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }
        public string ClientId { get; set; }

        public string FreelancerId { get; set; }
        public int SolutionId { get; set; }
        public int IndustryId { get; set; }
        public string? Feedback_Message { get; set; }

        public int? CommunicationRating { get; set; }

        public int? CollaborationRating { get; set; }

        public int? ProfessionalismRating { get; set; }

        public int? TechnicalRating { get; set; }

        public int? SatisfactionRating { get; set; }

        public int? ResponsivenessRating { get; set; }

        public int? LikeToWorkRating { get; set; }
        public DateTime CreateDateTime { get; set; }
    }
}
