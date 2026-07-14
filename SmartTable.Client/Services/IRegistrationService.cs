using SmartTable.Client.Models;

namespace SmartTable.Client.Services
{
    // Один пункт призначення: конкретний курс + (опційно) конкретна дата цього курсу.
    public record CourseAssignmentItem(Guid CourseId, Guid? CourseDateId);

    public interface IRegistrationService
    {
        Task<List<CandidateRegistration>> GetAllAsync();
        Task<CandidateRegistration?> CreateAsync(CandidateRegistration model);
        Task<bool> UpdateAsync(CandidateRegistration model);
        Task<bool> DeleteAsync(Guid id);

        // Призначення кандидата на один або кілька курсів, для кожного —
        // з опційно обраною конкретною датою проведення.
        Task<bool> AssignToCourseAsync(Guid registrationId, IEnumerable<CourseAssignmentItem> assignments);
    }
}
