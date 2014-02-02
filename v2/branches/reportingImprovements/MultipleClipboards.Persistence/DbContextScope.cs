using System;
using System.Data.Entity;

namespace MultipleClipboards.Persistence
{
    public class DbContextScope<T> : IDisposable
        where T : DbContext, new()
    {
        private readonly T dbContext;
        private readonly bool shouldDispose;

        public DbContextScope(bool requiresNew = false)
        {
            if (requiresNew || AmbientDbContext == null)
            {
                var sesssion = new T();
                DbContextStacks.Push(sesssion);
                shouldDispose = true;
            }
            else
            {
                dbContext = AmbientDbContext;
                shouldDispose = false;
            }
        }

        public T DbContext
        {
            get { return dbContext; }
        }

        public static T AmbientDbContext
        {
            get { return DbContextStacks.Any<T>() ? DbContextStacks.Peek<T>() : null; }
        }

        public int SaveChanges()
        {
            return dbContext.SaveChanges();
        }

        public void Dispose()
        {
            if (!Equals(AmbientDbContext, DbContext))
            {
                throw new InvalidOperationException("Attempted to dispose a DbContextScope but it is not the current ambient DbContext");
            }

            if (!shouldDispose)
            {
                return;
            }

            DbContextStacks.Pop<T>();
            DbContext.Dispose();
        }
    }
}