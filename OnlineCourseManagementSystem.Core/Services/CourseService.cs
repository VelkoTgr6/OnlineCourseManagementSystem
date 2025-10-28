using Microsoft.EntityFrameworkCore;
using OnlineCourseManagementSystem.Core.Contracts;
using OnlineCourseManagementSystem.Core.Models.Course;
using OnlineCourseManagementSystem.Infrastructure.Data.Common;
using OnlineCourseManagementSystem.Infrastructure.Data.Models;
using System.Linq.Expressions;

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

        public async Task<IEnumerable<CourseViewModel>> GetAllAsync()
        {
            return await repository.All<Course>()
                .Select(c => new CourseViewModel
                {
                    Id = c.Id,
                    Title = c.Title,
                    StartDate = c.StartDate,
                    EndDate = c.EndDate,
                    EnrollmentCap = c.EnrollmentCap,
                    EnrolledCount = c.EnrolledStudents.Count
                })
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

        public async Task<CourseViewModel?> GetByIdAsync(int id)
        {
            var course = await repository.GetByIdAsync<Course>(id);

            if (course != null)
            {
                var dto = new CourseViewModel
                {
                    Id = course.Id,
                    Title = course.Title,
                    StartDate = course.StartDate,
                    EndDate = course.EndDate,
                    EnrollmentCap = course.EnrollmentCap,
                    EnrolledCount = course.EnrolledStudents.Count
                };

                return dto;
            }
            throw new ArgumentNullException(nameof(id));
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

        public async Task CreateAsync(CreateCourseFormModel courseDto)
        {
            var course = new Course()
            {
                Title = courseDto.Title,
                StartDate = courseDto.StartDate,
                EndDate = courseDto.EndDate,
                EnrollmentCap= courseDto.EnrollmentCap,
            };

            await repository.AddAsync(course);
            await repository.SaveChangesAsync();

        }
    }
}
