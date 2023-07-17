namespace Aephy.WEB.Models
{
    public class ResgisterNewUser
    {
        public string? Id { get; set; }
        public string? FirstName { get; set; }

        public string? LastName { get; set; }
        public string? UserName { get; set; }
        public string? Email { get; set; }

        public string? Password { get; set; }

        public bool TermsCondition { get; set; }

        public string? UserType { get; set; }

        public IFormFile? ProfileImage { get; set; }
    }
}
