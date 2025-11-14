using Bio.Models;
using FollowUp.Components.Modules.ProjectManagement.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FollowUp.Components.Modules.ProjectManagement.Services;

public interface IHospitalStatisticsService
{
    Task<HospitalUserStatsDto> GetHospitalUserStatsAsync(Guid hospitalId, IReadOnlyList<DepartmentProjectMap> departmentProjects, int? year, CancellationToken cancellationToken = default);
    Task<IReadOnlyDictionary<Guid, DepartmentSummaryDto>> GetDepartmentSummariesAsync(Guid hospitalId, IReadOnlyList<DepartmentProjectMap> departmentProjects, CancellationToken cancellationToken = default);
    Task<ActivityStatsDto> GetActivityStatsAsync(Guid hospitalId, ActivityDimension dimension, DateTime targetDate, Guid? filterHospitalId = null, CancellationToken cancellationToken = default);
    Task<FollowupTrendDto> GetFollowupTrendAsync(Guid hospitalId, int year, CancellationToken cancellationToken = default);
    Task<EducationTrendDto> GetEducationTrendAsync(Guid hospitalId, int year, CancellationToken cancellationToken = default);
}

public sealed class HospitalStatisticsService : IHospitalStatisticsService
{
    private readonly IDbContextFactory<CubeDbContext> _contextFactory;
    private readonly ILogger<HospitalStatisticsService> _logger;

    public HospitalStatisticsService(IDbContextFactory<CubeDbContext> contextFactory, ILogger<HospitalStatisticsService> logger)
    {
        _contextFactory = contextFactory;
        _logger = logger;
    }

    public async Task<HospitalUserStatsDto> GetHospitalUserStatsAsync(Guid hospitalId, IReadOnlyList<DepartmentProjectMap> departmentProjects, int? year, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<DepartmentProjectMap> effectiveDepartments = departmentProjects ?? Array.Empty<DepartmentProjectMap>();
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

        var patientQuery = context.patient.AsNoTracking()
            .Where(p => p.hospital_id == hospitalId && p.is_valid == true && p.source_type == "followup");

        DateTime? start = null;
        DateTime? end = null;
        if (year.HasValue)
        {
            start = new DateTime(year.Value, 1, 1);
            end = start.Value.AddYears(1);
            patientQuery = patientQuery.Where(p => p.create_time >= start && p.create_time < end);
        }

        var patients = await patientQuery
            .Select(p => new PatientSlim(p.id, p.project_id, p.birthday, p.create_time))
            .ToListAsync(cancellationToken);

        var eventsQuery = context.patient_event.AsNoTracking()
            .Where(pe => pe.patient.hospital_id == hospitalId && pe.is_valid == true);
        if (year.HasValue)
        {
            eventsQuery = eventsQuery.Where(pe => pe.push_time.HasValue && pe.push_time.Value.Year == year.Value);
        }

        var events = await eventsQuery
            .Select(pe => new FollowupEventSlim(pe.patient_id, pe.project_id, pe.event_status, pe.push_time, pe.input_time))
            .ToListAsync(cancellationToken);

        var ageLabels = new[] { "小于40岁", "40-49岁", "50-59岁", "60-69岁", "70岁以上" };
        var ageValues = CalculateAgeBuckets(patients);

        var departmentLabels = effectiveDepartments.Select(d => d.DepartmentName).ToList();
        var departmentValues = CalculateDepartmentPatientCounts(patients, effectiveDepartments);

        var todayLabels = departmentLabels;
        var todayValues = CalculateTodayPatientCounts(patients, effectiveDepartments);

        var totalFollowupTasks = events.Count;
        var totalFollowupPatients = events.Select(e => e.PatientId).Distinct().Count();
        var today = DateTime.Today;
        var todayNewPatients = patients.Count(p => p.CreateTime.HasValue && p.CreateTime.Value.Date == today);

        return new HospitalUserStatsDto(
            patients.Count,
            totalFollowupTasks,
            totalFollowupPatients,
            todayNewPatients,
            ageLabels,
            ageValues,
            departmentLabels,
            departmentValues,
            todayLabels,
            todayValues);
    }

    public async Task<IReadOnlyDictionary<Guid, DepartmentSummaryDto>> GetDepartmentSummariesAsync(Guid hospitalId, IReadOnlyList<DepartmentProjectMap> departmentProjects, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<DepartmentProjectMap> effectiveDepartments = departmentProjects ?? Array.Empty<DepartmentProjectMap>();
        if (!effectiveDepartments.Any())
        {
            return new Dictionary<Guid, DepartmentSummaryDto>();
        }

        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

        var patients = await context.patient.AsNoTracking()
            .Where(p => p.hospital_id == hospitalId && p.is_valid == true && p.source_type == "followup")
            .Select(p => new { p.id, p.project_id })
            .ToListAsync(cancellationToken);

        var events = await context.patient_event.AsNoTracking()
            .Where(pe => pe.patient.hospital_id == hospitalId && pe.is_valid == true)
            .Select(pe => new { pe.project_id, pe.patient_id, pe.event_status, pe.push_time, pe.input_time })
            .ToListAsync(cancellationToken);

        var now = DateTime.Now;
        var result = new Dictionary<Guid, DepartmentSummaryDto>();

        foreach (var department in effectiveDepartments)
        {
            var projectSet = department.ProjectIds?.Where(id => id != Guid.Empty).ToHashSet() ?? new HashSet<Guid>();
            if (projectSet.Count == 0)
            {
                result[department.DepartmentId] = new DepartmentSummaryDto(
                    department.DepartmentId,
                    department.DepartmentName,
                    0,
                    0,
                    0,
                    CreateStatusDictionary(0, 0, 0, 0));
                continue;
            }

            var patientCount = patients.Count(p => projectSet.Contains(p.project_id));

            var departmentEvents = events
                .Where(e => projectSet.Contains(e.project_id))
                .ToList();

            var followupTaskCount = departmentEvents.Count;
            var statusCounts = CreateStatusDictionary(
                departmentEvents.Count(e => e.event_status == "已随访"),
                departmentEvents.Count(e => e.event_status == "待审核"),
                departmentEvents.Count(e => e.event_status == "患者未提交"),
                departmentEvents.Count(e => e.event_status == "已超时"));

            var currentMonthEvents = departmentEvents
                .Where(e => e.push_time.HasValue && e.push_time.Value.Year == now.Year && e.push_time.Value.Month == now.Month)
                .ToList();

            double followupRate = 0;
            if (currentMonthEvents.Count > 0)
            {
                var completed = currentMonthEvents.Count(e => e.input_time.HasValue);
                followupRate = Math.Round((double)completed / currentMonthEvents.Count * 100, 1);
            }

            result[department.DepartmentId] = new DepartmentSummaryDto(
                department.DepartmentId,
                department.DepartmentName,
                patientCount,
                followupTaskCount,
                followupRate,
                statusCounts);
        }

        return result;
    }

    public async Task<ActivityStatsDto> GetActivityStatsAsync(Guid hospitalId, ActivityDimension dimension, DateTime targetDate, Guid? filterHospitalId = null, CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

        var patientQuery = context.patient.AsNoTracking()
            .Where(p => p.is_valid == true && p.source_type == "followup");

        if (filterHospitalId.HasValue)
        {
            patientQuery = patientQuery.Where(p => p.hospital_id == filterHospitalId.Value);
        }
        else
        {
            patientQuery = patientQuery.Where(p => p.hospital_id == hospitalId);
        }

        // 查询患者及其最近登录时间（使用新字段 last_login_time）
        var patients = await patientQuery
            .Select(p => new { p.id, p.last_login_time })
            .ToListAsync(cancellationToken);
        
        var totalPatients = patients.Count;

        List<int> activeCounts = new();
        List<int> activePercents = new();
        List<string> labels = new();

        if (dimension == ActivityDimension.Daily)
        {
            var firstDay = new DateTime(targetDate.Year, targetDate.Month, 1);
            var daysInMonth = DateTime.DaysInMonth(targetDate.Year, targetDate.Month);

            for (int day = 0; day < daysInMonth; day++)
            {
                var currentDate = firstDay.AddDays(day);
                var activePatients = patients
                    .Count(p => p.last_login_time.HasValue && p.last_login_time.Value.Date == currentDate.Date);
                
                activeCounts.Add(activePatients);
                activePercents.Add(totalPatients == 0 ? 0 : (int)Math.Round((double)activePatients / totalPatients * 100));
                labels.Add($"{currentDate:dd}日");
            }
        }
        else
        {
            for (int month = 1; month <= 12; month++)
            {
                var activePatients = patients
                    .Count(p => p.last_login_time.HasValue 
                             && p.last_login_time.Value.Year == targetDate.Year 
                             && p.last_login_time.Value.Month == month);
                
                activeCounts.Add(activePatients);
                activePercents.Add(totalPatients == 0 ? 0 : (int)Math.Round((double)activePatients / totalPatients * 100));
                labels.Add($"{month}月");
            }
        }

        return new ActivityStatsDto(dimension, targetDate, activeCounts, activePercents, labels);
    }

    public async Task<FollowupTrendDto> GetFollowupTrendAsync(Guid hospitalId, int year, CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

        // 查询随访事件（使用新字段 push_time 和 input_time）
        var events = await context.patient_event.AsNoTracking()
            .Where(pe => pe.patient.hospital_id == hospitalId && pe.is_valid == true && pe.push_time.HasValue && pe.push_time.Value.Year == year)
            .Select(pe => new { pe.push_time, pe.input_time })
            .ToListAsync(cancellationToken);

        var monthlyRates = new List<double>();
        for (int month = 1; month <= 12; month++)
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

    public async Task<EducationTrendDto> GetEducationTrendAsync(Guid hospitalId, int year, CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

        // 查询宣教推送（使用新字段 read_time）
        var educationData = await context.followup_education_push.AsNoTracking()
            .Where(p => p.followup_education != null && p.followup_education.hospital_id == hospitalId && p.push_time.Year == year)
            .Select(p => new { p.push_time, p.read_time })
            .ToListAsync(cancellationToken);

        var monthlyRates = new List<double>();
        for (int month = 1; month <= 12; month++)
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
    private static IReadOnlyList<double> CalculateAgeBuckets(IEnumerable<PatientSlim> patients)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var buckets = new double[5];

        foreach (var patient in patients.Where(p => p.Birthday.HasValue))
        {
            var birthday = patient.Birthday!.Value;
            var age = today.Year - birthday.Year;
            if (birthday > today.AddYears(-age))
            {
                age--;
            }

            var index = age < 40 ? 0 :
                        age < 50 ? 1 :
                        age < 60 ? 2 :
                        age < 70 ? 3 : 4;
            buckets[index]++;
        }

        return buckets;
    }

    private static IReadOnlyList<double> CalculateDepartmentPatientCounts(IEnumerable<PatientSlim> patients, IReadOnlyList<DepartmentProjectMap> departments)
    {
        return departments.Select(d =>
        {
            var projectSet = d.ProjectIds?.Where(id => id != Guid.Empty).ToHashSet() ?? new HashSet<Guid>();
            if (projectSet.Count == 0)
            {
                return 0d;
            }

            var count = patients.Count(p => projectSet.Contains(p.ProjectId));
            return (double)count;
        }).ToList();
    }

    private static IReadOnlyList<double> CalculateTodayPatientCounts(IEnumerable<PatientSlim> patients, IReadOnlyList<DepartmentProjectMap> departments)
    {
        var today = DateTime.Today;
        var todayPatients = patients.Where(p => p.CreateTime.HasValue && p.CreateTime.Value.Date == today).ToList();

        return departments.Select(d =>
        {
            var projectSet = d.ProjectIds?.Where(id => id != Guid.Empty).ToHashSet() ?? new HashSet<Guid>();
            if (projectSet.Count == 0)
            {
                return 0d;
            }

            var count = todayPatients.Count(p => projectSet.Contains(p.ProjectId));
            return (double)count;
        }).ToList();
    }

    private static IReadOnlyDictionary<string, int> CreateStatusDictionary(int completed, int pending, int notSubmitted, int overdue)
    {
        return new Dictionary<string, int>
        {
            ["已随访"] = completed,
            ["待审核"] = pending,
            ["患者未提交"] = notSubmitted,
            ["已超时"] = overdue
        };
    }

    private sealed record PatientSlim(Guid Id, Guid ProjectId, DateOnly? Birthday, DateTime? CreateTime);
    private sealed record FollowupEventSlim(Guid PatientId, Guid ProjectId, string? Status, DateTime? PushTime, DateTime? InputTime);
}
