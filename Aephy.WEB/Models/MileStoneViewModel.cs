﻿namespace Aephy.WEB.Models
{
    public class MileStoneViewModel
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }

        public int IndustryId { get; set; }

        public int SolutionId { get; set; }

        public DateTime? DueDate { get; set; }

        public string? FreelancerId { get; set; }

        public string? ProjectType { get; set; }
        public int Days { get; set; }
        public string? UserId { get; set; }

        public int SolutionFundId { get; set; }

        public bool MileStoneCheckout { get; set; }

    }

    public class MileStoneDetailsViewModel
    {
        public int IndustryId { get; set; }

        public int SolutionId { get; set; }

        public string? FreelancerId { get; set; }

        public string? ProjectType { get; set; }

        public string? FreelancerLevel { get; set; }

        public string? UserId { get; set; }

        public int SolutionFundId { get; set; }

        public string? ClientPreferredCurrency { get; set; }
    }
    public class MileStoneIdViewModel
    {
        public int Id { get; set; }

        public string? UserId { get; set; }

        public int? pageNumber { get; set; }

        public int SolutionFundId { get; set; }
        public string? SolutionName { get; set; }

        public string? ClientPreferredCurrency { get; set; }
    }

    public class SolutionIndustryDetailsViewModel
    {
        public int Id { get; set; }

        public string? ProjectOutline { get; set; }

        public string? ProjectDetails { get; set; }

        public int IndustryId { get; set; }

        public int SolutionId { get; set; }

        public string? ProjectType { get; set; }

        public string? Duration { get; set; }

        public int TeamSize { get; set; }

    }

    public class SolutionFundModel 
    {
        public int Id { get; set; }
        public int SolutionId { get; set; }

        public int IndustryId { get; set; }

        public int MileStoneId { get; set; }

        public string? ClientId { get; set; }

        public string? ProjectType { get; set; }

        public string? ProjectPrice { get; set; }

        public bool IsDispute { get; set; }

        public string? DisputeReason { get; set; }
        public string? ProjectStatus { get; set; }

        public bool IsArchived { get; set; }

        public bool MileStoneCheckout { get; set; }

        public bool GetNextMileStoneData { get; set; }
        public int ContractId { get; set; }

        public int SolutionFundId { get; set; }

        public string? UserId { get; set; }

        public string? ClientPreferredCurrency { get; set; }

    }

    public class ActiveProjectDocumentViewModel
    {
        public int Id { get; set; }
        public string? ClientId { get; set; }

        public string? Description { get; set; }

        public int SolutionFundId { get; set; }

        public string? DocumentBlobStorageBaseUrl { get; set; }

        public string? DocumentPath { get; set; }

        public string? DocumentUrlWithSas { get; set; }

        public string? DocumentName { get; set; }


    }

    public class FreelancerToFreelancerReviewViewModel
    {
        public int Id { get; set; }
        public string? ToFreelancerId { get; set; }

        public string? FromFreelancerId { get; set; }

        public int SolutionId { get; set; }

        public int IndustryId { get; set; }

        public string? Feedback_Message { get; set; }

        public int CollaborationTeamWorkRating { get; set; }
        public int CommunicationRating { get; set; }
        public int ProfessionalismRating { get; set; }
        public int TechnicalRating { get; set; }
        public int ProjectManagementRating { get; set; }
        public int ResponsivenessRating { get; set; }
        public int WellDefinedProjectRating { get; set; }

    }

}
