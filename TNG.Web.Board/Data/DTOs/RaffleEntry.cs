using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TNG.Web.Board.Data.DTOs
{
    public class RaffleEntry
    {
        [Key]
        public Guid RaffleEntryId { get; set; }
        [Required]
        public int EntryQuanity { get; set; }
        [Required]
        [ForeignKey(nameof(Raffle))]
        public Guid RaffleId { get; set; }
        [Required]
        [ForeignKey(nameof(Member))]
        public Guid MemberId { get; set; }
        [Required]
        public bool PrivateDonation { get; set; } = false;
        public DateTime EntryDate { get; set; } = DateTime.UtcNow;
        public DateTime? PaidOnDate { get; set; }
        
        public virtual Raffle Raffle { get; set; }
        public virtual Member Member { get; set; }
    }
}
