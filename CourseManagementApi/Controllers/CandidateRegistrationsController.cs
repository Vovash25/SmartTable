using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CourseManagementApi.Data;
using CourseManagementApi.Data.Entities;
using CourseManagementApi.Auth;

namespace CourseManagementApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CandidateRegistrationsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IAuditLogger _audit;

        public CandidateRegistrationsController(ApplicationDbContext context, IAuditLogger audit)
        {
            _context = context;
            _audit = audit;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<CandidateRegistration>>> GetRegistrations()
        {
            return await _context.CandidateRegistrations.OrderByDescending(r => r.CreatedAt).ToListAsync();
        }

        [HttpPost]
        public async Task<ActionResult<CandidateRegistration>> CreateRegistration(CandidateRegistration registration)
        {
            var existingDuplicate = await _context.CandidateRegistrations.FirstOrDefaultAsync(r => r.PhoneE164 == registration.PhoneE164);

            if (existingDuplicate != null && registration.Status != "duplicate")
            {
                return Conflict(new { Message = "Możliwy duplikat. Osoba o podobnych danych już istnieje w systemie.", ExistingId = existingDuplicate.Id });
            }

            registration.CreatedAt = DateTime.UtcNow;
            _context.CandidateRegistrations.Add(registration);
            await _context.SaveChangesAsync();
            await _audit.LogAsync("CandidateRegistration", registration.Id, $"Utworzono kandydata '{registration.FullName}'");

            return CreatedAtAction(nameof(GetRegistrations), new { id = registration.Id }, registration);
        }

        [HttpPost("{id}/assign")]
        public async Task<IActionResult> AssignToCourse(Guid id, [FromBody] List<Guid> courseIds)
        {
            var candidate = await _context.CandidateRegistrations.FindAsync(id);
            if (candidate == null) return NotFound("Кандидата не знайдено.");

            if (courseIds == null || !courseIds.Any()) return BadRequest("Не обрано жодного курсу.");

            var courses = await _context.Courses.Where(c => courseIds.Contains(c.Id)).ToListAsync();
            if (!courses.Any()) return BadRequest("Організовані курси не знайдено.");

            var student = await _context.Students.FirstOrDefaultAsync(s => s.FullName == candidate.FullName);
            if (student == null)
            {
                student = new Student
                {
                    Id = Guid.NewGuid(),
                    FullName = candidate.FullName,
                    Citizenship = "", 
                    DateOfBirth = DateTime.UtcNow, 
                    CreatedAt = DateTime.UtcNow
                };
                _context.Students.Add(student);
            }

            foreach (var course in courses)
            {
                var existingEnrollment = await _context.Enrollments
                    .FirstOrDefaultAsync(e => e.StudentId == student.Id && e.CourseId == course.Id);

                if (existingEnrollment == null)
                {
                    var enrollment = new Enrollment
                    {
                        Id = Guid.NewGuid(),
                        StudentId = student.Id,
                        CourseId = course.Id,
                        ArrivalDate = candidate.PlannedArrivalDate ?? candidate.PreferredCourseDate ?? DateTime.UtcNow,
                        CompanyName = candidate.PayerType == "company" ? candidate.CompanyName : null,
                        NeedsHotel = candidate.NeedsHotel == "yes",
                        HotelStayRange = (candidate.HotelFrom.HasValue && candidate.HotelTo.HasValue) 
                                            ? $"{candidate.HotelFrom:dd.MM} - {candidate.HotelTo:dd.MM}" 
                                            : null,
                        HasInvoice = candidate.PayerType == "company",
                        Notes = $"Przeniesiono z rejestracji. {candidate.RegistrationNotes} {candidate.HotelNotes}",
                        Status = "Active"
                    };
                    _context.Enrollments.Add(enrollment);
                }
            }

            candidate.Status = "assigned_to_course";
            candidate.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            await _audit.LogAsync("CandidateRegistration", candidate.Id, $"Przypisano kandydata '{candidate.FullName}' do {courses.Count} kurs(ów)");

            return Ok(new { StudentId = student.Id });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateRegistration(Guid id, CandidateRegistration registration)
        {
            if (id != registration.Id) return BadRequest();

            registration.UpdatedAt = DateTime.UtcNow;
            _context.Entry(registration).State = EntityState.Modified;
            _context.Entry(registration).Property(x => x.CreatedAt).IsModified = false;
            _context.Entry(registration).Property(x => x.CreatedBy).IsModified = false;

            try { await _context.SaveChangesAsync(); }
            catch (DbUpdateConcurrencyException) { if (!_context.CandidateRegistrations.Any(e => e.Id == id)) return NotFound(); else throw; }

            await _audit.LogAsync("CandidateRegistration", id, $"Zaktualizowano kandydata '{registration.FullName}'");
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRegistration(Guid id)
        {
            var registration = await _context.CandidateRegistrations.FindAsync(id);
            if (registration == null) return NotFound();

            _context.CandidateRegistrations.Remove(registration);
            await _context.SaveChangesAsync();
            await _audit.LogAsync("CandidateRegistration", id, $"Usunięto kandydata '{registration.FullName}'");

            return NoContent();
        }
    }
}
