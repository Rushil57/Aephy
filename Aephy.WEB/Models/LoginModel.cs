namespace Aephy.WEB.Models
{
    public class LoginModel
    {
        public string? Id { get; set; }
        public string? Username { get; set; }
        public string? Password { get; set; }
        public bool RememberMe { get; set; }
    }

    public class UserModel
    {
        public string? Id { get; set; }

        public string? FirstName { get; set; }


        public string? LastName { get; set; }

        public string? Email { get; set; }

        public string? UserType { get; set; }
    }

    public class ChangePasswordModel
    {
        public string? Id { get; set; }
        public string? CurrentPassword { get; set; }

        public string? NewPassword { get; set; }
    }
}
