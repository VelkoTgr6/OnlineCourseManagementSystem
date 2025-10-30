using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using OnlineCourseManagementSystem.Core.Models.Course;
using OnlineCourseManagementSystem.Core.Services;
using OnlineCourseManagementSystem.Infrastructure.Data.Models;
using UnitTests.Infrastructure;
using Assert = NUnit.Framework.Assert;

namespace UnitTests
{
    [TestFixture]
    public class CourseServiceTests
    {
        private TestDbContext _db = null!;
        private InMemoryRepository _repo = null!;
        private Mock<ILogger<CourseService>> _logger = null!;
        private CourseService _service = null!;

        private static DbContextOptions<TestDbContext> CreateOptions()
        {
            return new DbContextOptionsBuilder<TestDbContext>()
                .UseInMemoryDatabase($"CoursesDb_{Guid.NewGuid()}")
                .EnableSensitiveDataLogging()
                .Options;
        }

        [SetUp]
        public void SetUp()
        {
            _db = new TestDbContext(CreateOptions());
            _repo = new InMemoryRepository(_db);
            _logger = new Mock<ILogger<CourseService>>();
            _service = new CourseService(_repo, _logger.Object);
        }

        [Test]
        public async Task AddAsync_Should_Add_Course_And_Save()
        {
            var course = new Course
            {
                Title = "Intro to C#",
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddDays(1),
                EnrollmentCap = 100,
                IsDeleted = false
            };

            await _service.AddAsync(course);

            var fromDb = await _db.Courses.FirstOrDefaultAsync(c => c.Title == "Intro to C#");
            Assert.That(fromDb != null);
            _logger.VerifyLog(LogLevel.Information, "Added new course", Times.AtLeastOnce());
        }

        [Test]
        public async Task CreateAsync_Should_Return_Id_And_Persist()
        {
            var id = await _service.CreateAsync(new CreateCourseFormModel
            {
                Title = "Algorithms 101",
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddDays(2),
                EnrollmentCap = 50
            });

            Assert.That(id > 0);
            var fromDb = await _db.Courses.FindAsync(id);
            Assert.That(fromDb != null);
            Assert.That(fromDb!.Title, Is.EqualTo("Algorithms 101"));
            Assert.That(fromDb.EnrollmentCap, Is.EqualTo(50));
            _logger.VerifyLog(LogLevel.Information, "Created course with Id", Times.AtLeastOnce());
        }

        [Test]
        public async Task DeleteAsync_Should_Mark_IsDeleted_True()
        {
            var c = new Course
            {
                Title = "Delete Me",
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddDays(1),
                EnrollmentCap = 10,
                IsDeleted = false
            };
            _db.Courses.Add(c);
            await _db.SaveChangesAsync();

            await _service.DeleteAsync(c.Id);

            var reloaded = await _db.Courses.FindAsync(c.Id);
            Assert.That(reloaded!.IsDeleted);
        }

        [Test]
        public void DeleteAsync_Should_Throw_For_Missing_Or_Deleted()
        {
            Assert.ThrowsAsync<KeyNotFoundException>(() => _service.DeleteAsync(12345));

            var c = new Course
            {
                Title = "Already Deleted",
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddDays(1),
                EnrollmentCap = 10,
                IsDeleted = true
            };
            _db.Courses.Add(c);
            _db.SaveChanges();

            Assert.ThrowsAsync<KeyNotFoundException>(() => _service.DeleteAsync(c.Id));
            _logger.VerifyLog(LogLevel.Warning, "Attempted to delete missing/deleted course", Times.AtLeastOnce());
        }

        [Test]
        public async Task GetAllAsync_Should_Return_NotDeleted_With_Enrolled_Count()
        {
            var st = new Student { FirstName = "S1", LastName = "L1", IsDeleted = false };
            var c1 = new Course
            {
                Title = "C1",
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddDays(1),
                EnrollmentCap = 50,
                IsDeleted = false
            };
            var c2 = new Course
            {
                Title = "C2",
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddDays(1),
                EnrollmentCap = 50,
                IsDeleted = true // should be filtered out
            };
            _db.AddRange(st, c1, c2);
            await _db.SaveChangesAsync();

            var e1 = new Enrollment { StudentId = st.Id, CourseId = c1.Id, EnrollmentDate = DateTime.UtcNow, Progress = 0, IsCompleted = false, IsDeleted = false };
            var e2 = new Enrollment { StudentId = st.Id, CourseId = c1.Id, EnrollmentDate = DateTime.UtcNow, Progress = 0, IsCompleted = false, IsDeleted = false };
            _db.Enrollments.AddRange(e1, e2);
            await _db.SaveChangesAsync();

            var result = (await _service.GetAllAsync()).ToList();

            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result[0].Title, Is.EqualTo("C1"));
            Assert.That(result[0].EnrolledCount, Is.EqualTo(2));
        }

        [Test]
        public async Task GetByIdAsync_Should_Return_ViewModel()
        {
            var c = new Course
            {
                Title = "Find Me",
                StartDate = DateTime.UtcNow.Date,
                EndDate = DateTime.UtcNow.Date.AddDays(3),
                EnrollmentCap = 20,
                IsDeleted = false
            };
            _db.Courses.Add(c);
            await _db.SaveChangesAsync();

            var vm = await _service.GetByIdAsync(c.Id);

            Assert.That(vm!.Id, Is.EqualTo(c.Id));
            Assert.That(vm.Title, Is.EqualTo("Find Me"));
            Assert.That(vm.EnrollmentCap, Is.EqualTo(20));
        }

        [Test]
        public void GetByIdAsync_Should_Throw_When_NotFound()
        {
            Assert.ThrowsAsync<KeyNotFoundException>(() => _service.GetByIdAsync(707070));
        }

        [Test]
        public async Task UpdateAsync_Should_Update_All_Fields()
        {
            var id = 1;
            var c = new Course
            {
                Id = 1,
                Title = "Old Title",
                StartDate = DateTime.UtcNow.Date,
                EndDate = DateTime.UtcNow.Date.AddDays(5),
                EnrollmentCap = 10,
                IsDeleted = false
            };
            _db.Courses.Add(c);
            await _db.SaveChangesAsync();

            var model = new UpdateCourseFormModel
            { 
                Title = "New Title",
                StartDate = c.StartDate.AddDays(1),
                EndDate = c.EndDate.AddDays(1),
                EnrollmentCap = 25
            };

            var resultId = await _service.UpdateAsync(id,model);

            Assert.That(resultId, Is.EqualTo(c.Id));
            var reloaded = await _db.Courses.FindAsync(c.Id);
            Assert.That(reloaded!.Title, Is.EqualTo("New Title"));
            Assert.That(reloaded.StartDate, Is.EqualTo(model.StartDate));
            Assert.That(reloaded.EndDate, Is.EqualTo(model.EndDate));
            Assert.That(reloaded.EnrollmentCap, Is.EqualTo(25));
            _logger.VerifyLog(LogLevel.Information, "Updated course", Times.AtLeastOnce());
        }

        [Test]
        public void UpdateAsync_Should_Throw_When_Missing_And_LogWarning()
        {
            var id = 1234;
            var model = new UpdateCourseFormModel
            {
                Title = "X",
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddDays(1),
                EnrollmentCap = 1
            };

            Assert.ThrowsAsync<KeyNotFoundException>(() => _service.UpdateAsync(id,model));
            _logger.VerifyLog(LogLevel.Warning, "Attempted to update missing/deleted course", Times.AtLeastOnce());
        }

        [Test]
        public void CreateAsync_Should_Throw_When_EndDate_Before_StartDate()
        {
            var model = new CreateCourseFormModel
            {
                Title = "Bad Dates",
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddHours(-1),
                EnrollmentCap = 10
            };

            Assert.ThrowsAsync<InvalidOperationException>(() => _service.CreateAsync(model));
        }
    }
}