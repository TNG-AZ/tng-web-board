using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.EntityFrameworkCore;
using TNG.Web.Board.Data;
using TNG.Web.Board.Data.DTOs;
using TNG.Web.Board.Utilities;

namespace TNG.Web.Board.Pages.Fetishes
{
    public partial class UserFetishManage
    {
#nullable disable
        [Inject]
        private ApplicationDbContext _context { get; set; }

        [Inject]
        private NavigationManager navigation { get; set; }
        [Inject]
        private AuthUtilities auth { get; set; }
#nullable enable

        private Member? _member { get; set; }
        private Member? Member

        {
            get
            {
                if ((_member ??= GetMember()) is not null)
                {
                    return _member;
                }
                navigation.NavigateTo("/members/new");
                return null;
            }
        }

        private Member? GetMember()
            => _context.Members
                .Include(m => m.MemberFetishes)
                .ThenInclude(mf => mf.Fetish)
                .FirstOrDefault(m => EF.Functions.Like(m.EmailAddress, auth.GetIdentity().Result.Name ?? string.Empty));

        private List<Fetish>? _fetishes { get; set; }
        private List<Fetish> Fetishes
            => _fetishes ??= _context.Fetishes.ToList();

        private EditContext editContext = new(new object());

        private string NewFetishName { get; set; }
        private FetishRoleEnum? NewFetishRole { get; set; }
        private bool? NewFetishWillingToTeach { get; set; }

        private void NewFetishRoleChange(ChangeEventArgs e)
        {
            if (string.IsNullOrEmpty(e.Value?.ToString()))
                NewFetishRole = null;
            else
                NewFetishRole = int.TryParse(e.Value.ToString(), out var roleId) ? (FetishRoleEnum)roleId : null;
        }
        private async Task AddFetishToMember()
        {
            if (!string.IsNullOrEmpty(NewFetishName)
                && !_context.MembersFetishes.Where(mf => mf.MemberId == Member.Id).Select(mf => mf.Fetish).Any(f => EF.Functions.Like(f.Name, NewFetishName)))
            {
                var fetish = await _context.Fetishes.FirstOrDefaultAsync(f => EF.Functions.Like(f.Name, NewFetishName));
                if (fetish is null)
                {
                    fetish = _context.Add(new Fetish { Name = NewFetishName }).Entity;
                    await _context.SaveChangesAsync();
                }
                _context.Add(new MemberFetish { MemberId = Member.Id, FetishId = fetish.Id, Role = NewFetishRole, WillingToTeach = NewFetishWillingToTeach });
                await _context.SaveChangesAsync();

                NewFetishName = string.Empty;
                NewFetishWillingToTeach = false;
                StateHasChanged();
            }
        }

        private async Task UnlinkFetish(Guid memberFetishId)
        {
            _context.Remove(_context.MembersFetishes.FirstOrDefault(mf => mf.Id == memberFetishId));
            await _context.SaveChangesAsync();
            StateHasChanged();
        }
    }
}
