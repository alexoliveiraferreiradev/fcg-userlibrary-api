using Fcg.Core.WebApi.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Fcg.UserLibrary.Infrastructure.Persistence
{
    public class LibraryDbContextFactory : IDesignTimeDbContextFactory<LibraryDbContext>
    {
        public LibraryDbContext CreateDbContext(string[] args)
        {
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";

            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false)
                .AddJsonFile($"appsettings.{environment}.json", optional: true)
                .AddUserSecrets<LibraryDbContextFactory>(optional: true)
                .AddEnvironmentVariables()
                .Build();

            var dbConfig = configuration.GetSection(DatabaseConnectionSettings.DatabaseSettingsSection).Get<DatabaseConnectionSettings>();
            ArgumentNullException.ThrowIfNull(dbConfig, nameof(DatabaseConnectionSettings));

            var options = new DbContextOptionsBuilder<LibraryDbContext>()
                .UseSqlServer(dbConfig.ToConnectionString())
                .Options;

            return new LibraryDbContext(options);
        }
    }
}
