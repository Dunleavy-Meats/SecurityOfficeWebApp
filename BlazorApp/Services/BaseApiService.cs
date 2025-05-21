using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace BlazorApp.Services
{
    public abstract class BaseApiService
    {
        protected readonly HttpClient _httpClient;
        protected readonly FirebaseAuthStateProvider _authStateProvider;
        protected readonly CacheService _cacheService;

        protected BaseApiService(
            HttpClient httpClient,
            FirebaseAuthStateProvider authStateProvider,
            CacheService cacheService)
        {
            _httpClient = httpClient;
            _authStateProvider = authStateProvider;
            _cacheService = cacheService;
        }

        protected async Task AddAuthHeader()
        {
            var user = await _authStateProvider.GetFirebaseUser();
            _httpClient.DefaultRequestHeaders.Clear();
            if (user != null && !string.IsNullOrEmpty(user.token))
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", user.token);
            }
            _httpClient.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
        }

        protected async Task<bool> HasDataChanged(string entityType)
        {
            await AddAuthHeader();
            try
            {
                var response = await _httpClient.GetAsync($"api/DataSync/lastmodified/{entityType}");
                if (!response.IsSuccessStatusCode) return true;

                var lastModified = await response.Content.ReadFromJsonAsync<DateTime>();
                return _cacheService.HasNewerData(entityType, lastModified);
            }
            catch
            {
                return true; // On error, assume data has changed
            }
        }
    }
}