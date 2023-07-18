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

        public DbSet<ClientDetails> ClientDetails { get; set; }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
        }
    }
}