using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using OnlineCourseManagementSystem.Core.Models.Enroll;
using OnlineCourseManagementSystem.Core.Services;
using OnlineCourseManagementSystem.Infrastructure.Data.Models;
using UnitTests.Infrastructure;
using Assert = NUnit.Framework.Assert;

namespace UnitTests
{
    [TestFixture]
    public class EnrollmentServiceTests
    {
        private TestDbContext _db = null!;
        private InMemoryRepository _repo = null!;
        private Mock<ILogger<EnrollmentService>> _logger = null!;
        private EnrollmentService _service = null!;

        private static DbContextOptions<TestDbContext> CreateOptions()
        {
            return new DbContextOptionsBuilder<TestDbContext>()
                .UseInMemoryDatabase($"EnrollmentsDb_{Guid.NewGuid()}")
                .EnableSensitiveDataLogging()
                .Options;
        }

        [SetUp]
        public void SetUp()
        {
            _db = new TestDbContext(CreateOptions());
            _repo = new InMemoryRepository(_db);
            _logger = new Mock<ILogger<EnrollmentService>>();
            _service = new EnrollmentService(_logger.Object, _repo);
        }

        [Test]
        public async Task CreateAsync_Should_Return_Id_And_Persist()
        {
            var student = new Student { FirstName = "S", LastName = "1", IsDeleted = false, EnrolledCourses = new List<Enrollment>() };
            var course = new Course { Title = "C1", StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddDays(1), EnrollmentCap = 10, IsDeleted = false };
            _db.AddRange(student, course);
            await _db.SaveChangesAsync();

            var id = await _service.CreateAsync(new CreateEnrollmentFormModel
            {
                StudentId = student.Id,
                CourseId = course.Id
            });

            Assert.That(id, Is.GreaterThan(0));
            var fromDb = await _db.Enrollments.FindAsync(id);
            Assert.That(fromDb, Is.Not.Null);
            Assert.That(fromDb!.StudentId, Is.EqualTo(student.Id));
            Assert.That(fromDb.CourseId, Is.EqualTo(course.Id));
            _logger.VerifyLog(LogLevel.Information, "Created enrollment with Id", Times.AtLeastOnce());
        }

        [Test]
        public void CreateAsync_Should_Throw_When_Student_Or_Course_Missing_And_LogWarning()
        {
            Assert.ThrowsAsync<KeyNotFoundException>(() => _service.CreateAsync(new CreateEnrollmentFormModel
            {
                StudentId = 9999,
                CourseId = 8888
            }));

            _logger.VerifyLog(LogLevel.Warning, "Attempted to create enrollment with missing/deleted student or course", Times.AtLeastOnce());
        }

        [Test]
        public async Task DeleteAsync_Should_Mark_IsDeleted_True()
        {
            var e = new Enrollment { StudentId = 1, CourseId = 1, EnrollmentDate = DateTime.UtcNow, Progress = 0, IsCompleted = false, IsDeleted = false };
            _db.Enrollments.Add(e);
            await _db.SaveChangesAsync();

            await _service.DeleteAsync(e.Id);

            var reloaded = await _db.Enrollments.FindAsync(e.Id);
            Assert.That(reloaded, Is.Not.Null);
            Assert.That(reloaded!.IsDeleted, Is.True);
            _logger.VerifyLog(LogLevel.Information, "Deleted enrollment", Times.AtLeastOnce());
        }

        [Test]
        public void DeleteAsync_Should_Throw_For_Missing_Or_Deleted()
        {
            Assert.ThrowsAsync<KeyNotFoundException>(() => _service.DeleteAsync(12345));
            _logger.VerifyLog(LogLevel.Warning, "Attempted to delete missing/deleted enrollment", Times.AtLeastOnce());

            var e = new Enrollment { StudentId = 1, CourseId = 1, EnrollmentDate = DateTime.UtcNow, Progress = 0, IsCompleted = false, IsDeleted = true };
            _db.Enrollments.Add(e);
            _db.SaveChanges();

            Assert.ThrowsAsync<KeyNotFoundException>(() => _service.DeleteAsync(e.Id));
            _logger.VerifyLog(LogLevel.Warning, "Attempted to delete missing/deleted enrollment", Times.AtLeastOnce());
        }

        [Test]
        public async Task EnrollStudentAsync_Should_Add_Enrollment_And_Log()
        {
            var st = new Student { FirstName = "Ada", LastName = "L", IsDeleted = false, EnrolledCourses = new List<Enrollment>() };
            var c = new Course { Title = "Course", StartDate = DateTime.UtcNow.AddDays(1), EndDate = DateTime.UtcNow.AddDays(2), EnrollmentCap = 5, IsDeleted = false, EnrolledStudents = new List<Enrollment>() };
            _db.AddRange(st, c);
            await _db.SaveChangesAsync();

            var id = await _service.EnrollStudentAsync(new EnrollStudentFormModel
            {
                StudentId = st.Id,
                CourseId = c.Id,
                EnrollmentDate = DateTime.UtcNow
            });

            Assert.That(id, Is.GreaterThan(0));
            var created = await _db.Enrollments.FindAsync(id);
            Assert.That(created, Is.Not.Null);
            Assert.That(created!.StudentId, Is.EqualTo(st.Id));
            Assert.That(created.CourseId, Is.EqualTo(c.Id));
            _logger.VerifyLog(LogLevel.Information, "Enrolled student ID", Times.AtLeastOnce());
        }

        [Test]
        public void EnrollStudentAsync_Should_Throw_When_Course_Missing_Or_Deleted()
        {
            var st = new Student { FirstName = "S", LastName = "2", IsDeleted = false, EnrolledCourses = new List<Enrollment>() };
            _db.Students.Add(st);
            _db.SaveChanges();

            Assert.ThrowsAsync<KeyNotFoundException>(() => _service.EnrollStudentAsync(new EnrollStudentFormModel
            {
                StudentId = st.Id,
                CourseId = 9999,
                EnrollmentDate = DateTime.UtcNow
            }));

            var deletedCourse = new Course { Title = "Del", StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddDays(1), EnrollmentCap = 1, IsDeleted = true, EnrolledStudents = new List<Enrollment>() };
            _db.Courses.Add(deletedCourse);
            _db.SaveChanges();

            Assert.ThrowsAsync<KeyNotFoundException>(() => _service.EnrollStudentAsync(new EnrollStudentFormModel
            {
                StudentId = st.Id,
                CourseId = deletedCourse.Id,
                EnrollmentDate = DateTime.UtcNow
            }));
        }

        [Test]
        public async Task EnrollStudentAsync_Should_Throw_When_Course_Is_Full()
        {
            var st1 = new Student { FirstName = "S1", LastName = "X", IsDeleted = false, EnrolledCourses = new List<Enrollment>() };
            var st2 = new Student { FirstName = "S2", LastName = "Y", IsDeleted = false, EnrolledCourses = new List<Enrollment>() };
            var c = new Course { Title = "Limited", StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddDays(1), EnrollmentCap = 1, IsDeleted = false, EnrolledStudents = new List<Enrollment>() };

            _db.AddRange(st1, st2, c);
            await _db.SaveChangesAsync();

            // Pre-fill course with one enrollment to hit the cap
            var existing = new Enrollment { StudentId = st1.Id, CourseId = c.Id, EnrollmentDate = DateTime.UtcNow, IsDeleted = false, Progress = 0, IsCompleted = false, Course = c, Student = st1 };
            c.EnrolledStudents!.Add(existing);
            st1.EnrolledCourses!.Add(existing);
            _db.Enrollments.Add(existing);
            await _db.SaveChangesAsync();

            Assert.ThrowsAsync<InvalidOperationException>(() => _service.EnrollStudentAsync(new EnrollStudentFormModel
            {
                StudentId = st2.Id,
                CourseId = c.Id,
                EnrollmentDate = DateTime.UtcNow
            }));
        }

        [Test]
        public async Task EnrollStudentAsync_Should_Throw_When_EnrollmentDate_After_Course_Start()
        {
            var st = new Student { FirstName = "S", LastName = "Late", IsDeleted = false, EnrolledCourses = new List<Enrollment>() };
            var c = new Course { Title = "OnTime", StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddDays(1), EnrollmentCap = 10, IsDeleted = false, EnrolledStudents = new List<Enrollment>() };
            _db.AddRange(st, c);
            await _db.SaveChangesAsync();

            // EnrollmentDate strictly after StartDate should fail
            Assert.ThrowsAsync<InvalidOperationException>(() => _service.EnrollStudentAsync(new EnrollStudentFormModel
            {
                StudentId = st.Id,
                CourseId = c.Id,
                EnrollmentDate = c.StartDate.AddMinutes(1)
            }));
        }

        [Test]
        public async Task GetAllAsync_Should_Return_NotDeleted_Projected()
        {
            var e1 = new Enrollment { StudentId = 1, CourseId = 2, EnrollmentDate = DateTime.UtcNow, Progress = 10, IsCompleted = false, IsDeleted = false };
            var e2 = new Enrollment { StudentId = 3, CourseId = 4, EnrollmentDate = DateTime.UtcNow, Progress = 20, IsCompleted = false, IsDeleted = true };
            _db.Enrollments.AddRange(e1, e2);
            await _db.SaveChangesAsync();

            var list = (await _service.GetAllAsync()).ToList();

            Assert.That(list.Count, Is.EqualTo(1));
            Assert.That(list[0].StudentId, Is.EqualTo(1));
            Assert.That(list[0].CourseId, Is.EqualTo(2));
            Assert.That(list[0].Progress, Is.EqualTo(10));
        }

        [Test]
        public async Task GetByIdAsync_Should_Return_ViewModel()
        {
            var e = new Enrollment { StudentId = 7, CourseId = 8, EnrollmentDate = DateTime.UtcNow, Progress = 33, IsCompleted = false, IsDeleted = false };
            _db.Enrollments.Add(e);
            await _db.SaveChangesAsync();

            var vm = await _service.GetByIdAsync(e.Id);

            Assert.That(vm, Is.Not.Null);
            Assert.That(vm!.Id, Is.EqualTo(e.Id));
            Assert.That(vm.StudentId, Is.EqualTo(7));
            Assert.That(vm.CourseId, Is.EqualTo(8));
            Assert.That(vm.Progress, Is.EqualTo(33));
        }

        [Test]
        public void GetByIdAsync_Should_Throw_When_NotFound()
        {
            Assert.ThrowsAsync<KeyNotFoundException>(() => _service.GetByIdAsync(5555));
        }

        [Test]
        public async Task UpdateProgressAsync_Should_Set_Progress_And_Completion_Flag()
        {
            var e = new Enrollment { StudentId = 1, CourseId = 1, EnrollmentDate = DateTime.UtcNow, Progress = 0, IsCompleted = false, IsDeleted = false };
            _db.Enrollments.Add(e);
            await _db.SaveChangesAsync();

            await _service.UpdateProgressAsync(e.Id, 100);

            var reloaded = await _db.Enrollments.FindAsync(e.Id);
            Assert.That(reloaded, Is.Not.Null);
            Assert.That(reloaded!.Progress, Is.EqualTo(100));
            Assert.That(reloaded.IsCompleted, Is.True);
            _logger.VerifyLog(LogLevel.Information, "Updated progress for enrollment ID", Times.AtLeastOnce());
        }

        [Test]
        public void UpdateProgressAsync_Should_Throw_When_OutOfRange()
        {
            var e = new Enrollment { StudentId = 1, CourseId = 1, EnrollmentDate = DateTime.UtcNow, Progress = 0, IsCompleted = false, IsDeleted = false };
            _db.Enrollments.Add(e);
            _db.SaveChanges();

            Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => _service.UpdateProgressAsync(e.Id, -1));
            Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => _service.UpdateProgressAsync(e.Id, 101));
        }

        [Test]
        public async Task UpdateEnrollmentAsync_Should_Update_Fields_And_Log()
        {
            var e = new Enrollment { StudentId = 1, CourseId = 2, EnrollmentDate = DateTime.UtcNow, Progress = 10, IsCompleted = false, IsDeleted = false };
            _db.Enrollments.Add(e);
            await _db.SaveChangesAsync();

            var model = new UpdateEnrollmentFormModel
            {
                StudentId = 5,
                CourseId = 6,
                Progress = 77,
                IsCompleted = true
            };

            var id = await _service.UpdateEnrollmentAsync(e.Id, model);

            Assert.That(id, Is.EqualTo(e.Id));
            var reloaded = await _db.Enrollments.FindAsync(e.Id);
            Assert.That(reloaded, Is.Not.Null);
            Assert.That(reloaded!.StudentId, Is.EqualTo(5));
            Assert.That(reloaded.CourseId, Is.EqualTo(6));
            Assert.That(reloaded.Progress, Is.EqualTo(77));
            Assert.That(reloaded.IsCompleted, Is.True);
            _logger.VerifyLog(LogLevel.Information, "Updated enrollment with ID", Times.AtLeastOnce());
        }

        [Test]
        public void UpdateEnrollmentAsync_Should_Throw_When_Missing_And_LogWarning()
        {
            var model = new UpdateEnrollmentFormModel
            {
                StudentId = 1,
                CourseId = 2,
                Progress = 0,
                IsCompleted = false
            };

            Assert.ThrowsAsync<KeyNotFoundException>(() => _service.UpdateEnrollmentAsync(9999, model));
            _logger.VerifyLog(LogLevel.Warning, "Attempted to update missing/deleted enrollment", Times.AtLeastOnce());
        }
    }
}