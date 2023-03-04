using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TNG.Web.Board.Data.DTOs
{
    [Index(nameof(MemberId), nameof(DiscordId), IsUnique = true)]
    public class MemberDiscordIntegration
    {
        [Key]
        public Guid Id { get; set; }
        [Required]
        [ForeignKey(nameof(Member))]
        public Guid MemberId { get; set; }
        [Required]
        public long DiscordId { get; set; }

        public virtual Member Member { get; set; }
    }
}
