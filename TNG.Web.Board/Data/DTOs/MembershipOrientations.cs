#nullable disable

using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TNG.Web.Board.Data.DTOs
{
    [Index(nameof(DateReceived), AllDescending = true)]
    public class MembershipOrientations
    {
        [Key]
        public Guid Id { get; set; }
        [Required]
        [ForeignKey(nameof(Member))]
        public Guid MemberId { get; set; }
        [Required]
        public DateTime DateReceived { get; set; }

        public virtual Member Member { get; set; }
    }
}
