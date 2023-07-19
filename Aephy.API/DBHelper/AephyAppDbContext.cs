using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

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
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
        }
    }
}