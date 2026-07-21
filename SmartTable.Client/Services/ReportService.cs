using System.Net.Http.Json;
using Microsoft.JSInterop;

namespace SmartTable.Client.Services;

public record MonthlyReportRow(
    string CourseType, string Language,
    int EnrolledCount, int ArrivedCount, int CompletedCount, int NoShowCount, int DroppedCount,
    decimal AmountDue, decimal AmountPaid, decimal Remaining,
    decimal CashSum, decimal TransferSum, decimal CompanySum, decimal ServicesSum);

public record TermReportPerson(
    string StudentName, string AttendanceStatus, string DocumentsStatus,
    string PaymentStatus, string? PaymentMethod, decimal AmountDue, decimal AmountPaid, decimal Remaining);

public record TermReport(
    string CourseType, string Language, DateTime StartDate, DateTime EndDate,
    List<TermReportPerson> People,
    decimal TotalDue, decimal TotalPaid, decimal TotalRemaining);

public interface IReportService
{
    Task<List<MonthlyReportRow>> GetMonthlyReportAsync(int year, int month, Guid? courseId);
    Task<TermReport?> GetTermReportAsync(Guid termId);
    Task DownloadMonthlyExcelAsync(int year, int month, Guid? courseId);
    Task DownloadTermExcelAsync(Guid termId);
}

public class ReportService : IReportService
{
    private readonly HttpClient _http;
    private readonly IJSRuntime _js;

    public ReportService(HttpClient http, IJSRuntime js)
    {
        _http = http;
        _js = js;
    }

    public async Task<List<MonthlyReportRow>> GetMonthlyReportAsync(int year, int month, Guid? courseId)
    {
        var url = $"api/reports/monthly?year={year}&month={month}" + (courseId.HasValue ? $"&courseId={courseId}" : "");
        var result = await _http.GetFromJsonAsync<List<MonthlyReportRow>>(url);
        return result ?? new List<MonthlyReportRow>();
    }

    public async Task<TermReport?> GetTermReportAsync(Guid termId)
    {
        return await _http.GetFromJsonAsync<TermReport>($"api/reports/term/{termId}");
    }

    public async Task DownloadMonthlyExcelAsync(int year, int month, Guid? courseId)
    {
        var url = $"api/reports/monthly/export?year={year}&month={month}" + (courseId.HasValue ? $"&courseId={courseId}" : "");
        await DownloadFile(url, $"Raport_{month:00}_{year}.xlsx");
    }

    public async Task DownloadTermExcelAsync(Guid termId)
    {
        await DownloadFile($"api/reports/term/{termId}/export", "Termin.xlsx");
    }

    private async Task DownloadFile(string url, string fallbackFileName)
    {
        var response = await _http.GetAsync(url);
        if (!response.IsSuccessStatusCode) return;

        var bytes = await response.Content.ReadAsByteArrayAsync();
        var base64 = Convert.ToBase64String(bytes);

        var fileName = response.Content.Headers.ContentDisposition?.FileNameStar
                        ?? response.Content.Headers.ContentDisposition?.FileName
                        ?? fallbackFileName;

        await _js.InvokeVoidAsync("smartTableFileSaver.saveAsFile", fileName.Trim('"'), base64);
    }
}
