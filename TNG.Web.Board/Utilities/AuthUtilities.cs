using Microsoft.AspNetCore.Components.Authorization;
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
    }
}
