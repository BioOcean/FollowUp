using Bio.Models;
using FollowUp.Components.Modules.ProjectManagement.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;


namespace FollowUp.Components.Modules.EducationManagement.Services;

public interface IEducationStatisticsService
{
    Task<EducationTrendDto> GetEducationTrendAsync(
        Guid hospitalId,
        int year,
        Guid? departmentId = null,
        IReadOnlyCollection<Guid>? projectIds = null,
        CancellationToken cancellationToken = default);
}

public sealed class EducationStatisticsService : IEducationStatisticsService
{
    private readonly IDbContextFactory<CubeDbContext> _contextFactory;

    public EducationStatisticsService(IDbContextFactory<CubeDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<EducationTrendDto> GetEducationTrendAsync(
        Guid hospitalId,
        int year,
        Guid? departmentId = null,
        IReadOnlyCollection<Guid>? projectIds = null,
        CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

        var educationDataQuery = context.followup_education_push.AsNoTracking()
            .Where(p => p.followup_education != null
                        && p.followup_education.hospital_id == hospitalId
                        && p.push_time.Year == year);

        if (departmentId.HasValue)
        {
            educationDataQuery = educationDataQuery
                .Where(p => p.followup_education!.department_id == departmentId.Value);
        }
        else if (projectIds != null && projectIds.Count > 0)
        {
            educationDataQuery = educationDataQuery
                .Where(p => p.followup_education!.project_id.Any(id => projectIds.Contains(id)));
        }

        var educationData = await educationDataQuery
            .Select(p => new { p.push_time, p.read_time })
            .ToListAsync(cancellationToken);

        var monthlyRates = new List<double>();
        for (var month = 1; month <= 12; month++)
        {
            var monthlyData = educationData.Where(e => e.push_time.Month == month).ToList();
            if (monthlyData.Count == 0)
            {
                monthlyRates.Add(0);
                continue;
            }

            var readCount = monthlyData.Count(e => e.read_time.HasValue);
            monthlyRates.Add(Math.Round((double)readCount / monthlyData.Count * 100, 2));
        }

        return new EducationTrendDto(year, monthlyRates);
    }
}

