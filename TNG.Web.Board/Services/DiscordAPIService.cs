using Azure.Identity;
using Microsoft.EntityFrameworkCore;
using TNG.Web.Board.Data;
using TNG.Web.Board.Data.DTOs;

namespace TNG.Web.Board.Services
{
    public static class DiscordAPIService
    {

        public static IResult GetAgedOutMembers(IConfiguration configuration, ApplicationDbContext context, string apiKey)
        {
            if (string.IsNullOrWhiteSpace(apiKey) || apiKey != configuration["DiscordAPIKey"])
            {
                return Results.Unauthorized();
            }
            var agedMembers = context.Members
                .Include(m => m.MemberDiscords)
                .Where(m => m.MemberDiscords.Any() && m.MemberType == MemberType.Member && m.Birthday < DateTime.UtcNow.AddYears(-40));

            return Results.Ok(agedMembers.SelectMany(m => m.MemberDiscords).Select(m => m.DiscordId));
        }

        public static IResult GetLapsedMembers(IConfiguration configuration, ApplicationDbContext context, string apiKey)
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

            return Results.Ok(agedMembers.SelectMany(m => m.MemberDiscords).Select(m => m.DiscordId));
        }

        public static IResult GetCurrentMembers(IConfiguration configuration, ApplicationDbContext context, string apiKey)
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
                    && (m.MemberType == MemberType.Member || m.MemberType == MemberType.Honorary)
                    && m.Orientations.Any(o => o.DateReceived > DateTime.UtcNow.AddYears(-1))
                    && m.Payments.Any(p => p.PaidOn > DateTime.UtcNow.AddYears(-1)));

            return Results.Ok(agedMembers.SelectMany(m => m.MemberDiscords).Select(m => m.DiscordId));
        }

        public static IResult GetAttendedMembers(IConfiguration configuration, ApplicationDbContext context, string apiKey, string calendarId)
        {
            if (string.IsNullOrWhiteSpace(apiKey) || apiKey != configuration["DiscordAPIKey"])
            {
                return Results.Unauthorized();
            }
            var attendedMembers = context.EventRsvps
                .Include(e => e.Member)
                .Include(e => e.Member.MemberDiscords)
                .Where(e => e.EventId == calendarId && e.Attended != null && e.Attended.Value);

            return Results.Ok(attendedMembers.SelectMany(m => m.Member.MemberDiscords).Select(m => m.DiscordId));
        }
}
