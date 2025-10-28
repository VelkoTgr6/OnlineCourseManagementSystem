using Microsoft.EntityFrameworkCore;
using OnlineCourseManagementSystem.Infrastructure.Data.Models;
using System.Threading.Tasks;

namespace OnlineCourseManagementSystem.Infrastructure.Data.Common
{
    public class Repository : IRepository
    {
        private readonly CourseManagementDbContext context;

        public Repository(CourseManagementDbContext _context)
        {
            context = _context;
        }
        private DbSet<T> DbSet<T>() where T : class
        {
            return context.Set<T>();
        }

        public async Task AddAsync<T>(T entity) where T : class
        {
            await DbSet<T>().AddAsync(entity);
        }

        public IQueryable<T> All<T>() where T : class
        {
            return DbSet<T>();
        }

        public IQueryable<T> AllAsReadOnly<T>() where T : class
        {
            return DbSet<T>().AsNoTracking();
        }

        public void Delete<T>(T entity) where T : class
        {
            if (entity is Student student)
            {
                DbSet<Student>().Where(s => s.Id == student.Id)
                .Select(s => s.IsDeleted == true)
                .FirstOrDefault();
            }
            else if (entity is Course course)
            {
                DbSet<Course>().Where(c => c.Id == course.Id)
                .Select(c => c.IsDeleted == true)
                .FirstOrDefault();
            }
        }

        public async Task<T?> GetByIdAsync<T>(object id) where T : class
        {
            return await DbSet<T>().FindAsync(id);
        }

        public async Task<int> SaveChangesAsync()
        {
            return await context.SaveChangesAsync();
        }
    }
}
