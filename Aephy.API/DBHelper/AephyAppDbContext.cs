using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Aephy.API.DBHelper
{
    public class AephyAppDbContext : IdentityDbContext<ApplicationUser>
    {
        public AephyAppDbContext(DbContextOptions<AephyAppDbContext> options) : base(options)
        {

        }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
        }
    }
}