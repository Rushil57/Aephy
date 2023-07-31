using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Aephy.API.DBHelper
{
    public class SolutionMilestone
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public string? Description { get; set; }

        public string? Title { get; set; }

        public DateTime DueDate { get; set;}

        public int SolutionId { get; set; }

        public int IndustryId { get; set; }

        public string? FreelancerId { get; set; }
    }
}
