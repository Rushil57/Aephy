namespace Aephy.API.Models
{
    public class UserDetail
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string ProfileImage { get; set; } = "";
    }

    public class UserIdsModel
    {
        public string[]? Ids { get; set; }
    }
    public class UserWiseLavelDetail
    {
        public string? Id { get; set; }

        public string? FirstName { get; set; }

        public string? LastName { get; set; }

        public string? Lavel { get; set; }
    }
}
