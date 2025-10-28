using Microsoft.EntityFrameworkCore;
using OnlineCourseManagementSystem.Core.Contracts;
using OnlineCourseManagementSystem.Infrastructure.Data.Common;
using OnlineCourseManagementSystem.Infrastructure.Data.Models;

namespace OnlineCourseManagementSystem.Core.Services
{
    public class StudentService : IStudentService
    {
        private readonly IRepository repository;

        public StudentService(IRepository _repository)
        {
            repository = _repository;
        }

        public async Task AddAsync(Student student)
        {
            await repository.AddAsync(student);
            await repository.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var student =await repository.GetByIdAsync<Student>(id);

            if (student != null)
            {
                repository.Delete(student);
                await repository.SaveChangesAsync();
            }

        }

        public async Task<bool> ExistsAsync(int id)
        {
            var exists = await repository.AllAsReadOnly<Student>()
                .AnyAsync(s => s.Id == id && !s.IsDeleted);

            return exists;
        }

        public async Task<IEnumerable<Student>> GetAllAsync()
        {
            return await repository.All<Student>()
                .Where(s => !s.IsDeleted)
                .ToListAsync();
        }

        public async Task<Student?> GetByIdAsync(int id)
        {
            return await repository.GetByIdAsync<Student>(id);
        }

        public async Task UpdateAsync(Student student)
        {
            var studentEntity = await repository.GetByIdAsync<Student>(student.Id);

            if (studentEntity != null)
            {
                studentEntity.FirstName = student.FirstName;
                studentEntity.LastName = student.LastName;
                await repository.SaveChangesAsync();
            }
        }
    }
}
