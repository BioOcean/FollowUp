namespace FollowUp.Services;

/// <summary>
/// 导航服务：管理用户首页路径和导航逻辑
/// </summary>
public interface INavigationService
{
    /// <summary>
    /// 获取当前用户的首页路径
    /// </summary>
    Task<string> GetHomePageAsync();
    
    /// <summary>
    /// 判断当前页面是否为用户的首页
    /// </summary>
    Task<bool> IsHomePageAsync(string currentUrl);
}

/// <summary>
/// 导航服务实现
/// </summary>
public class NavigationService : INavigationService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IUserContextService _userContextService;
    private readonly ILogger<NavigationService> _logger;
    private string? _cachedHomePage;

    public NavigationService(
        IHttpContextAccessor httpContextAccessor,
        IUserContextService userContextService,
        ILogger<NavigationService> logger)
    {
        _httpContextAccessor = httpContextAccessor;
        _userContextService = userContextService;
        _logger = logger;
    }
    
    public async Task<string> GetHomePageAsync()
    {
        // 使用缓存避免重复计算
        if (!string.IsNullOrEmpty(_cachedHomePage))
        {
            return _cachedHomePage;
        }

        try
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (user?.Identity?.IsAuthenticated != true)
            {
                return "/login";
            }

            // 从 Claims 读取角色列表
            var roles = user.FindAll("roleName")
                .Select(c => System.Text.RegularExpressions.Regex.Unescape(c.Value))
                .ToList();

            if (roles.Count == 0)
            {
                return "/login";
            }

            // 角色优先级：医院管理员 > 科室管理员 > 课题管理员 > 随访角色 > 系统管理员
            // 医院、科室、课题管理员使用精确匹配
            if (roles.Contains("医院管理员"))
            {
                var hospitalId = await _userContextService.GetHospitalIdAsync();
                if (hospitalId.HasValue)
                {
                    _cachedHomePage = $"/hospitalMain?hospitalId={hospitalId.Value}";
                    return _cachedHomePage;
                }
                return "/error?message=您暂无医院权限，请联系系统管理员分配权限";
            }
            else if (roles.Contains("科室管理员"))
            {
                var department = await _userContextService.GetDepartmentAsync();
                if (department != null)
                {
                    _cachedHomePage = $"/departmentMain?departmentId={department.id}";
                    return _cachedHomePage;
                }
                return "/error?message=您暂无科室权限，请联系系统管理员分配权限";
            }
            else if (roles.Contains("课题管理员"))
            {
                var project = await _userContextService.GetProjectAsync();
                if (project != null)
                {
                    _cachedHomePage = $"/projectMain?projectId={project.id}";
                    return _cachedHomePage;
                }
                return "/error?message=您暂无课题权限，请联系系统管理员分配权限";
            }
            // 随访和系统角色使用 Contains 模糊匹配（因为可能有多个随访角色）
            else if (roles.Any(r => r.Contains("随访")))
            {
                _cachedHomePage = "/followupTaskManager/PendingReview";
                return _cachedHomePage;
            }
            else if (roles.Any(r => r.Contains("系统")))
            {
                _cachedHomePage = "/adminMain";
                return _cachedHomePage;
            }

            // 默认跳转到任务管理
            _cachedHomePage = "/followupTaskManager/PendingReview";
            return _cachedHomePage;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取用户首页路径失败");
            return "/login";
        }
    }
    
    public async Task<bool> IsHomePageAsync(string currentUrl)
    {
        try
        {
            var homePage = await GetHomePageAsync();
            
            // 提取路径部分（去除查询参数）
            var currentPath = currentUrl.Split('?')[0].TrimEnd('/');
            var homePath = homePage.Split('?')[0].TrimEnd('/');
            
            // 比较路径（忽略大小写）
            return currentPath.EndsWith(homePath, StringComparison.OrdinalIgnoreCase);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "判断是否为首页失败");
            return false;
        }
    }
}

