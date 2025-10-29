using Microsoft.EntityFrameworkCore;
using OnlineCourseManagementSystem.Infrastructure.Data.Common;

namespace UnitTests.Infrastructure
{
    public class InMemoryRepository : IRepository
    {
        private readonly DbContext context;

        public InMemoryRepository(DbContext context)
        {
            this.context = context;
        }

        public IQueryable<T> All<T>() where T : class
            => context.Set<T>();

        public IQueryable<T> AllAsReadOnly<T>() where T : class
            => context.Set<T>().AsNoTracking();

        public Task AddAsync<T>(T entity) where T : class
            => context.AddAsync(entity).AsTask();

        public Task<int> SaveChangesAsync()
            => context.SaveChangesAsync();

        public void Delete<T>(T entity) where T : class
            => context.Remove(entity);

        public async Task<T?> GetByIdAsync<T>(object id) where T : class
            => await context.Set<T>().FindAsync(id);
    }
}