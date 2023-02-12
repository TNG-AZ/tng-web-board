using System.Collections;
using TNG.Web.Board.Data.DTOs;

namespace TNG.Web.Board.Data.ViewModels
{
    public class MemberPreview
    {

        public MemberType? MemberType { get; set; }
        public string? LegalName { get; set; }
        public string? SceneName { get; set; }
        public DateTime? Birthday { get; set; }
        public string? Email { get; set; }
        public bool? Covid19VaxProofReceived { get; set; }
        public DateTime? MemberSince { get; set; }
        public DateTime? Timestamp { get; set; } = null;
    } 
}
