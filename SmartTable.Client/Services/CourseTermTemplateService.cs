using System.Net.Http.Json;
using SmartTable.Client.Models;

namespace SmartTable.Client.Services;

public record GenerateResult(int CreatedCount, int SkippedExisting);

public interface ICourseTermTemplateService
{
    Task<List<CourseTermTemplate>> GetAllAsync(Guid? courseId = null);
    Task<CourseTermTemplate?> CreateAsync(CourseTermTemplate model);
    Task<bool> DeleteAsync(Guid id);
    Task<GenerateResult?> GenerateAsync(Guid templateId);
}

public class CourseTermTemplateService : ICourseTermTemplateService
{
    private readonly HttpClient _http;
    public CourseTermTemplateService(HttpClient http) => _http = http;

    public async Task<List<CourseTermTemplate>> GetAllAsync(Guid? courseId = null)
    {
        var url = courseId.HasValue ? $"api/coursetermtemplates?courseId={courseId}" : "api/coursetermtemplates";
        var result = await _http.GetFromJsonAsync<List<CourseTermTemplate>>(url);
        return result ?? new List<CourseTermTemplate>();
    }

    public async Task<CourseTermTemplate?> CreateAsync(CourseTermTemplate model)
    {
        var response = await _http.PostAsJsonAsync("api/coursetermtemplates", model);
        if (response.IsSuccessStatusCode)
            return await response.Content.ReadFromJsonAsync<CourseTermTemplate>();
        return null;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var response = await _http.DeleteAsync($"api/coursetermtemplates/{id}");
        return response.IsSuccessStatusCode;
    }

    public async Task<GenerateResult?> GenerateAsync(Guid templateId)
    {
        var response = await _http.PostAsync($"api/coursetermtemplates/{templateId}/generate", null);
        if (response.IsSuccessStatusCode)
            return await response.Content.ReadFromJsonAsync<GenerateResult>();
        return null;
    }
}
