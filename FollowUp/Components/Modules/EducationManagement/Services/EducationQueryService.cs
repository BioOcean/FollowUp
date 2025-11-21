using Bio.Models;
using Microsoft.EntityFrameworkCore;

namespace FollowUp.Components.Modules.EducationManagement.Services;

public interface IEducationQueryService
{
    /// <summary>
    /// 按单一维度筛选宣教列表（三选一：课题 / 科室 / 医院）
    /// </summary>
    Task<IReadOnlyList<EducationItem>> GetEducationsAsync(
        Guid? hospitalId,
        Guid? departmentId,
        Guid? projectId,
        CancellationToken cancellationToken = default);
}

public sealed class EducationQueryService : IEducationQueryService
{
    private readonly IDbContextFactory<CubeDbContext> _contextFactory;

    public EducationQueryService(IDbContextFactory<CubeDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<IReadOnlyList<EducationItem>> GetEducationsAsync(
        Guid? hospitalId,
        Guid? departmentId,
        Guid? projectId,
        CancellationToken cancellationToken = default)
    {
        var projectGuid = projectId ?? Guid.Empty;
        var departmentGuid = departmentId ?? Guid.Empty;
        var hospitalGuid = hospitalId ?? Guid.Empty;

        // 三选一：优先课题，其次科室，最后医院；都为空时直接返回空列表
        var mode = projectGuid != Guid.Empty
            ? EducationQueryMode.Project
            : (departmentGuid != Guid.Empty
                ? EducationQueryMode.Department
                : (hospitalGuid != Guid.Empty ? EducationQueryMode.Hospital : EducationQueryMode.None));

        if (mode == EducationQueryMode.None)
        {
            return Array.Empty<EducationItem>();
        }

        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

        Guid? resolvedDepartmentId = departmentGuid != Guid.Empty ? departmentGuid : null;

        if (mode == EducationQueryMode.Project)
        {
            var project = await context.form_project
                .AsNoTracking()
                .Where(p => p.id == projectGuid)
                .Select(p => new { p.department_id })
                .FirstOrDefaultAsync(cancellationToken);

            if (project?.department_id != null && project.department_id != Guid.Empty)
            {
                resolvedDepartmentId = project.department_id;
            }
        }

        var query = context.followup_education
            .AsNoTracking()
            .Where(e => true);

        switch (mode)
        {
            case EducationQueryMode.Project:
                query = query.Where(e =>
                    (e.project_id != null && e.project_id.Contains(projectGuid)) ||
                    ((e.project_id == null || e.project_id.Count == 0) &&
                     resolvedDepartmentId.HasValue &&
                     e.department_id == resolvedDepartmentId.Value));
                break;

            case EducationQueryMode.Department:
                query = query.Where(e => e.department_id == departmentGuid);
                break;

            case EducationQueryMode.Hospital:
                query = query.Where(e => e.hospital_id == hospitalGuid);
                break;
        }

        var list = await query
            .OrderByDescending(e => e.create_time)
            .Select(e => new EducationItem
            {
                Id = e.id,
                Title = string.IsNullOrWhiteSpace(e.title) ? "未命名宣教" : e.title
            })
            .ToListAsync(cancellationToken);

        return list;
    }

    private enum EducationQueryMode
    {
        None,
        Project,
        Department,
        Hospital
    }
}

public sealed class EducationItem
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
}
