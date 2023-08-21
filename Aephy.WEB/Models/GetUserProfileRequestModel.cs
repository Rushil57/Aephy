namespace Aephy.WEB.Models
{
    public class GetUserProfileRequestModel
    {
        public string? UserId { get; set; } = "";
    }

    public class ImageClass
    {
        public int Id { get; set; }

        public string? BlobStorageBaseUrl { get; set; }
        public string? ImagePath { get; set; }
        public string? ImageUrlWithSas { get; set; }

        public string? FreelancerId { get; set; }
        
    }
}
