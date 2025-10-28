using OnlineCourseManagementSystem.Core.Models.Course;
using OnlineCourseManagementSystem.Infrastructure.Data.Models;

namespace OnlineCourseManagementSystem.Core.Contracts
{
    public interface ICourseService
    {
        Task<IEnumerable<CourseViewModel>> GetAllAsync();
        Task<CourseViewModel?> GetByIdAsync(int id);
        Task CreateAsync(CreateCourseFormModel courseDto);
        Task AddAsync(Course course);
        Task UpdateAsync(Course course);
        Task DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
    }
}
