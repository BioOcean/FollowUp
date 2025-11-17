using System.Text.RegularExpressions;

namespace FollowUp.Services;

/// <summary>
/// 权限服务实现
/// 极简实现：直接从 HttpContext.User.Claims 读取角色信息
/// </summary>
public class AuthorizationService : IAuthorizationService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<AuthorizationService> _logger;

    public AuthorizationService(
        IHttpContextAccessor httpContextAccessor,
        ILogger<AuthorizationService> logger)
    {
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    public List<string> GetUserRoles()
    {
        try
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (user?.Identity?.IsAuthenticated != true)
            {
                return new List<string>();
            }

            var roles = user.FindAll("roleName")
                .Select(c => Regex.Unescape(c.Value))
                .ToList();

            return roles;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取用户角色失败");
            return new List<string>();
        }
    }

    public bool HasRole(string roleName)
    {
        var roles = GetUserRoles();
        return roles.Any(r => r.Contains(roleName, StringComparison.OrdinalIgnoreCase));
    }

    public bool HasAnyRole(params string[] roleNames)
    {
        var roles = GetUserRoles();
        return roleNames.Any(roleName => roles.Any(r => r.Contains(roleName, StringComparison.OrdinalIgnoreCase)));
    }

    public bool HasAllRoles(params string[] roleNames)
    {
        var roles = GetUserRoles();
        return roleNames.All(roleName => roles.Any(r => r.Contains(roleName, StringComparison.OrdinalIgnoreCase)));
    }

    public bool IsSystemAdmin()
    {
        return HasRole("系统");
    }

    public bool IsHospitalAdmin()
    {
        var roles = GetUserRoles();
        return roles.Contains("医院管理员");
    }

    public bool IsDepartmentAdmin()
    {
        var roles = GetUserRoles();
        return roles.Contains("科室管理员");
    }

    public bool IsProjectAdmin()
    {
        var roles = GetUserRoles();
        return roles.Contains("课题管理员");
    }

    public bool IsFollowupRole()
    {
        return HasAnyRole("随访医生", "随访护士", "随访组长", "随访");
    }

    public bool IsAdminPage(string currentUrl)
    {
        if (string.IsNullOrWhiteSpace(currentUrl))
        {
            return false;
        }

        // 系统管理页面路径列表
        var adminPaths = new[]
        {
            "/adminMain",
            "/followupDeptMgr",
            "/followupProjMgr",
            "/operationMgr",
            "/followupFuncConfig",
            "/followupPremMgr",
            "/healthyNews",
            "/data-migration"
        };

        return adminPaths.Any(path => currentUrl.Contains(path, StringComparison.OrdinalIgnoreCase));
    }
}

