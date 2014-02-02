using System;
using System.Data.Entity;

namespace MultipleClipboards.Persistence
{
    internal enum DatabaseInitializationStrategy
    {
        DropAndCreateIfModelChanges,
        DropAndCreateAlways
    }

    internal class MultipleClipboardsDatabaseInitializer : IDatabaseInitializer<MultipleClipboardsDataContext>
    {
        private readonly DatabaseInitializationStrategy strategy;

        public MultipleClipboardsDatabaseInitializer(DatabaseInitializationStrategy strategy)
        {
            this.strategy = strategy;
        }

        /// <summary>
        /// Executes the strategy to initialize the database for the given context.
        /// </summary>
        /// <param name="context">The context. </param>
        public void InitializeDatabase(MultipleClipboardsDataContext context)
        {
            IDatabaseInitializer<MultipleClipboardsDataContext> initializer;
            switch (strategy)
            {
                case DatabaseInitializationStrategy.DropAndCreateIfModelChanges:
                    initializer = new DropAndCreateIfModelChangesStrategy();
                    break;
                case DatabaseInitializationStrategy.DropAndCreateAlways:
                    initializer = new DropAndCreateAlwaysStrategy();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            initializer.InitializeDatabase(context);
        }

        private static void SeedDatabase(MultipleClipboardsDataContext context)
        {
            var blacklist = new[]
            {
                new DataFormatBlacklist("FileContents", true),
                new DataFormatBlacklist("MetaFilePict", true)
            };
            context.BlacklistedFormats.AddRange(blacklist);
        }

        private class DropAndCreateIfModelChangesStrategy : DropCreateDatabaseIfModelChanges<MultipleClipboardsDataContext>
        {
            protected override void Seed(MultipleClipboardsDataContext context)
            {
                SeedDatabase(context);
            }
        }

        private class DropAndCreateAlwaysStrategy : DropCreateDatabaseAlways<MultipleClipboardsDataContext>
        {
            protected override void Seed(MultipleClipboardsDataContext context)
            {
                SeedDatabase(context);
            }
        }
    }
}