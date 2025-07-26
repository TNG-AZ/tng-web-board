#nullable disable

using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TNG.Web.Board.Data.DTOs
{
    public enum EventRsvpStatus
    {
        Going,
        MaybeGoing
    }

    [Index(nameof(EventId))]
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

        public bool? Attended { get; set; }
        public bool? Approved { get; set; }
        public bool? Paid { get; set; }
        public DateTime? AddedDate { get; set; } = DateTime.UtcNow;
        public DateTime? VoidedDate { get; set; }

#nullable enable
        public string? Notes { get; set; }
#nullable disable

        public virtual Member Member { get; set; }
    }
}
