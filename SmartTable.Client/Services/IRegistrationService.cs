using SmartTable.Client.Models;

namespace SmartTable.Client.Services
{
    public interface IRegistrationService
    {
        Task<List<CandidateRegistration>> GetAllAsync();
        Task<CandidateRegistration?> CreateAsync(CandidateRegistration model);
        Task<bool> UpdateAsync(CandidateRegistration model);
        Task<bool> DeleteAsync(Guid id);
        
        // Новий метод для перенесення кандидата на курс
        Task<bool> AssignToCourseAsync(Guid registrationId, IEnumerable<Guid> courseIds);
    }
}