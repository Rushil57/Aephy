using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Aephy.API.DBHelper;
using static Aephy.API.DBHelper.SolutionFund;
using Stripe.Terminal;

namespace Aephy.API.Models
{
    public class AdminViewModel
    {
        public class ServicesModel
        {
            public int Id { get; set; }
            public string? ServiceName { get; set; }
            public bool Active { get; set; }

            public bool IsActiveFreelancer { get; set; }

            public bool IsActiveClient { get; set; }
        }

        public class IndustriesModel
        {
            public int Id { get; set; }
            public string IndustryName { get; set; }
            public bool isActive { get; set; }

            public bool IsActiveFreelancer { get; set; }

            public bool IsActiveClient { get; set; }
        }
        public class SolutionsModel
        {
            public int Id { get; set; }

            public string? Title { get; set; }

            public string? SubTitle { get; set; }

            public string? Description { get; set; }

            public List<int>? solutionIndustries { get; set; }

            public int solutionServices { get; set; }

            public string? Industries { get; set; }

            public string? Services { get; set; }
            public string? Image { get; set; }

            public string? ImageUrlWithSas { get; set; }

            public string? ImagePath { get; set; }

            public bool IsProjectSaved { get; set; }

            public string? PaymentStatus { get; set; }
            public string? MileStoneTitle { get; set; }

            public int SolutionId { get; set; }
            public int IndustryId { get; set; }
            public int ServiceId { get; set; }
            public int ContractId { get; set; }
            public string? ProjectStatus { get; set; }

            public string? ClientId { get; set; }

            public string? ClientName { get; set; }

            //public IFormFile[] ImageFile { get; set; }
        }

        public class SolutionServicesViewModel
        {
            public int Id { get; set; }

            public int SolutionId { get; set; }

            public int ServicesId { get; set; }
        }

        public class SolutionIndustryViewModel
        {
            public int Id { get; set; }

            public int SolutionId { get; set; }

            public int IndustryId { get; set; }
        }

        public class SolutionIdModel
        {
            public int Id { get; set; }

            public string? ImagePath { get; set; }

            public string? ImageUrlWithSas { get; set; }

        }

        public class SolutionImage
        {
            public int Id { get; set; }

            public string? ImagePath { get; set; }

            public string? BlobStorageBaseUrl { get; set; }

            public string? ImageUrlWithSas { get; set; }
            public bool HasImageFile { get; set; }

            public string? FreelancerId { get; set; }
        }

        public class EditSolutionImage
        {
            public int Id { get; set; }

            public string? ImagePath { get; set; }
        }

        public class OpenGigRolesModel
        {
            public int ID { get; set; }
            public string? FreelancerID { get; set; }
            public int GigOpenRoleId { get; set; }
            public bool IsApproved { get; set; }
            public DateTime CreatedDateTime { get; set; }
            public string? Description { get; set; }
            public string? CVPath { get; set; }
            public string? CVUrlWithSas { get; set; }

            public bool AlreadyExistCv { get; set; }
        }

        public class OpenGigRolesCV
        {
            public int ID { get; set; }

            public string? CVPath { get; set; }

            public string? BlobStorageBaseUrl { get; set; }

            public string? CVUrlWithSas { get; set; }
            public bool AlreadyExist { get; set; }

            public string? FreelancerId { get; set; }
        }
        public class GigOpenRolesModel
        {
            public int ID { get; set; }

            public int SolutionId { get; set; }

            public int IndustryId { get; set; }

            public string? Title { get; set; }

            public string? Level { get; set; }

            public string? Description { get; set; }

            public string? CreatedDateTime { get; set; }

            public string? Name { get; set; }

            public string? ApproveOrReject { get; set; }

            public string? IndustriesName { get; set; }
            public string? SolutionName { get; set; }
            public string? FreeLancerLavel { get; set; }

            public string? FreelancerId { get; set; }

            public string? CurrentLoggedInId { get; set; }
        }

        public class SolutionDescribeModel
        {
            public int Id { get; set; }
            public int IndustryId { get; set; }
            public int SolutionId { get; set; }

            public string[]? AssignedFreelancerIds { get; set; }
            public string[]? IsArchitectIds { get; set; }

            public string? Description { get; set; }

            public string? ImageUpload { get; set; }

            public string? ImagePath { get; set; }
            public string? ActiveByAdmin { get; set; }

            public bool IsActiveForClient { get; set; }
            public bool IsActiveForFreelancer { get; set; }
        }

        public class UserIdModel
        {
            public string? Id { get; set; }

            public string? UserId { get; set; }
        }

        public class UserViewModel
        {
            public string? Id { get; set; }

            public string? FirstName { get; set; }

            public string? LastName { get; set; }

            public string? EmailAddress { get; set; }

            public string? UserRole { get; set; }

            public string? FreelancerLevel { get; set; }



        }

        public class MileStoneModel
        {
            public int Id { get; set; }
            public string? Title { get; set; }
            public string? Description { get; set; }
            public int IndustryId { get; set; }

            public int SolutionId { get; set; }
            public DateTime? DueDate { get; set; }

            public string? FreelancerId { get; set; }

            public string? UserId { get; set; }

            public string? ProjectType { get; set; }
            public int Days { get; set; }

            public string? MilestoneStatus { get; set; }

            public decimal MilestonePrice { get; set; }

        }

        public class SolutionIndustryDetailsModel
        {
            public int Id { get; set; }

            public string? ProjectOutline { get; set; }

            public string? ProjectDetails { get; set; }

            public int IndustryId { get; set; }

            public int SolutionId { get; set; }

            public string? ProjectType { get; set; }

            public string? Duration { get; set; }

            public int TeamSize { get; set; }

            public string? UserId { get; set; }

            public int SolutionFundId { get; set; }

            public bool MileStoneCheckout { get; set; }

        }
        public class MileStoneIdViewModel
        {
            public int Id { get; set; }

            public string? UserId { get; set; }

            public int? pageNumber { get; set; }

            public int SolutionFundId { get; set; }

            public string? SolutionName { get; set; }

            public string? ClientPreferredCurrency { get; set; }
        }

        public class MileStoneDetailsViewModel
        {
            public int IndustryId { get; set; }

            public int SolutionId { get; set; }

            public string? FreelancerId { get; set; }

            public string? ProjectType { get; set; }

            public string? FreelancerLevel { get; set; }

            public string? UserId { get; set; }

            public int SolutionFundId { get; set; }

            public string? ClientPreferredCurrency { get; set; }

            public int SolutionIndustryId { get; set; }
        }

        public class UserCvFileModel
        {
            public string? UserId { get; set; }

            public string? CVPath { get; set; }

            public string? BlobStorageBaseUrl { get; set; }

            public string? CVUrlWithSas { get; set; }
        }

        public class SolutionDefineRequestViewModel
        {
            public int IndustryId { get; set; }

            public int SolutionId { get; set; }

            public string? ProjectType { get; set; }

            public string? FreelancerId { get; set; }

            public bool ProjectArchitect { get; set; }
        }

        public class SolutionTopProfessionalModel
        {
            public int IndustryId { get; set; }

            public int SolutionId { get; set; }

            public string? FreelancerId { get; set; }

            public string? Title { get; set; }

            public string? Description { get; set; }

            public string? Rate { get; set; }

            public string? ImagePath { get; set; }

            public string? ImageUrlWithSas { get; set; }

        }

        public class SuccessfullProjectModel
        {
            public int Id { get; set; }

            public string? Title { get; set; }

            public string? Description { get; set; }

            public List<SolutionSuccessfullProjectResult> projectResultList { get; set; }
        }

        public class solutionFundViewModel
        {
            public int Id { get; set; }

            public int SolutionId { get; set; }

            public int IndustryId { get; set; }

            public int MileStoneId { get; set; }

            public string? ClientId { get; set; }

            public string? ProjectType { get; set; }

            public string? ProjectPrice { get; set; }

            public bool IsDispute { get; set; }

            public string? DisputeReason { get; set; }
            public string? ProjectStatus { get; set; }

            public bool IsArchived { get; set; }

            public SolutionFund.FundTypes FundType { get; set; }

            public bool IsCheckOutDone { get; set; }

            public bool MileStoneCheckout { get; set; }

            public bool GetNextMileStoneData { get; set; }

            public SolutionFund? solutionFunds { get; set; }

            public int SolutionFundId { get; set; }

            public string? UserId { get; set; }

            public bool RevolutStatus { get; set; }

            public string? ClientPreferredCurrency { get; set; }

            public int TotalAssociate { get; set; }

            public string? TotalExpert { get; set; }

            public string? TotalProjectManager { get; set; }

            public string? StartHour { get; set; }
            public string? EndHour { get; set; }

            public bool? onMonday { get; set; }

            public bool? onTuesday { get; set; }

            public bool? onWednesday { get; set; }

            public bool? onThursday { get; set; }

            public bool? onFriday { get; set; }

            public bool? onSaturday { get; set; }

            public bool? onSunday { get; set; }
        }

        public class SolutionDisputeViewModel
        {
            public int Id { get; set; }
            public int ContractId { get; set; }

            public string? ClientName { get; set; }

            public string? SolutionName { get; set; }

            public string? IndustryName { get; set; }

            public string? AdminEmailId { get; set; }

            public DateTime CreatedDate { get; set; }

            public string? FreelancerName { get; set; }

            public string? FreelancerId { get; set; }

            public string? TransferAmount { get; set; }

            public string? Currency { get; set; }

            public string? StripeConnectedId { get; set; }

            public string? LatestChargeId { get; set; }

            public bool IsDisputeResolved { get; set; }

            public string? Milestone { get; set; }

            public string? StopPaymentReason { get; set; }

            public bool IsPaymentStop { get; set; }

            public bool IsClientRefund { get; set; }
            public bool IsFreelancerRefund { get; set; }

            public string? ProjectPrice { get; set; }

            public string? Address { get; set; }

            public string? RevoultOrderId { get; set; }
        }
        public class CustomSolutionsModel
        {
            public int ID { get; set; }
            public string? ClientId { get; set; }
            public int ServiceId { get; set; }
            public int SolutionId { get; set; }
            public int IndustryId { get; set; }
            public string SolutionTitle { get; set; }
            public string SoultionDescription { get; set; }
            public string? DocumentPath { get; set; }
            public string? BlobStorageBaseUrl { get; set; }
            public string? DocumentUrlWithSas { get; set; }
            public DateTime DeliveryTime { get; set; }
            public decimal Budget { get; set; }
            public DateTime StartDate { get; set; }
            public DateTime EndDate { get; set; }
            [NotMapped]
            public bool AlreadyExistDocument { get; set; }
        }
        public class CustomSolutionDocument
        {
            public int ID { get; set; }

            public string? DocumentPath { get; set; }

            public string? BlobStorageBaseUrl { get; set; }

            public string? DocumentUrlWithSas { get; set; }

            public bool AlreadyExistDocument { get; set; }

            public string? ClientId { get; set; }
        }

        public class EmployeeOpenRolesModel
        {
            public int Id { get; set; }
            public string? Department { get; set; }
            public string? Title { get; set; }
            public string? Type { get; set; }
            public string? Location { get; set; }
            public string? JobDescription { get; set; }
            public DateTime? CreatedDateTime { get; set; }
        }

        public class SolutionTeamViewModel
        {
            public string? FreelancerId { get; set; }
            public string? FreelancerName { get; set; }
            public string? FreelancerLevel { get; set; }
            public string? ImagePath { get; set; }

            public string? ImageUrlWithSas { get; set; }

            public int SolutionId { get; set; }
            public int IndustryId { get; set; }

            public string? ClientId { get; set; }

            public string? UserId { get; set; }

            public int SolutionFundId { get; set; }

            public decimal ProjectManagerPlatformFees { get; set; }

            public string? ClientFees { get; set; }
        }

        public class UserDetailsModel
        {
            public string? UserId { get; set; }

            public string? FirstName { get; set; }

            public string? LastName { get; set; }

            public string? Email { get; set; }

            public string? ProfileUrl { get; set; }

            public string? Role { get; set; }

            public string? Description { get; set; }

            public string? ClientAddress { get; set; }

            public string? HourlyRate { get; set; }

            public string? Education { get; set; }

            public string? ProffessionalExperience { get; set; }

            public string? FreelancerAddress { get; set; }

            public string? FreelancerLevel { get; set; }

            public string? CVPath { get; set; }

            public string? ImagePath { get; set; }

            public string? ImageUrlWithSas { get; set; }

            public int CountryId { get; set; }

            public string? CountryName { get; set; }

            public string? CompanyName { get; set; }

            public string? StripeAccountStatus { get; set; }
            public string? BackCountry { get; set; }

            public bool IsIBanMandantory { get; set; }

            public bool RevoultStatus { get; set; }

            public bool ShowIndiaField { get; set; }
            public bool ShowAustraliaField { get; set; }
            public bool ShowMexicanField { get; set; }
            public bool ShowUsField { get; set; }

            public string? FreelancerCity { get; set; }

            public string? FreelancerPostCode { get; set; }

            public string? ClientCity { get; set; }

            public string? ClientPostCode { get; set; }

            public string? PreferredCurrency { get; set; }

            public string? TaxType { get; set; }

            public string? TaxNumber { get; set; }

            public bool IsRevoultBankAccount { get; set; }

            public string? RevTag { get; set; }

            public DateTime? StartHour { get; set; }

            public DateTime? EndHour { get; set; }

            public DateTime? StartDate { get; set; }

            public DateTime? EndDate { get; set; }

            public bool? IsWeekendExclude { get; set; }

            public bool? IsNotAvailableForNextSixMonth { get; set; }

            public bool? IsWorkEarlier { get; set; }

            public bool? IsWorkLater { get; set; }

            public DateTime? StartHoursEarlier { get; set; }

            public DateTime? EndHoursEarlier { get; set; }

            public DateTime? StartHoursLater { get; set; }

            public DateTime? EndHoursLater { get; set; }

            public bool? onMonday { get; set; }

            public bool? onTuesday { get; set; }

            public bool? onWednesday { get; set; }

            public bool? onThursday { get; set; }

            public bool? onFriday { get; set; }

            public bool? onSaturday { get; set; }

            public bool? onSunday { get; set; }

        }

        public class InvoiceViewModel
        {
            public int Id { get; set; }

            public int SolutionId { get; set; }

            public int IndustryId { get; set; }

            public int SolutionFundId { get; set; }

            public string? InvoiceNumber { get; set; }

            public DateTime? Date { get; set; }

            public DateTime? DueDate { get; set; }

            public string? TotalAmount { get; set; }

            public string? DueAmount { get; set; }

            public string? ClientId { get; set; }

            public string? ClientName { get; set; }

            public string? ClientAddress { get; set; }

            public string? VatId { get; set; }

            public string? TaxId { get; set; }

            public string? Title { get; set; }

            public string? Amount { get; set; }

            public string? VatPercentage { get; set; }

            public string? VatAmount { get; set; }

            public string? FundType { get; set; }
        }

        public class FreelancerToFreelancerReviewModel
        {
            public int Id { get; set; }
            public string? ToFreelancerId { get; set; }

            public string? FromFreelancerId { get; set; }

            public int SolutionId { get; set; }

            public int IndustryId { get; set; }

            public string? Feedback_Message { get; set; }

            public int CollaborationTeamWorkRating { get; set; }
            public int CommunicationRating { get; set; }
            public int ProfessionalismRating { get; set; }
            public int TechnicalRating { get; set; }
            public int ProjectManagementRating { get; set; }
            public int ResponsivenessRating { get; set; }
            public int WellDefinedProjectRating { get; set; }
        }


        public class CounterpartyAccountResp
        {
            public string? Id { get; set; }
            public string? Currency { get; set; }
            public string? Type { get; set; }
            public string? AccountNo { get; set; }
            public string? Iban { get; set; }
            public string? SortCode { get; set; }
            public string? RoutingNumber { get; set; }
            public string? Bic { get; set; }
            public string? RecipientCharges { get; set; }
        }

        public class AuthToken
        {
            public string? access_token { get; set; }
            public string? token_type { get; set; }
            public int expires_in { get; set; }
            public string? refresh_token { get; set; }
        }

        public class ResponseDto
        {
            public string id { get; set; }
            public string token { get; set; }
            public string type { get; set; }
            public string state { get; set; }
            public DateTime created_at { get; set; }
            public DateTime updated_at { get; set; }
            public int amount { get; set; }
            public string currency { get; set; }
            public int outstanding_amount { get; set; }
            public string capture_mode { get; set; }
            public string checkout_url { get; set; }
        }

        public class RevoultCheckOutModel
        {
            public int Id { get; set; }

            public string? RevoultOrderId { get; set; }

            public string? Token { get; set; }

            public string? UserId { get; set; }

            public int SolutionId { get; set; }

            public int IndustryId { get; set; }

            public int SolutionFundId { get; set; }

            public string? ProjectPrice { get; set; }

        }

        public class RefundPaymentRequest
        {
            public string? id { get; set; }

            public string? OrderId { get; set; }

            public string? Amount { get; set; }

            public string? Description { get; set; }

            public string? state { get; set; }

            public DateTime? created_at { get; set; }

            public DateTime? updated_at { get; set; }
        }

        public class InvoiceListViewModel
        {
            public int Id { get; set; }

            public int InvoiceId {  get; set; }

            public string? InvoiceType { get; set; }

            public string? InvoiceNumber { get; set; }

            public DateTime? InvoiceDate { get; set; }

            public string? TotalAmount { get; set; }

            public string? BillToClientId { get; set; }

            public string? TransactionType { get; set; }

            public int ContractId { get; set; }

            public List<InvoiceListDetails>? InvoicelistDetails { get; set; }

            public string? ClientFullName { get; set; }

            public string? ClientAddress { get; set; }

            public string? UserId { get; set; }

            public string? PreferredCurrency {  get; set; }

            public string? TaxId { get; set; }

            public string? TaxType { get; set; }

            public string? FreelancerFullName { get; set; }

            public string? FreelancerTaxId { get; set; }

            public string? FreelancerTaxType { get; set; }

            public string? FreelancerPreferredCurrency { get; set; }

            public string? FreelancerAddress { get; set; }

            public string? FreelancerCountry { get; set; }

            public string? ClientCountry { get; set; }
        }

        public class CustomSolutionModel
        {
            public int Id { get; set; }

            public string? TotalAssociate { get; set; }

            public string? TotalExpert { get; set; }

            public string? TotalProjectManager { get; set; }

            public string? CustomProjectOutline { get; set; }

            public string? CustomProjectDetail { get; set; }
            public decimal CustomPrice { get; set; }

            public string? CustomStartDate { get; set; }
            public string? CustomEndDate { get; set; }

            public bool CustomExcludeWeekend { get; set; }

            public string? CustomOtherHolidayList { get; set; }

            public string? CustomStartHour { get; set; }

            public string? CustomEndHour { get; set; }

            public string? UserId { get; set; }

            public int SolutionId { get; set; }

            public int IndustryId { get; set; }
            public string? ProjectType { get; set; }

            public string? CustomProjectDuration { get; set; }

            public bool IsSingleFreelancer { get; set; }

            public string? SingleFreelancer { get; set; }

        }


    }
}


