#nullable disable

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TNG.Web.Board.Data.DTOs
{
    public enum FetishRoleEnum
    {
        Top,
        Bottom,
        Switch
    }

    public class MemberFetish
    {
        [Key]
        public Guid Id { get; set; }
        [Required]
        [ForeignKey(nameof(Member))]
        public Guid MemberId { get; set; }
        [Required]
        [ForeignKey(nameof(Fetish))]
        public Guid FetishId { get; set; }
        public FetishRoleEnum? Role { get; set; }
        public bool? WillingToTeach { get; set; }

        public Member Member { get; set; }
        public Fetish Fetish { get; set; }
    }
}
