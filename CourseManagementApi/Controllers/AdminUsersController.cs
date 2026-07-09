using System.Security.Claims;
using CourseManagementApi.Auth;
using CourseManagementApi.Data;
using CourseManagementApi.Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CourseManagementApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "SuperAdmin")] // Увесь контролер — лише для SuperAdmin
    public class AdminUsersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IAuditLogger _audit;

        public AdminUsersController(ApplicationDbContext context, IAuditLogger audit)
        {
            _context = context;
            _audit = audit;
        }

        public record UserDto(Guid Id, string Username, string Role, DateTime CreatedAt);
        public record CreateAdminRequest(string Username, string Password);

        // GET: api/adminusers
        // Список адмінів. Паролі/хеші НІКОЛИ не повертаються.
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetUsers()
        {
            var users = await _context.Users
                .OrderBy(u => u.Username)
                .Select(u => new UserDto(u.Id, u.Username, u.Role, u.CreatedAt))
                .ToListAsync();

            return Ok(users);
        }

        // POST: api/adminusers
        // Створює нового звичайного адміна (роль завжди "Admin" —
        // через цей ендпоінт не можна створити ще одного SuperAdmin).
        [HttpPost]
        public async Task<ActionResult<UserDto>> CreateAdmin(CreateAdminRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest(new { message = "Login i hasło są wymagane." });
            }

            if (request.Password.Length < 8)
            {
                return BadRequest(new { message = "Hasło musi mieć co najmniej 8 znaków." });
            }

            var exists = await _context.Users.AnyAsync(u => u.Username == request.Username);
            if (exists)
            {
                return Conflict(new { message = "Taki login już istnieje." });
            }

            var (hash, salt) = PasswordHasher.Hash(request.Password);
            var user = new AppUser
            {
                Id = Guid.NewGuid(),
                Username = request.Username,
                PasswordHash = hash,
                PasswordSalt = salt,
                Role = "Admin",
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            await _audit.LogAsync("AppUser", user.Id, $"Utworzono konto admina '{user.Username}'");

            return Ok(new UserDto(user.Id, user.Username, user.Role, user.CreatedAt));
        }

        // DELETE: api/adminusers/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAdmin(Guid id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user is null)
            {
                return NotFound();
            }

            if (user.Role == "SuperAdmin")
            {
                return BadRequest(new { message = "Nie można usunąć konta SuperAdmin." });
            }

            var currentUsername = User.FindFirstValue(ClaimTypes.Name);
            if (string.Equals(user.Username, currentUsername, StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest(new { message = "Nie można usunąć własnego konta." });
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            await _audit.LogAsync("AppUser", id, $"Usunięto konto admina '{user.Username}'");

            return NoContent();
        }
    }
}
