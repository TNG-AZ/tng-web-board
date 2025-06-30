using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TNG.Web.Board.Data.DTOs
{
    public class VolunteerSlotMember
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey(nameof(Slot))]
        public int SlotId { get; set; }
        [ForeignKey(nameof(Member))]
        public Guid MemberId { get; set; }

        public DateTime RequestedDateTime { get; set; }
        public bool? Approval { get; set; }

        public virtual VolunteerEventSlot Slot { get; set; }
        public virtual Member Member { get; set; }
    }
}
