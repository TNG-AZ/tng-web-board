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
        public IEnumerable<long> DiscordIds { get; set; }
        public Guid? MemberId { get; set; }
        public string SceneName { get; set; }
        public bool Suspended { get; set; }
        public MembershipStatusFlag Status { get; set; }
    }

    public class BatchMemberInfoResponse
    {
        public long DiscordId { get; set; }
        public IEnumerable<MemberInfoResponse> Records { get; set; }
    }
}
