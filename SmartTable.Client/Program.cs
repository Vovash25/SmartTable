using Microsoft.EntityFrameworkCore;
using CourseManagementApi.Data;
using CourseManagementApi.Data.Entities;
using CourseManagementApi.Auth;
using System.Text.Json.Serialization;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);


var builder = WebApplication.CreateBuilder(args);

// 1. Реєстрація сервісів (Services)
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IAuditLogger, AuditLogger>();

// ── Автентифікація (JWT) ──────────────────────────────────────────────────
var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("Brak konfiguracji Jwt:Key. Ustaw zmienną środowiskową Jwt__Key.");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });

// Усі API-ендпоінти вимагають авторизації за замовчуванням.
builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

var app = builder.Build();

// Клієнт і API тепер на одному origin (той самий процес/домен) —
// окремий CORS більше не потрібен.

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// ── Роздача Blazor WASM клієнта як статичних файлів ────────────────────────
app.UseBlazorFrameworkFiles();
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Будь-який маршрут, що не збігся з API-контролером і не є файлом
// (напр. /students, /login, /kandydaci) — віддає index.html,
// а далі Blazor-роутер сам розбирається, яку сторінку показати.
// AllowAnonymous() — ОБОВ'ЯЗКОВО: інакше глобальна вимога авторизації
// заблокує саму можливість завантажити застосунок (сервер віддасть 401
// ще до того, як Blazor встигне запуститись і показати /login).
app.MapFallbackToFile("index.html").AllowAnonymous();

// ── Засіювання початкових акаунтів (виконується один раз, якщо таблиця порожня) ──
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();

    if (!db.Users.Any())
    {
        // ⚠️ ОБОВ'ЯЗКОВО змініть ці паролі перед першим деплоєм на прод —
        // значення нижче використовувались лише під час розробки.
        var seedUsers = new[]
        {
            ("admin1", Environment.GetEnvironmentVariable("SEED_ADMIN1_PASSWORD") ?? "ZmiencieHaslo1!", "Admin"),
            ("admin2", Environment.GetEnvironmentVariable("SEED_ADMIN2_PASSWORD") ?? "ZmiencieHaslo2!", "Admin"),
            ("superadmin", Environment.GetEnvironmentVariable("SEED_SUPERADMIN_PASSWORD") ?? "SpS1JwuDtUASGI7k", "SuperAdmin")
        };

        foreach (var (username, password, role) in seedUsers)
        {
            var (hash, salt) = PasswordHasher.Hash(password);
            db.Users.Add(new AppUser
            {
                Id = Guid.NewGuid(),
                Username = username,
                PasswordHash = hash,
                PasswordSalt = salt,
                Role = role,
                CreatedAt = DateTime.UtcNow
            });
        }

        db.SaveChanges();
        Console.WriteLine("Zasiano początkowe konta: admin1, admin2, superadmin.");
    }
}

app.Run();
