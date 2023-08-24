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
}
