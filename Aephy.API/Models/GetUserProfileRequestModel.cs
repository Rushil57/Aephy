using System.ComponentModel.DataAnnotations;

namespace Aephy.API.Models
{
    public class GetUserProfileRequestModel
    {
        public string UserId { get; set; } = "";
    }
    public class UserModel
    {
        [Required(ErrorMessage = "Id is required")]
        public string? Id { get; set; }
        [Required(ErrorMessage = "First Name is required")]
        public string? FirstName { get; set; }

        [Required(ErrorMessage = "Last Name is required")]
        public string? LastName { get; set; }
        public IFormFile? ProfileImage { get; set; }
    }

    public class ChangePasswordModel
    {
        [Required(ErrorMessage = "Id is required")]
        public string? Id { get; set; }

        [Required(ErrorMessage = "Current Password is required")]
        public string? CurrentPassword { get; set; }
        [Required(ErrorMessage = "New Password is required")]
        public string? NewPassword { get; set; }
    }
}
