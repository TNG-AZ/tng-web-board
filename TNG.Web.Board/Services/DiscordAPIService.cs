using Microsoft.EntityFrameworkCore;
using TNG.Web.Board.Data;
using TNG.Web.Board.Data.DTOs;
using TNG.Web.Board.Data.Migrations;
using TNG.Web.Board.Data.ResponseModels;

namespace TNG.Web.Board.Services
{
    public static class DiscordAPIService
    {
        public static async Task<IResult> GetMemberInfoByDiscordId(IConfiguration configuration, ApplicationDbContext context, string apiKey, long discordId)
        {
            if (string.IsNullOrWhiteSpace(apiKey) || apiKey != configuration["DiscordAPIKey"])
            {
                return Results.Unauthorized();
            }

            var discordMembers = await context.Members
                .Include(m => m.MemberDiscords)
                .Include(m => m.Payments)
                .Include(m => m.Suspensions)
                .Include(m => m.Orientations)
                .Where(m => m.MemberDiscords.Any(d => d.DiscordId == discordId))
                .Select(m => new MemberInfoResponse()
                {
                    MemberId = m.Id,
                    SceneName = m.SceneName,
                    Suspended = m.Suspensions.Any(s => s.EndDate == null || s.EndDate > DateTime.UtcNow),
                    Status = (m.Payments.Any(p => p.PaidOn >= DateTime.UtcNow.AddYears(-1))
                        && m.Orientations.Any(o => o.DateReceived >= DateTime.UtcNow.AddYears(-1)))
                        ? MembershipStatusFlag.TNGMember
                        : MembershipStatusFlag.CommunityMember
                })
                .ToListAsync();

            

            return Results.Ok((discordMembers?.Any() ?? false) 
                ? discordMembers 
                : new List<MemberInfoResponse>() {  new() { Status = MembershipStatusFlag.NotFound} });
        }

        public static async Task<IResult> GetMemberByDiscordId(IConfiguration configuration, ApplicationDbContext context, string apiKey, long discordId)
        {
            if (string.IsNullOrWhiteSpace(apiKey) || apiKey != configuration["DiscordAPIKey"])
            {
                return Results.Unauthorized();
            }

            var discordMembers = context.Members
                .Include(m => m.MemberDiscords)
                .Where(m => m.MemberDiscords.Any(d => d.DiscordId == discordId));

            return Results.Ok(await discordMembers.Select(m => m.Id).ToListAsync());
        }

        public static async Task<IResult> GetAgedOutMembers(IConfiguration configuration, ApplicationDbContext context, string apiKey)
        {
            if (string.IsNullOrWhiteSpace(apiKey) || apiKey != configuration["DiscordAPIKey"])
            {
                return Results.Unauthorized();
            }
            var agedMembers = context.Members
                .Include(m => m.MemberDiscords)
                .Where(m => m.MemberDiscords.Any() && m.MemberType == Data.DTOs.MemberType.Member && m.Birthday < DateTime.UtcNow.AddYears(-40));

            return Results.Ok(await agedMembers.SelectMany(m => m.MemberDiscords).Select(m => m.DiscordId).ToListAsync());
        }

        public static async Task<IResult> GetLapsedMembers(IConfiguration configuration, ApplicationDbContext context, string apiKey)
        {
            if (string.IsNullOrWhiteSpace(apiKey) || apiKey != configuration["DiscordAPIKey"])
            {
                return Results.Unauthorized();
            }
            var agedMembers = context.Members
                .Include(m => m.MemberDiscords)
                .Include(m => m.Payments)
                .Include(m => m.Orientations)
                .Where(m => m.MemberDiscords.Any()
                    && (m.Orientations == null || !m.Orientations.Any(o => o.DateReceived > DateTime.UtcNow.AddYears(-1)))
                    || m.Payments == null || !m.Payments.Any(p => p.PaidOn > DateTime.UtcNow.AddYears(-1)));

            return Results.Ok(await agedMembers.SelectMany(m => m.MemberDiscords).Select(m => m.DiscordId).ToListAsync());
        }

        public static async Task<IResult> GetCurrentMembers(IConfiguration configuration, ApplicationDbContext context, string apiKey)
        {
            if (string.IsNullOrWhiteSpace(apiKey) || apiKey != configuration["DiscordAPIKey"])
            {
                return Results.Unauthorized();
            }
            var agedMembers = context.Members
                .Include(m => m.MemberDiscords)
                .Include(m => m.Payments)
                .Include(m => m.Orientations)
                .Where(m => m.MemberDiscords.Any() && m.Birthday >= DateTime.UtcNow.AddYears(-40)
                    && (m.MemberType == Data.DTOs.MemberType.Member || m.MemberType == Data.DTOs.MemberType.Honorary)
                    && m.Orientations.Any(o => o.DateReceived > DateTime.UtcNow.AddYears(-1))
                    && m.Payments.Any(p => p.PaidOn > DateTime.UtcNow.AddYears(-1)));

            return Results.Ok(await agedMembers.SelectMany(m => m.MemberDiscords).Select(m => m.DiscordId).ToListAsync());
        }

        public static async Task<IResult> GetAttendedMembers(IConfiguration configuration, ApplicationDbContext context, string apiKey, string calendarId)
        {
            if (string.IsNullOrWhiteSpace(apiKey) || apiKey != configuration["DiscordAPIKey"])
            {
                return Results.Unauthorized();
            }
            var attendedMembers = context.EventRsvps
                .Include(e => e.Member)
                .Include(e => e.Member.MemberDiscords)
                .Where(e => e.EventId == calendarId && e.Attended != null && e.Attended.Value);

            return Results.Ok(await attendedMembers.SelectMany(m => m.Member.MemberDiscords).Select(m => m.DiscordId).ToListAsync());
        }
    }
}
