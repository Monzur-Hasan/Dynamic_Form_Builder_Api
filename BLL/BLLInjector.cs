using BLL.Service.Implementation;
using BLL.Service.Interface;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shared;

public static class BLLInjector
{
    public static void BLLConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        // Inject Shared Services
        SharedInjector.SharedConfigureServices(services, configuration);

        // Inject Repositories
        services.AddScoped<IFormRepository, FormRepository>();
        services.AddScoped<IOptionRepository, OptionRepository>();
    }
}

