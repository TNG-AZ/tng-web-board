using Google.Apis.Calendar.v3.Data;
using iText.IO.Image;
using iText.IO.Source;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.JSInterop;
using Polly;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.RegularExpressions;
using TNG.Web.Board.Data;
using TNG.Web.Board.Data.DTOs;
using TNG.Web.Board.Services;
using TNG.Web.Board.Utilities;

namespace TNG.Web.Board.Pages.Events.Venue.TheCastingHouse
{
    public partial class LiabilityWaiver
    {
        [Parameter]
        public string? eventId { get; set; }
#nullable disable
        [Inject]
        private ApplicationDbContext context { get; set; }
        [Inject]
        public IConfiguration Configuration { get; set; }
        [Inject]
        private GoogleServices google { get; set; }
        [Inject]
        private IJSRuntime js { get; set; }
        [Inject]
        private AuthUtilities auth { get; set; }
        [Inject]
        private NavigationManager navigation { get; set; }
#nullable enable

        private string agreementHtml = """
<h1>CASTING HOUSE AGREEMENT AND RELEASE OF LIABILITY</h1>
<h2>Voluntary Participation</h2>
    1. I declare that I am at least 18 years of age and of sound mind and I am freely and voluntarily choosing to attend and participate in this event and to view and/or participate in activities that I know are adult orientated, sexually explicit, and that involve acts of domination, submission, bondage, discipline, sadism, masochism, and other explicit and extreme sexual fetishes and activities including, but not limited to, spanking, paddling, whipping, wax play, piercing, fire play, knife or edge play, suspension, fisting or other oral, vaginal and anal penetration.  I understand that these activities involve certain risks including, but not limited to, the possible negligent or reckless conduct of other participants.
<h2>Assumption of Risk</h2>
    2. I am aware that these activities are considered extremely hazardous activities.  I am voluntarily participating in these activities with full knowledge of the dangers involved, and I accept and assume full responsibility for any and all risks of property damage, personal injury, or death.  By signing this document I verify this statement.
<h2>Release</h2>
    3. As consideration for being permitted to enter the event, to view and participate in these activities, and to use the facilities and equipment being provided, I agree that neither I nor anyone through me or on my behalf will make any claim against or sue the Casting House, for any injury or damage suffered by me at any time during my presence at the event or as a result of my participation in any activities at the event.  I hereby release, waive, and forever discharge them, their heirs, administrators, executors and assigns, from any and all claims, demands, actions or rights of action, of whatever kind or nature, either in law or in equity, arising from , or by reason of, my attendance at, or participation in, the event or activities.
<h2>Knowing and Voluntary Execution</h2>
    4. I HAVE CAREFULLY READ THIS DOCUMENT AND I FULLY UNDERSTAND ITS CONTENTS.  I AM AWARE THAT THIS IS A RELEASE OF LIABILITY AND A CONTRACTURE BETWEEN ME AND THE CASTING HOUSE AND THAT I AM GIVING UP MANY LEGAL RIGHTS AND REMEDIES BY SIGNING IT.
""";

        private Member? GetMember()
        {
            var name = auth.GetIdentity().Result?.Name ?? string.Empty;
            return context.Members.Include(m => m.Events).FirstOrDefault(m => EF.Functions.Like(m.EmailAddress, name));
        }
        private Event? _event { get; set; }
        private Event? CalendarEvent
            => _event ??= google.GetEvent(Configuration["CalendarId"]!, eventId!);
        private Member? _member { get; set; }
        private Member? Member
            => _member ??= GetMember();

        private WaiverForm formModel = new WaiverForm();

        public class WaiverForm
        {
            [Required]
            [IsToday(ErrorMessage = "Must be set to today in AZ time")]
            public DateTime? TodaysDate { get; set; }
            [Required]
            public string LegalName { get; set; }
            [Required]
            public string SceneName { get; set; }
            [MinLength(1, ErrorMessage = "Signature cannot be blank")]
            public byte[] Signature { get; set; } = Array.Empty<byte>();
        }

        protected override async Task OnInitializedAsync()
        {
            if (string.IsNullOrWhiteSpace(eventId))
            {
                navigation.NavigateTo("/", true);
            }

            var loggedIn = auth.GetIdentity().Result?.Name != null;
            if (Member is null)
                navigation.NavigateTo(loggedIn ? "/members/new" : "/Identity/Account/Login", true);
        }

        protected override async Task OnAfterRenderAsync(bool _)
        {
            if (CalendarEvent == null)
            {
                await js.InvokeVoidAsync("alert", "Invalid calendar event");
                navigation.NavigateTo("/", true);
            }
        }

        public async Task GeneratePdf()
        {
            
            using var bytes = new ByteArrayOutputStream();
            using var writer = new PdfWriter(bytes);
            using var document = new PdfDocument(writer);
            using var pdf = new Document(document);

            if (CalendarEvent is not null)
            {
                pdf.Add(new Paragraph($"Event: {CalendarEvent.Summary}"));
                if (CalendarEvent.Start is not null)
                {
                    pdf.Add(new Paragraph($"Date: {CalendarEvent.Start.DateTime.ToAZTime()?.ToString("MM/dd/yyyy")}"));
                }
            }
            pdf.Add(new Paragraph(Regex.Replace(agreementHtml, "<.*?>", string.Empty)).SetFontSize(10));
            pdf.Add(new Paragraph($"Legal Name: {formModel.LegalName}"));
            pdf.Add(new Paragraph($"Scene Name: {formModel.SceneName}"));
            var image = Convert.FromBase64String(Encoding.UTF8.GetString(formModel.Signature).Replace("data:image/png;base64,", ""));
            var pdfImage = ImageDataFactory.CreatePng(image);
            pdf.Add(new Image(pdfImage));
            pdf.Add(new Paragraph($"Todays Date: {formModel.TodaysDate.Value.ToString("MM/dd/yyyy")}"));

            pdf.Close();
            var pdfBytes = bytes.ToArray();
            await context.Signatures.AddAsync(new()
            {
                CreatedDatetime = DateTime.UtcNow,
                MemberId = Member.Id,
                EventId = eventId,
                LegalName = formModel.LegalName,
                SceneName = formModel.SceneName,
                SignatureImage = image,
                SignatureDatetime = formModel.TodaysDate.Value,
                SignedForm = pdfBytes

            });
            await context.SaveChangesAsync();
            using var streamRef = new DotNetStreamReference(stream: new MemoryStream(pdfBytes));

            await js.InvokeVoidAsync("downloadFileFromStream", "liabilityForm.pdf", streamRef);

            var confirm = await js.InvokeAsync<bool>("confirm", "Successfully signed. Leave page?");

            if (confirm)
                navigation.NavigateTo("/");
        }
    }
}
