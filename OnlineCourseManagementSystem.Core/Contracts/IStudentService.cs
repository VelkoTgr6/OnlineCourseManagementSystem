using OnlineCourseManagementSystem.Core.Models.Student;
using OnlineCourseManagementSystem.Infrastructure.Data.Models;

namespace OnlineCourseManagementSystem.Core.Contracts
{
    public interface IStudentService
    {
        Task<IEnumerable<StudentViewModel>> GetAllAsync();
        Task<StudentViewModel?> GetByIdAsync(int id);
        Task AddAsync(Student student);
        Task<int> UpdateAsync(UpdateStudentFormModel student);
        Task DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
        Task<int> CreateAsync(CreateStudentFormModel model);
    }
}
