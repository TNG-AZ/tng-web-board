namespace TNG.Web.Board.Data.RequestModels
{
    public class CreateLinkRequest
    {
        public string? Title { get; set; }
        public string? Image { get; set; }
        public string? Description { get; set; }
        public string? Redirect { get; set; }
        public string? Url { get; set; }
        public string? Type { get; set; }
    }
}
