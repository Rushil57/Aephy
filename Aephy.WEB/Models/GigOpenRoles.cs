using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Aephy.WEB.Models
{
    public class GigOpenRoles
    {
            [Key]
            [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
            public int ID { get; set; }

            public int SolutionId { get; set; }

            public string? Title { get; set; }

            public string? Level { get; set; }

            public string? Description { get; set; }

            public bool isActive { get; set; }

            public int IndustryId { get; set; }

            public DateTime CreatedDateTime { get; set; }
    }
}
