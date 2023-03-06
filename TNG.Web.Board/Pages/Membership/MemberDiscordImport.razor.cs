using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using TNG.Web.Board.Data.DTOs;
using TNG.Web.Board.Data.ViewModels;
using TNG.Web.Board.Data;
using Microsoft.AspNetCore.Authorization;

namespace TNG.Web.Board.Pages.Membership
{
    [Authorize(Roles = "Boardmember")]
    public partial class MemberDiscordImport
    {
        [Inject]
        private ApplicationDbContext _context { get; set; }
#nullable enable

        private string ImportSeparator { get; set; } = ",";
        private bool? ParseSuccess { get; set; }

        private long? SuccessfulCommits { get; set; }
        private long? FailedCommits { get; set; }


        private async void LoadFile(InputFileChangeEventArgs e)
        {
            SuccessfulCommits = 0;
            FailedCommits = 0;
            try
            {
                if (e.File.Size > 0)
                {
                    using var sr = new StreamReader(e.File.OpenReadStream());

                    string? data;

                    while ((data = await sr.ReadLineAsync()) != null)
                    {
                        var mapping = data.Split(ImportSeparator.Trim(), StringSplitOptions.TrimEntries);
                        if (mapping.Length == 2 && !String.IsNullOrEmpty(mapping[1]))
                        {
                            var member = await _context.Members
                                .Include(m => m.MemberDiscords)
                                .FirstOrDefaultAsync(m => EF.Functions.Like(m.EmailAddress, mapping[1]));

                            if (member is not null 
                                && long.TryParse(mapping[0], out var discordId)
                                && !(member.MemberDiscords?.Any(m => m.DiscordId == discordId) ?? false))
                            {
                                _context.MembersDiscordIntegrations.Add(new() {  DiscordId = discordId, MemberId = member.Id });
                                await _context.SaveChangesAsync();
                                SuccessfulCommits++;
                            }
                        }
                    }
                    ParseSuccess = true;
                }
            }
            catch
            {
                FailedCommits++;
                ParseSuccess = false;
            }

            StateHasChanged();
        }
    }
}
