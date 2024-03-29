﻿using Blazored.Modal;
using Microsoft.AspNetCore.Components;
using Microsoft.CodeAnalysis.CSharp.Syntax;
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

        private IEnumerable<EventRsvpPlusOne>? _plusOnes { get; set; }
        private IEnumerable<EventRsvpPlusOne> PlusOnes
            => _plusOnes ?? context.EventRsvpPlusOnes
                .Include(e => e.Member)
                .Include(e => e.PlusOne)
                .Where(e => e.EventId == Rsvp.EventId && e.MemberId == Rsvp.MemberId);

        private IEnumerable<Member>? _members { get; set; }
        private IEnumerable<Member> Members
            => _members ?? context.Members
                .Where(m => !m.PrivateProfile);

        private IEnumerable<Member>? MemberSearchResults { get; set; }

        private void SearchBySceneName()
        {
            if (!string.IsNullOrWhiteSpace(NewPlusOneSceneName))
                MemberSearchResults = Members.Where(m => m.SceneName.Trim().Contains(NewPlusOneSceneName.Trim(), StringComparison.OrdinalIgnoreCase));
        }

        private string? NewPlusOneSceneName { get; set; }
        private string? NewPlusOneEmail { get; set; }

        private async Task AddPlusOne(Guid? memberId = null)
        {
            try
            {
                shouldRender = false;
                if (memberId is null && !string.IsNullOrEmpty(NewPlusOneEmail))
                {
                    var member = await context.Members.FirstOrDefaultAsync(m => EF.Functions.Like(m.EmailAddress, NewPlusOneEmail.Trim()));
                    if (member != null)
                    {
                        memberId = member.Id;
                    }
                }
                if (memberId is null)
                {
                    return;
                }
                await context.EventRsvpPlusOnes.AddAsync(new()
                {
                    EventId = Rsvp.EventId,
                    MemberId = Rsvp.MemberId,
                    PlusOneMemberId = memberId.Value
                });
                await context.SaveChangesAsync();
                MemberSearchResults = null;
                NewPlusOneSceneName = null;
                NewPlusOneEmail = null;
                _plusOnes = null;
            }
            finally
            {
                shouldRender = true;
            }
        }

        private async Task RemovePlusOne(EventRsvpPlusOne plusOne)
        {
            try
            {
                shouldRender = false;
                context.EventRsvpPlusOnes.Remove(plusOne);
                await context.SaveChangesAsync();
                _plusOnes = null;
            }
            finally
            {
                shouldRender = true;
            }
        }

        private async Task SaveNote()
        {
            try
            {
                shouldRender = false;
                if (string.IsNullOrEmpty(Rsvp.Notes?.Trim()))
                    Rsvp.Notes = null;

                context.Entry(Rsvp).State = EntityState.Modified;

                await context.SaveChangesAsync();
                await BlazoredModal.CloseAsync();
            }
            finally 
            { 
                shouldRender = true; 
            }
        }

        private bool shouldRender = true;

        protected override bool ShouldRender()
        {
            return shouldRender;
        }
    }
}
