using Microsoft.Extensions.DependencyInjection;

namespace FollowUp.Components.Modules.ProjectManagement.Services;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddProjectManagementServices(this IServiceCollection services)
    {
        services.AddMemoryCache();
        services.AddScoped<IHospitalStatisticsService, HospitalStatisticsService>();
        return services;
    }
}



