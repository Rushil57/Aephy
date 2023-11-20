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
    public class AdminToFreelancerReview
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string? UserId { get; set; }

        public string? FreelancerId { get; set; }

        public string? Feedback_Message { get; set; }

        public int? Professionalism { get; set; }

        public int? HourlyRate { get; set; }

        public int? Availability { get; set; }

        public int? ProjectAcceptance { get; set; }

        public int? Education { get; set; }

        public int? SoftSkillsExperience { get; set; }

        public int? HardSkillsExperience { get; set; }

        public int? ProjectSuccessRate { get; set; }
        public DateTime? CreateDateTime { get; set; }
    }
}
