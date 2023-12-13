namespace Aephy.API.Models;

public class FreelancerRequestModel
{
    public int Id { get; set; }
    public int RequestStatus { get; set; }
}

public class FindExchangeRateModel
{
    public string? Level { get; set; }
    public string? FreelancerId { get; set; }
    public decimal hourlyRate { get; set; }
    public string? CurrencyType { get; set; }
}