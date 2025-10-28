using Microsoft.EntityFrameworkCore;
using OnlineCourseManagementSystem.Core.Contracts;
using OnlineCourseManagementSystem.Infrastructure.Data.Common;
using OnlineCourseManagementSystem.Infrastructure.Data.Models;

namespace OnlineCourseManagementSystem.Core.Services
{
    public class CourseService : ICourseService
    {
        private readonly IRepository repository;

        public CourseService(IRepository _repository)
        {
            repository = _repository;
        }

        public async Task AddAsync(Course course)
        {
            await repository.AddAsync(course);
            await repository.SaveChangesAsync();
        }

        public async Task<IEnumerable<Course>> GetAllAsync()
        {
            return await repository.All<Course>()
                .Where(c => !c.IsDeleted)
                .ToListAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var course = await repository.GetByIdAsync<Course>(id);
            if (course != null)
            {
                repository.Delete(course);
                await repository.SaveChangesAsync();
            }
        }

        public Task<bool> ExistsAsync(int id)
        {
            var exists = repository.AllAsReadOnly<Course>()
                .AnyAsync(c => c.Id == id && !c.IsDeleted);

            return exists;
        }

        public async Task<Course?> GetByIdAsync(int id)
        {
            var course = await repository.GetByIdAsync<Course>(id);
            return course;
        }

        public async Task UpdateAsync(Course course)
        {
            var courseEntity = await repository.GetByIdAsync<Course>(course.Id);

            if (courseEntity != null)
            {
                courseEntity.Title = course.Title;
                courseEntity.StartDate = course.StartDate;
                courseEntity.EndDate = course.EndDate;
                courseEntity.EnrollmentCap = course.EnrollmentCap;
                await repository.SaveChangesAsync();
            }
        }
    }
}
