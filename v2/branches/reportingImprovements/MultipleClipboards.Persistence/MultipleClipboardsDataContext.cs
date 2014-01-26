using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;

namespace MultipleClipboards.Persistence
{
    public class MultipleClipboardsDataContext : DbContext
    {
        static MultipleClipboardsDataContext()
        {
            Database.SetInitializer(new DropCreateDatabaseIfModelChanges<MultipleClipboardsDataContext>());
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