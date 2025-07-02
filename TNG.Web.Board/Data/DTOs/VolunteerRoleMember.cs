using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TNG.Web.Board.Data.DTOs
{
    public class VolunteerRoleMember
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey(nameof(Member))]
        public Guid MemberId { get; set; }
        [ForeignKey(nameof(Role))]
        public int RoleId { get; set; }

        public DateTime RequestedDate { get; set; }
        public bool? Approval { get; set; }


        public virtual Member Member { get; set; }
        public virtual VolunteerPositionRole Role { get; set; }

    }
}
