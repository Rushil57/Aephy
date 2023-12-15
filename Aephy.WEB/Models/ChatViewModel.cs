namespace Aephy.WEB.Models;

public class ChatViewModel
{

}

public class ChatPopupRequestViewModel
{
    public int SolutionID { get; set; }

    public int IndustryId { get; set; }

    public int SolutionFundId { get; set; }

    public string? UserId { get; set; }

    public string? UserRole { get; set; }

    public string? LoginFreelancerId { get; set; }
}

public class ChatPopupResponseViewModel
{
    public string? FreelancerName { get; set; }

    public string? FreelancerId { get; set; }
}

public class ChatGridRequestViewModel
{
    public string? UserId { get; set; }
    public string? UserRole { get; set; }
}
