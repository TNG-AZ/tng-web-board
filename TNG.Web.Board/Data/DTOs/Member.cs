#nullable disable
using System.ComponentModel.DataAnnotations;

namespace TNG.Web.Board.Data.DTOs
{
    public class Member
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
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
