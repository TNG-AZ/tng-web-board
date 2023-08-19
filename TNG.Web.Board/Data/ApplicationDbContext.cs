using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
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
        public virtual DbSet<MembershipNote> MemberNotes { get; set; }
        public virtual DbSet<MembershipOrientation> MemberOrientations { get; set; }
        public virtual DbSet<MembershipPayment> MemberDuesPayments { get; set; }
        public virtual DbSet<MembershipSuspension> MemberSuspensions { get; set; }
        public virtual DbSet<Tag> Tags { get; set; }
        public virtual DbSet<NoteTag> NoteTagMappings { get; set; }
        public virtual DbSet<EventRsvp> EventRsvps { get; set; }
        public virtual DbSet<Fetish> Fetishes { get; set;}
        public virtual DbSet<MemberFetish> MembersFetishes { get; set; }
        public virtual DbSet<MemberDiscordIntegration> MembersDiscordIntegrations { get; set;}
        public virtual DbSet<EventFees> EventsFees { get; set; }
        public virtual DbSet<EventInvoice> EventsInvoices { get; set; }
        public virtual DbSet<Signature> Signatures { get; set; }
    }
}