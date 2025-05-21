using System.Net.Http.Json;
using Models;

namespace BlazorApp.Services
{
    public class VisitorService : BaseApiService
    {
        private const string VISITORS_CACHE_KEY = "visitors_data";
        private const string ENTITY_TYPE = "visitors";

        public VisitorService(
            HttpClient httpClient,
            FirebaseAuthStateProvider authStateProvider,
            CacheService cacheService) 
            : base(httpClient, authStateProvider, cacheService)
        {
        }

        public async Task<List<VisitorWithQuestionerData>> GetVisitorWithQuestionerDataAsync(bool forceRefresh = false)
        {
            if (!forceRefresh && !await HasDataChanged(ENTITY_TYPE) && 
                _cacheService.TryGetValue<List<VisitorWithQuestionerData>>(VISITORS_CACHE_KEY, out var cachedData))
            {
                return cachedData;
            }

            await AddAuthHeader();
            try
            {
                var visitors = await _httpClient.GetFromJsonAsync<List<Visitor>>("api/Visitors/getvisitors") ?? new();
                var answers = await _httpClient.GetFromJsonAsync<List<QuestionerData>>("api/Visitors/GetAllAnswer") ?? new();
                
                var result = visitors.Select(visitor => new VisitorWithQuestionerData(
                    visitor, 
                    answers.Where(x => x.VisitorID == visitor.Id).OrderByDescending(x => x.CreatedOn).ToList()
                )).ToList();

                // Get the current timestamp from server for this entity type
                var response = await _httpClient.GetAsync($"api/DataSync/lastmodified/{ENTITY_TYPE}");
                if (response.IsSuccessStatusCode)
                {
                    var timestamp = await response.Content.ReadFromJsonAsync<DateTime>();
                    _cacheService.SetLastModified(ENTITY_TYPE, timestamp);
                }

                _cacheService.Set(VISITORS_CACHE_KEY, result);
                return result;
            }
            catch (HttpRequestException)
            {
                return new();
            }
        }

        // Rest of your methods remain the same
        public async Task<List<Visitor>> GetVisitorsAsync()
        {
            await AddAuthHeader();
            try
            {
                return await _httpClient.GetFromJsonAsync<List<Visitor>>("api/Visitors/getvisitors") ?? new List<Visitor>();
            }
            catch (HttpRequestException)
            {
                // Handle error
                return new List<Visitor>();
            }
        }

        public async Task<List<Questioner>> GetQuestionsAsync()
        {
            await AddAuthHeader();
            try
            {
                return await _httpClient.GetFromJsonAsync<List<Questioner>>("api/Visitors/getQuestions") ?? new List<Questioner>();
            }
            catch (HttpRequestException)
            {
                // Handle error
                return new List<Questioner>();
            }
        }

        public async Task ApproveVisitor(string visitorId)
        {
            await AddAuthHeader();
            try
            {
                 await _httpClient.GetAsync($"api/Visitors/ApproveVisitor/{visitorId}");
            }
            catch (HttpRequestException ex)
            {
                // Handle error
                throw new Exception("Error approving visitor", ex);
            }
        }
        
        public async Task<byte[]> GetPDFForVisitor(string visitorId)
        {
            await AddAuthHeader();
            try
            {
                var response = await _httpClient.GetAsync($"api/PDF/GetAnswerForVisitor/{visitorId}");
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsByteArrayAsync();
            }
            catch (HttpRequestException ex)
            {
                throw new Exception("Error getting PDF for visitor", ex);
            }
        }

        public async Task<string> AddVisitorAsync(Visitor visitor)
        {
            await AddAuthHeader();
            try
            {
                var response = await _httpClient.PostAsJsonAsync(
                    "api/Visitors/addFrequentVisitor", 
                    visitor);
                
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsStringAsync();
            }
            catch (HttpRequestException ex)
            {
                throw new Exception("Error adding visitor", ex);
            }
        }
    }
}