using System;
using System.Configuration;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;

namespace MultipleClipboards.Persistence
{
    public class MultipleClipboardsDataContext : DbContext
    {
        static MultipleClipboardsDataContext()
        {
            DatabaseInitializationStrategy strategy;
            if (!Enum.TryParse(ConfigurationManager.AppSettings["databaseInitializationStrategy"], true, out strategy))
            {
                strategy = DatabaseInitializationStrategy.DropAndCreateIfModelChanges;
            }
            Database.SetInitializer(new MultipleClipboardsDatabaseInitializer(strategy));
        }

        public MultipleClipboardsDataContext()
        {
        }

        public MultipleClipboardsDataContext(string connectionString)
            : base(connectionString)
        {
        }

        public DbSet<DataFormat> DataFormats { get; set; }
        public DbSet<FailedDataFormat> FailedDataFormats { get; set; }
        public DbSet<DataObject> DataObjects { get; set; }
        public DbSet<DataFormatBlacklist> BlacklistedFormats { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<DataObject>()
                        .HasMany(o => o.AllFormats)
                        .WithMany();
            
            modelBuilder.Entity<DataObject>()
                        .HasMany(o => o.FailedDataFormats)
                        .WithMany();

            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
        }
    }
}