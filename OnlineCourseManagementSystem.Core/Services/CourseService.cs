using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OnlineCourseManagementSystem.Core.Contracts;
using OnlineCourseManagementSystem.Core.Factories;
using OnlineCourseManagementSystem.Core.Models.Course;
using OnlineCourseManagementSystem.Infrastructure.Data.Common;
using OnlineCourseManagementSystem.Infrastructure.Data.Models;

namespace OnlineCourseManagementSystem.Core.Services
{
    public class CourseService : ICourseService
    {
        private readonly IRepository repository;
        private readonly ILogger<CourseService> logger;

        public CourseService(IRepository _repository,ILogger<CourseService> _logger)
        {
            repository = _repository;
            logger = _logger;
        }

        public async Task AddAsync(Course courseEntity)
        {
            await repository.AddAsync(courseEntity);

            logger.LogInformation($"Added new course with Title: {courseEntity.Title}");

            await repository.SaveChangesAsync();
        }

        public async Task<IEnumerable<CourseViewModel>> GetAllAsync()
        {
            return await repository.All<Course>()
                .Where(c => !c.IsDeleted)
                .Include(c => c.EnrolledStudents)
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

            if (course == null || course.IsDeleted)
            {
                logger.LogWarning($"Attempted to delete missing/deleted course with ID: {id}");
                throw new KeyNotFoundException($"Course with id {id} not found or deleted.");
            }

            course.IsDeleted = true;
            await repository.SaveChangesAsync();

            return ResponseFactory.Success(id, $"Course with id {id} deleted successfully.");
        }

        public Task<bool> ExistsAsync(int id)
        {
            var exists = repository.AllAsReadOnly<Course>()
                .AnyAsync(c => c.Id == id && !c.IsDeleted);

            return exists;
        }

        public async Task<CourseViewModel?> GetByIdAsync(int id)
        {
            var course = await repository.All<Course>()
                .Include(c => c.EnrolledStudents)
                .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);

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
            throw new KeyNotFoundException($"Course with id {id} not found.");
        }

        public async Task<int> UpdateAsync(UpdateCourseFormModel model)
        {
            var course = await repository.GetByIdAsync<Course>(model.Id);

            if (course == null || course.IsDeleted)
            {
                logger.LogWarning($"Attempted to update missing/deleted course with ID: {model.Id}");
                throw new KeyNotFoundException($"Course with id {model.Id} not found.");
            }

            ValidateCourseDates(model.StartDate, model.EndDate);

            course.Title = model.Title;
            course.StartDate = model.StartDate;
            course.EndDate = model.EndDate;
            course.EnrollmentCap = model.EnrollmentCap;

            await repository.SaveChangesAsync();

            logger.LogInformation($"Updated course with Id: {course.Id}");

            return model.Id;
        }

        public async Task<int> CreateAsync(CreateCourseFormModel model)
        {
            ValidateCourseDates(model.StartDate,model.EndDate);

            var course = CourseFactory.Create(model.Title, model.StartDate, model.EndDate, model.EnrollmentCap);
            await repository.AddAsync(course);
            await repository.SaveChangesAsync();

            logger.LogInformation($"Created course with Id: {course.Id}");

            return course.Id;
        }

        private void ValidateCourseDates(DateTime start, DateTime end)
        {
            if (end <= start)
                throw new InvalidOperationException("EndDate must be after StartDate.");
        }
    }
}
