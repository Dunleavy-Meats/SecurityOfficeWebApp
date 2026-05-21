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
                var visitorsEnvelope = await _httpClient.GetFromJsonAsync<ApiPagedResponse<Visitor>>("api/visitors?page=1&pageSize=200");
                var answers = await _httpClient.GetFromJsonAsync<List<QuestionerData>>("api/questionnaires") ?? new();
                var visitors = visitorsEnvelope?.Items ?? new();
                
                var result = visitors.Select(visitor => new VisitorWithQuestionerData(
                    visitor, 
                    answers.Where(x => x.VisitorID == visitor.Id).OrderByDescending(x => x.CreatedOn).ToList()
                )).ToList();

                // Get the current timestamp from server for this entity type
                var response = await _httpClient.GetAsync($"api/sync/{ENTITY_TYPE}/last-modified");
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
                var response = await _httpClient.GetFromJsonAsync<ApiPagedResponse<Visitor>>("api/visitors?page=1&pageSize=200");
                return response?.Items ?? new List<Visitor>();
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
                return await _httpClient.GetFromJsonAsync<List<Questioner>>("api/question-catalog") ?? new List<Questioner>();
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
                 var body = new { approved = NA_Yes_No.Yes };
                 await _httpClient.PatchAsJsonAsync($"api/visitors/{visitorId}/approval", body);
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
                var response = await _httpClient.GetAsync($"api/visitors/{visitorId}/documents/questionnaire-pdf");
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
                    "api/visitors", 
                    visitor);
                
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsStringAsync();
            }
            catch (HttpRequestException ex)
            {
                throw new Exception("Error adding visitor", ex);
            }
        }

        public async Task<byte[]> GetAttendanceRecordsPDF(string visitorId, DateTime fromDate, DateTime toDate, bool forDownload)
        {
            await AddAuthHeader();
            try
            {
                var requestUrl = $"api/visitors/{visitorId}/documents/attendance-pdf?fromDate={fromDate:yyyy-MM-dd}&toDate={toDate:yyyy-MM-dd}";
                var response = await _httpClient.GetAsync(requestUrl);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsByteArrayAsync();
            }
            catch (HttpRequestException ex)
            {
                throw new Exception("Error getting attendance records PDF for visitor", ex);
            }
        }
    }
}