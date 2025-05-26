using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;
using Microsoft.JSInterop;
using Models;
using System.Net.Http.Json;
using System.Net.Http;
using System.Net.Http.Headers;

namespace BlazorApp.Services
{
    public class FirebaseAuthStateProvider : AuthenticationStateProvider
    {
        private readonly IJSRuntime _jsRuntime;
        private readonly HttpClient _httpClient;

        public FirebaseAuthStateProvider(IJSRuntime jsRuntime, HttpClient httpClient)
        {
            _jsRuntime = jsRuntime;
            _httpClient = httpClient;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var user = await GetFirebaseUser();
            return await CreateAuthenticationState(user);
        }

        public async Task NotifyAuthenticationStateChanged()
        {
            var user = await GetFirebaseUser();
            var authState = await CreateAuthenticationState(user);
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

        private async Task<AuthenticationState> CreateAuthenticationState(FirebaseUser user)
        {
            if (user != null)
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.uid),
                    new Claim(ClaimTypes.Email, user.email),
                };

                // Fetch additional user details from your database
                try
                {
                    var userDetails = await GetUserDetailsFromDatabase(user.uid);
                    if (userDetails != null)
                    {
                        // Add name claim
                        if (!string.IsNullOrEmpty(userDetails.name))
                        {
                            claims.Add(new Claim(ClaimTypes.Name, userDetails.name));
                        }
                        
                        // Add role claim
                        if (!string.IsNullOrEmpty(userDetails.role))
                        {
                            claims.Add(new Claim(ClaimTypes.Role, userDetails.role));
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error fetching user details for claims: {ex.Message}");
                }

                var identity = new ClaimsIdentity(claims, "Firebase");
                return new AuthenticationState(new ClaimsPrincipal(identity));
            }

            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }

        protected async Task AddAuthHeader()
        {
            var user = await GetFirebaseUser();
            _httpClient.DefaultRequestHeaders.Clear();
            if (user != null && !string.IsNullOrEmpty(user.token))
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", user.token);
            }
            _httpClient.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
        }

        private async Task<User> GetUserDetailsFromDatabase(string userId)
        {
            try
            {
                await AddAuthHeader();
                var response = await _httpClient.GetAsync($"api/users/{userId}");
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<User>();
                }
                return null;
            }
            catch
            {
                return null;
            }
        }
    }

    public class FirebaseUser
    {
        public string uid { get; set; }
        public string email { get; set; }
        public string token { get; set; }
    }
}