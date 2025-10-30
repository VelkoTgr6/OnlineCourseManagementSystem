using OnlineCourseManagementSystem.Core.Models.Course;
using OnlineCourseManagementSystem.Infrastructure.Data.Models;

namespace OnlineCourseManagementSystem.Core.Contracts
{
    public interface ICourseService
    {
        Task<IEnumerable<CourseViewModel>> GetAllAsync();
        Task<CourseViewModel?> GetByIdAsync(int id);
        Task<int> CreateAsync(CreateCourseFormModel courseDto);
        Task<IEnumerable<CourseStudentViewModel>> GerStudentsByCourseIdAsync(int courseId);
        Task AddAsync(Course course);
        Task<int> UpdateAsync(int id,UpdateCourseFormModel course);
        Task DeleteAsync(int id);
    }
}
