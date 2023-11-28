using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Aephy.API.DBHelper;

public class FreelancerFindProcessHeader
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    public string? ClientId { get; set; }
    public int SolutionId { get; set; }
    public int IndustryId { get; set; }
    public string? ProjectType { get; set; }
    public int CurrentAlgorithumStage { get; set; }
    public int TotalProjectManager { get; set; }
    public int TotalAssociate { get; set; }
    public int TotalExpert { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime ExecuteDate { get; set; }
    public bool IsTeamCompleted { get; set; }
}
