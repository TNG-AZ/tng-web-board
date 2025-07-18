using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore;
using TNG.Web.Board.Data;
using TNG.Web.Board.Data.DTOs;
using TNG.Web.Board.Data.ViewModels;
using Microsoft.AspNetCore.Authorization;

namespace TNG.Web.Board.Pages.Membership
{
    [Authorize(Roles = "Boardmember")]
    public partial class MemberImport
    {
#nullable disable
        [Inject]
        private ApplicationDbContext _context { get; set; }
#nullable enable

        private string ImportSeparator { get; set; } = ",";
        private bool? ParseSuccess { get; set; }

        private long? SuccessfulCommits { get; set; }

        private List<MemberPreview>? Members { get; set; }

        private async void LoadFile(InputFileChangeEventArgs e)
        {
            Members = new();
            try
            {
                if (e.File.Size > 0)
                {
                    using var sr = new StreamReader(e.File.OpenReadStream());

                    string? data;

                    var allMemberData = new List<MemberPreview>();
                    //clear header
                    await sr.ReadLineAsync();

                    while ((data = await sr.ReadLineAsync()) != null)
                    {
                        var member = data.Split(ImportSeparator.Trim(), StringSplitOptions.TrimEntries);
                        allMemberData.Add(new()
                        {
                            Timestamp = DateTime.Parse(member[0]),
                            MemberType = member[1][0].ToString().ToLower() switch
                            {
                                "n" => MemberType.Member,
                                "g" => MemberType.Guest,
                                "h" => MemberType.Honorary,
                                _ => null
                            },
                            LegalName = member[3],
                            SceneName = member[4],
                            Birthday = DateTime.TryParse(member[5], out var birthday) ? birthday : null,
                            Email = member[6].ToString().ToLower(),
                            MemberSince = DateTime.TryParse(member[10], out var lastPaid) ? lastPaid.AddYears(-1) : null
                        }
                        );
                    }

                    var emailAddresses = allMemberData.Where(e => !string.IsNullOrEmpty(e.Email)).GroupBy(m => m.Email);

                    Members = emailAddresses.Select(entries =>
                        new MemberPreview()
                        {
                            MemberType = entries.OrderByDescending(g => g.Timestamp).FirstOrDefault()?.MemberType,
                            LegalName = entries.OrderByDescending(g => g.Timestamp).FirstOrDefault(e => !string.IsNullOrEmpty(e.LegalName))?.LegalName,
                            SceneName = entries.OrderByDescending(g => g.Timestamp).FirstOrDefault(e => !string.IsNullOrEmpty(e.SceneName))?.SceneName,
                            Birthday = entries.Max(e => e.Birthday),
                            Email = entries.OrderByDescending(g => g.Timestamp).FirstOrDefault(e => !string.IsNullOrEmpty(e.Email))?.Email,
                            MemberSince = entries.Max(e => e.MemberSince)
                        }).ToList();

                    ParseSuccess = true;
                }
            }
            catch
            {
                ParseSuccess = false;
            }

            StateHasChanged();
        }

        private async void CommitMembers()
        {
            if (!Members?.Any() ?? false)
            {
                SuccessfulCommits = -1;
                StateHasChanged();
                return;
            }

            SuccessfulCommits = 0;

            var membersLeft = Members!.Select(m => m).ToList();

            foreach(var member in Members!)
            {
                try
                {
                    var newMemberEntity = _context.Add(new Member
                    {
                        LegalName = member.LegalName,
                        SceneName = member.SceneName,
                        Birthday = member.Birthday!.Value,
                        EmailAddress = member.Email,
                        HasAttendedSocial = member.MemberSince.HasValue
                    }).Entity;

                    await _context.SaveChangesAsync();

                    if(member.MemberSince.HasValue)
                    {
                        _context.Add(new MembershipOrientation
                        {
                            MemberId = newMemberEntity.Id,
                            DateReceived = member.MemberSince.Value
                        });
                        _context.Add(new MembershipPayment
                        {
                            MemberId = newMemberEntity.Id,
                            PaidOn = member.MemberSince.Value
                        });

                        await _context.SaveChangesAsync();
                    }

                    membersLeft.RemoveAll(m => m.Email.Equals(member.Email, StringComparison.OrdinalIgnoreCase));

                    SuccessfulCommits++;
                }
                catch { 
                    Rollback(_context);
                }
                
            }

            Members = membersLeft;

            StateHasChanged();
        }

        public static void Rollback(DbContext context)
        {
            foreach (var entry in context.ChangeTracker.Entries())
            {
                switch (entry.State)
                {
                    case EntityState.Modified:
                        entry.State = EntityState.Unchanged;
                        break;
                    case EntityState.Added:
                        entry.State = EntityState.Detached;
                        break;
                    case EntityState.Deleted:
                        entry.Reload();
                        break;
                    default: break;
                }
            }
        }
    }
}
