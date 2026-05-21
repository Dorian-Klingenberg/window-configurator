using Microsoft.EntityFrameworkCore;

namespace WindowConfigurator.Tests.Infrastructure
{
    /// <summary>
    /// Smoke tests that verify the EF Core SQLite provider packages are present and functional.
    /// These tests fail to compile if the packages are missing.
    /// </summary>
    public class EfCorePackagesTests
    {
        [Fact]
        public void SqliteProvider_CanBeConfiguredViaOptionsBuilder()
        {
            var options = new DbContextOptionsBuilder()
                .UseSqlite("Data Source=:memory:")
                .Options;

            Assert.NotNull(options);
        }

        [Fact]
        public void SqliteProvider_OptionsAreNotNull_AfterConfiguration()
        {
            var options = new DbContextOptionsBuilder()
                .UseSqlite("Data Source=:memory:")
                .Options;

            // FindExtension returns non-null only when the Sqlite provider is registered
            var ext = options.Extensions.FirstOrDefault(e => e.GetType().Name.Contains("Sqlite"));
            Assert.NotNull(ext);
        }
    }
}
