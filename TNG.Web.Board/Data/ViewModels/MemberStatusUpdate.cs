using TNG.Web.Board.Data.DTOs;

namespace TNG.Web.Board.Data.ViewModels
{
    public class MemberStatusUpdate
    {
        public required Guid MemberId { get; set; }
        public required MemberType MemberType { get; set; }
        public required string SceneName { get; set; }
        public DateTime? LastDues { get; set; }
        public DateTime? LastOrientation { get; set; }
        public bool? ManuallyPaid { get; set; }
    }
}
