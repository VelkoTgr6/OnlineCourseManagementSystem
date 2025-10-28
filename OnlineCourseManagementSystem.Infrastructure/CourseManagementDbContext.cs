using Microsoft.EntityFrameworkCore;
using OnlineCourseManagementSystem.Infrastructure.Data.Models;

namespace OnlineCourseManagementSystem.Infrastructure
{
    public class CourseManagementDbContext : DbContext
    {
        public CourseManagementDbContext(DbContextOptions<CourseManagementDbContext> options)
            : base(options)
        {
        }

        public DbSet<Student> Students { get; set; }
        public DbSet<Course> Courses { get; set; }
    }
}
