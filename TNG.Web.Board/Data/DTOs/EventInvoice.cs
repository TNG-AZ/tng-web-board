#nullable disable

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TNG.Web.Board.Data.DTOs
{
    public class EventInvoice
    {
        [Key]
        public Guid Id { get; set; }
        [Required]
        public string EventId { get; set; }
        [Required]
        [ForeignKey(nameof(Member))]
        public Guid MemberId { get; set; }
        [Required]
        public DateTime SendOnDate { get; set; } = DateTime.UtcNow;
        public DateTime? PaidOnDate { get; set; }

        public virtual Member Member { get; set; }
    }
}
