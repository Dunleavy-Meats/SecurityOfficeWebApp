using Microsoft.JSInterop;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

namespace BlazorApp.Services
{
    public class FirebaseService
    {
        private readonly IJSRuntime _jsRuntime;
        private readonly NavigationManager _navigation;
        private readonly AuthenticationStateProvider _authStateProvider;

        public FirebaseService(
            IJSRuntime jsRuntime,
            NavigationManager navigation,
            AuthenticationStateProvider authStateProvider)
        {
            _jsRuntime = jsRuntime;
            _navigation = navigation;
            _authStateProvider = authStateProvider;
        }

        public async Task<string?> SignInWithEmailAndPasswordAsync(string email, string password)
        {
            try
            {
                var userId = await _jsRuntime.InvokeAsync<string>(
                    "firebaseSignInWithEmailAndPassword",
                    email,
                    password);

                // Notify about authentication state change
                await ((FirebaseAuthStateProvider)_authStateProvider).NotifyAuthenticationStateChanged();

                return userId;
            }
            catch (JSException ex)
            {
                Console.WriteLine($"Login error: {ex.Message}");
                return null;
            }
        }

        public async Task SignOutAsync()
        {
            try
            {
                await _jsRuntime.InvokeVoidAsync("firebaseSignOut");
                await ((FirebaseAuthStateProvider)_authStateProvider).NotifyAuthenticationStateChanged();
            }
            catch (JSException ex)
            {
                Console.WriteLine($"Sign out error: {ex.Message}");
            }
        }
    }
}