using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using TNG.Web.Board.Data.RequestModels;
using TNG.Web.Board.Services;

namespace TNG.Web.Board.Pages.Admin
{
    [Authorize(Roles = "Boardmember")]
    public partial class LinkManager
    {
#nullable disable
        [Inject]
        private LinksService links { get; set; }

#nullable enable

        private string? linkRoute { get; set; }
        private CreateLinkRequest linkRequest { get; set; } = new();
        private string createLinkOutput { get; set; }
        private string? deleteRoute { get; set; }
        private bool? deleteSuccess { get; set; }

        private async Task CreateLink()
        {
            if (!string.IsNullOrWhiteSpace(linkRoute))
            {
                createLinkOutput = await links.CreateUrl(linkRoute, linkRequest);
                linkRoute = null;
                linkRequest = new();
            }
            else
            {
                createLinkOutput = "Please enter a valid route";
            }
        }

        private async Task DeleteLink()
        {
            if (!string.IsNullOrWhiteSpace(deleteRoute))
            {
                deleteSuccess = await links.RemoveUrl(deleteRoute);
                deleteRoute = null;
            }
            else
            {
                deleteSuccess = null;
            }
        }
    }
}
