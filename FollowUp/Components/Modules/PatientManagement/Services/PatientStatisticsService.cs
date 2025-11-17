using Bio.Models;
using FollowUp.Components.Modules.ProjectManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace FollowUp.Components.Modules.PatientManagement.Services;

public interface IPatientStatisticsService
{
    Task<ActivityStatsDto> GetActivityStatsAsync(
        Guid hospitalId,
        ActivityDimension dimension,
        DateTime targetDate,
        IReadOnlyCollection<Guid>? projectIds = null,
        Guid? filterHospitalId = null,
        CancellationToken cancellationToken = default);
}

public sealed class PatientStatisticsService : IPatientStatisticsService
{
    private readonly IDbContextFactory<CubeDbContext> _contextFactory;

    public PatientStatisticsService(IDbContextFactory<CubeDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<ActivityStatsDto> GetActivityStatsAsync(
        Guid hospitalId,
        ActivityDimension dimension,
        DateTime targetDate,
        IReadOnlyCollection<Guid>? projectIds = null,
        Guid? filterHospitalId = null,
        CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

        var patientQuery = context.patient.AsNoTracking()
            .Where(p => p.is_valid == true && p.source_type == "followup");

        var effectiveHospitalId = filterHospitalId ?? hospitalId;
        patientQuery = patientQuery.Where(p => p.hospital_id == effectiveHospitalId);

        if (projectIds != null && projectIds.Count > 0)
        {
            patientQuery = patientQuery.Where(p => projectIds.Contains(p.project_id));
        }

        var patientIds = await patientQuery
            .Select(p => p.id)
            .ToListAsync(cancellationToken);

        var totalPatients = patientIds.Count;

        var visitRecords = await context.followup_patient_visit_behavior_record.AsNoTracking()
            .Where(v => patientIds.Contains(v.patient_id))
            .Select(v => new { v.patient_id, v.visit_time })
            .ToListAsync(cancellationToken);

        List<int> activeCounts = new();
        List<int> activePercents = new();
        List<string> labels = new();

        if (dimension == ActivityDimension.Daily)
        {
            var firstDay = new DateTime(targetDate.Year, targetDate.Month, 1);
            var daysInMonth = DateTime.DaysInMonth(targetDate.Year, targetDate.Month);

            for (var day = 0; day < daysInMonth; day++)
            {
                var currentDate = firstDay.AddDays(day);
                var activePatients = visitRecords
                    .Where(v => v.visit_time.Date == currentDate.Date)
                    .Select(v => v.patient_id)
                    .Distinct()
                    .Count();

                activeCounts.Add(activePatients);
                activePercents.Add(totalPatients == 0
                    ? 0
                    : (int)Math.Round((double)activePatients / totalPatients * 100));
                labels.Add($"{currentDate:dd}日");
            }
        }
        else
        {
            for (var month = 1; month <= 12; month++)
            {
                var activePatients = visitRecords
                    .Where(v => v.visit_time.Year == targetDate.Year && v.visit_time.Month == month)
                    .Select(v => v.patient_id)
                    .Distinct()
                    .Count();

                activeCounts.Add(activePatients);
                activePercents.Add(totalPatients == 0
                    ? 0
                    : (int)Math.Round((double)activePatients / totalPatients * 100));
                labels.Add($"{month}月");
            }
        }

        return new ActivityStatsDto(dimension, targetDate, activeCounts, activePercents, labels);
    }
}

