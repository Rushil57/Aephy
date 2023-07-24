namespace Aephy.WEB.Models
{
    public class RegisterNewUser
    {
        public string? Id { get; set; }
        public string? FirstName { get; set; }

        public string? LastName { get; set; }
        public string? UserName { get; set; }
        public string? Email { get; set; }

        public string? Password { get; set; }

        public bool TermsCondition { get; set; }

        public string? UserType { get; set; }

        public FreelancerDetail freelancerDetail { get; set; }

        public ClientDetail clientDetail { get; set; }
        public string? FreelancerLevel { get; set; }
        public IFormFile? ProfileImage { get; set; }
    }

    public class FreelancerDetail
    {
        public string? HourlyRate { get; set; }

        public string? ProffessionalExperience { get; set; }

        public string? Education { get; set; }

        public string? FreelancerAddress { get; set; }
        public string? FreelancerLevel { get; set; }
    }

    public class ClientDetail
    {
        public string? ClientAddress { get; set; }
        public string? Description { get; set; }
    }
}
