namespace Aephy.WEB.Admin.Models
{
    public class Feedback
    {
    }
    public class FreelancerReview
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
}
