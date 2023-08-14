using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Aephy.WEB.Models
{
    public class OpenGigRolesModel
    {
        public int ID { get; set; }
        public string? FreelancerID { get; set; }
        public int GigOpenRoleId { get; set; }
        public bool IsApproved { get; set; }
        public DateTime CreatedDateTime { get; set; }
        public string? Description { get; set; }
		public string? CVPath { get; set; }
		public string? CVUrlWithSas { get; set; }

        public bool AlreadyExistCv { get; set; }
	}

    public class OpenGigRolesCV
    {
        public int ID { get; set; }

        public string? CVPath { get; set; }

        public string? BlobStorageBaseUrl { get; set; }

        public string? CVUrlWithSas { get; set; }

        public bool AlreadyExist { get; set; }

        public string? FreelancerId { get; set; }
    }

	public class OpenGigRolesApplications
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int ID { get; set; }

		public string? FreelancerID { get; set; }

		public int GigOpenRoleId { get; set; }

		public bool IsApproved { get; set; }

		public DateTime CreatedDateTime { get; set; }

		public string? Description { get; set; }
		public string? CVPath { get; set; }
		public string? BlobStorageBaseUrl { get; set; }
		public string? CVUrlWithSas { get; set; }

		public bool IsRejected { get; set; }

	}
}
