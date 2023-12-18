﻿namespace Aephy.API.Models
{
    public class FeedbackModel
    {
    }
    public class SolutionTeamReview
    {
        public string? ClientId { get; set; }

        public string? FreelancerId { get; set; }
        public int? SolutionId { get; set; }
        public int? IndustryId { get; set; }
        public string? Feedback_Message { get; set; }

        public int? CommunicationRating { get; set; }

        public int? CollaborationRating { get; set; }

        public int? ProfessionalismRating { get; set; }

        public int? TechnicalRating { get; set; }

        public int? SatisfactionRating { get; set; }

        public int? ResponsivenessRating { get; set; }

        public int? LikeToWorkRating { get; set; }
        public DateTime? CreateDateTime { get; set; }
    }
    public class SolutionReview
    {
        public string? ClientId { get; set; }
        public int? SolutionId { get; set; }
        public int? IndustryId { get; set; }
        public string? Feedback_Message { get; set; }

        public int? WellDefinedProjectScope { get; set; }

        public int? AdherenceToProjectScope { get; set; }

        public int? DeliverablesQuality { get; set; }

        public int? MeetingTimeliness { get; set; }

        public int? Clientsatisfaction { get; set; }

        public int? AdherenceToBudget { get; set; }

        public int? LikeToRecommend { get; set; }

        public DateTime? CreateDateTime { get; set; }
    }

    public class AdminToFreelancerReviewModel
    {
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
    public class TopProfessionalReviews
    {
        public string? FreelancerId { get; set; }
        public string? ClientId { get; set; }
        public int? SolutionId { get; set; }
        public int? IndustryId { get; set; }
        public string? Feedback_Message { get; set; }

        public string? ClientName { get; set; }

        public string? FreelancerName { get; set; }

        public int? CommunicationRating { get; set; }

        public int? CollaborationRating { get; set; }
        public int? ProfessionalismRating { get; set; }
        public int? TechnicalRating { get; set; }
        public int? SatisfactionRating { get; set; }
        public int? ResponsivenessRating { get; set; }
        public DateTime? ReviewDateTime { get; set; }
        public string? Rate { get; set; }
    }
}
