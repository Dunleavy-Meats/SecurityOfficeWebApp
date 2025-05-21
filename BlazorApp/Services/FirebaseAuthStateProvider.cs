using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;
using Microsoft.JSInterop;

namespace BlazorApp.Services
{
    public class FirebaseAuthStateProvider : AuthenticationStateProvider
    {
        private readonly IJSRuntime _jsRuntime;

        public FirebaseAuthStateProvider(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var user = await GetFirebaseUser();
            return CreateAuthenticationState(user);
        }

        public async Task NotifyAuthenticationStateChanged()
        {
            var user = await GetFirebaseUser();
            var authState = CreateAuthenticationState(user);
            NotifyAuthenticationStateChanged(Task.FromResult(authState));
        }

        public async Task<FirebaseUser> GetFirebaseUser()
        {
            try
            {
                return await _jsRuntime.InvokeAsync<FirebaseUser>("firebaseGetCurrentUser");
            }
            catch
            {
                return null;
            }
        }

        private AuthenticationState CreateAuthenticationState(FirebaseUser user)
        {
            if (user != null)
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.uid),
                    new Claim(ClaimTypes.Email, user.email),
                };

                var identity = new ClaimsIdentity(claims, "Firebase");
                return new AuthenticationState(new ClaimsPrincipal(identity));
            }

            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }
    }

    public class FirebaseUser
    {
        public string uid { get; set; }
        public string email { get; set; }
        public string token { get; set; }
    }
}