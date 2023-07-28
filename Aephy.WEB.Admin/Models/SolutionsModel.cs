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

        public string? AssignedFreelancerId { get; set; }

        public string? ImageUpload { get; set; }

        //public string? Image { get; set; }

        //public string? ImageUrlWithSas { get; set; }

        public string? ImagePath { get; set; }
    }
}
