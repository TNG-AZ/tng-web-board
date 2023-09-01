using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TNG.Web.Board.Data.DTOs
{
    public class EventRsvpPlusOne
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string EventId { get; set; }
        [ForeignKey(nameof(Member))]
        public Guid MemberId { get; set; }
        [ForeignKey(nameof(PlusOne))]
        public Guid PlusOneMemberId { get; set; }
        public DateTime CreatedDT { get; set; } = DateTime.UtcNow;

        public virtual Member Member { get; set; }
        public virtual Member PlusOne { get; set; }
    }
}
