﻿using System.ComponentModel.DataAnnotations;

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

        public FreelancerDetail freelancerDetail { get; set; }

        public ClientDetail clientDetail { get; set; }

        public string? UserName { get; set; }

        public string? City { get; set; }

        public string? PostCode { get; set; }

        public string? PreferredCurrency { get; set; }

        public string? TaxType { get; set; }

        public string? TaxNumber { get; set; }

        public DateTime? StartHour { get; set; }

        public DateTime? EndHour { get; set; }
    }

    public class ChangePasswordModel
    {
        public string? Id { get; set; }
        public string? CurrentPassword { get; set; }

        public string? NewPassword { get; set; }
    }

    public class FreelancerDetail
    {
        public string? UserId { get; set; }
        public string? HourlyRate { get; set; }

        public string? ProffessionalExperience { get; set; }

        public string? Education { get; set; }

        public string? FreelancerAddress { get; set; }

        public int CountryId { get; set; }

        public string? City { get; set; }

        public string? PostCode { get; set; }

        public bool IsRevoultBankAccount { get; set; }

        public string? RevTag { get; set; }
    }

    public class ClientDetail
    {
        public string? UserId { get; set; }
        public string? ClientAddress { get; set; }
        public string? Description { get; set; }

        public int CountryId { get; set; }
        public string? CompanyName { get; set; }

        public string? City { get; set; }

        public string? PostCode { get; set; }

    }
}
