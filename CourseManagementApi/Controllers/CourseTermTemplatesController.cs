using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CourseManagementApi.Data;
using CourseManagementApi.Data.Entities;
using CourseManagementApi.Auth;

namespace CourseManagementApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CourseTermTemplatesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IAuditLogger _audit;

        public CourseTermTemplatesController(ApplicationDbContext context, IAuditLogger audit)
        {
            _context = context;
            _audit = audit;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<CourseTermTemplate>>> GetAll([FromQuery] Guid? courseId)
        {
            var query = _context.CourseTermTemplates.Include(t => t.Course).AsQueryable();
            if (courseId.HasValue) query = query.Where(t => t.CourseId == courseId.Value);
            return await query.OrderByDescending(t => t.CreatedAt).ToListAsync();
        }

        [HttpPost]
        public async Task<ActionResult<CourseTermTemplate>> Create(CourseTermTemplate model)
        {
            var courseExists = await _context.Courses.AnyAsync(c => c.Id == model.CourseId);
            if (!courseExists) return BadRequest("Курсу з таким Id не існує.");

            model.Id = Guid.NewGuid();
            model.CreatedAt = DateTime.UtcNow;

            _context.CourseTermTemplates.Add(model);
            await _context.SaveChangesAsync();
            await _audit.LogAsync("CourseTermTemplate", model.Id, "Utworzono szablon terminów");

            return CreatedAtAction(nameof(GetAll), new { id = model.Id }, model);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var template = await _context.CourseTermTemplates.FindAsync(id);
            if (template == null) return NotFound();

            _context.CourseTermTemplates.Remove(template);
            await _context.SaveChangesAsync();
            await _audit.LogAsync("CourseTermTemplate", id, "Usunięto szablon terminów");

            return NoContent();
        }

        public record GenerateResult(int CreatedCount, int SkippedExisting);

        // POST: api/coursetermtemplates/{id}/generate
        // Генерує CourseTerm-записи за правилом шаблону в діапазоні GenerateFrom..GenerateTo.
        // Пропускає дати, для яких термін з таким же CourseId+StartDate вже існує
        // (щоб повторний запуск генератора не плодив дублікати).
        [HttpPost("{id}/generate")]
        public async Task<ActionResult<GenerateResult>> Generate(Guid id)
        {
            var template = await _context.CourseTermTemplates.FindAsync(id);
            if (template == null) return NotFound();

            if (template.Mode != "cyclic" || template.Frequency == "manual_only")
            {
                return BadRequest("Цей шаблон не призначений для автоматичної генерації (режим 'ręczny'/'tylko ręcznie').");
            }

            if (template.DayOfWeek is null)
            {
                return BadRequest("Для циклічної генерації потрібно вказати день тижня.");
            }

            var existingStartDates = await _context.CourseTerms
                .Where(t => t.CourseId == template.CourseId)
                .Select(t => t.StartDate.Date)
                .ToListAsync();
            var existingSet = existingStartDates.ToHashSet();

            var stepDays = template.Frequency switch
            {
                "weekly" => 7,
                "biweekly" => 14,
                "every_3_weeks" => 21,
                "monthly" => -1, // спеціальний випадок — рахуємо місяцями, не днями
                _ => 7
            };

            var targetDayOfWeek = (DayOfWeek)template.DayOfWeek.Value;

            // Знаходимо першу дату в діапазоні, що збігається з потрібним днем тижня
            var cursor = template.GenerateFrom.Date;
            while (cursor.DayOfWeek != targetDayOfWeek && cursor <= template.GenerateTo.Date)
            {
                cursor = cursor.AddDays(1);
            }

            var created = 0;
            var skipped = 0;
            var newTerms = new List<CourseTerm>();

            while (cursor.Date <= template.GenerateTo.Date)
            {
                if (existingSet.Contains(cursor.Date))
                {
                    skipped++;
                }
                else
                {
                    var term = new CourseTerm
                    {
                        Id = Guid.NewGuid(),
                        CourseId = template.CourseId,
                        StartDate = cursor,
                        EndDate = cursor.AddDays(Math.Max(template.DefaultDurationDays - 1, 0)),
                        DefaultPrice = template.DefaultPrice,
                        SeatLimit = template.SeatLimit,
                        Status = "planned",
                        Source = "generated",
                        ManuallyModified = false,
                        Notes = template.Notes,
                        CreatedAt = DateTime.UtcNow
                    };
                    newTerms.Add(term);
                    existingSet.Add(cursor.Date);
                    created++;
                }

                cursor = template.Frequency == "monthly"
                    ? cursor.AddMonths(1)
                    : cursor.AddDays(stepDays);
            }

            if (newTerms.Any())
            {
                _context.CourseTerms.AddRange(newTerms);
                await _context.SaveChangesAsync();
                await _audit.LogAsync("CourseTermTemplate", template.Id, $"Wygenerowano {created} termin(ów) z szablonu");
            }

            return Ok(new GenerateResult(created, skipped));
        }
    }
}
