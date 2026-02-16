using System.ComponentModel.DataAnnotations;

namespace TNG.Web.Board.Data.DTOs
{
    public enum EventEmailRecipientFilter
    {
        All,
        Paid,
        Attended
    }
    public enum EmailDomain
    {
        General,
        Event
    }
    public class ScheduledEmail
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public DateTime SendAtDateTime { get; set; }
        public bool? Success { get; set; }
        public int AttemptCount { get; set; } = 0;
        [Required]
        public required string Subject { get; set; }
        [Required]
        public required string Body { get; set; }
        public string? RecipientsCSV { get; set; }
        public string? EventId { get; set; }
        public EventEmailRecipientFilter? EventRecipientFilter { get; set; }
        public EmailDomain? EmailDomain { get; set; }
    }
}
