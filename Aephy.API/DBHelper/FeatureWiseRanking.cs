using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Aephy.API.DBHelper;

public class FeatureWiseRanking
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    public string Feature { get; set; } = null!;

    public int Ranking { get; set; }

    public int? FeatureChildId { get; set; }
}
