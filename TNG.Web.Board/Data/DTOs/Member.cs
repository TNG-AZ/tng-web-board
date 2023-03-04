#nullable disable
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
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

        public virtual List<MembershipPayment> Payments { get; set; }
        public virtual List<MembershipSuspension> Suspensions { get; set; }
        public virtual List<MembershipNote> Notes { get; set; }
        public virtual List<MembershipOrientation> Orientations { get; set; }
        public virtual List<EventRsvp> Events { get; set; }
        public virtual List<MemberFetish> MemberFetishes { get; set; }
        public virtual List<MemberDiscordIntegration> MemberDiscords { get; set; }
    }
}
