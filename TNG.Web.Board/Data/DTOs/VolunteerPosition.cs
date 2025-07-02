using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TNG.Web.Board.Data.DTOs
{
    public class VolunteerPosition
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        [ForeignKey(nameof(RequiredRole))]
        public int? RequiredRoleId { get; set; }
        public bool? RequireRoleApproval { get; set; }


        public virtual VolunteerPositionRole? RequiredRole { get; set; }

        public virtual ICollection<VolunteerEventSlot> EventSlots { get; set; }
    }
}
