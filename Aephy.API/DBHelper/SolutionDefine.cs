using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Aephy.API.DBHelper;

public class SolutionDefine
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    public int SolutionIndustryDetailsId { get; set; }

    public string? ProjectOutline { get; set; }

    public string? ProjectDetails { get; set; }

    public string? ProjectType { get; set; }

    public DateTime CreatedDateTime { get; set; }

    public bool IsActive { get; set; }
}
