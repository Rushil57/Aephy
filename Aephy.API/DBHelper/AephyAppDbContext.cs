using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;
using System.Reflection.Metadata;

namespace Aephy.API.DBHelper
{
    public class AephyAppDbContext : IdentityDbContext<ApplicationUser>
    {
        public AephyAppDbContext(DbContextOptions<AephyAppDbContext> options) : base(options)
        {

        }

        public DbSet<FreelancerDetails> FreelancerDetails { get; set; }

        public DbSet<Industries> Industries { get; set; }

        public DbSet<Services> Services { get; set; }

        public DbSet<Solutions> Solutions { get; set; }

        public DbSet<SolutionServices> SolutionServices { get; set; }

        public DbSet<SolutionIndustry> SolutionIndustry { get; set; }

        public DbSet<ClientDetails> ClientDetails { get; set; }

        public DbSet<OpenGigRolesApplications> OpenGigRolesApplications { get; set; }
        public DbSet<GigOpenRoles> GigOpenRoles { get; set; }
        public DbSet<SolutionIndustryDetails> SolutionIndustryDetails { get; set; }
        public DbSet<SolutionMilestone> SolutionMilestone { get; set; }

        public DbSet<SolutionPoints> SolutionPoints { get; set; }
        public DbSet<FreelancerPool> FreelancerPool { get; set; }
        public DbSet<SolutionDefine> SolutionDefine { get; set; }
        public DbSet<SolutionTopProfessionals> SolutionTopProfessionals { get; set; }
        public DbSet<SolutionSuccessfullProject> SolutionSuccessfullProject { get; set; }
        public DbSet<SolutionSuccessfullProjectResult> SolutionSuccessfullProjectResult { get; set; }

        public DbSet<LevelRange> LevelRanges { get; set; }

        public DbSet<Notifications> Notifications { get; set; }

        public DbSet<SavedProjects> SavedProjects { get; set; }

        public DbSet<Contract> Contract { get; set; }

        public DbSet<ContractUser> ContractUser { get; set; }

        public DbSet<SolutionFund> SolutionFund { get; set; }

        public DbSet<SolutionDispute> SolutionDispute { get; set; }
        public DbSet<ClientAvailability> ClientAvailability { get; set; }

        public DbSet<CustomSolutions> CustomSolutions { get; set; }

        public DbSet<SolutionStopPayment> SolutionStopPayment { get; set; }

        public DbSet<EmployeeOpenRole> EmployeeOpenRole { get; set; }

        public DbSet<SolutionTeam> SolutionTeam { get; set; }

        public DbSet<Country> Country { get; set; }

        public DbSet<ProjectReview> ProjectReview { get; set; }

        public DbSet<FreelancerReview> FreelancerReview { get; set; }

        public DbSet<Invoices> Invoices { get; set; }

        public DbSet<ActiveProjectDocuments> ActiveProjectDocuments { get; set; }

        
        public DbSet<ActiveSolutionMilestoneStatus> ActiveSolutionMilestoneStatus { get; set; }

        public DbSet<FreelancerToFreelancerReview> FreelancerToFreelancerReview { get; set; }

        public DbSet<EphylinkRevolutAccount> EphylinkRevolutAccount { get; set; }

        public DbSet<InvoiceList> InvoiceList { get; set; }

        public DbSet<InvoiceListDetails> InvoiceListDetails { get; set; }

        public DbSet<ExchangeRates> ExchangeRates { get; set; }

        public DbSet<SolutionLeave> SolutionLeave { get; set; }

        public DbSet<EphylinkRevolutAccountTransferLog> EphylinkRevolutAccountTransferLog { get; set; }

        public DbSet<CustomProjectDetials> CustomProjectDetials { get; set; }

        public DbSet<FreelancerExcludeDate> FreelancerExcludeDate { get; set; }


        protected override void OnModelCreating(ModelBuilder builder)
		{
            base.OnModelCreating(builder);
			builder.Entity<GigOpenRoles>().ToTable(tb => tb.HasTrigger("SomeTrigger"));
            builder.Entity<OpenGigRolesApplications>().ToTable(tb => tb.HasTrigger("SomeTrigger"));
        }
    }
}