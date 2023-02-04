using Microsoft.AspNetCore.Components;
using TNG.Web.Board.Data;

namespace TNG.Web.Board.Pages.Membership
{
    public partial class ViewMembers
    {
#nullable disable
        [Inject]
        private ApplicationDbContext _context { get; set; }
#nullable enable
    }
}
