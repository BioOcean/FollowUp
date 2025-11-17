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

        // 数据库层过滤：医院 + 年份 + 可选科室
        var query = context.followup_education_push.AsNoTracking()
            .Where(p => p.followup_education != null
                        && p.followup_education.hospital_id == hospitalId
                        && p.push_time.Year == year);

        if (departmentId.HasValue)
        {
            query = query.Where(p => p.followup_education!.department_id == departmentId.Value);
        }

        // 查询到内存（包含 project_id 数组字段）
        var allData = await query
            .Select(p => new
            {
                PushTime = p.push_time,
                ReadTime = p.read_time,
                ProjectIds = p.followup_education!.project_id
            })
            .ToListAsync(cancellationToken);

        // 内存过滤：按项目 ID 数组（EF Core 无法将数组查询转换为 SQL）
        var filteredData = (projectIds != null && projectIds.Count > 0)
            ? allData.Where(p => p.ProjectIds != null && p.ProjectIds.Any(id => projectIds.Contains(id)))
            : allData;

        // 按月统计阅读率
        var monthlyRates = new List<double>(12);
        for (var month = 1; month <= 12; month++)
        {
            var monthData = filteredData.Where(d => d.PushTime.Month == month).ToList();
            if (monthData.Count == 0)
            {
                monthlyRates.Add(0);
            }
            else
            {
                var readCount = monthData.Count(d => d.ReadTime.HasValue);
                monthlyRates.Add(Math.Round((double)readCount / monthData.Count * 100, 2));
            }
        }

        return new EducationTrendDto(year, monthlyRates);
    }
}

