namespace Aephy.WEB.Models
{
    public class LevelRangeModel
    {
        public int ID { get; set; }

        public string? Level { get; set; }

        public decimal minLevel { get; set; }

        public decimal maxLevel { get; set; }
    }
}
