using Org.BouncyCastle.Asn1.X509;

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

    public class CalendarData
    {
        public string? Id { get; set; }

        public DateTime? StartHour { get; set; }

        public DateTime? EndHour { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public bool? IsWeekendExclude { get; set; }

        public bool IsNotAvailableForNextSixMonth { get; set; }
    }

}
