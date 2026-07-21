using System.Net.Http.Json;
using SmartTable.Client.Models;

namespace SmartTable.Client.Services;

public interface ICourseTermService
{
    Task<List<CourseTerm>> GetAllAsync(Guid? courseId = null);
    Task<CourseTerm?> CreateAsync(CourseTerm model);
    Task<bool> UpdateAsync(CourseTerm model);
    Task<bool> DeleteAsync(Guid id);
}

public class CourseTermService : ICourseTermService
{
    private readonly HttpClient _http;
    public CourseTermService(HttpClient http) => _http = http;

    public async Task<List<CourseTerm>> GetAllAsync(Guid? courseId = null)
    {
        var url = courseId.HasValue ? $"api/courseterms?courseId={courseId}" : "api/courseterms";
        var result = await _http.GetFromJsonAsync<List<CourseTerm>>(url);
        return result ?? new List<CourseTerm>();
    }

    public async Task<CourseTerm?> CreateAsync(CourseTerm model)
    {
        var response = await _http.PostAsJsonAsync("api/courseterms", model);
        if (response.IsSuccessStatusCode)
            return await response.Content.ReadFromJsonAsync<CourseTerm>();
        return null;
    }

    public async Task<bool> UpdateAsync(CourseTerm model)
    {
        var response = await _http.PutAsJsonAsync($"api/courseterms/{model.Id}", model);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var response = await _http.DeleteAsync($"api/courseterms/{id}");
        return response.IsSuccessStatusCode;
    }
}
