using Microsoft.Extensions.DependencyInjection;

namespace FollowUp.Components.Modules.PatientManagement.Services;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPatientManagementServices(this IServiceCollection services)
    {
        services.AddScoped<IPatientStatisticsService, PatientStatisticsService>();
        return services;
    }
}

