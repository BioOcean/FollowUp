using Microsoft.Extensions.DependencyInjection;

namespace FollowUp.Components.Modules.FollowUpManagement.Services;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddFollowUpManagementServices(this IServiceCollection services)
    {
        services.AddScoped<IFollowupStatisticsService, FollowupStatisticsService>();
        return services;
    }
}

