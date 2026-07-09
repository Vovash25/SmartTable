using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CourseManagementApi.Data;
using CourseManagementApi.Data.Entities;
using CourseManagementApi.Auth;

namespace CourseManagementApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StudentsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IAuditLogger _audit;

        public StudentsController(ApplicationDbContext context, IAuditLogger audit)
        {
            _context = context;
            _audit = audit;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Student>>> GetStudents()
        {
            return await _context.Students.ToListAsync();
        }

        [HttpPost]
        public async Task<ActionResult<Student>> CreateStudent(Student student)
        {
            _context.Students.Add(student);
            await _context.SaveChangesAsync();
            await _audit.LogAsync("Student", student.Id, $"Utworzono studenta '{student.FullName}'");

            return CreatedAtAction(nameof(GetStudents), new { id = student.Id }, student);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateStudent(Guid id, Student student)
        {
            if (id != student.Id)
            {
                return BadRequest();
            }

            _context.Entry(student).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Students.Any(s => s.Id == id)) return NotFound();
                else throw;
            }

            await _audit.LogAsync("Student", id, $"Zaktualizowano studenta '{student.FullName}'");
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteStudent(Guid id)
        {
            var student = await _context.Students.FindAsync(id);
            if (student == null)
            {
                return NotFound();
            }

            _context.Students.Remove(student);
            await _context.SaveChangesAsync();
            await _audit.LogAsync("Student", id, $"Usunięto studenta '{student.FullName}'");

            return NoContent();
        }
    }
}
