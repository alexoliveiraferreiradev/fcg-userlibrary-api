using Fcg.Core.Abstractions.Common;
using Fcg.UserLibrary.Infrastructure.Caching;
using Fcg.UserLibrary.Infrastructure.Persistence;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using StackExchange.Redis;

namespace Fcg.UserLibrary.Infrastructure.Extentions
{
    internal static class ObservabilityExtension
    {
        public static IServiceCollection AddHealthCheckExtension(this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddHealthChecks()
                .AddCheck("live", () => HealthCheckResult.Healthy(), tags: new[] { HealthCheckTags.Live })
                .AddDbContextCheck<LibraryDbContext>(
                name: "database-healthcheck",
                tags: new[] { HealthCheckTags.Ready })
                .AddRedis(
                    sp => sp.GetRequiredService<IConnectionMultiplexer>(),
                    name: "redis-healthcheck",
                    tags: new[] { HealthCheckTags.Ready });

            return services;
        }
    }
}
