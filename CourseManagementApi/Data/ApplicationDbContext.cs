using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CourseManagementApi.Data;
using CourseManagementApi.Data.Entities;
using CourseManagementApi.Auth;

namespace CourseManagementApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CourseDatesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IAuditLogger _audit;

        public CourseDatesController(ApplicationDbContext context, IAuditLogger audit)
        {
            _context = context;
            _audit = audit;
        }

        // GET: api/coursedates            -> усі дати всіх курсів
        // GET: api/coursedates?courseId=.. -> дати конкретного курсу
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CourseDate>>> GetAll([FromQuery] Guid? courseId)
        {
            var query = _context.CourseDates
                .Include(cd => cd.Course)
                .AsQueryable();

            if (courseId.HasValue)
            {
                query = query.Where(cd => cd.CourseId == courseId.Value);
            }

            return await query.OrderBy(cd => cd.StartDate).ToListAsync();
        }

        [HttpPost]
        public async Task<ActionResult<CourseDate>> Create(CourseDate model)
        {
            var courseExists = await _context.Courses.AnyAsync(c => c.Id == model.CourseId);
            if (!courseExists)
            {
                return BadRequest("Курсу з таким Id не існує.");
            }

            model.Id = Guid.NewGuid();
            model.CreatedAt = DateTime.UtcNow;

            _context.CourseDates.Add(model);
            await _context.SaveChangesAsync();
            await _audit.LogAsync("CourseDate", model.Id, $"Utworzono termin kursu od {model.StartDate:dd.MM.yyyy}");

            return CreatedAtAction(nameof(GetAll), new { id = model.Id }, model);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var date = await _context.CourseDates.FindAsync(id);
            if (date == null)
            {
                return NotFound();
            }

            _context.CourseDates.Remove(date);
            await _context.SaveChangesAsync();
            await _audit.LogAsync("CourseDate", id, "Usunięto termin kursu");

            return NoContent();
        }
    }
}
