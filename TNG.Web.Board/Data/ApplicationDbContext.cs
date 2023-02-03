using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using TNG.Web.Board.Data.DTOs;

namespace TNG.Web.Board.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Member> Members { get; set; }
        public virtual DbSet<MembershipPayment> MembershipPayments { get; set; }
        public virtual DbSet<MembershipSuspensions> MembershipSuspensions { get; set; }
    }
}