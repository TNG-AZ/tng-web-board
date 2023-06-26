#nullable disable

using System.ComponentModel.DataAnnotations;

namespace TNG.Web.Board.Data.DTOs
{
    public class EventFees
    {
        [Key]
        public Guid Id { get; set; }
        [Required]
        public string EventId { get; set; }
        [Required]
        public decimal MembershipDues { get; set; }
        [Required]
        public decimal MemberEntry { get; set; }
        [Required]
        public decimal GuestEntry { get; set; }
    }
}
