using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;

namespace SmartTable.Client.Services;

public class CustomAuthStateProvider : AuthenticationStateProvider
{
    private readonly IJSRuntime _js;
    private readonly HttpClient _http;
    private const string TokenStorageKey = "smarttable_auth_token";

    private static readonly ClaimsPrincipal Anonymous = new(new ClaimsIdentity());

    public CustomAuthStateProvider(IJSRuntime js, HttpClient http)
    {
        _js = js;
        _http = http;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        string? token;
        try
        {
            token = await _js.InvokeAsync<string?>("localStorage.getItem", TokenStorageKey);
        }
        catch
        {
            // JS interop може бути недоступний під час prerender — вважаємо, що не залогінений
            return new AuthenticationState(Anonymous);
        }

        if (string.IsNullOrWhiteSpace(token))
        {
            return new AuthenticationState(Anonymous);
        }

        var claims = ParseClaimsFromJwt(token);
        var expClaim = claims.FirstOrDefault(c => c.Type == "exp");
        if (expClaim is not null && long.TryParse(expClaim.Value, out var expUnix))
        {
            var expiresAt = DateTimeOffset.FromUnixTimeSeconds(expUnix);
            if (expiresAt <= DateTimeOffset.UtcNow)
            {
                // Токен протух — прибираємо його і показуємо як неавторизованого
                await _js.InvokeVoidAsync("localStorage.removeItem", TokenStorageKey);
                return new AuthenticationState(Anonymous);
            }
        }

        _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var identity = new ClaimsIdentity(claims, "jwt");
        return new AuthenticationState(new ClaimsPrincipal(identity));
    }

    public void NotifyUserAuthentication(string token)
    {
        var claims = ParseClaimsFromJwt(token);
        var identity = new ClaimsIdentity(claims, "jwt");
        var user = new ClaimsPrincipal(identity);
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user)));
    }

    public void NotifyUserLogout()
    {
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(Anonymous)));
    }

    // JWT payload — це звичайний Base64Url-закодований JSON. Парсимо вручну,
    // без зовнішніх бібліотек.
    private static IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
    {
        var parts = jwt.Split('.');
        if (parts.Length < 2) return Enumerable.Empty<Claim>();

        var payload = parts[1];
        var json = System.Text.Encoding.UTF8.GetString(Base64UrlDecode(payload));
        var keyValuePairs = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json)
            ?? new Dictionary<string, JsonElement>();

        var claims = new List<Claim>();
        foreach (var kvp in keyValuePairs)
        {
            if (kvp.Value.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in kvp.Value.EnumerateArray())
                    claims.Add(new Claim(kvp.Key, item.ToString()));
            }
            else
            {
                claims.Add(new Claim(kvp.Key, kvp.Value.ToString()));
            }
        }

        // ASP.NET Core JwtBearer перекладає стандартні короткі назви claim'ів
        // (name/role) у довгі XML-неймспейси. Робимо те саме тут,
        // щоб AuthorizeView Roles="Admin" й @context.User.Identity.Name працювали однаково.
        return claims.Select(c => c.Type switch
        {
            "name" => new Claim(ClaimTypes.Name, c.Value),
            "role" => new Claim(ClaimTypes.Role, c.Value),
            _ => c
        });
    }

    private static byte[] Base64UrlDecode(string input)
    {
        var padded = input.Replace('-', '+').Replace('_', '/');
        switch (padded.Length % 4)
        {
            case 2: padded += "=="; break;
            case 3: padded += "="; break;
        }
        return Convert.FromBase64String(padded);
    }
}
