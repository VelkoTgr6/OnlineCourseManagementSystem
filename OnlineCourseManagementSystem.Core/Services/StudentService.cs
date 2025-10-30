using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OnlineCourseManagementSystem.Core.Contracts;
using OnlineCourseManagementSystem.Core.Factories;
using OnlineCourseManagementSystem.Core.Models.Student;
using OnlineCourseManagementSystem.Infrastructure.Data.Common;
using OnlineCourseManagementSystem.Infrastructure.Data.Models;
using System;

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

        public async Task<int> CourseEnrollmentUpdate(StudentCourseEnrollmentUpdateFormModel model)
        {
            var student = await repository.All<Student>()
                .Include(s => s.EnrolledCourses)
                .FirstOrDefaultAsync(s => s.Id == model.StudentId && !s.IsDeleted);

            if (student == null)
            {
                throw new KeyNotFoundException($"Student with id {model.StudentId} not found or deleted.");
            }

            var enrollment = student.EnrolledCourses
                .FirstOrDefault(e => e.CourseId == model.CourseId && !e.IsDeleted);

            if (enrollment == null)
            {
                throw new KeyNotFoundException($"Enrollment for CourseId {model.CourseId} not found for StudentId {model.StudentId}.");
            }

            if (model.ProgressPercentage < 0 || model.ProgressPercentage > 100)
            {
                throw new ArgumentOutOfRangeException(nameof(model.ProgressPercentage), "Progress must be between 0 and 100.");
            }

            if (model.ProgressPercentage >= 100)
            {
                enrollment.IsCompleted = true;
                enrollment.Progress = 100;

                await repository.SaveChangesAsync();

                logger.LogInformation($"Student ID {model.StudentId} completed Course ID {model.CourseId}.");
                return enrollment.Id;
            }

            enrollment.Progress = model.ProgressPercentage;

            await repository.SaveChangesAsync();
            return enrollment.Id;
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
            var student = await GetStudentById(id);

            if (student == null)
            {
                logger.LogWarning($"Attempted to delete missing/deleted student with ID: {id}");
                throw new KeyNotFoundException($"Student with id {id} not found or deleted.");
            }

            student.IsDeleted = true;
            await repository.SaveChangesAsync();
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
            var student = await GetStudentById(id);

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

        public async Task<IEnumerable<StudentCoursesViewModel>> GetStudentCoursesAsync(int studentId)
        {
            var student = await GetStudentById(studentId);

            if (student == null)
            {
                logger.LogWarning($"Student with ID {studentId} not found.");
                throw new KeyNotFoundException($"Student with id {studentId} not found.");
            }

            var courses = await repository.All<Enrollment>()
                .Where(e => e.StudentId == studentId && !e.IsDeleted)
                .Include(e => e.Course)
                .Select(e => new StudentCoursesViewModel
                {
                    CourseTitle = e.Course.Title,
                    StartDate = e.Course.StartDate,
                    EndDate = e.Course.EndDate
                })
                .ToListAsync();

            return courses;
        }

        public async Task<int> UpdateAsync(int id,CreateStudentFormModel model)
        {
            var student = await GetStudentById(id);

            if (student == null)
            {
                logger.LogWarning($"Attempted to update missing/deleted student with ID: {id}");
                throw new KeyNotFoundException($"Student with id {id} not found or deleted.");
            }

            student.FirstName = model.FirstName;
            student.LastName = model.LastName;

            await repository.SaveChangesAsync();

            logger.LogInformation($"Updated student: {student.FirstName} {student.LastName}");

            return student.Id;
        }

        private async Task<Student?> GetStudentById(int id)
        {
            return await repository.All<Student>()
                .FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted);
        }
    }
}
