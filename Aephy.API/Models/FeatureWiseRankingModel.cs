namespace Aephy.API.Models;

public class FeatureWiseRankingModel
{
    public int Id { get; set; }

    public string Feature { get; set; } = null!;

    public int Ranking { get; set; }

    public int? FeatureChildId { get; set; }

    public decimal Weight { get; set; }

    public List<FeatureWiseRankingModel> FeatureChild { get; set; }

    public string ParantFeature { get; set; }

    public int RatingSum { get; set; }
    public int FreeLanceId { get; set; }
    public decimal TempScore { get; set; }
    public decimal TempWeight { get; set; }
}
