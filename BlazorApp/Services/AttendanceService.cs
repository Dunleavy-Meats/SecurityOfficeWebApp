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
                var request = new DateOnlyRequest
                {
                    Date = date.Date // Ensures time is 00:00:00
                };
                var response = await _httpClient.PostAsJsonAsync("/api/Attendance/by-date", request);

                response.EnsureSuccessStatusCode();
                List<AttendanceRecord> attendanceRecords = await response.Content.ReadFromJsonAsync<List<AttendanceRecord>>();
                List<Visitor> visitors = await _httpClient.GetFromJsonAsync<List<Visitor>>("api/Visitors/getvisitors");

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
                var timestampResponse = await _httpClient.GetAsync($"api/DataSync/lastmodified/{ENTITY_TYPE}");
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
