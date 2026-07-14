using System.Net.Http.Json;
using SmartTable.Client.Models;

namespace SmartTable.Client.Services;

public interface ICourseDateService
{
    Task<List<CourseDate>> GetAllAsync(Guid? courseId = null);
    Task<CourseDate?> CreateAsync(CourseDate model);
    Task<bool> DeleteAsync(Guid id);
}

public class CourseDateService : ICourseDateService
{
    private readonly HttpClient _http;
    public CourseDateService(HttpClient http) => _http = http;

    public async Task<List<CourseDate>> GetAllAsync(Guid? courseId = null)
    {
        var url = courseId.HasValue ? $"api/coursedates?courseId={courseId}" : "api/coursedates";
        var result = await _http.GetFromJsonAsync<List<CourseDate>>(url);
        return result ?? new List<CourseDate>();
    }

    public async Task<CourseDate?> CreateAsync(CourseDate model)
    {
        var response = await _http.PostAsJsonAsync("api/coursedates", model);
        if (response.IsSuccessStatusCode)
            return await response.Content.ReadFromJsonAsync<CourseDate>();
        return null;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var response = await _http.DeleteAsync($"api/coursedates/{id}");
        return response.IsSuccessStatusCode;
    }
}
