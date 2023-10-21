using Blazored.Modal;
using Blazored.Modal.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using Polly;
using TNG.Web.Board.Data;
using TNG.Web.Board.Data.DTOs;
using TNG.Web.Board.Data.Migrations;

namespace TNG.Web.Board.Pages.Raffles
{
    public partial class DrawRaffleWinnerModal
    {
#nullable disable
        [Parameter]
        public Raffle Raffle { get; set; }

        [Inject]
        private ApplicationDbContext context { get; set; }
        [CascadingParameter]
        private BlazoredModalInstance Modal { get; set; }
#nullable enable
        private List<DrawingEntry>? _drawingEntries { get; set; }
        private IEnumerable<DrawingEntry>? DrawingEntries
        {
            get
            {
                if (_drawingEntries == null)
                {
                    _drawingEntries = new();
                    var paidEntries = Raffle.Entries.Where(e => e.PaidOnDate != null);
                    foreach (var entry in paidEntries)
                    {
                        _drawingEntries.AddRange(Enumerable.Range(0, entry.EntryQuanity).Select(_ => new DrawingEntry() { MemberId = entry.MemberId, SceneName = entry.PrivateDonation ? "Private Donation" : entry.Member.SceneName }));
                    }
                }
                return _drawingEntries;
            }
        }

        private bool QuickPick { get; set; }

        private Random rng = new Random();

        private void Shuffle()
            => _drawingEntries = _drawingEntries?.OrderBy(e => rng.Next()).ToList();

        private void MoveSelected()
        {
            if (_drawingEntries != null)
            {
                var selectedOld = _drawingEntries.FirstOrDefault(e => e.Selected);
                if (selectedOld != null)
                    selectedOld.Selected = false;
                var selectedNew = _drawingEntries[rng.Next(_drawingEntries.Count())];
                selectedNew.Selected = true;

            }
        }
                        
        private async Task SaveRaffle()
        {
            if (!(_drawingEntries?.Any() ?? false))
            {
                return;
            }
            if (QuickPick)
            {
                MoveSelected();
                Raffle.WinnerMemberId = DrawingEntries.FirstOrDefault(e => e.Selected)!.MemberId;
            }
            else
            {
                MoveSelected();
                await Task.Delay(1000);

                var totalSteps = rng.Next(7,11);
                var currentStep = 0;
                while (currentStep < totalSteps)
                {
                    var action = rng.Next(2);
                    if (action == 0)
                    {
                        Shuffle();
                        MoveSelected();
                    }
                    else
                    {
                        MoveSelected();
                    }
                    currentStep++;
                    StateHasChanged();
                    await Task.Delay(1000);
                }
                Raffle.WinnerMemberId = DrawingEntries.FirstOrDefault(e => e.Selected)!.MemberId;
            }
            context.Entry(Raffle).State = EntityState.Modified;
            await context.SaveChangesAsync();
            Raffle = await context.Raffles
                .Include(r => r.Winner)
                .FirstOrDefaultAsync(r => r.RaffleId == Raffle.RaffleId);
            StateHasChanged();
        }

        private class DrawingEntry
        {
            public Guid MemberId { get; set; }
            public string SceneName { get; set; }
            public bool Selected { get; set; } = false;
        }
    }
}
