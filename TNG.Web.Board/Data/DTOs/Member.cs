#nullable disable
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

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
        public Guid Id { get; set; } = Guid.NewGuid();
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

        public virtual IEnumerable<MembershipPayment> Payments { get; set; }
    }
}
