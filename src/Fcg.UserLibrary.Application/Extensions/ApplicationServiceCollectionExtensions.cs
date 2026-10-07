using Fcg.UserLibrary.Application.Common.Behaviors;
using Fcg.UserLibrary.Application.Features.Commands.OrderFinalize;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Fcg.UserLibrary.Application.Extensions
{
    public static class ApplicationServiceCollectionExtensions
    {
        public static IServiceCollection AddAplicationServices(this IServiceCollection services)
        {
            services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssembly(typeof(LibraryOrderCommand).Assembly);
            });

            services.AddTransient(
                typeof(IPipelineBehavior<,>),
                typeof(LoggingBehavior<,>));

            return services;
        }
    }
}
