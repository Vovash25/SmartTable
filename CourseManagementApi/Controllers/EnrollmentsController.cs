using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CourseManagementApi.Data;
using CourseManagementApi.Data.Entities;
using CourseManagementApi.Auth;

namespace CourseManagementApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EnrollmentsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IAuditLogger _audit;

        public EnrollmentsController(ApplicationDbContext context, IAuditLogger audit)
        {
            _context = context;
            _audit = audit;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Enrollment>>> GetEnrollments()
        {
            return await _context.Enrollments
                .Include(e => e.Student)
                .Include(e => e.Course)
                .Include(e => e.CourseTerm)
                .ToListAsync();
        }

        [HttpPost]
        public async Task<ActionResult<Enrollment>> CreateEnrollment(Enrollment enrollment)
        {
            var studentExists = await _context.Students.AnyAsync(s => s.Id == enrollment.StudentId);
            var courseExists = await _context.Courses.AnyAsync(c => c.Id == enrollment.CourseId);

            if (!studentExists || !courseExists)
            {
                return BadRequest("Студента або курсу з таким ID не існує в базі даних.");
            }

            _context.Enrollments.Add(enrollment);
            await _context.SaveChangesAsync();
            await _audit.LogAsync("Enrollment", enrollment.Id, "Utworzono zapis na kurs");

            return CreatedAtAction(nameof(GetEnrollments), new { id = enrollment.Id }, enrollment);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateEnrollment(Guid id, Enrollment enrollment)
        {
            if (id != enrollment.Id)
            {
                return BadRequest();
            }

            var studentExists = await _context.Students.AnyAsync(s => s.Id == enrollment.StudentId);
            var courseExists = await _context.Courses.AnyAsync(c => c.Id == enrollment.CourseId);

            if (!studentExists || !courseExists)
            {
                return BadRequest("Студента або курсу з таким ID не існує.");
            }

            _context.Entry(enrollment).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Enrollments.Any(e => e.Id == id)) return NotFound();
                else throw;
            }

            await _audit.LogAsync("Enrollment", id, "Zaktualizowano dane zapisu na kurs");
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEnrollment(Guid id)
        {
            var enrollment = await _context.Enrollments.FindAsync(id);
            if (enrollment == null)
            {
                return NotFound();
            }

            _context.Enrollments.Remove(enrollment);
            await _context.SaveChangesAsync();
            await _audit.LogAsync("Enrollment", id, "Usunięto zapis na kurs");

            return NoContent();
        }
    }
}
