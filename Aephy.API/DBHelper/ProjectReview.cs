using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Aephy.API.DBHelper
{
    public class ProjectReview
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }
        public string ClientId { get; set; }
        public int SolutionId { get; set; }
        public int IndustryId { get; set; }
        public string? Feedback_Message { get; set; }

        public int? WellDefinedProjectScope { get; set; }

        public int? AdherenceToProjectScope { get; set; }

        public int? DeliverablesQuality { get; set; }

        public int? MeetingTimeliness { get; set; }

        public int? Clientsatisfaction { get; set; }

        public int? AdherenceToBudget { get; set; }

        public int? LikeToRecommend { get; set; }

        public DateTime CreateDateTime { get; set; }
    }

    public class FreelancerToFreelancerReview
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public string? ToFreelancerId { get; set; }

        public string? FromFreelancerId { get; set; }

        public int SolutionId { get; set; }

        public int IndustryId { get; set; }

        public string? Feedback_Message { get; set; }

        public int CollaborationAndTeamWork { get; set; }
        public int Communication { get; set; }
        public int Professionalism { get; set; }
        public int TechnicalSkills { get; set; }
        public int ProjectManagement { get; set; }
        public int Responsiveness { get; set; }
        public int WellDefinedProjectScope { get; set; }
    }
}
