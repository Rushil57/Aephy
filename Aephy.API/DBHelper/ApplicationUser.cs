﻿using Microsoft.AspNetCore.Identity;

namespace Aephy.API.DBHelper
{
    public class ApplicationUser : IdentityUser
    {
        public string? RefreshToken { get; set; }
        public DateTime RefreshTokenExpiryTime { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? ProfileUrl { get; set; }
        public DateTime CreatedDateTime { get; set; }
        public string? FCMToken { get; set; }
        public string? Device { get; set; }

        public string? UserType { get; set; }

        public bool IsActive { get; set; }

        public bool IsDeleted { get; set; }

        public bool IsInvited { get; set; }

        public bool IsInvitedRemoved { get; set; }

        public int CountryId { get; set; }

        public string? RevolutConnectId { get; set; }

        public string? RevolutAccountId { get; set; }

        public bool RevolutStatus { get; set; }

        public string? PreferredCurrency { get; set; }

        public string? TaxType { get; set; }

        public string? TaxNumber { get; set; }

        public DateTime? StartHours { get; set; }

        public DateTime? EndHours { get; set; }

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

        /// <summary>
        /// StripeAccountStatus
        /// </summary>
        public enum StripeAccountStatuses
        {
            NotCreated,
            Initiated,
            Incomplete,
            Complete
        }

        /// <summary>
        /// Stripe connect account Id
        /// </summary>
        public string? StripeConnectedId { get; set; }

        public StripeAccountStatuses StripeAccountStatus { get; set; }
    }

    public class Country
    {
        public int Id { get; set; }

        public string? Code { get; set; }

        public string? CountryName { get; set; }
        public bool IsIBANMandatory { get; set; }
    }
}
