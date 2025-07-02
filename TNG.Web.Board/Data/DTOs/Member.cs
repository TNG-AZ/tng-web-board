#nullable disable
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Net;

namespace TNG.Web.Board.Data.DTOs
{
    public enum MemberType
    {
        Member,
        Guest,
        Honorary
    }

    [Index(nameof(EmailAddress), IsUnique = true)]
    [Index(nameof(SceneName), nameof(LegalName))]
    public class Member
    {
        [Key]
        public Guid Id { get; set; }
        public DateTime? AddedDate { get; set; } = DateTime.UtcNow;
        [Required]
        public MemberType MemberType { get; set; }
        [Required]
        public bool HasAttendedSocial { get; set; } = false;
        [Required]
        public string LegalName { get; set; }
        [Required]
        public string SceneName { get; set; }
        [Required]
        public DateTime Birthday { get; set; }
        [Required]
        public string EmailAddress { get; set; }
        public bool ReceivedProofOfCovid19Vaccination { get; set; } = false;
        public bool PrivateProfile { get; set; } = false;
        public DateTime? LastDMTrainingDate { get; set; }
#nullable enable
        [StringLength(20)]
        [Column(TypeName = "NVARCHAR(20)")]
        public string? ProfileUrl { get; set; }
        [Column(TypeName = "TEXT")]
        public string? AboutMe { get; set; }
        public string? Pronouns { get; set; }
#nullable disable

        public virtual IEnumerable<MembershipPayment> Payments { get; set; }
        public virtual IEnumerable<MembershipSuspension> Suspensions { get; set; }
        public virtual IEnumerable<MembershipNote> Notes { get; set; }
        public virtual IEnumerable<MembershipOrientation> Orientations { get; set; }
        public virtual IEnumerable<EventRsvp> Events { get; set; }
        public virtual IEnumerable<MemberFetish> MemberFetishes { get; set; }
        public virtual IEnumerable<MemberDiscordIntegration> MemberDiscords { get; set; }
        public virtual IEnumerable<EventInvoice> Invoices { get; set; }
        public virtual IEnumerable<Signature> Signatures { get; set; }
        [InverseProperty(nameof(EventRsvpPlusOne.Member))]
        public virtual IEnumerable<EventRsvpPlusOne> RsvpPlusOnesAsPrimary { get; set; }
        [InverseProperty(nameof(EventRsvpPlusOne.PlusOne))]
        public virtual IEnumerable<EventRsvpPlusOne> RsvpPlusOnesAsGuest { get; set; }
        public virtual IEnumerable<RaffleEntry> RaffleEntries { get; set; }
        public virtual IEnumerable<Raffle> RaffleWins { get; set; }

        public virtual ICollection<VolunteerPositionRole> VolunteerRoles { get; set; }
        public virtual ICollection<VolunteerSlotMember> VolunteerSlots { get; set; }
    }
}
