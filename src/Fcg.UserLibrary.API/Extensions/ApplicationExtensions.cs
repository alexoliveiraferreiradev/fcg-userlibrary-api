using Fcg.Core.Abstractions.Common;
using Fcg.Core.WebApi.Middleware;
using Fcg.UserLibrary.API.Endpoints;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Serilog;

namespace Fcg.UserLibrary.API.Extensions
{
    public static class ApplicationExtensions
    {
        public static WebApplication AddAppConfiguration(this WebApplication app)
        {
            app.ConfigureEndpoints();
            app.UseSwaggerDocumentation();
            app.UseSerilogRequestLogging();
            app.UseMiddleware<ExceptionMiddleware>();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseHttpsRedirection();
            return app;
        }

        private static WebApplication ConfigureEndpoints(this WebApplication app)
        {
            #region User Library Endpoints
            app.MapLibraryUserEndpoints();
            #endregion

            #region Health Check
            app.MapHealthChecks("/health/liveness", new HealthCheckOptions { Predicate = check => check.Tags.Contains(HealthCheckTags.Live) });
            app.MapHealthChecks("/health/readiness", new HealthCheckOptions
            {
                Predicate = check => check.Tags.Contains(HealthCheckTags.Ready),
                ResultStatusCodes =
                {
                    [HealthStatus.Healthy] = StatusCodes.Status200OK,
                    [HealthStatus.Degraded] = StatusCodes.Status503ServiceUnavailable,
                    [HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable
                }
            });
            #endregion
            return app;
        }
    }
}
