namespace Aephy.WEB.Admin.Models
{
	public class GigOpenRolesModel
	{
		public int ID { get; set; }

		public int SolutionId { get; set; }

		public string? Title { get; set; }

		public string? Level { get; set; }

		public string? Description { get; set; }

        public bool isActive { get; set; }

        public int IndustryId { get; set; }

        public DateTime CreatedDateTime { get; set; }

		public string? Name { get; set; }

        public string? ApproveOrReject { get; set; }
    }
}
