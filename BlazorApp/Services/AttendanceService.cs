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

                var attendanceRecords = attendanceEnvelope?.Items;

                if (attendanceRecords == null)
                {
                    throw new Exception("Null attendance response from API.");
                }

                // Fetch only the visitors referenced by these records — avoids page-size gaps
                var visitorIds = attendanceRecords
                    .Select(r => r.VisitorID)
                    .Where(id => !string.IsNullOrEmpty(id))
                    .Distinct()
                    .ToList();

                List<Visitor> visitors = new();
                if (visitorIds.Count > 0)
                {
                    var idsParam = string.Join(",", visitorIds);
                    visitors = await _httpClient.GetFromJsonAsync<List<Visitor>>(
                        $"api/visitors/by-ids?visitorIds={Uri.EscapeDataString(idsParam)}") ?? new();
                }

                var visitorMap = visitors.ToDictionary(v => v.Id);

                List<AttendanceWithVisitor> attendanceWithVisitors = new List<AttendanceWithVisitor>();
                foreach (var item in attendanceRecords)
                {
                    if (!visitorMap.TryGetValue(item.VisitorID, out var visitor))
                    {
                        Console.WriteLine($"[AttendanceService] Warning: no visitor found for VisitorID={item.VisitorID}");
                        continue; // skip orphaned records rather than producing a null Visitor
                    }

                    attendanceWithVisitors.Add(new AttendanceWithVisitor
                    {
                        Record = item,
                        Visitor = visitor
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
