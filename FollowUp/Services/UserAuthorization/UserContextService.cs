using Bio.Models;
using Microsoft.EntityFrameworkCore;

namespace FollowUp.Services;

/// <summary>
/// 用户上下文服务实现
/// 极简实现：从数据库查询用户权限数据，并缓存结果
/// </summary>
public class UserContextService : IUserContextService
{
    private readonly IDbContextFactory<CubeDbContext> _contextFactory;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<UserContextService> _logger;
    
    // 缓存用户权限数据，避免频繁查询
    private List<UserPermissionData>? _cachedPermissionData;
    private Guid? _cachedUserId;

    public UserContextService(
        IDbContextFactory<CubeDbContext> contextFactory,
        IHttpContextAccessor httpContextAccessor,
        ILogger<UserContextService> logger)
    {
        _contextFactory = contextFactory;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }
    
    /// <summary>
    /// 获取用户权限数据列表（带缓存）
    /// </summary>
    private async Task<List<UserPermissionData>> GetUserPermissionDataAsync()
    {
        var userId = GetCurrentUserId();
        if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userGuid))
        {
            return new List<UserPermissionData>();
        }

        // 如果缓存的用户ID与当前用户ID一致，直接返回缓存
        if (_cachedUserId == userGuid && _cachedPermissionData != null)
        {
            return _cachedPermissionData;
        }

        // 查询用户权限数据（会返回多条记录）
        await using var context = await _contextFactory.CreateDbContextAsync();

        var permissionDataList = await (
            from u in context.sys_user
            join ur in context.Set<Dictionary<string, object>>("sys_map_user_role") on u.id equals EF.Property<Guid>(ur, "user_id")
            join r in context.sys_role on EF.Property<Guid>(ur, "role_id") equals r.id
            join pd in context.sys_permission_data on r.id equals pd.role_id
            where u.id == userGuid
            select new UserPermissionData
            {
                HospitalId = pd.hospital_id,
                DepartmentId = pd.department_id,
                ProjectId = pd.form_project_id
            }
        ).ToListAsync();

        // 缓存结果
        _cachedUserId = userGuid;
        _cachedPermissionData = permissionDataList;

        return permissionDataList;
    }

    public async Task<Guid?> GetHospitalIdAsync()
    {
        try
        {
            var permissionDataList = await GetUserPermissionDataAsync();
            // 从列表中提取第一个不为空的 HospitalId
            return permissionDataList
                .Select(p => p.HospitalId)
                .FirstOrDefault(id => id != null && id != Guid.Empty);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取用户医院ID失败");
            return null;
        }
    }

    public async Task<sys_department?> GetDepartmentAsync()
    {
        try
        {
            var permissionDataList = await GetUserPermissionDataAsync();
            // 从列表中提取第一个不为空的 DepartmentId
            var departmentId = permissionDataList
                .Select(p => p.DepartmentId)
                .FirstOrDefault(id => id != null && id != Guid.Empty);

            if (departmentId == null)
            {
                return null;
            }

            await using var context = await _contextFactory.CreateDbContextAsync();
            
            // 查询科室详细信息
            var department = await context.sys_department
                .AsNoTracking()
                .Include(d => d.hospital)
                .FirstOrDefaultAsync(d => d.id == departmentId);

            return department;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取用户科室信息失败");
            return null;
        }
    }

    public async Task<form_project?> GetProjectAsync()
    {
        try
        {
            var permissionDataList = await GetUserPermissionDataAsync();
            // 从列表中提取第一个不为空的 ProjectId
            var projectId = permissionDataList
                .Select(p => p.ProjectId)
                .FirstOrDefault(id => id != null);

            if (projectId == null)
            {
                return null;
            }

            await using var context = await _contextFactory.CreateDbContextAsync();

            // 查询课题详细信息
            var project = await context.form_project
                .AsNoTracking()
                .Include(p => p.hospital)
                .Include(p => p.department)
                .FirstOrDefaultAsync(p => p.id == projectId);

            return project;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取用户课题信息失败");
            return null;
        }
    }

    /// <summary>
    /// 从 HttpContext 获取当前用户ID
    /// </summary>
    private string? GetCurrentUserId()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        if (user?.Identity?.IsAuthenticated != true)
        {
            return null;
        }

        return user.FindFirst("userId")?.Value
            ?? user.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
    }
    
    /// <summary>
    /// 用户权限数据（内部类）
    /// </summary>
    private class UserPermissionData
    {
        public Guid? HospitalId { get; set; }
        public Guid? DepartmentId { get; set; }
        public Guid? ProjectId { get; set; }
    }
}

