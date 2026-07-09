using System.Net.Http.Json;
using SmartTable.Client.Models;

namespace SmartTable.Client.Services;

// ─────────────────────────────────────────────────────────────
//  Student Service
// ─────────────────────────────────────────────────────────────
public interface IStudentService
{
    Task<List<Student>> GetAllAsync();
    Task<Student?> CreateAsync(Student student);
    Task<bool> UpdateAsync(Student student);
    Task<bool> DeleteAsync(Guid id);
}

public class StudentService : IStudentService
{
    private readonly HttpClient _http;
    public StudentService(HttpClient http) => _http = http;

    public async Task<List<Student>> GetAllAsync()
    {
        var result = await _http.GetFromJsonAsync<List<Student>>("api/students");
        return result ?? new List<Student>();
    }

    public async Task<Student?> CreateAsync(Student student)
    {
        var response = await _http.PostAsJsonAsync("api/students", student);
        if (response.IsSuccessStatusCode)
            return await response.Content.ReadFromJsonAsync<Student>();
        return null;
    }

    public async Task<bool> UpdateAsync(Student student)
    {
        var response = await _http.PutAsJsonAsync($"api/students/{student.Id}", student);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var response = await _http.DeleteAsync($"api/students/{id}");
        return response.IsSuccessStatusCode;
    }
}



// ─────────────────────────────────────────────────────────────
//  Enrollment Service
// ─────────────────────────────────────────────────────────────
public interface IEnrollmentService
{
    Task<List<Enrollment>> GetAllAsync();
    Task<Enrollment?> CreateAsync(Enrollment enrollment);
    Task<bool> UpdateAsync(Enrollment enrollment);
    Task<bool> DeleteAsync(Guid id);
}

public class EnrollmentService : IEnrollmentService
{
    private readonly HttpClient _http;
    public EnrollmentService(HttpClient http) => _http = http;

    public async Task<List<Enrollment>> GetAllAsync()
    {
        var result = await _http.GetFromJsonAsync<List<Enrollment>>("api/enrollments");
        return result ?? new List<Enrollment>();
    }

    public async Task<Enrollment?> CreateAsync(Enrollment enrollment)
    {
        var response = await _http.PostAsJsonAsync("api/enrollments", enrollment);
        if (response.IsSuccessStatusCode)
            return await response.Content.ReadFromJsonAsync<Enrollment>();
        return null;
    }

    public async Task<bool> UpdateAsync(Enrollment enrollment)
    {
        var response = await _http.PutAsJsonAsync($"api/enrollments/{enrollment.Id}", enrollment);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var response = await _http.DeleteAsync($"api/enrollments/{id}");
        return response.IsSuccessStatusCode;
    }
}

// ─────────────────────────────────────────────────────────────
//  Audit Service
// ─────────────────────────────────────────────────────────────
public interface IAuditService
{
    Task<List<AuditLog>> GetLogsAsync(int page = 1, int pageSize = 50);
    string GetNavigationUrl(AuditLog log);
}

public class AuditService : IAuditService
{
    private readonly HttpClient _http;
    public AuditService(HttpClient http) => _http = http;

    public async Task<List<AuditLog>> GetLogsAsync(int page = 1, int pageSize = 50)
    {
        var result = await _http.GetFromJsonAsync<List<AuditLog>>(
            $"api/auditlogs?page={page}&pageSize={pageSize}");
        return result ?? new List<AuditLog>();
    }

    /// <summary>
    /// Returns the SPA route for the entity referenced in the log.
    /// e.g. Student → /students/{id}  |  Enrollment → /enrollments/{id}
    /// </summary>
    public string GetNavigationUrl(AuditLog log) =>
        log.EntityType.ToLower() switch
        {
            "student"    => $"/students/{log.EntityId}",
            "enrollment" => $"/enrollments/{log.EntityId}",
            _            => "/"
        };
}
