using Fcg.UserLibrary.Application.Extensions;
using Fcg.UserLibrary.Infrastructure.Extentions;

namespace Fcg.UserLibrary.API.Extensions
{
    public static class ServicesExtensions
    {
        public static WebApplicationBuilder AddServicesExtensions(this WebApplicationBuilder builder)
        {
            builder.AddObservabilityExtension()
                   .AddSwaggerExtension()
                   .AddAuthorizationExtension();

            builder.Services.AddInfrastructure(builder.Configuration);
            builder.Services.AddAplicationServices();                   
            return builder;
        }
        
       
    }
}
