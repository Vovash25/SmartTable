using System.Net.Http.Json;
using SmartTable.Client.Models;

namespace SmartTable.Client.Services
{
    public interface ICourseService
    {
        Task<List<Course>> GetAllAsync();
        Task<Course?> CreateAsync(Course course);
        Task<bool> UpdateAsync(Course course);
        Task<bool> DeleteAsync(Guid id);
    }

    public class CourseService : ICourseService
    {
        private readonly HttpClient _http;

        public CourseService(HttpClient http)
        {
            _http = http;
        }

        public async Task<List<Course>> GetAllAsync()
        {
            try
            {
                return await _http.GetFromJsonAsync<List<Course>>("api/courses") ?? new List<Course>();
            }
            catch
            {
                return new List<Course>();
            }
        }

        public async Task<Course?> CreateAsync(Course course)
        {
            var response = await _http.PostAsJsonAsync("api/courses", course);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<Course>();
            }
            return null;
        }

        public async Task<bool> UpdateAsync(Course course)
        {
            var response = await _http.PutAsJsonAsync($"api/courses/{course.Id}", course);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var response = await _http.DeleteAsync($"api/courses/{id}");
            return response.IsSuccessStatusCode;
        }
    }
}