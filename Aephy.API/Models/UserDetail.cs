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

        public int SolutionId { get; set; }

        public int IndustryId { get; set; }
    }
    public class UserWiseLavelDetail
    {
        public string? Id { get; set; }

        public string? FirstName { get; set; }

        public string? LastName { get; set; }

        public string? Lavel { get; set; }

        public bool IsProjectArchitect { get; set; }
    }
    public class CalendarData
    {
        public string? Id { get; set; }

        public DateTime? StartHour { get; set; }

        public DateTime? EndHour { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public bool IsWeekendExclude { get; set; }

        public bool IsNotAvailableForNextSixMonth { get; set; }

        public bool IsWorkEarlier { get; set; }

        public bool IsWorkLater { get; set; }

        public DateTime? StartHoursEarlier { get; set; }

        public DateTime? EndHoursEarlier { get; set; }

        public DateTime? StartHoursLater { get; set; }

        public DateTime? EndHoursLater { get; set; }

        public DateTime? StartHoursFinal { get; set; }

        public DateTime? EndHoursFinal { get; set; }

        public bool? onMonday { get; set; }

        public bool? onTuesday { get; set; }

        public bool? onWednesday { get; set; }

        public bool? onThursday { get; set; }

        public bool? onFriday { get; set; }

        public bool? onSaturday { get; set; }

        public bool? onSunday { get; set; }
    }
}
