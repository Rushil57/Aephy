namespace Aephy.WEB.Models
{
    public class LevelRangeModel
    {
        public int ID { get; set; }

        public string? Level { get; set; }

        public decimal minLevel { get; set; }

        public decimal maxLevel { get; set; }
    }

    public class FindExchangeRateModel
    {
        public string? Level { get; set; }
        public string? FreelancerId { get; set; }
        public decimal hourlyRate { get; set; }
        public string? CurrencyType { get; set; }
    }
}
