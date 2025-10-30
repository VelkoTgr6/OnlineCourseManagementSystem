using OnlineCourseManagementSystem.Core.Models.Enroll;

namespace OnlineCourseManagementSystem.Core.Contracts
{
    public interface IEnrollmentService
    {
        Task<int> CreateAsync(CreateEnrollmentFormModel model);
        Task<int> EnrollStudentAsync(EnrollStudentFormModel model);
        Task UpdateProgressAsync(int enrollmentId, int progress);
        Task DeleteAsync(int enrollmentId);
        Task<int> UpdateEnrollmentAsync(int id,UpdateEnrollmentFormModel model);
        Task<IEnumerable<EnrollViewModel>> GetAllAsync();
        Task<EnrollViewModel?> GetByIdAsync(int enrollmentId);
    }
}
