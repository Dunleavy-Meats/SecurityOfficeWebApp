using System.Net.Http.Json;
using System.Net.Http.Headers;
using Models;

namespace BlazorApp.Services
{
    public class AttendanceService : BaseApiService
    {
        private const string ENTITY_TYPE = "attendance";
        private const string ATTENDANCE_CACHE_KEY_PREFIX = "attendance_";

        public AttendanceService(
            HttpClient httpClient,
            FirebaseAuthStateProvider authStateProvider,
            CacheService cacheService)
            : base(httpClient, authStateProvider, cacheService)
        {
        }

        public async Task<List<AttendanceWithVisitor>> GetAttendanceAsync(DateTime date)
        {
            string cacheKey = $"{ATTENDANCE_CACHE_KEY_PREFIX}{date.ToString("yyyy-MM-dd")}";
            
            if (!await HasDataChanged(ENTITY_TYPE) && 
                _cacheService.TryGetValue<List<AttendanceWithVisitor>>(cacheKey, out var cachedData))
            {
                return cachedData;
            }

            await AddAuthHeader();

            try
            {
                var attendanceEnvelope = await _httpClient.GetFromJsonAsync<ApiPagedResponse<AttendanceRecord>>(
                    $"/api/attendance-records?date={date:yyyy-MM-dd}&page=1&pageSize=200");
                var visitorsEnvelope = await _httpClient.GetFromJsonAsync<ApiPagedResponse<Visitor>>("api/visitors?page=1&pageSize=200");

                var attendanceRecords = attendanceEnvelope?.Items;
                var visitors = visitorsEnvelope?.Items;

                if(attendanceRecords == null || visitors == null)
                {
                    throw new Exception();
                }

                List<AttendanceWithVisitor> attendanceWithVisitors = new List<AttendanceWithVisitor>();
                foreach (var item in attendanceRecords)
                {
                    attendanceWithVisitors.Add(new AttendanceWithVisitor
                    {
                        Record = item,
                        Visitor = visitors.FirstOrDefault(v => v.Id == item.VisitorID)
                    });
                }

                // Get the current timestamp for this entity type
                var timestampResponse = await _httpClient.GetAsync($"api/sync/{ENTITY_TYPE}/last-modified");
                if (timestampResponse.IsSuccessStatusCode)
                {
                    var timestamp = await timestampResponse.Content.ReadFromJsonAsync<DateTime>();
                    _cacheService.SetLastModified(ENTITY_TYPE, timestamp);
                }

                _cacheService.Set(cacheKey, attendanceWithVisitors);
                return attendanceWithVisitors;
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Error fetching attendance records: {ex.Message}");
                return new List<AttendanceWithVisitor>();
            }
        }
    }
}
