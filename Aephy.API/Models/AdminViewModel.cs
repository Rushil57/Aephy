﻿using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Aephy.API.Models
{
    public class AdminViewModel
    {
        public class ServicesModel
        {
            public int Id { get; set; }
            public string? ServiceName { get; set; }
            public bool Active { get; set; }

            public bool IsActiveFreelancer { get; set; }

            public bool IsActiveClient { get; set; }
        }

        public class IndustriesModel
        {
            public int Id { get; set; }
            public string IndustryName { get; set; }
            public bool isActive { get; set; }

            public bool IsActiveFreelancer { get; set; }

            public bool IsActiveClient { get; set; }
        }
        public class SolutionsModel
        {
            public int Id { get; set; }

            public string? Title { get; set; }

            public string? SubTitle { get; set; }

            public string? Description { get; set; }

            public List<int> solutionIndustries { get; set; }

            public int solutionServices { get; set; }

            public string? Industries { get; set; }

            public string? Services { get; set; }
            public string? Image { get; set; }

            public string? ImageUrlWithSas { get; set; }

            public string? ImagePath { get; set; }

            //public IFormFile[] ImageFile { get; set; }
        }

        public class SolutionServicesViewModel
        {
            public int Id { get; set; }

            public int SolutionId { get; set; }

            public int ServicesId { get; set; }
        }

        public class SolutionIndustryViewModel
        {
            public int Id { get; set; }

            public int SolutionId { get; set; }

            public int IndustryId { get; set; }
        }

        public class SolutionIdModel
        {
            public int Id { get; set; }

            public string? ImagePath { get; set; }

            public string? ImageUrlWithSas { get; set; }

        }

        public class SolutionImage
        {
            public int Id { get; set; }

            public string? ImagePath { get; set; }

            public string? BlobStorageBaseUrl { get; set; }

            public string? ImageUrlWithSas { get; set; }
            public bool HasImageFile { get; set; }
        }

        public class EditSolutionImage
        {
            public int Id { get; set; }

            public string? ImagePath { get; set; }
        }

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
        }

        public class OpenGigRolesCV
        {
            public int ID { get; set; }

            public string? CVPath { get; set; }

            public string? BlobStorageBaseUrl { get; set; }

            public string? CVUrlWithSas { get; set; }
        }
        public class GigOpenRolesModel
        {
            public int ID { get; set; }

            public int SolutionId { get; set; }

            public string? Title { get; set; }

            public string? Level { get; set; }

            public string? Description { get; set; }

            public DateTime CreatedDateTime { get; set; }
        }

        public class SolutionDescribeModel
        {
            public int Id { get; set; }
            public int IndustryId { get; set; }
            public int SolutionId { get; set; }
            public string? Description { get; set; }

            public string? ImageUpload { get; set; }

            public string? ImagePath { get; set; }
        }

        public class UserIdModel
        {
            public string? Id { get; set; }
        }

        public class UserViewModel
        {
            public string? Id { get; set; }

            public string? FirstName { get; set; }

            public string? LastName { get; set; }

            public string? EmailAddress { get; set; }

            public string? UserRole { get; set; }

            public string? FreelancerLevel { get; set; }



        }
    }
}
