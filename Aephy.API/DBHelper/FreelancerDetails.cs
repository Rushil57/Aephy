using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Aephy.API.DBHelper
{
    public class FreelancerDetails
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string? UserId { get; set; }

        public string? HourlyRate { get; set; }

        public string? Education { get; set; }

        public string? ProffessionalExperience { get; set; }

        public string? Address { get; set; }

        public string? FreelancerLevel { get; set; }
        public string? CVPath { get; set; }
        public string? BlobStorageBaseUrl { get; set; }
        public string? CVUrlWithSas { get; set; }

        public string? ImagePath { get; set; }
        public string? ImageBlobStorageBaseUrl { get; set; }
        public string? ImageUrlWithSas { get; set; }

        public string? City { get; set; }

        public string? PostCode { get; set; }

        public bool IsRevoultBankAccount { get; set; }

        public string? RevTag { get; set; }

    }
}
