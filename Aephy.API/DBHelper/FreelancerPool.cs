using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Aephy.API.DBHelper;

public class FreelancerPool
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int ID { get; set; }

    public int SolutionID { get; set; }

    public int IndustryId { get; set; }

    public string? FreelancerID { get; set; }

    public bool IsProjectArchitect { get; set; }

}
