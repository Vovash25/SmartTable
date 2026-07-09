using System.Net.Http.Json;

namespace SmartTable.Client.Services;

public record AdminUserDto(Guid Id, string Username, string Role, DateTime CreatedAt);
public record CreateAdminRequest(string Username, string Password);

public interface IAdminUserService
{
    Task<List<AdminUserDto>> GetAllAsync();
    Task<(bool Success, string? Error)> CreateAsync(string username, string password);
    Task<(bool Success, string? Error)> DeleteAsync(Guid id);
}

public class AdminUserService : IAdminUserService
{
    private readonly HttpClient _http;
    public AdminUserService(HttpClient http) => _http = http;

    public async Task<List<AdminUserDto>> GetAllAsync()
    {
        var result = await _http.GetFromJsonAsync<List<AdminUserDto>>("api/adminusers");
        return result ?? new List<AdminUserDto>();
    }

    public async Task<(bool Success, string? Error)> CreateAsync(string username, string password)
    {
        var response = await _http.PostAsJsonAsync("api/adminusers", new CreateAdminRequest(username, password));
        if (response.IsSuccessStatusCode)
        {
            return (true, null);
        }

        var error = await TryReadError(response);
        return (false, error ?? "Błąd podczas tworzenia konta.");
    }

    public async Task<(bool Success, string? Error)> DeleteAsync(Guid id)
    {
        var response = await _http.DeleteAsync($"api/adminusers/{id}");
        if (response.IsSuccessStatusCode)
        {
            return (true, null);
        }

        var error = await TryReadError(response);
        return (false, error ?? "Błąd podczas usuwania konta.");
    }

    private static async Task<string?> TryReadError(HttpResponseMessage response)
    {
        try
        {
            var payload = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();
            return payload != null && payload.TryGetValue("message", out var msg) ? msg : null;
        }
        catch
        {
            return null;
        }
    }
}
