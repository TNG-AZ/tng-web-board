using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Policy;

namespace TNG.Web.Board.Data.DTOs
{
    public class VolunteerEventSlot
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey(nameof(Position))]
        public int PositionId { get; set; }
        public string EventId { get; set; }

        public TimeOnly? StartTime { get; set; }
        public int? DurationMinutes { get; set; }
        public int? NeededCount { get; set; }

        public int Priority { get; set; }


        public virtual VolunteerPosition Position { get; set; }

        public virtual ICollection<VolunteerSlotMember> SlotMembers { get; set; }
    }
}
