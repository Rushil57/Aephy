using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Aephy.API.DBHelper
{
    public class OpenGigRolesApplications
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        public string? FreelancerID { get;set; }

        public int GigOpenRoleId { get; set; }

        public bool IsApproved { get; set; }

        public DateTime CreatedDateTime { get; set; }

		public string? Description { get; set; }
		public string? CVPath { get; set; }
		public string? BlobStorageBaseUrl { get; set; }
		public string? CVUrlWithSas { get; set; }

	}
    public class GigOpenRoles
    {
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        public int SolutionId { get; set; }

        public string? Title { get; set; }

        public string? Level { get; set; }

        public string? Description { get; set; }

        public DateTime CreatedDateTime { get; set; }
	}
	public class GigOpenRolesModel
	{
		public int ID { get; set; }

		[NotMapped]
		public string? ServiceName { get; set; }

		public int SolutionId { get; set; }

		public string? Title { get; set; }

		public string? Level { get; set; }

		[NotMapped]
		public string? IndustryName { get; set; }

		public DateTime CreatedDateTime { get; set; }
	}
}
