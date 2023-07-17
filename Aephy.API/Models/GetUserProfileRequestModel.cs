using System.ComponentModel.DataAnnotations;

namespace Aephy.API.Models
{
    public class GetUserProfileRequestModel
    {
        public string UserId { get; set; } = "";
    }
    public class UserModel
    {
        public string? Id { get; set; }

        public string? FirstName { get; set; }


        public string? LastName { get; set; }

        public string? Email { get; set; }

        public string? UserType { get; set; }

        public string? UserName { get; set; }
    }

    public class ChangePasswordModel
    {
        public string? Id { get; set; }
        public string? CurrentPassword { get; set; }

        public string? NewPassword { get; set; }
    }
}
