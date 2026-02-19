using System;
using System.Globalization;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NuGet.Packaging;
using Square.TeamMembers;
using TNG.Web.Board.Data;
using TNG.Web.Board.Services;
using TNG.Web.Board.Utilities;
using TNG.Web.Board.Data.DTOs;

namespace TNG.Web.Functions
{
    public class ScheduledEventProcessor(ILoggerFactory loggerFactory, GoogleServices google, ApplicationDbContext context)
    {
        private readonly ILogger _logger = loggerFactory.CreateLogger<ScheduledEventProcessor>();

        [Function(nameof(ScheduledEventProcessor))]
        public async Task Run([TimerTrigger("*/5 * * * *")] TimerInfo myTimer)
        {
            await SendEmails();
        }

        private async Task SendEmails()
        {
            var now = DateTime.Now.ToAZTime();
            var emailsToSend = await context.EmailQueue
                .Where(e => e.SendAtDateTime <= now
                    && !(e.Success ?? false)
                    && e.AttemptCount <= 5)
                .ToListAsync();
            foreach (var e in emailsToSend)
            {
                try
                {
                    var recipients = (e.RecipientsCSV?.Split(",") ?? []).ToList();
                    if (e.EventId != null && e.EventRecipientFilter != null)
                    {
                        recipients.AddRange(e.EventRecipientFilter!.Value switch
                        {
                            EventEmailRecipientFilter.All => await context
                                .EventRsvps
                                .Where(r =>
                                    r.EventId == e.EventId
                                    && r.VoidedDate == null)
                                .Select(r => r.Member.EmailAddress)
                                .ToListAsync(),
                            EventEmailRecipientFilter.Attended => await context
                                .EventRsvps
                                .Where(r =>
                                    r.EventId == e.EventId
                                    && r.VoidedDate == null
                                    && r.Attended != null
                                    && r.Attended!.Value)
                                .Select(r => r.Member.EmailAddress)
                                .ToListAsync(),
                            EventEmailRecipientFilter.Paid => await context
                                .EventRsvps
                                .Where(r =>
                                    r.EventId == e.EventId
                                    && r.VoidedDate == null
                                    && ((r.Paid != null && r.Paid!.Value)
                                        || r.Member.Invoices.Any(i => i.EventId == e.EventId && i.PaidOnDate != null)))
                                .Select(r => r.Member.EmailAddress)
                                .ToListAsync(),
                            _ => throw new NotImplementedException($"Not implemented for filter type {e.EventRecipientFilter}")
                        });
                    }
                    if (recipients.Count != 0 && !string.IsNullOrWhiteSpace(e.Body) && !string.IsNullOrWhiteSpace(e.Subject))
                        await google.EmailListAsync(recipients, e.Subject, e.Body);
                    e.Success = true;
                }
                catch(Exception ex)
                {
                    _logger.LogError(ex, "failed to send email {EmailId}", e.Id);
                    e.Success = false;
                }
                finally
                {
                    e.AttemptCount++;
                    await context.SaveChangesAsync();
                }
            }
        }
    }
}
