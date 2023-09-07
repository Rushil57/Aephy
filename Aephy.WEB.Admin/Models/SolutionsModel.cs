using Microsoft.OpenApi.Attributes;
using System.ComponentModel.DataAnnotations.Schema;

namespace Aephy.WEB.Admin.Models
{
    public class SolutionsModel
    {
        public int Id { get; set; }

        public string? Title { get; set; }

        public string? SubTitle { get; set; }

        public string? Description { get; set; }

        public List<int> solutionIndustries { get; set; }

        public int solutionServices { get; set; }

        public string? Industries { get; set; }

        public string? Services { get; set; }
        public string? Image { get; set; }

        public string? ImageUrlWithSas { get; set; }

        public string? ImagePath { get; set; }

        public bool IsProjectSaved { get; set; }

        public string? MileStoneTitle { get; set; }

        public string? PaymentStatus { get; set; }

        // public IFormFile[] ImageFile { get; set; }
    }

    public class SolutionServicesViewModel
    {
        public int Id { get; set; }

        public int SolutionId { get; set; }

        public int ServicesId { get; set; }
    }

    public class SolutionIndustryViewModel
    {
        public int Id { get; set; }

        public int SolutionId { get; set; }

        public int IndustryId { get; set; }
    }

    public class SolutionIdModel
    {
        public int Id { get; set; }

        public string? ImagePath { get; set; }

        public string? ImageUrlWithSas { get; set; }
    }

    public class SolutionImage
    {
        public int Id { get; set; }

        public string? ImagePath { get; set; }

        public string? BlobStorageBaseUrl { get; set; }

        public string? ImageUrlWithSas { get; set; }

        public bool? HasImageFile { get; set; }

        public string? FreelancerId { get; set; }
    }

    public class EditSolutionImage
    {
        public int Id { get; set; }

        public string? ImagePath { get; set; }
    }

    public class SolutionDescribeModel
    {
        public int Id { get; set; }
        public int IndustryId { get; set; }
        public int SolutionId { get; set; }
        public string? Description { get; set; }

        public string[]? AssignedFreelancerIds { get; set; }

        public string[]? IsArchitectIds { get; set; }

        public string? ImageUpload { get; set; }

        //public string? Image { get; set; }

        //public string? ImageUrlWithSas { get; set; }

        public string? ImagePath { get; set; }
        public string? ActiveByAdmin { get; set; }

        public bool IsActiveForFreelancer { get; set; }
        public bool IsActiveForClient { get; set; }
    }

    public class SolutionDefineRequestViewModel
    {
        public int IndustryId { get; set; }

        public int SolutionId { get; set; }

        public string? ProjectType { get; set; }

        public string? FreelancerId { get; set; }

        public bool ProjectArchitect { get; set; }
    }

    public class SolutionTopProfessionalViewModel
    {
        public int Id { get; set; } 
        public int IndustryId { get; set; }

        public int SolutionId { get; set; }

        public string? FreelancerId { get; set; }

        public string? TopProfessionalTitle { get; set; }

        public string? Description { get; set; }

        public string? Rate { get; set; }

        public bool IsVisibleOnLandingPage { get; set; }

    }

    public class SolutionSuccessfullProjectViewModel
    {
        public int Id { get; set; }
        public int IndustryId { get; set; }

        public int SolutionId { get; set; }

        public string? Title { get; set; }

        public string? Description { get; set; }

        public bool IsActive { get; set; }

    }

    public class SolutionSuccessfullProjectResultViewModel
    {
        public int Id { get; set; }
        public int SolutionSuccessfullProjectId { get; set; }

        public string? ResultKey { get; set; }

        public string? ResultValue { get; set; }

    }

    public class SolutionDisputeModel
    {
        public int Id { get; set; }

        public string? FreelancerId { get; set; }

        public string? TransferAmount { get; set; }

        public string? Currency { get; set; }

        public string? StripeConnectedId { get; set;}

        public string? LatestChargeId { get; set; }

        public int ContractId { get; set; }

        public bool IsDisputeResolved { get; set; }

        public string? StopPaymentReason { get; set; }
    }
}
