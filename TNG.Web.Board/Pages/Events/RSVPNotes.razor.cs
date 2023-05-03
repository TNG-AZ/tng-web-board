using Blazored.Modal;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using TNG.Web.Board.Data;
using TNG.Web.Board.Data.DTOs;

namespace TNG.Web.Board.Pages.Events
{
    public partial class RSVPNotes
    {
#nullable disable
        [Parameter]
        public EventRsvp Rsvp { get; set; }

        [Inject]
        private ApplicationDbContext context { get; set; }

        [CascadingParameter]
        BlazoredModalInstance BlazoredModal { get; set; }
#nullable enable

        private async Task SaveNote()
        {
            try
            {
                if (string.IsNullOrEmpty(Rsvp.Notes?.Trim()))
                    Rsvp.Notes = null;

                context.EventRsvps.Attach(Rsvp);
                context.Entry(Rsvp).State = EntityState.Modified;

                await context.SaveChangesAsync();
                await BlazoredModal.CloseAsync();
            }
            catch { }
        }
    }
}
