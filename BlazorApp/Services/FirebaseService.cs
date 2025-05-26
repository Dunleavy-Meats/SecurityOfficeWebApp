using Microsoft.JSInterop;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using System.Net.Http.Json;
using Models;

namespace BlazorApp.Services
{
    public class FirebaseService
    {
        private readonly IJSRuntime _jsRuntime;
        private readonly NavigationManager _navigation;
        private readonly AuthenticationStateProvider _authStateProvider;
        private readonly HttpClient _httpClient;

        public FirebaseService(
            IJSRuntime jsRuntime,
            NavigationManager navigation,
            AuthenticationStateProvider authStateProvider,
            HttpClient httpClient)
        {
            _jsRuntime = jsRuntime;
            _navigation = navigation;
            _authStateProvider = authStateProvider;
            _httpClient = httpClient;
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

        public async Task<User?> GetUserDetailsAsync(string userId)
        {
            try
            {
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
}