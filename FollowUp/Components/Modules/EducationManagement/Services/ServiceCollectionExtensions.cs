using Microsoft.Extensions.DependencyInjection;

namespace FollowUp.Components.Modules.EducationManagement.Services;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddEducationManagementServices(this IServiceCollection services)
    {
        services.AddScoped<IEducationStatisticsService, EducationStatisticsService>();
        return services;
    }
}

