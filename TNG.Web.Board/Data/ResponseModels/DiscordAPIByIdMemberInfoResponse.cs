namespace TNG.Web.Board.Data.ResponseModels
{
    public enum MembershipStatusFlag
    {
        CommunityMember,
        TNGMember,
        NotFound
    }

    public class MemberInfoResponse
    {
        public Guid? MemberId { get; set; }
        public string SceneName { get; set; }
        public bool Suspended { get; set; }
        public MembershipStatusFlag Status { get; set; }
    }
}
