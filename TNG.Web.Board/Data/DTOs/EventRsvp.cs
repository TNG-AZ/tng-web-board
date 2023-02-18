#nullable disable

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TNG.Web.Board.Data.DTOs
{
    public enum EventRsvpStatus
    {
        Going,
        MaybeGoing
    }

    public class EventRsvp
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [ForeignKey(nameof(Member))]
        public Guid MemberId { get; set; }
        [Required]
        public string EventId { get; set; }
        [Required]
        public EventRsvpStatus Status { get; set; }

        public virtual Member Member { get; set; }
    }
}
