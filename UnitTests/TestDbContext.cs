using Microsoft.EntityFrameworkCore;
using OnlineCourseManagementSystem.Infrastructure.Data.Models;

namespace UnitTests.Infrastructure
{
    public class TestDbContext : DbContext
    {
        public TestDbContext(DbContextOptions<TestDbContext> options) : base(options) { }

        public DbSet<Student> Students => Set<Student>();
        public DbSet<Course> Courses => Set<Course>();
        public DbSet<Enrollment> Enrollments => Set<Enrollment>();
    }
}