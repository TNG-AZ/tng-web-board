using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TNG.Web.Board.Utilities;

namespace TNG.Web.Board.Data.DTOs
{
    public class Raffle
    {
        [Key]
        public Guid RaffleId { get; set; }
        [Required]
        public string Title { get; set; }
        [Required]
        public string Description { get; set; }
        [Required]
        public DateTime DrawingDate { get; set; } = DateTime.UtcNow.ToAZTime().AddDays(14);
        [Required]
        public int RaffleEntryCostCents { get; set; }
        public string? FundraiserCause { get; set; }
        public string? ImageUrl { get; set; }
        [ForeignKey(nameof(Winner))]
        public Guid? WinnerMemberId { get; set; }

        public virtual IEnumerable<RaffleEntry> Entries { get; set; }
        public virtual Member Winner { get; set; }
    }
}
