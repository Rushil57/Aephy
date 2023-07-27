namespace Aephy.WEB.Admin.Models
{
    public class ServicesModel
    {
        public int Id { get; set; }
        public string? ServiceName { get; set; }
        public bool Active { get; set; }

        public bool IsActiveFreelancer { get; set; }

        public bool IsActiveClient { get; set; }
    }
}
