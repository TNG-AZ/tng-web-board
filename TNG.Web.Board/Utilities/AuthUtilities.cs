using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Security.Claims;
using System.Security.Principal;

namespace TNG.Web.Board.Utilities
{
    public class AuthUtilities
    {
        private readonly AuthenticationStateProvider _authenticationStateProvider;

        public AuthUtilities(AuthenticationStateProvider authenticationStateProvider)
        {
            _authenticationStateProvider = authenticationStateProvider;
        }

        public async Task<IIdentity?> GetIdentity()
        {
            var authState = await _authenticationStateProvider.GetAuthenticationStateAsync();
            var user = authState.User;
            return user.Identity;
        }

        public async Task<bool> IsAdmin()
        {
            var authState = await _authenticationStateProvider.GetAuthenticationStateAsync();
            return authState.User.HasClaim(ClaimTypes.Role, "Administrator");
        }

        public async Task<bool> IsBoardmember()
        {
            var authState = await _authenticationStateProvider.GetAuthenticationStateAsync();
            return authState.User.HasClaim(ClaimTypes.Role, "Boardmember") 
                || authState.User.HasClaim(ClaimTypes.Role, "Administrator");
        }

        public async Task<bool> IsAmbassador()
        {
            var authState = await _authenticationStateProvider.GetAuthenticationStateAsync();
            return authState.User.HasClaim(ClaimTypes.Role, "Ambassador") 
                || authState.User.HasClaim(ClaimTypes.Role, "Boardmember") 
                || authState.User.HasClaim(ClaimTypes.Role, "Administrator");
        }
    }
}
