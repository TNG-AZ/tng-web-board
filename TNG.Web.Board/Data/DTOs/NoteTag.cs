#nullable disable

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Net;

namespace TNG.Web.Board.Data.DTOs
{
    public class NoteTag
    {
        [Key]
        public Guid Id { get; set; }

        [ForeignKey(nameof(Note))]
        public Guid NoteId { get; set; }
        [ForeignKey(nameof(Tag))]
        public Guid TagId { get; set; }

        public virtual MembershipNote Note { get; set; }
        public virtual Tag Tag { get; set; }
    }
}
