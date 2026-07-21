using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CourseManagementApi.Data;
using CourseManagementApi.Data.Entities;
using CourseManagementApi.Auth;

namespace CourseManagementApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CourseTermsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IAuditLogger _audit;

        public CourseTermsController(ApplicationDbContext context, IAuditLogger audit)
        {
            _context = context;
            _audit = audit;
        }

        // GET: api/courseterms                -> усі терміни всіх курсів
        // GET: api/courseterms?courseId=..    -> терміни конкретного курсу
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CourseTerm>>> GetAll([FromQuery] Guid? courseId)
        {
            var query = _context.CourseTerms.Include(t => t.Course).AsQueryable();

            if (courseId.HasValue)
            {
                query = query.Where(t => t.CourseId == courseId.Value);
            }

            return await query.OrderBy(t => t.StartDate).ToListAsync();
        }

        [HttpPost]
        public async Task<ActionResult<CourseTerm>> Create(CourseTerm model)
        {
            var courseExists = await _context.Courses.AnyAsync(c => c.Id == model.CourseId);
            if (!courseExists)
            {
                return BadRequest("Курсу з таким Id не існує.");
            }

            model.Id = Guid.NewGuid();
            model.CreatedAt = DateTime.UtcNow;
            if (string.IsNullOrWhiteSpace(model.Source)) model.Source = "manual";

            _context.CourseTerms.Add(model);
            await _context.SaveChangesAsync();
            await _audit.LogAsync("CourseTerm", model.Id, $"Utworzono termin kursu {model.StartDate:dd.MM.yyyy}–{model.EndDate:dd.MM.yyyy}");

            return CreatedAtAction(nameof(GetAll), new { id = model.Id }, model);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, CourseTerm model)
        {
            if (id != model.Id) return BadRequest();

            model.ManuallyModified = true;
            _context.Entry(model).State = EntityState.Modified;
            _context.Entry(model).Property(x => x.CreatedAt).IsModified = false;
            _context.Entry(model).Property(x => x.Source).IsModified = false;

            try { await _context.SaveChangesAsync(); }
            catch (DbUpdateConcurrencyException) { if (!_context.CourseTerms.Any(t => t.Id == id)) return NotFound(); else throw; }

            await _audit.LogAsync("CourseTerm", id, "Zaktualizowano termin kursu");
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var term = await _context.CourseTerms.FindAsync(id);
            if (term == null) return NotFound();

            _context.CourseTerms.Remove(term);
            await _context.SaveChangesAsync();
            await _audit.LogAsync("CourseTerm", id, "Usunięto termin kursu");

            return NoContent();
        }
    }
}
