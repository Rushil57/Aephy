﻿using Org.BouncyCastle.Asn1.X509;

namespace Aephy.WEB.Models
{
    public class RegisterNewUser
    {
        public string? Id { get; set; }
        public string? FirstName { get; set; }

        public string? LastName { get; set; }
        public string? UserName { get; set; }
        public string? Email { get; set; }

        public string? Password { get; set; }

        public bool TermsCondition { get; set; }

        public string? UserType { get; set; }

        public FreelancerDetail freelancerDetail { get; set; }

        public ClientDetail clientDetail { get; set; }
        public string? FreelancerLevel { get; set; }
        public IFormFile? ProfileImage { get; set; }
        public string? IsInvited { get; set; }
        public bool IsInvitedRemoved { get; set; }

        public string? PreferredCurrency { get; set; }

        public string? TaxType { get; set; }

        public string? TaxNumber { get; set; }

        public DateTime? StartHour { get; set; }

        public DateTime? EndHour { get; set; }

    }

    public class FreelancerDetail
    {
        public string? HourlyRate { get; set; }

        public string? ProffessionalExperience { get; set; }

        public string? Education { get; set; }

        public string? FreelancerAddress { get; set; }
        public string? FreelancerLevel { get; set; }

        public int CountryId { get; set; }

        public string? City { get; set; }

        public string? PostCode { get; set; }

        public bool IsRevoultBankAccount { get; set; }

        public string? RevTag { get; set; }
    }

    public class ClientDetail
    {
        public string? ClientAddress { get; set; }
        public string? Description { get; set; }
        public string? CompanyName { get; set; }
        public int CountryId { get; set; }

        public string? City { get; set; }

        public string? PostCode { get; set; }
    }

    public class UserCvFile
    {
        public string? UserId { get; set; }

        public string? CVPath { get; set; }

        public string? BlobStorageBaseUrl { get; set; }

        public string? CVUrlWithSas { get; set; }
    }
}
