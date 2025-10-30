using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using OnlineCourseManagementSystem.Core.Models.Student;
using OnlineCourseManagementSystem.Core.Services;
using OnlineCourseManagementSystem.Infrastructure.Data.Models;
using UnitTests.Infrastructure;
using Microsoft.Extensions.Logging;
using Assert = NUnit.Framework.Assert;

namespace UnitTests
{
    [TestFixture]
    public class StudentServiceTests
    {
        private TestDbContext _db = null!;
        private InMemoryRepository _repo = null!;
        private Mock<ILogger<StudentService>> _logger = null!;
        private StudentService _service = null!;

        private static DbContextOptions<TestDbContext> CreateOptions()
            => new DbContextOptionsBuilder<TestDbContext>()
               .UseInMemoryDatabase($"StudentsDb_{Guid.NewGuid()}")
               .EnableSensitiveDataLogging()
               .Options;

        [SetUp]
        public void SetUp()
        {
            _db = new TestDbContext(CreateOptions());
            _repo = new InMemoryRepository(_db);
            _logger = new Mock<ILogger<StudentService>>();
            _service = new StudentService(_repo, _logger.Object);
        }

        [Test]
        public async Task CreateAsync_Should_Return_Id_And_Persist()
        {
            var id = await _service.CreateAsync(new CreateStudentFormModel
            {
                FirstName = "Alan",
                LastName = "Turing"
            });

            Assert.That(id, Is.GreaterThan(0));
            var fromDb = await _db.Students.FindAsync(id);
            Assert.That(fromDb, Is.Not.Null);
            Assert.That(fromDb!.FirstName, Is.EqualTo("Alan"));
            Assert.That(fromDb.LastName, Is.EqualTo("Turing"));
            _logger.VerifyLog(LogLevel.Information, "Created student with Id", Times.AtLeastOnce());
        }

        [Test]
        public async Task DeleteAsync_Should_Mark_IsDeleted_True()
        {
            var s = new Student { FirstName = "Grace", LastName = "Hopper", IsDeleted = false };
            _db.Students.Add(s);
            await _db.SaveChangesAsync();

            await _service.DeleteAsync(s.Id);

            var reloaded = await _db.Students.FindAsync(s.Id);
            Assert.That(reloaded, Is.Not.Null);
            Assert.That(reloaded!.IsDeleted, Is.True);
        }

        [Test]
        public void DeleteAsync_Should_Throw_For_Missing_Or_Deleted()
        {
            Assert.ThrowsAsync<KeyNotFoundException>(() => _service.DeleteAsync(12345));

            var s = new Student { FirstName = "Linus", LastName = "Torvalds", IsDeleted = true };
            _db.Students.Add(s);
            _db.SaveChanges();

            Assert.ThrowsAsync<KeyNotFoundException>(() => _service.DeleteAsync(s.Id));
            _logger.VerifyLog(LogLevel.Warning, "Attempted to delete missing/deleted student", Times.AtLeastOnce());
        }

        [Test]
        public async Task GetAllAsync_Should_Return_NotDeleted_With_Enrollment_Count()
        {
            var st1 = new Student { FirstName = "S1", LastName = "L1", IsDeleted = false, EnrolledCourses = new List<Enrollment>() };
            var st2 = new Student { FirstName = "S2", LastName = "L2", IsDeleted = true };
            var course = new Course { Title = "C# 12", StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddDays(1), EnrollmentCap = 50, IsDeleted = false };
            _db.AddRange(st1, st2, course);
            await _db.SaveChangesAsync();

            var e1 = new Enrollment { StudentId = st1.Id, CourseId = course.Id, EnrollmentDate = DateTime.UtcNow, Progress = 0, IsCompleted = false, IsDeleted = false };
            var e2 = new Enrollment { StudentId = st1.Id, CourseId = course.Id, EnrollmentDate = DateTime.UtcNow, Progress = 0, IsCompleted = false, IsDeleted = false };
            _db.Enrollments.AddRange(e1, e2);
            await _db.SaveChangesAsync();

            var result = (await _service.GetAllAsync()).ToList();

            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result[0].FirstName, Is.EqualTo("S1"));
            Assert.That(result[0].EnrolledCoursesCount, Is.EqualTo(2));
        }

        [Test]
        public async Task GetByIdAsync_Should_Return_ViewModel()
        {
            var st = new Student { FirstName = "Find", LastName = "Me", IsDeleted = false, EnrolledCourses = new List<Enrollment>() };
            _db.Students.Add(st);
            await _db.SaveChangesAsync();

            var vm = await _service.GetByIdAsync(st.Id);

            Assert.That(vm, Is.Not.Null);
            Assert.That(vm!.Id, Is.EqualTo(st.Id));
            Assert.That(vm.FirstName, Is.EqualTo("Find"));
        }

        [Test]
        public void GetByIdAsync_Should_Throw_When_NotFound_And_LogWarning()
        {
            Assert.ThrowsAsync<KeyNotFoundException>(() => _service.GetByIdAsync(707070));
            _logger.VerifyLog(LogLevel.Warning, "not found", Times.AtLeastOnce());
        }

        [Test]
        public async Task UpdateAsync_Should_Update_First_And_Last_Name()
        {
            var st = new Student { FirstName = "Old", LastName = "Name", IsDeleted = false, EnrolledCourses = new List<Enrollment>() };
            _db.Students.Add(st);
            await _db.SaveChangesAsync();

            var model = new CreateStudentFormModel
            {
                FirstName = "New",
                LastName = "Name",
            };

            var resultId = await _service.UpdateAsync(st.Id, model);

            Assert.That(resultId, Is.EqualTo(st.Id));
            var reloaded = await _db.Students.FindAsync(st.Id);
            Assert.That(reloaded, Is.Not.Null);
            Assert.That(reloaded!.FirstName, Is.EqualTo("New"));
            Assert.That(reloaded.LastName, Is.EqualTo("Name"));
            _logger.VerifyLog(LogLevel.Information, "Updated student", Times.AtLeastOnce());
        }

        [Test]
        public void UpdateAsync_Should_Throw_When_Missing_And_LogWarning()
        {
            var id = 9999;
            var model = new CreateStudentFormModel
            {
                FirstName = "X",
                LastName = "Y"
            };

            Assert.ThrowsAsync<KeyNotFoundException>(() => _service.UpdateAsync(id, model));
            _logger.VerifyLog(LogLevel.Warning, "Attempted to update missing/deleted student", Times.AtLeastOnce());
        }

        [Test]
        public async Task CourseEnrollmentUpdate_Should_Update_Progress_And_Complete_At_100()
        {
            var student = new Student { FirstName = "Ada", LastName = "Lovelace", IsDeleted = false, EnrolledCourses = new List<Enrollment>() };
            var course = new Course { Title = "Math", StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddDays(3), EnrollmentCap = 10, IsDeleted = false };
            _db.AddRange(student, course);
            await _db.SaveChangesAsync();

            var enrollment = new Enrollment
            {
                StudentId = student.Id,
                CourseId = course.Id,
                EnrollmentDate = DateTime.UtcNow,
                Progress = 40,
                IsCompleted = false,
                IsDeleted = false
            };
            student.EnrolledCourses!.Add(enrollment);
            _db.Enrollments.Add(enrollment);
            await _db.SaveChangesAsync();

            var id = await _service.CourseEnrollmentUpdate(new StudentCourseEnrollmentUpdateFormModel
            {
                StudentId = student.Id,
                CourseId = course.Id,
                ProgressPercentage = 100
            });

            Assert.That(id, Is.EqualTo(enrollment.Id));
            var reloaded = await _db.Enrollments.FindAsync(enrollment.Id);
            Assert.That(reloaded, Is.Not.Null);
            Assert.That(reloaded!.Progress, Is.EqualTo(100));
            Assert.That(reloaded.IsCompleted, Is.True);
            _logger.VerifyLog(LogLevel.Information, "completed Course ID", Times.AtLeastOnce());
        }

        [Test]
        public void CourseEnrollmentUpdate_Should_Throw_When_OutOfRange()
        {
            var student = new Student { FirstName = "S", LastName = "1", IsDeleted = false, EnrolledCourses = new List<Enrollment>() };
            var course = new Course { Title = "C", StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddDays(1), EnrollmentCap = 5, IsDeleted = false };
            _db.AddRange(student, course);
            _db.SaveChanges();

            var enrollment = new Enrollment
            {
                StudentId = student.Id,
                CourseId = course.Id,
                EnrollmentDate = DateTime.UtcNow,
                Progress = 0,
                IsCompleted = false,
                IsDeleted = false
            };
            student.EnrolledCourses!.Add(enrollment);
            _db.Enrollments.Add(enrollment);
            _db.SaveChanges();

            Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => _service.CourseEnrollmentUpdate(new StudentCourseEnrollmentUpdateFormModel
            {
                StudentId = student.Id,
                CourseId = course.Id,
                ProgressPercentage = -1
            }));

            Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => _service.CourseEnrollmentUpdate(new StudentCourseEnrollmentUpdateFormModel
            {
                StudentId = student.Id,
                CourseId = course.Id,
                ProgressPercentage = 101
            }));
        }

        [Test]
        public void CourseEnrollmentUpdate_Should_Throw_When_Student_Not_Found()
        {
            Assert.ThrowsAsync<KeyNotFoundException>(() => _service.CourseEnrollmentUpdate(new StudentCourseEnrollmentUpdateFormModel
            {
                StudentId = 9999,
                CourseId = 1,
                ProgressPercentage = 50
            }));
        }

        [Test]
        public async Task CourseEnrollmentUpdate_Should_Throw_When_Enrollment_Not_Found()
        {
            var student = new Student { FirstName = "Only", LastName = "Student", IsDeleted = false, EnrolledCourses = new List<Enrollment>() };
            _db.Students.Add(student);
            await _db.SaveChangesAsync();

            Assert.ThrowsAsync<KeyNotFoundException>(() => _service.CourseEnrollmentUpdate(new StudentCourseEnrollmentUpdateFormModel
            {
                StudentId = student.Id,
                CourseId = 777,
                ProgressPercentage = 50
            }));
        }

        [Test]
        public async Task GetStudentCoursesAsync_Should_Return_Courses()
        {
            var student = new Student { FirstName = "S", LastName = "C", IsDeleted = false, EnrolledCourses = new List<Enrollment>() };
            var course = new Course { Title = "Title 1", StartDate = DateTime.UtcNow.Date, EndDate = DateTime.UtcNow.Date.AddDays(10), EnrollmentCap = 10, IsDeleted = false };
            _db.AddRange(student, course);
            await _db.SaveChangesAsync();

            var enrollment = new Enrollment
            {
                StudentId = student.Id,
                CourseId = course.Id,
                EnrollmentDate = DateTime.UtcNow,
                Progress = 0,
                IsCompleted = false,
                IsDeleted = false
            };
            student.EnrolledCourses!.Add(enrollment);
            _db.Enrollments.Add(enrollment);
            await _db.SaveChangesAsync();

            var courses = (await _service.GetStudentCoursesAsync(student.Id)).ToList();

            Assert.That(courses.Count, Is.EqualTo(1));
            Assert.That(courses[0].CourseTitle, Is.EqualTo("Title 1"));
            Assert.That(courses[0].StartDate, Is.EqualTo(course.StartDate));
            Assert.That(courses[0].EndDate, Is.EqualTo(course.EndDate));
        }
    }

    internal static class LoggerMoqExtensions
    {
        public static void VerifyLog<T>(this Mock<ILogger<T>> logger, LogLevel level, string contains, Times times)
        {
            logger.Verify(x => x.Log(
                It.Is<LogLevel>(l => l == level),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((o, t) => o.ToString()!.Contains(contains, StringComparison.OrdinalIgnoreCase)),
                It.IsAny<Exception?>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)
            ), times);
        }
    }
}