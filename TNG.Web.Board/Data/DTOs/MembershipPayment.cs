#nullable disable

using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TNG.Web.Board.Data.DTOs
{

    [Index(nameof(PaidOn), AllDescending = true)]
    public class MembershipPayment
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        [Required]
        [ForeignKey(nameof(Member))]
        public Guid MemberId { get; set; }
        [Required]
        public DateTime PaidOn { get; set; }

        public virtual Member Member { get; set; }
    }
}
