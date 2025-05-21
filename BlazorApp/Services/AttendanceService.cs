using System.Net.Http.Json;
using System.Net.Http.Headers;
using Models;

namespace BlazorApp.Services
{
    public class AttendanceService
    {
        private readonly HttpClient _httpClient;
        private readonly FirebaseAuthStateProvider _authStateProvider;

        public AttendanceService(
            HttpClient httpClient,
            FirebaseAuthStateProvider authStateProvider)
        {
            _httpClient = httpClient;
            _authStateProvider = authStateProvider;
        }

        private async Task AddAuthHeader()
        {
            var user = await _authStateProvider.GetFirebaseUser();
            if (user != null && !string.IsNullOrEmpty(user.token))
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", user.token);
            }
        }

        public async Task<List<AttendanceWithVisitor>> GetAttendanceAsync(DateTime date)
        {
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
