using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CourseManagementApi.Data;
using CourseManagementApi.Data.Entities;

namespace CourseManagementApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ReportsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ── DTO для рядка місячного звіту ──────────────────────────────────
        public record MonthlyReportRow(
            string CourseType, string Language,
            int EnrolledCount, int ArrivedCount, int CompletedCount, int NoShowCount, int DroppedCount,
            decimal AmountDue, decimal AmountPaid, decimal Remaining,
            decimal CashSum, decimal TransferSum, decimal CompanySum, decimal ServicesSum);

        // GET: api/reports/monthly?year=2026&month=8&courseId=..(опційно)
        [HttpGet("monthly")]
        public async Task<ActionResult<List<MonthlyReportRow>>> GetMonthlyReport(
            [FromQuery] int year, [FromQuery] int month, [FromQuery] Guid? courseId)
        {
            var rows = await BuildMonthlyReportRows(year, month, courseId);
            return Ok(rows);
        }

        // GET: api/reports/monthly/export?year=2026&month=8&courseId=..
        [HttpGet("monthly/export")]
        public async Task<IActionResult> ExportMonthlyReport(
            [FromQuery] int year, [FromQuery] int month, [FromQuery] Guid? courseId)
        {
            var rows = await BuildMonthlyReportRows(year, month, courseId);

            using var workbook = new XLWorkbook();
            var sheet = workbook.Worksheets.Add($"Raport {month:00}.{year}");

            string[] headers = {
                "Kurs", "Język", "Zapisani", "Dojechali", "Ukończyli", "Nie dojechali", "Skreśleni",
                "Kwota naliczona", "Kwota zapłacona", "Pozostało", "Gotówka", "Przelew", "Firma", "Usługi dodatkowe"
            };
            for (int i = 0; i < headers.Length; i++) sheet.Cell(1, i + 1).Value = headers[i];
            sheet.Row(1).Style.Font.Bold = true;

            int r = 2;
            foreach (var row in rows)
            {
                sheet.Cell(r, 1).Value = row.CourseType;
                sheet.Cell(r, 2).Value = row.Language;
                sheet.Cell(r, 3).Value = row.EnrolledCount;
                sheet.Cell(r, 4).Value = row.ArrivedCount;
                sheet.Cell(r, 5).Value = row.CompletedCount;
                sheet.Cell(r, 6).Value = row.NoShowCount;
                sheet.Cell(r, 7).Value = row.DroppedCount;
                sheet.Cell(r, 8).Value = row.AmountDue;
                sheet.Cell(r, 9).Value = row.AmountPaid;
                sheet.Cell(r, 10).Value = row.Remaining;
                sheet.Cell(r, 11).Value = row.CashSum;
                sheet.Cell(r, 12).Value = row.TransferSum;
                sheet.Cell(r, 13).Value = row.CompanySum;
                sheet.Cell(r, 14).Value = row.ServicesSum;
                r++;
            }
            sheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            var fileName = $"Raport_{month:00}_{year}.xlsx";
            return File(stream.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        private async Task<List<MonthlyReportRow>> BuildMonthlyReportRows(int year, int month, Guid? courseId)
        {
            var termsQuery = _context.CourseTerms.Include(t => t.Course)
                .Where(t => t.StartDate.Year == year && t.StartDate.Month == month);
            if (courseId.HasValue) termsQuery = termsQuery.Where(t => t.CourseId == courseId.Value);
            var terms = await termsQuery.ToListAsync();
            var termIds = terms.Select(t => t.Id).ToList();

            var enrollments = await _context.Enrollments
                .Where(e => e.CourseTermId != null && termIds.Contains(e.CourseTermId.Value))
                .Include(e => e.Course)
                .ToListAsync();

            var rows = new List<MonthlyReportRow>();
            foreach (var group in enrollments.GroupBy(e => new { e.CourseId, CourseType = e.Course!.CourseType, Language = e.Course!.Language }))
            {
                decimal AmountDueOf(Enrollment e) =>
                    e.CoursePrice
                    + (e.SvcMedical ? (e.MedicalExamPrice ?? 0) : 0)
                    + (e.SvcPsychological ? (e.PsychologicalExamPrice ?? 0) : 0)
                    + (e.SvcTranslation ? (e.TranslationPrice ?? 0) : 0)
                    + (e.SvcPowerOfAttorney ? (e.PowerOfAttorneyPrice ?? 0) : 0)
                    + (e.PkzPrice ?? 0);

                decimal ServicesOf(Enrollment e) =>
                    (e.SvcMedical ? (e.MedicalExamPrice ?? 0) : 0)
                    + (e.SvcPsychological ? (e.PsychologicalExamPrice ?? 0) : 0)
                    + (e.SvcTranslation ? (e.TranslationPrice ?? 0) : 0)
                    + (e.SvcPowerOfAttorney ? (e.PowerOfAttorneyPrice ?? 0) : 0)
                    + (e.PkzPrice ?? 0);

                var due = group.Sum(AmountDueOf);
                var paid = group.Sum(e => e.AmountPaid);

                rows.Add(new MonthlyReportRow(
                    group.Key.CourseType, group.Key.Language,
                    EnrolledCount: group.Count(),
                    ArrivedCount: group.Count(e => e.AttendanceStatus == "arrived"),
                    CompletedCount: group.Count(e => e.Status == "Finished"),
                    NoShowCount: group.Count(e => e.AttendanceStatus == "no_show"),
                    DroppedCount: group.Count(e => e.Status == "Dropped"),
                    AmountDue: due,
                    AmountPaid: paid,
                    Remaining: due - paid,
                    CashSum: group.Where(e => e.PaymentMethod == "cash").Sum(e => e.AmountPaid),
                    TransferSum: group.Where(e => e.PaymentMethod == "bank_transfer").Sum(e => e.AmountPaid),
                    CompanySum: group.Where(e => e.PaymentMethod == "company").Sum(e => e.AmountPaid),
                    ServicesSum: group.Sum(ServicesOf)));
            }

            return rows.OrderBy(r => r.CourseType).ThenBy(r => r.Language).ToList();
        }

        // ── DTO для звіту по конкретному терміну ───────────────────────────
        public record TermReportPerson(
            string StudentName, string AttendanceStatus, string DocumentsStatus,
            string PaymentStatus, string? PaymentMethod, decimal AmountDue, decimal AmountPaid, decimal Remaining);

        public record TermReport(
            string CourseType, string Language, DateTime StartDate, DateTime EndDate,
            List<TermReportPerson> People,
            decimal TotalDue, decimal TotalPaid, decimal TotalRemaining);

        // GET: api/reports/term/{termId}
        [HttpGet("term/{termId}")]
        public async Task<ActionResult<TermReport>> GetTermReport(Guid termId)
        {
            var report = await BuildTermReport(termId);
            if (report == null) return NotFound();
            return Ok(report);
        }

        // GET: api/reports/term/{termId}/export
        [HttpGet("term/{termId}/export")]
        public async Task<IActionResult> ExportTermReport(Guid termId)
        {
            var report = await BuildTermReport(termId);
            if (report == null) return NotFound();

            using var workbook = new XLWorkbook();
            var sheet = workbook.Worksheets.Add("Termin");

            sheet.Cell(1, 1).Value = $"{report.CourseType} {report.Language} — {report.StartDate:dd.MM.yyyy} do {report.EndDate:dd.MM.yyyy}";
            sheet.Range(1, 1, 1, 7).Merge();
            sheet.Row(1).Style.Font.Bold = true;

            string[] headers = { "Student", "Obecność", "Dokumenty", "Status płatności", "Metoda płatności", "Kwota naliczona", "Kwota zapłacona" };
            for (int i = 0; i < headers.Length; i++) sheet.Cell(3, i + 1).Value = headers[i];
            sheet.Row(3).Style.Font.Bold = true;

            int r = 4;
            foreach (var p in report.People)
            {
                sheet.Cell(r, 1).Value = p.StudentName;
                sheet.Cell(r, 2).Value = p.AttendanceStatus;
                sheet.Cell(r, 3).Value = p.DocumentsStatus;
                sheet.Cell(r, 4).Value = p.PaymentStatus;
                sheet.Cell(r, 5).Value = p.PaymentMethod ?? "—";
                sheet.Cell(r, 6).Value = p.AmountDue;
                sheet.Cell(r, 7).Value = p.AmountPaid;
                r++;
            }

            r += 1;
            sheet.Cell(r, 1).Value = "Razem:";
            sheet.Cell(r, 6).Value = report.TotalDue;
            sheet.Cell(r, 7).Value = report.TotalPaid;
            sheet.Row(r).Style.Font.Bold = true;

            sheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            var fileName = $"Termin_{report.StartDate:yyyyMMdd}_{report.CourseType}_{report.Language}.xlsx";
            return File(stream.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        private async Task<TermReport?> BuildTermReport(Guid termId)
        {
            var term = await _context.CourseTerms.Include(t => t.Course).FirstOrDefaultAsync(t => t.Id == termId);
            if (term == null) return null;

            var enrollments = await _context.Enrollments
                .Include(e => e.Student)
                .Where(e => e.CourseTermId == termId)
                .ToListAsync();

            decimal AmountDueOf(Enrollment e) =>
                e.CoursePrice
                + (e.SvcMedical ? (e.MedicalExamPrice ?? 0) : 0)
                + (e.SvcPsychological ? (e.PsychologicalExamPrice ?? 0) : 0)
                + (e.SvcTranslation ? (e.TranslationPrice ?? 0) : 0)
                + (e.SvcPowerOfAttorney ? (e.PowerOfAttorneyPrice ?? 0) : 0)
                + (e.PkzPrice ?? 0);

            var people = enrollments.Select(e => new TermReportPerson(
                e.Student?.FullName ?? "—", e.AttendanceStatus, e.DocumentsStatus,
                e.PaymentStatus, e.PaymentMethod, AmountDueOf(e), e.AmountPaid, AmountDueOf(e) - e.AmountPaid
            )).ToList();

            return new TermReport(
                term.Course?.CourseType ?? "", term.Course?.Language ?? "",
                term.StartDate, term.EndDate, people,
                people.Sum(p => p.AmountDue), people.Sum(p => p.AmountPaid), people.Sum(p => p.Remaining));
        }
    }
}
