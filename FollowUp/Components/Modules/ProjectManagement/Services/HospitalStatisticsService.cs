using Bio.Models;
using FollowUp.Components.Modules.ProjectManagement.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FollowUp.Components.Modules.ProjectManagement.Services;

public interface IOverviewStatisticsService
{
    Task<HospitalUserStatsDto> GetUserStatsAsync(Guid hospitalId, IReadOnlyList<DepartmentProjectMap> departmentProjects, int? year, CancellationToken cancellationToken = default);
    Task<IReadOnlyDictionary<Guid, DepartmentSummaryDto>> GetDepartmentSummariesAsync(Guid hospitalId, IReadOnlyList<DepartmentProjectMap> departmentProjects, CancellationToken cancellationToken = default);
    Task<IReadOnlyDictionary<Guid, ProjectSummaryDto>> GetProjectSummariesAsync(Guid hospitalId, IReadOnlyList<Guid> projectIds, int? year = null, CancellationToken cancellationToken = default);
}

public sealed class OverviewStatisticsService : IOverviewStatisticsService
{
    private readonly IDbContextFactory<CubeDbContext> _contextFactory;
    private readonly ILogger<OverviewStatisticsService> _logger;

    public OverviewStatisticsService(IDbContextFactory<CubeDbContext> contextFactory, ILogger<OverviewStatisticsService> logger)
    {
        _contextFactory = contextFactory;
        _logger = logger;
    }

    public async Task<HospitalUserStatsDto> GetUserStatsAsync(Guid hospitalId, IReadOnlyList<DepartmentProjectMap> departmentProjects, int? year, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<DepartmentProjectMap> effectiveDepartments = departmentProjects ?? Array.Empty<DepartmentProjectMap>();
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

        var patientQuery = context.patient.AsNoTracking()
            .Where(p => p.hospital_id == hospitalId && p.is_valid == true && p.source_type == "followup");

        // 如果传入了科室/课题映射，则按课题范围过滤患者
        var projectIds = effectiveDepartments
            .SelectMany(d => d.ProjectIds ?? Array.Empty<Guid>())
            .Where(id => id != Guid.Empty)
            .Distinct()
            .ToList();

        if (projectIds.Any())
        {
            patientQuery = patientQuery.Where(p => projectIds.Contains(p.project_id));
        }

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
            .Where(pe => pe.patient.hospital_id == hospitalId && pe.is_valid == true && pe.push_time.HasValue);

        if (projectIds.Any())
        {
            eventsQuery = eventsQuery.Where(pe => projectIds.Contains(pe.project_id));
        }

        if (year.HasValue)
        {
            eventsQuery = eventsQuery.Where(pe => pe.push_time.Value.Year == year.Value);
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

        // GetActivityStatsAsync / GetFollowupTrendAsync / GetEducationTrendAsync 已迁移到各自领域模块的统计服务中。
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

    public async Task<IReadOnlyDictionary<Guid, ProjectSummaryDto>> GetProjectSummariesAsync(Guid hospitalId, IReadOnlyList<Guid> projectIds, int? year = null, CancellationToken cancellationToken = default)
    {
        if (projectIds == null || !projectIds.Any())
        {
            return new Dictionary<Guid, ProjectSummaryDto>();
        }

        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

        var today = DateTime.Today;
        var now = DateTime.Now;
        var currentYear = now.Year;
        var yearStart = year.HasValue ? new DateTime(year.Value, 1, 1) : new DateTime(currentYear, 1, 1);
        var yearEnd = year.HasValue ? new DateTime(year.Value, 12, 31) : new DateTime(currentYear, 12, 31);

        // 查询患者数据
        var patients = await context.patient.AsNoTracking()
            .Where(p => p.hospital_id == hospitalId && p.is_valid == true && p.source_type == "followup" && projectIds.Contains(p.project_id))
            .Select(p => new { p.id, p.project_id, p.create_time })
            .ToListAsync(cancellationToken);

        // 查询随访事件
        var events = await context.patient_event.AsNoTracking()
            .Where(pe => pe.patient.hospital_id == hospitalId && pe.is_valid == true && projectIds.Contains(pe.project_id))
            .Select(pe => new { pe.project_id, pe.patient_id, pe.event_status, pe.push_time, pe.input_time, pe.create_time })
            .ToListAsync(cancellationToken);

        // 查询课题名称
        var projects = await context.form_project.AsNoTracking()
            .Where(p => projectIds.Contains(p.id))
            .Select(p => new { p.id, p.name, p.display_name })
            .ToListAsync(cancellationToken);

        var result = new Dictionary<Guid, ProjectSummaryDto>();

        foreach (var projectId in projectIds)
        {
            var projectInfo = projects.FirstOrDefault(p => p.id == projectId);
            var projectName = projectInfo?.display_name ?? projectInfo?.name ?? "未知课题";

            var projectPatients = patients.Where(p => p.project_id == projectId).ToList();
            var projectEvents = events.Where(e => e.project_id == projectId).ToList();

            var patientCount = projectPatients.Count;
            var followupTaskCount = projectEvents.Count;

            // 状态统计
            var statusCounts = CreateStatusDictionary(
                projectEvents.Count(e => e.event_status == "已随访"),
                projectEvents.Count(e => e.event_status == "待审核"),
                projectEvents.Count(e => e.event_status == "患者未提交"),
                projectEvents.Count(e => e.event_status == "已超时"));

            // 本月随访率
            var currentMonthEvents = projectEvents
                .Where(e => e.push_time.HasValue && e.push_time.Value.Year == now.Year && e.push_time.Value.Month == now.Month)
                .ToList();

            double followupRate = 0;
            if (currentMonthEvents.Count > 0)
            {
                var completed = currentMonthEvents.Count(e => e.input_time.HasValue);
                followupRate = Math.Round((double)completed / currentMonthEvents.Count * 100, 1);
            }

            // 今日新增（患者）
            var todayNewCount = projectPatients.Count(p => p.create_time.HasValue && p.create_time.Value.Date == today);

            // 年度统计
            var yearlyEvents = projectEvents.Where(e => e.push_time.HasValue && e.push_time.Value >= yearStart && e.push_time.Value <= yearEnd).ToList();
            var yearlyTaskCount = yearlyEvents.Count;
            var yearlyPatientCount = yearlyEvents.Select(e => e.patient_id).Distinct().Count();

            // 未到推送时间
            var notPushedCount = projectEvents.Count(e => !e.push_time.HasValue || e.push_time.Value > now);

            result[projectId] = new ProjectSummaryDto(
                projectId,
                projectName,
                patientCount,
                followupTaskCount,
                followupRate,
                statusCounts,
                todayNewCount,
                yearlyTaskCount,
                yearlyPatientCount,
                notPushedCount);
        }

        return result;
    }

    private sealed record PatientSlim(Guid Id, Guid ProjectId, DateOnly? Birthday, DateTime? CreateTime);
    private sealed record FollowupEventSlim(Guid PatientId, Guid ProjectId, string? Status, DateTime? PushTime, DateTime? InputTime);
}
