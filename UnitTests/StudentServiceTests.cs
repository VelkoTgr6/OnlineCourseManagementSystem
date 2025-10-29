using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using OnlineCourseManagementSystem.Core.Models.Course;
using OnlineCourseManagementSystem.Core.Models.Student;
using OnlineCourseManagementSystem.Core.Services;
using OnlineCourseManagementSystem.Infrastructure.Data.Models;
using UnitTests.Infrastructure;
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
        {
            return new DbContextOptionsBuilder<TestDbContext>()
                       .UseInMemoryDatabase($"StudentsDb_{Guid.NewGuid()}")
                       .EnableSensitiveDataLogging()
                       .Options;
        }

        [SetUp]
        public void SetUp()
        {
            _db = new TestDbContext(CreateOptions());
            _repo = new InMemoryRepository(_db);
            _logger = new Mock<ILogger<StudentService>>();
            _service = new StudentService(_repo, _logger.Object);
        }

        [Test]
        public async Task AddAsync_Should_Add_Student_And_Save()
        {
            var student = new Student { FirstName = "Ada", LastName = "Lovelace" };

            await _service.AddAsync(student);

            var fromDb = await _db.Students.FirstOrDefaultAsync(s => s.FirstName == "Ada" && s.LastName == "Lovelace");
            Assert.That(fromDb != null);
            _logger.VerifyLog(LogLevel.Information, "Added new student", Times.AtLeastOnce());
        }

        [Test]
        public async Task CreateAsync_Should_Return_Id_And_Persist()
        {
            var id = await _service.CreateAsync(new CreateStudentFormModel
            {
                FirstName = "Alan",
                LastName = "Turing"
            });

            Assert.That(id > 0);
            var fromDb = await _db.Students.FindAsync(id);
            Assert.That(fromDb != null);
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
            Assert.That(reloaded!.IsDeleted);
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
        public async Task ExistsAsync_Should_Return_Correct_Value()
        {
            var active = new Student { FirstName = "Active", LastName = "User", IsDeleted = false };
            var deleted = new Student { FirstName = "Deleted", LastName = "User", IsDeleted = true };
            _db.Students.AddRange(active, deleted);
            await _db.SaveChangesAsync();

            var existsActive = await _service.ExistsAsync(active.Id);
            var existsDeleted = await _service.ExistsAsync(deleted.Id);
            var existsMissing = await _service.ExistsAsync(999);

            Assert.That(existsActive, Is.True);
            Assert.That(existsDeleted, Is.False);
            Assert.That(existsMissing, Is.False);
        }

        [Test]
        public async Task GetAllAsync_Should_Return_NotDeleted_With_Enrollment_Count()
        {
            var st1 = new Student { FirstName = "S1", LastName = "L1", IsDeleted = false };
            var st2 = new Student { FirstName = "S2", LastName = "L2", IsDeleted = true };  // should be filtered out
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
            var st = new Student { FirstName = "Find", LastName = "Me" };
            _db.Students.Add(st);
            await _db.SaveChangesAsync();

            var vm = await _service.GetByIdAsync(st.Id);

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

            var model = new UpdateStudentFormModel
            {
                Id = st.Id,
                FirstName = "New",
                LastName = "Name",
                Courses = new List<CourseViewModel>() // keep it empty to avoid enrollment complexity
            };

            var resultId = await _service.UpdateAsync(model);

            Assert.That(resultId, Is.EqualTo(st.Id));
            var reloaded = await _db.Students.FindAsync(st.Id);
            Assert.That(reloaded!.FirstName, Is.EqualTo("New"));
            Assert.That(reloaded.LastName, Is.EqualTo("Name"));
            _logger.VerifyLog(LogLevel.Information, "Updated student", Times.AtLeastOnce());
        }

        [Test]
        public void UpdateAsync_Should_Throw_When_Missing_And_LogWarning()
        {
            var model = new UpdateStudentFormModel
            {
                Id = 9999,
                FirstName = "X",
                LastName = "Y",
                Courses = new List<CourseViewModel>()
            };

            Assert.ThrowsAsync<KeyNotFoundException>(() => _service.UpdateAsync(model));
            _logger.VerifyLog(LogLevel.Warning, "Attempted to update missing/deleted student", Times.AtLeastOnce());
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