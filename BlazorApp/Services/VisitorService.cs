using System.Net.Http.Json;
using System.Net.Http.Headers;
using Models;

namespace BlazorApp.Services
{
    public class VisitorService
    {
        private readonly HttpClient _httpClient;
        private readonly FirebaseAuthStateProvider _authStateProvider;

        public VisitorService(
            HttpClient httpClient,
            FirebaseAuthStateProvider authStateProvider)
        {
            _httpClient = httpClient;
            _authStateProvider = authStateProvider;
        }

        private async Task AddAuthHeader()
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

        public async Task<List<VisitorWithQuestionerData>> GetVisitorWithQuestionerDataAsync()
        {
            await AddAuthHeader();
            try
            {
                var vivitors = await _httpClient.GetFromJsonAsync<List<Visitor>>("api/Visitors/getvisitors") ?? new List<Visitor>();
                var answers = await _httpClient.GetFromJsonAsync<List<QuestionerData>>("api/Visitors/GetAllAnswer") ?? new List<QuestionerData>();
				answers.OrderByDescending(x => x.CreatedOn);
				List<VisitorWithQuestionerData> result = new List<VisitorWithQuestionerData>();
				foreach (var visitor in vivitors)
				{
                    var temp = new VisitorWithQuestionerData(visitor, answers.Where(x => x.VisitorID == visitor.Id).ToList());
					result.Add(temp);
				}

				return result;
            }
            catch (HttpRequestException)
            {
                // Handle error
                return new List<VisitorWithQuestionerData>();
            }
        }

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