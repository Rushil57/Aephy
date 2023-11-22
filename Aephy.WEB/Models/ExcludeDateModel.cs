namespace Aephy.WEB.Models;

public class ExcludeDateModel
{
    public int Id { get; set; }
    public List<DateTime> ExcludeDateList { get; set; }
    public DateTime ExcludeDate { get; set; }
    public string? FreelancerId { get; set; }
}
public class ExcludeDateRequestModel
{
    public string DateRange { get; set; }
}
public class ExcludeDateGridModel
{
    public int Id { get; set; }
    public DateTime ExcludeDate { get; set; }
    public string? FreelancerId { get; set; }
}