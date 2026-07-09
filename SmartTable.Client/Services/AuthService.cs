using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;

namespace SmartTable.Client.Services;

public class AuthService
{
    private readonly HttpClient _http;
    private readonly CustomAuthStateProvider _authStateProvider;
    private readonly IJSRuntime _js;

    private const string TokenStorageKey = "smarttable_auth_token";

    public AuthService(HttpClient http, AuthenticationStateProvider authStateProvider, IJSRuntime js)
    {
        _http = http;
        _authStateProvider = (CustomAuthStateProvider)authStateProvider;
        _js = js;
    }

    public record LoginResult(bool Success, string? ErrorMessage);

    public async Task<LoginResult> LoginAsync(string username, string password)
    {
        HttpResponseMessage response;
        try
        {
            response = await _http.PostAsJsonAsync("api/auth/login", new { username, password });
        }
        catch
        {
            return new LoginResult(false, "Nie można połączyć się z serwerem.");
        }

        if (!response.IsSuccessStatusCode)
        {
            return new LoginResult(false, "Nieprawidłowy login lub hasło.");
        }

        var payload = await response.Content.ReadFromJsonAsync<LoginResponsePayload>();
        if (payload is null || string.IsNullOrWhiteSpace(payload.Token))
        {
            return new LoginResult(false, "Błąd odpowiedzi serwera.");
        }

        await _js.InvokeVoidAsync("localStorage.setItem", TokenStorageKey, payload.Token);
        _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", payload.Token);
        _authStateProvider.NotifyUserAuthentication(payload.Token);

        return new LoginResult(true, null);
    }

    public async Task LogoutAsync()
    {
        await _js.InvokeVoidAsync("localStorage.removeItem", TokenStorageKey);
        _http.DefaultRequestHeaders.Authorization = null;
        _authStateProvider.NotifyUserLogout();
    }

    private record LoginResponsePayload(string Token, string Username, DateTime ExpiresAtUtc);
}
