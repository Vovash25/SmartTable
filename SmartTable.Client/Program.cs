using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Authorization;
using MudBlazor;
using MudBlazor.Services;
using SmartTable.Client;
using SmartTable.Client.Services;


var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// ── API Base Address ──────────────────────────────────────────────────────
// Клієнт і API тепер живуть на одному origin (той самий процес),
// тому просто беремо адресу, з якої завантажена сторінка.
builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)
});

// ── MudBlazor v7+ Configuration ──────────────────────────────────────────
builder.Services.AddMudServices(config =>
{
    config.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.BottomRight;
    config.SnackbarConfiguration.PreventDuplicates = false;
    config.SnackbarConfiguration.NewestOnTop = true;
    config.SnackbarConfiguration.ShowCloseIcon = true;
    config.SnackbarConfiguration.VisibleStateDuration = 4000;
    config.SnackbarConfiguration.HideTransitionDuration = 300;
    config.SnackbarConfiguration.ShowTransitionDuration = 300;
    config.SnackbarConfiguration.SnackbarVariant = Variant.Filled;
});

// ── Authorization & Authentication (реальний JWT-логін) ───────────────────
builder.Services.AddAuthorizationCore(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<CustomAuthStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(sp => sp.GetRequiredService<CustomAuthStateProvider>());
builder.Services.AddScoped<AuthService>();

builder.Services.AddSingleton<LocalizationService>();
// ── Domain Services ───────────────────────────────────────────────────────
builder.Services.AddScoped<IStudentService, StudentService>();
builder.Services.AddScoped<ICourseService, CourseService>();
builder.Services.AddScoped<ICourseTermService, CourseTermService>();
builder.Services.AddScoped<ICourseTermTemplateService, CourseTermTemplateService>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<IEnrollmentService, EnrollmentService>();
builder.Services.AddScoped<IAuditService, AuditService>();
builder.Services.AddScoped<IRegistrationService, RegistrationService>();
builder.Services.AddScoped<IAdminUserService, AdminUserService>();

var host = builder.Build();

// ── "Прогрів" авторизації ПЕРЕД стартом рендера ────────────────────────────
// Без цього перша сторінка після завантаження могла встигнути зробити
// API-запит на мілісекунду раніше, ніж CustomAuthStateProvider встигав
// асинхронно дістати токен з localStorage і прикріпити заголовок
// Authorization — звідси й одноразовий "шум" 401 у консолі. Тепер токен
// гарантовано підхоплений ще до того, як Blazor покаже хоч одну сторінку.
var authStateProvider = host.Services.GetRequiredService<AuthenticationStateProvider>();
await authStateProvider.GetAuthenticationStateAsync();

await host.RunAsync();
