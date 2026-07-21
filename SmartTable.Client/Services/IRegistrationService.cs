using SmartTable.Client.Models;

namespace SmartTable.Client.Services
{
    // Призначення кандидата відбувається ЛИШЕ через вибір уже створеного терміну
    // курсу — без ручного вводу довільної дати.
    public record CourseAssignmentItem(Guid CourseTermId);

    public interface IRegistrationService
    {
        Task<List<CandidateRegistration>> GetAllAsync();
        Task<CandidateRegistration?> CreateAsync(CandidateRegistration model);
        Task<bool> UpdateAsync(CandidateRegistration model);
        Task<bool> DeleteAsync(Guid id);

        Task<bool> AssignToCourseAsync(Guid registrationId, IEnumerable<CourseAssignmentItem> assignments);
    }
}
