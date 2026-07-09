using System.Security.Claims;
using CourseManagementApi.Data;
using CourseManagementApi.Data.Entities;

namespace CourseManagementApi.Auth
{
    public interface IAuditLogger
    {
        Task LogAsync(string entityType, Guid entityId, string action);
    }

    public class AuditLogger : IAuditLogger
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuditLogger(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task LogAsync(string entityType, Guid entityId, string action)
        {
            var username = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.Name)
                ?? "unknown";

            _context.AuditLogs.Add(new AuditLog
            {
                Id = Guid.NewGuid(),
                ChangedBy = username,
                EntityType = entityType,
                EntityId = entityId,
                Action = action,
                Timestamp = DateTime.UtcNow
            });

            // Окремий SaveChanges, щоб запис логу не залежав від того,
            // чи ще не закомічена транзакція основної дії.
            await _context.SaveChangesAsync();
        }
    }
}
