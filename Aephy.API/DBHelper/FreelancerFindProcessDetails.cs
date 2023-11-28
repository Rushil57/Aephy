using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Aephy.API.DBHelper;

public class FreelancerFindProcessDetails
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    public int FreelancerFindProcessHeaderId { get; set; }
    public int AlgorithumStage { get; set; }
    public string? FreelancerType { get; set; }
    public string? FreelancerId { get; set; }
    public int ApproveStatus { get; set; }
    public DateTime CreatedDate { get; set; }
}
