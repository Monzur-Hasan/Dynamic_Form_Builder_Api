using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shared.Services;


namespace Shared
{
    public static class SharedInjector
    {
        public static void SharedConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            ConfigurationHelper.Initialize(configuration);
            services.AddAutoMapper(typeof(SharedInjector));
        }
    }
}
