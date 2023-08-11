namespace Aephy.WEB.Admin.Models
{
    public class UserModel
    {
        public string? Id { get; set; }

        public string? FirstName { get; set; }

        public string? LastName { get; set; }

        public string? EmailAddress { get; set; }

        public string? UserRole { get; set; }

        public string? FreelancerLevel { get; set; }


    }

    public class UserIdModel
    {
        public string? Id { get; set; }

        public string? UserId { get; set; }
    }

    public class UserIdsModel
    {
        public string[]? Ids { get; set; }
    }

    public class DropdownViewModel
    {
        public int Id { get; set; }

        public string? UserId { get; set; }
    }
}

