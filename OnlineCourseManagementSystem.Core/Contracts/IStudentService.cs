using OnlineCourseManagementSystem.Core.Models.Student;

namespace OnlineCourseManagementSystem.Core.Contracts
{
    public interface IStudentService
    {
        Task<IEnumerable<StudentViewModel>> GetAllAsync();
        Task<StudentViewModel?> GetByIdAsync(int id);
        Task<int> UpdateAsync(int id,CreateStudentFormModel student);
        Task<int> CourseEnrollmentUpdate(StudentCourseEnrollmentUpdateFormModel model);
        Task<IEnumerable<StudentCoursesViewModel>> GetStudentCoursesAsync(int studentId);
        Task DeleteAsync(int id);
        Task<int> CreateAsync(CreateStudentFormModel model);
    }
}
