using Fcg.UserLibrary.API.Extensions;
using Fcg.UserLibrary.Application.Common.Interfaces;
using Fcg.UserLibrary.Domain.Repositories;
using Fcg.UserLibrary.Infrastructure.Caching;
using Fcg.UserLibrary.Infrastructure.Persistence;
using Fcg.UserLibrary.Infrastructure.Queries;
using Fcg.UserLibrary.Infrastructure.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Data;

namespace Fcg.UserLibrary.Infrastructure.Extentions
{
    public static class InfrastructureServiceCollectionExtensions
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services,IConfiguration configuration)
        {
            services.AddHealthCheckExtension(configuration);
            services.AddMongoExtension(configuration);
            services.AddEntityExtension(configuration);
            services.AddRedisExtension(configuration);  
            services.AddMassTransitExtension(configuration);    
            services.AddScoped<IDbConnection>(sp => sp.GetRequiredService<LibraryDbContext>().Database.GetDbConnection());
            services.AddScoped<ICacheService, CacheService>();
            services.AddScoped<ILibraryEntryRepository, LibraryEntryRepository>();
            services.AddScoped<IGameRepository, GameSnapshotRepository>();
            services.AddScoped<ILibraryEntryQueryRepository, LibraryEntryQueryRepository>();
            return  services;
        }
    }
}
