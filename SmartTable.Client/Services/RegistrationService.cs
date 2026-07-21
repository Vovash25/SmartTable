using System.Net.Http.Json;
using SmartTable.Client.Models;

namespace SmartTable.Client.Services
{
    public class RegistrationService : IRegistrationService
    {
        private readonly HttpClient _http;

        public RegistrationService(HttpClient http)
        {
            _http = http;
        }

        public async Task<List<CandidateRegistration>> GetAllAsync()
        {
            return await _http.GetFromJsonAsync<List<CandidateRegistration>>("api/candidateregistrations") ?? new();
        }

        public async Task<CandidateRegistration?> CreateAsync(CandidateRegistration model)
        {
            var response = await _http.PostAsJsonAsync("api/candidateregistrations", model);
            
            if (response.StatusCode == System.Net.HttpStatusCode.Conflict)
            {
                return null; 
            }
            
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<CandidateRegistration>();
            }
            return null;
        }

        public async Task<bool> UpdateAsync(CandidateRegistration model)
        {
            var response = await _http.PutAsJsonAsync($"api/candidateregistrations/{model.Id}", model);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var response = await _http.DeleteAsync($"api/candidateregistrations/{id}");
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> AssignToCourseAsync(Guid registrationId, IEnumerable<Guid> courseIds)
        {
            var response = await _http.PostAsJsonAsync($"api/candidateregistrations/{registrationId}/assign", courseIds);
            return response.IsSuccessStatusCode;
        }
    }
}