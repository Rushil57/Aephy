namespace Aephy.WEB.Models
{
    public class MileStoneViewModel
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }

        public int IndustryId { get; set; }

        public int SolutionId { get; set; }

        public DateTime DueDate { get; set; }

        public string? FreelancerId { get; set; }

        public string? ProjectType { get; set; }

    }

    public class MileStoneDetailsViewModel
    {
        public int IndustryId { get; set; }

        public int SolutionId { get; set; }

        public string? FreelancerId { get; set; }

        public string? ProjectType { get; set; }
    }
    public class MileStoneIdViewModel
    {
        public int Id { get; set; }
    }

    public class SolutionIndustryDetailsViewModel
    {
        public int Id { get; set; }

        public string? ProjectOutline { get; set; }

        public string? ProjectDetails { get; set; }

        public int IndustryId { get; set; }

        public int SolutionId { get; set; }

        public string? ProjectType { get; set; }

    }
}
