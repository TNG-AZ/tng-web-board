using System;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace TNG.Web.Functions
{
    public class ScheduledEventProcessor
    {
        private readonly ILogger _logger;

        public ScheduledEventProcessor(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<ScheduledEventProcessor>();
        }

        [Function(nameof(ScheduledEventProcessor))]
        public void Run([TimerTrigger("0 */5 * * * *")] TimerInfo myTimer)
        {
            _logger.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");
            
            if (myTimer.ScheduleStatus is not null)
            {
                _logger.LogInformation($"Next timer schedule at: {myTimer.ScheduleStatus.Next}");
            }
        }
    }
}
