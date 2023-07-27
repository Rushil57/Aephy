namespace Aephy.WEB.Admin.Models
{
    public class IndustriesModel
    {
        public int Id { get; set; }
        public string IndustryName { get; set; }
        public bool Active { get; set; }

        public bool IsActiveFreelancer { get; set; }

        public bool IsActiveClient { get; set; }
    }
}
