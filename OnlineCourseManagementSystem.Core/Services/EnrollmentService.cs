using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OnlineCourseManagementSystem.Core.Contracts;
using OnlineCourseManagementSystem.Core.Models.Enroll;
using OnlineCourseManagementSystem.Infrastructure.Data.Common;
using OnlineCourseManagementSystem.Infrastructure.Data.Models;

namespace OnlineCourseManagementSystem.Core.Services
{
    public class EnrollmentService : IEnrollmentService
    {
        private readonly ILogger<EnrollmentService> logger;
        private readonly IRepository repository;

        public EnrollmentService(ILogger<EnrollmentService> _logger, IRepository _repository)
        {
            logger = _logger;
            repository = _repository;
        }

        public async Task<int> CreateAsync(CreateEnrollmentFormModel model)
        {
            var enrollment = new Enrollment
            {
                StudentId = model.StudentId,
                CourseId = model.CourseId,
                EnrollmentDate = DateTime.UtcNow,
                Progress = 0,
                IsCompleted = false
            };

            await repository.AddAsync(enrollment);
            await repository.SaveChangesAsync();

            logger.LogInformation($"Created enrollment with Id: {enrollment.Id}");

            return enrollment.Id;
        }

        public Task DeleteAsync(int enrollmentId)
        {
            var enrollment = repository.All<Enrollment>()
                .FirstOrDefault(e => e.Id == enrollmentId && !e.IsDeleted);

            if (enrollment == null)
            {
                logger.LogWarning($"Attempted to delete missing/deleted enrollment with ID: {enrollmentId}");
                throw new KeyNotFoundException($"Enrollment with id {enrollmentId} not found or deleted.");
            }

            enrollment.IsDeleted = true;

            logger.LogInformation($"Deleted enrollment with ID: {enrollmentId}");

            return repository.SaveChangesAsync();
        }

        public async Task<int> EnrollStudentAsync(EnrollStudentFormModel model)
        {
            var student = await repository.All<Student>()
                .Include(s => s.EnrolledCourses)
                .FirstAsync(s => s.Id == model.StudentId && !s.IsDeleted);

            if (student == null)
            {
                throw new KeyNotFoundException($"Student with id {model.StudentId} not found or deleted.");
            }

            var course = await repository.All<Course>()
                .FirstOrDefaultAsync(c => c.Id == model.CourseId);

            if (course == null || course.IsDeleted)
            {
                throw new KeyNotFoundException($"Course with id {model.CourseId} not found or deleted.");
            }

            if (student.EnrolledCourses.Any(c => c.Id == model.CourseId))
            {
                logger.LogWarning($"Student with ID {model.StudentId} is already enrolled in course ID {model.CourseId}");
                throw new InvalidOperationException($"Student with id {model.StudentId} is already enrolled in course id {model.CourseId}.");
            }

            var enrollment = new Enrollment
            {
                StudentId = model.StudentId,
                CourseId = model.CourseId,
                EnrollmentDate = DateTime.UtcNow,
                Progress = 0,
                IsCompleted = false
            };

            student.EnrolledCourses.Add(enrollment);
            await repository.SaveChangesAsync();

            logger.LogInformation($"Enrolled student ID {model.StudentId} in course ID {model.CourseId}");

            return enrollment.Id;
        }

        public async Task<IEnumerable<EnrollViewModel>> GetAllAsync()
        {
            return await repository.All<Enrollment>()
                .Where(e => !e.IsDeleted)
                .Select(e => new EnrollViewModel
                {
                    Id = e.Id,
                    StudentId = e.StudentId,
                    CourseId = e.CourseId,
                    EnrollmentDate = e.EnrollmentDate,
                    Progress = e.Progress,
                    IsCompleted = e.IsCompleted
                })
                .ToListAsync();
        }

        public async Task<EnrollViewModel?> GetByIdAsync(int enrollmentId)
        {
            var enrollment = await repository.All<Enrollment>()
                .FirstOrDefaultAsync(e => e.Id == enrollmentId && !e.IsDeleted);

            if (enrollment == null)
            {
                throw new KeyNotFoundException($"Enrollment with id {enrollmentId} not found or deleted.");
            }

            return await repository.All<Enrollment>()
                .Where(e => e.Id == enrollmentId && !e.IsDeleted)
                .Select(e => new EnrollViewModel
                {
                    Id = e.Id,
                    StudentId = e.StudentId,
                    CourseId = e.CourseId,
                    EnrollmentDate = e.EnrollmentDate,
                    Progress = e.Progress,
                    IsCompleted = e.IsCompleted
                })
                .FirstOrDefaultAsync();
        }

        public async Task UpdateProgressAsync(int enrollmentId, int progress)
        {
            var enrollment = await repository.All<Enrollment>()
                .FirstOrDefaultAsync(e => e.Id == enrollmentId && !e.IsDeleted);

            if (enrollment == null)
            {
                throw new KeyNotFoundException($"Enrollment with ID {enrollmentId} not found.");
            }

            if (progress < 0 || progress > 100)
            {
                throw new ArgumentOutOfRangeException(nameof(progress), "Progress must be between 0 and 100.");
            }

            enrollment.Progress = progress;
            enrollment.IsCompleted = progress == 100;

            await repository.SaveChangesAsync();
            logger.LogInformation($"Updated progress for enrollment ID {enrollmentId} to {progress}%");
        }

        public async Task<int> UpdateEnrollmentAsync(UpdateEnrollmentFormModel model)
        {
            var enrollment = repository.All<Enrollment>()
                .FirstOrDefault(e => e.Id == model.Id && !e.IsDeleted);

            if (enrollment == null)
            {
                logger.LogWarning($"Attempted to update missing/deleted enrollment with ID: {model.Id}");
                throw new KeyNotFoundException($"Enrollment with id {model.Id} not found or deleted.");
            }

            enrollment.StudentId = model.StudentId;
            enrollment.CourseId = model.CourseId;
            enrollment.Progress = model.Progress;
            enrollment.IsCompleted = model.IsCompleted;

            await repository.SaveChangesAsync();

            logger.LogInformation($"Updated enrollment with ID: {model.Id}");

            return enrollment.Id;
        }
    }
}
