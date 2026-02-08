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
                var newEvent = await Calendar.Events.Get(CalendarId, eventId).ExecuteAsync();
                cache.Set(key, newEvent);
                return newEvent;
            }
            catch { }
            return null;
        }

        public async Task<IEnumerable<Event>> GetEvents(DateTime startTime, DateTime endTime)
        {
            var range = Enumerable.Range(0, 1 + endTime.Subtract(startTime).Days)
                .Select(offset => startTime.AddDays(offset));
            var events = new List<Event>();

            if (range.Count() < 2)
            {
                return Enumerable.Empty<Event>();
            }

            var queryDates = new List<DateTime>();
            foreach (var date in range)
            {
                var key = $"event:{CalendarId}:{date:MM/dd/yyyy}";
                if (cache.TryGetValue(key, out IEnumerable<Event>? cachedEvents))
                {
                    if (cachedEvents?.Any() ?? false)
                    {
                        events.AddRange(cachedEvents);
                    }
                }
                else
                {
                    queryDates.Add(date);
                }
            }
            if (queryDates.Count == 1)
            {
                var date = queryDates.First();
                queryDates = new List<DateTime>()
                {
                    date.AddDays(-1),
                    date,
                    date.AddDays(1)
                };
            }
            if (queryDates.Count() == 0)
            {
                return events;
            }

            var queryDateRanges = new List<Tuple<DateTime, DateTime>>();
            var startDate = queryDates.FirstOrDefault();
            var endDate = queryDates.Skip(1).FirstOrDefault();
            var previousDate = queryDates.FirstOrDefault();
            foreach (var date in queryDates.Skip(1))
            {
                if (date == previousDate.AddDays(1))
                {
                    endDate = date;
                }
                else
                {
                    queryDateRanges.Add(new(startDate, endDate));
                    startDate = date;
                    endDate = date.AddDays(1);
                }
                previousDate = date;
            }
            if(endDate == queryDates.LastOrDefault())
            {
                queryDateRanges.Add(new(startDate, queryDates.LastOrDefault()));
            }

            var newEvents = new ConcurrentBag<Event>();

            var request = new BatchRequest(Calendar);
            foreach (var dr in queryDateRanges)
            {
                var call = Calendar.Events.List(CalendarId);
                call.SingleEvents = true;
                call.TimeMinDateTimeOffset = dr.Item1;
                call.TimeMaxDateTimeOffset = dr.Item2;
                request.Queue<Events>(call,
                    (content, error, i, message) =>
                    {
                        foreach (var e in content.Items)
                        {
                            newEvents.Add(e);
                        }
                    });
            }
               
            await request.ExecuteAsync();

            var emptyDates = queryDates.Where(d => !newEvents.Select(e => e.Start.DateTimeDateTimeOffset?.ToString("MM/dd/yyyy")).Contains(d.ToString("MM/dd/yyyy")));

            if (newEvents?.Any() ?? false)
            {
                events.AddRange(newEvents);
            }
            CacheEvents(emptyDates, newEvents).FireAndForget();
            return events.OrderBy(e => e.Start.DateTimeDateTimeOffset);
        }

        public async Task CacheEvents(IEnumerable<DateTime> emptyDates, IEnumerable<Event> events)
        {
            var groupedEvents = events.GroupBy(e => e.Start.DateTimeDateTimeOffset?.ToString("MM/dd/yyyy"));
            foreach (var eventsByDate in groupedEvents.Where(e => e.Key != null))
            {
                var key = $"event:{CalendarId}:{eventsByDate.Key}";
                cache.Set(key, eventsByDate.Select(e => e));
            }
            foreach (var date in emptyDates.Take(emptyDates.Count() - 1))
            {
                var key = $"event:{CalendarId}:{date:MM/dd/yyyy}";
                cache.Set(key, Enumerable.Empty<Event>());
            }
        }

        public void ClearEventCache()
        {
            cache.Dispose();
            cache = new MemoryCache(new MemoryCacheOptions());
        }
    }
}
