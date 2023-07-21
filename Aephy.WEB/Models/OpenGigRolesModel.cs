﻿namespace Aephy.WEB.Models
{
    public class OpenGigRolesModel
    {
        public int ID { get; set; }
        public int FreelancerID { get; set; }
        public int ServiceID { get; set; }
        public int IndustriesID { get; set; }
        public int SolutionID { get; set; }
        public string? Title { get; set; }
        public string? Level { get; set; }
        public bool IsApproved { get; set; }
        public DateTime CreatedDateTime { get; set; }
        public string? Description { get; set; }
		public string? CVPath { get; set; }
		public string? CVUrlWithSas { get; set; }
	}

    public class OpenGigRolesCV
    {
        public int ID { get; set; }

        public string? CVPath { get; set; }

        public string? BlobStorageBaseUrl { get; set; }

        public string? CVUrlWithSas { get; set; }
    }
}
