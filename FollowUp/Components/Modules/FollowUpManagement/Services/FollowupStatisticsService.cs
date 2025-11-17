using Bio.Models;
using FollowUp.Components.Modules.ProjectManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace FollowUp.Components.Modules.FollowUpManagement.Services;

public interface IFollowupStatisticsService
{
    Task<FollowupTrendDto> GetFollowupTrendAsync(
        Guid hospitalId,
        int year,
        IReadOnlyCollection<Guid>? projectIds = null,
        CancellationToken cancellationToken = default);
}

public sealed class FollowupStatisticsService : IFollowupStatisticsService
{
    private readonly IDbContextFactory<CubeDbContext> _contextFactory;

    public FollowupStatisticsService(IDbContextFactory<CubeDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<FollowupTrendDto> GetFollowupTrendAsync(
        Guid hospitalId,
        int year,
        IReadOnlyCollection<Guid>? projectIds = null,
        CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

        var eventsQuery = context.patient_event.AsNoTracking()
            .Where(pe => pe.patient.hospital_id == hospitalId && pe.is_valid == true && pe.push_time.HasValue && pe.push_time.Value.Year == year);

        if (projectIds != null && projectIds.Count > 0)
        {
            eventsQuery = eventsQuery.Where(pe => projectIds.Contains(pe.project_id));
        }

        var events = await eventsQuery
            .Select(pe => new { pe.push_time, pe.input_time })
            .ToListAsync(cancellationToken);

        var monthlyRates = new List<double>();
        for (var month = 1; month <= 12; month++)
        {
            var monthlyEvents = events.Where(e => e.push_time!.Value.Month == month).ToList();
            if (monthlyEvents.Count == 0)
            {
                monthlyRates.Add(0);
                continue;
            }

            var completed = monthlyEvents.Count(e => e.input_time.HasValue);
            monthlyRates.Add(Math.Round((double)completed / monthlyEvents.Count * 100, 2));
        }

        return new FollowupTrendDto(year, monthlyRates);
    }
}

