#nullable disable

using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TNG.Web.Board.Data.DTOs
{
    [Index(nameof(DateAdded), AllDescending = true)]
    public class MembershipNote
    {
        [Key]
        public Guid Id { get; set; }
        [Required]
        [ForeignKey(nameof(Member))]
        public Guid MemberId { get; set; }
        [Required]
        public DateTime DateAdded { get; set; } = DateTime.Now;
        [Required]
        public string Note { get; set; }

        public virtual Member Member { get; set; }

#nullable enable
        public virtual List<NoteTag>? NoteTags { get; set; }
#nullable disable
    }
}
