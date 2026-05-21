using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace WindowConfigurator.Data
{
    /// <summary>
    /// Provides a design-time DbContext instance for EF Core CLI tools (migrations).
    /// Using this factory avoids the need to reference the web host project when running
    /// dotnet ef from the WindowConfigurator.Data project directory.
    /// </summary>
    public class WindowConfiguratorDbContextFactory : IDesignTimeDbContextFactory<WindowConfiguratorDbContext>
    {
        public WindowConfiguratorDbContext CreateDbContext(string[] args)
        {
            var options = new DbContextOptionsBuilder<WindowConfiguratorDbContext>()
                .UseSqlite("Data Source=windowconfigurator.db")
                .Options;

            return new WindowConfiguratorDbContext(options);
        }
    }
}
