using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OnlineCourseManagementSystem.Core.Contracts;
using OnlineCourseManagementSystem.Core.Models.Student;
using OnlineCourseManagementSystem.Infrastructure.Data.Common;
using OnlineCourseManagementSystem.Infrastructure.Data.Models;
using OnlineCourseManagementSystem.Core.Factories;

namespace OnlineCourseManagementSystem.Core.Services
{
    public class StudentService : IStudentService
    {
        private readonly IRepository repository;
        private readonly ILogger<StudentService> logger;

        public StudentService(IRepository _repository,ILogger<StudentService> _logger)
        {
            repository = _repository;
            logger = _logger;
        }

        public async Task AddAsync(Student student)
        {
            await repository.AddAsync(student);

            logger.LogInformation($"Added new student: {student.FirstName} {student.LastName}");

            await repository.SaveChangesAsync();
        }

        public async Task<int> CreateAsync(CreateStudentFormModel model)
        {
            var student = StudentFactory.Create(model.FirstName, model.LastName);

            await repository.AddAsync(student);
            await repository.SaveChangesAsync();

            logger.LogInformation($"Created student with Id: {student.Id}");

            return student.Id;
        }

        public async Task DeleteAsync(int id)
        {
            var student = await repository.GetByIdAsync<Student>(id);

            if (student == null || student.IsDeleted)
            {
                logger.LogWarning($"Attempted to delete missing/deleted student with ID: {id}");
                throw new KeyNotFoundException($"Student with id {id} not found or deleted.");
            }

            student.IsDeleted = true;
            await repository.SaveChangesAsync();
        }

        public async Task<bool> ExistsAsync(int id)
        {
            var exists = await repository.AllAsReadOnly<Student>()
                .AnyAsync(s => s.Id == id && !s.IsDeleted);

            return exists;
        }

        public async Task<IEnumerable<StudentViewModel>> GetAllAsync()
        {
            return await repository.All<Student>()
                .Where(s => !s.IsDeleted)
                .Include(s => s.EnrolledCourses)
                .Select(s => new StudentViewModel
                {
                    Id = s.Id,
                    FirstName = s.FirstName,
                    LastName = s.LastName,
                    EnrolledCoursesCount = s.EnrolledCourses.Count
                })
                .ToListAsync();
        }

        public async Task<StudentViewModel?> GetByIdAsync(int id)
        {
            var student = await repository.All<Student>()
                .Include(s => s.EnrolledCourses)
                .FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted);

            if (student == null)
            {
                logger.LogWarning($"Student with ID {id} not found.");
                throw new KeyNotFoundException($"Student with id {id} not found.");
            }
           
            return new StudentViewModel
            {
                Id = student.Id,
                FirstName = student.FirstName,
                LastName = student.LastName,
                EnrolledCoursesCount = student.EnrolledCourses.Count
            };
        }

        public async Task<int> UpdateAsync(UpdateStudentFormModel model)
        {
            var student = await repository.All<Student>()
                .Where(s => s.Id == model.Id && !s.IsDeleted)
                .FirstOrDefaultAsync();

            if (student == null)
            {
                logger.LogWarning($"Attempted to update missing/deleted student with ID: {model.Id}");
                throw new KeyNotFoundException($"Student with id {model.Id} not found or deleted.");
            }

            student.FirstName = model.FirstName;
            student.LastName = model.LastName;

            student.EnrolledCourses.Clear();

            var existingCourses = await repository.All<Enrollment>()
                .Where(c => model.Courses.Select(x => x.Id).Contains(c.Id))
                .ToListAsync();

            student.EnrolledCourses = existingCourses;

            await repository.SaveChangesAsync();

            logger.LogInformation($"Updated student: {student.FirstName} {student.LastName}");

            return student.Id;
        }
    }
}
