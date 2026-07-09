# SmartTable — Blazor WebAssembly Frontend

Інтерфейс для системи управління курсами. Побудований на Blazor WebAssembly + MudBlazor, темна тема.

---

## Структура проекту

```
SmartTable.Client/
├── App.razor                        ← Кореневий маршрутизатор
├── _Imports.razor                   ← Глобальні @using
├── Program.cs                       ← DI, HttpClient, MudBlazor
├── SmartTable.Client.csproj
│
├── Models/
│   └── AppModels.cs                 ← Student, Course, Enrollment, AuditLog
│
├── Services/
│   └── ApiServices.cs               ← IStudentService, ICourseService,
│                                       IEnrollmentService, IAuditService
│
├── Layout/
│   └── MainLayout.razor             ← Темна тема, бічне меню, AppBar
│
├── Pages/
│   ├── Students.razor               ← DataGrid + пошук + сортування
│   ├── Enrollments.razor            ← DataGrid реєстрацій + фільтри
│   └── AdminLogs.razor              ← Аудит логи [Authorize(Roles="Admin")]
│
├── Components/
│   ├── StudentDialog.razor          ← Діалог: форма студента
│   ├── EnrollmentDialog.razor       ← Діалог: форма реєстрації
│   └── SmartPriceField.razor        ← 🔑 MudAutocomplete для цін
│
└── wwwroot/
    ├── index.html                   ← Host page, Inter font, dark scrollbar
    └── appsettings.json             ← { "ApiBaseUrl": "..." }
```

---

## Кроки налаштування

### 1. Встановити Blazor WebAssembly у вашому проекті

```bash
# Перейдіть у папку SmartTable (батьківська для CourseManagementApi)
cd C:\Users\ypkab\Documents\SmartTable

# Створити WASM проект
dotnet new blazorwasm -n SmartTable.Client -o SmartTable.Client

# Перейти в нього
cd SmartTable.Client

# Видалити шаблонний сміттник
Remove-Item -Recurse -Force Pages, Shared, Layout, wwwroot\sample-data
```

### 2. Встановити MudBlazor

```bash
dotnet add package MudBlazor
```

### 3. Скопіювати файли з цього архіву

Скопіюйте всі файли з папки `SmartTable.Client/` цього архіву у відповідні папки проекту.

> **Важливо:** Замініть `App.razor`, `_Imports.razor`, `Program.cs` і `wwwroot/index.html`.

### 4. Налаштувати URL API

Відкрийте `wwwroot/appsettings.json` та вкажіть реальну адресу вашого ASP.NET Core API:

```json
{
  "ApiBaseUrl": "https://localhost:7001/"
}
```

Порт `7001` — типовий для `dotnet run` з HTTPS. Перевірте у `CourseManagementApi/Properties/launchSettings.json`.

### 5. Запустити обидва проекти

```bash
# Термінал 1 — API
cd CourseManagementApi
dotnet run

# Термінал 2 — Blazor UI
cd SmartTable.Client
dotnet run
```

---

## Ключові компоненти

### SmartPriceField — розумне введення цін

```razor
<SmartPriceField Label="Ціна курсу"
                 @bind-Value="_coursePrice"
                 Presets="new[]{ 800m, 1200m, 2600m }"
                 Required="true" />
```

**Як це працює:**
- При кліку показує список стандартних сум (пресети)
- Якщо ввести будь-яке число — воно приймається як **Custom Input**
- Повертає `decimal?` через двостороннє прив'язування

### AdminLogs — клікабельна навігація

```csharp
// IAuditService.GetNavigationUrl() повертає маршрут:
// Student    → /students/{id}
// Enrollment → /enrollments/{id}

private void NavigateToEntity(AuditLog log)
{
    var url = AuditSvc.GetNavigationUrl(log);
    Nav.NavigateTo(url);
}
```

При переході на `/students/{id}` або `/enrollments/{id}` — сторінка автоматично відкриває діалог редагування відповідного запису.

---

## Що залишилось зробити на бекенді

Для повної роботи інтерфейсу ваш `CourseManagementApi` потребує:

### 1. Додати CORS для Blazor

```csharp
// Program.cs
builder.Services.AddCors(opt =>
    opt.AddDefaultPolicy(policy =>
        policy.WithOrigins("https://localhost:5001") // порт Blazor
              .AllowAnyHeader()
              .AllowAnyMethod()));

// ...
app.UseCors();
```

### 2. Додати ендпоінти PUT/DELETE для Students та Enrollments

Поточні контролери мають лише `GET` та `POST`. Потрібно додати:

```csharp
// StudentsController.cs
[HttpPut("{id}")]
public async Task<IActionResult> UpdateStudent(Guid id, Student student) { ... }

[HttpDelete("{id}")]
public async Task<IActionResult> DeleteStudent(Guid id) { ... }
```

### 3. AuditLog таблиця та контролер

Якщо `AuditLog` ще не реалізовано — або підключіть бібліотеку EF Core Audit, або:

```csharp
// AppModels.cs — додати
[Table("audit_logs")]
public class AuditLog
{
    [Key] public Guid Id { get; set; } = Guid.NewGuid();
    public string EntityType { get; set; } = string.Empty;
    public Guid EntityId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string? ChangedBy { get; set; }
    public DateTime ChangedAt { get; set; } = DateTime.UtcNow;
    public string? FieldName { get; set; }
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
}

// ApplicationDbContext.cs — додати
public DbSet<AuditLog> AuditLogs { get; set; } = null!;
```

---

## Кольорова схема (MudTheme)

| Елемент | Колір |
|---|---|
| Primary (акцент) | `#7C8CF8` (індиго) |
| Background | `#0F1117` |
| Surface (карти) | `#1A1D27` |
| AppBar / Drawer | `#13161F` |
| Success | `#22D3A0` |
| Warning | `#FBBF24` |
| Error | `#F87171` |
