using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Aephy.API.DBHelper
{
    public class SolutionTopProfessionals
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int SolutionId { get; set; }

        public int IndustryId { get; set; }

        public string? FreelancerId { get; set; }

        public string? TopProfessionalTitle { get; set; }

        public string? Description { get; set; }

        public string? Rate { get; set; }

        public bool IsVisibleOnLandingPage { get; set; }
    }
}
