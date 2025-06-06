using CbUpdate.Configuration;
using CbUpdate.Infrastructure.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CbUpdate.Configuration;

public static class AppSettingsConfiguration
{
    public static IServiceCollection AddAppSettingsModule(this IServiceCollection services, IConfiguration configuration)
    {
        // Use this to load settings from appSettings file
        services.Configure<SecuritySettings>(options => configuration.GetSection("security").Bind(options));

        return services;
    }
}
