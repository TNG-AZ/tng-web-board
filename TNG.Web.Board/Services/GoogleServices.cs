using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Gmail.v1;
using Google.Apis.Gmail.v1.Data;
using Google.Apis.Requests;
using Google.Apis.Services;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;
using System.Collections.Concurrent;
using TNG.Web.Board.Utilities;

namespace TNG.Web.Board.Services
{
    public class GoogleServices : IEmailSender
    {
        public readonly CalendarService Calendar;
        public readonly GmailService Gmail;
        private IMemoryCache cache;
        private readonly string? CalendarId;

        public GoogleServices(IConfiguration Configuration)
        {
            cache= new MemoryCache(new MemoryCacheOptions());
            CalendarId = Configuration["CalendarId"];

            string[] Scopes = { CalendarService.Scope.Calendar, GmailService.Scope.GmailSend, GmailService.Scope.MailGoogleCom };
            using var stream = new FileStream(Configuration["Google_API_PRIVATE_KEYFILE"]!, FileMode.Open, FileAccess.Read);
            var credential = CredentialFactory.FromStream<ServiceAccountCredential>(stream)
                .ToGoogleCredential()
                .CreateScoped(Scopes)
                .CreateWithUser("board@tngaz.org");

            var baseClient = new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "TNG WWW Calendar Integration",
            };

            Calendar = new CalendarService(baseClient);

            Gmail = new GmailService(baseClient);
        }

        private static string GetEmailRaw(string toEmail, string subject, string body)
            => Base64UrlEncoder.Encode($"To: {toEmail}\r\nSubject: {subject}\r\nContent-Type: text/html;charset=utf-8\r\n\r\n{body}");

        private static string GetEmailBccRaw(string toEmail, string subject, string body)
            => Base64UrlEncoder.Encode($"Bcc: {toEmail}\r\nSubject: {subject}\r\nContent-Type: text/html;charset=utf-8\r\n\r\n{body}");

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            Gmail.Users.Messages.Send(new Message { Raw = GetEmailRaw(email, subject, htmlMessage) }, "me").Execute();
        }

        public async Task EmailListAsync(IEnumerable<string> emails, string subject, string body)
        {
            for (int i = 0; i < emails.Count(); i += 100)
            {
                var batchEmails = emails.Skip(i).Take(100);
                Gmail.Users.Messages.Send(new Message { Raw = GetEmailBccRaw(string.Join(", ", batchEmails), subject, body) }, "me").Execute();
            }
            
        }

        public async Task<Event?> GetEvent(string eventId)
        {
            try
            {
                var key = $"event:{CalendarId}:{eventId}";
                if (cache.TryGetValue(key, out Event? cachedEvent))
                {
                    return cachedEvent;
                }
                var newEvent = Calendar.Events.Get(CalendarId, eventId).Execute();
                cache.Set(key, newEvent);
                return newEvent;
            }
            catch { }
            return null;
        }

        public async Task<IEnumerable<Event>> GetEvents(DateTime startTime, DateTime endTime)
        {
            var call = Calendar.Events.List(CalendarId);
                call.SingleEvents = true;
                call.TimeMinDateTimeOffset = startTime;
                call.TimeMaxDateTimeOffset = endTime;

            var events = call.Execute();
            return events.Items.OrderBy(e => e.Start.DateTimeDateTimeOffset);
        }

        public void ClearEventCache()
        {
            cache.Dispose();
            cache = new MemoryCache(new MemoryCacheOptions());
        }
    }
}
