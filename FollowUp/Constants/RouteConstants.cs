namespace FollowUp.Constants;

/// <summary>
/// 路由常量类
/// 统一管理所有路由路径，避免硬编码
/// </summary>
public static class RouteConstants
{
    /// <summary>
    /// 登录页面
    /// </summary>
    public const string Login = "/login";

    /// <summary>
    /// 错误页面
    /// </summary>
    public const string Error = "/error";

    /// <summary>
    /// 系统管理员主页
    /// </summary>
    public const string AdminMain = "/adminMain";

    /// <summary>
    /// 医院主页
    /// </summary>
    public const string HospitalMain = "/hospitalMain";

    /// <summary>
    /// 科室主页
    /// </summary>
    public const string DepartmentMain = "/departmentMain";

    /// <summary>
    /// 课题主页
    /// </summary>
    public const string ProjectMain = "/projectMain";

    /// <summary>
    /// 患者管理
    /// </summary>
    public const string PatientManage = "/FollowupPatientManage";

    /// <summary>
    /// 任务管理
    /// </summary>
    public const string TaskManager = "/followupTaskManager";

    /// <summary>
    /// 模板管理
    /// </summary>
    public const string TemplateManager = "/taskVisitManager";

    /// <summary>
    /// 宣教模板
    /// </summary>
    public const string EducationManage = "/PublicityAndEducationManage";

    /// <summary>
    /// 科室管理（系统管理员）
    /// </summary>
    public const string DepartmentManager = "/followupDeptMgr";

    /// <summary>
    /// 课题管理（系统管理员）
    /// </summary>
    public const string ProjectManager = "/followupProjMgr";

    /// <summary>
    /// 功能配置（系统管理员）
    /// </summary>
    public const string FunctionConfig = "/followupFuncConfig";

    /// <summary>
    /// 健康资讯（系统管理员）
    /// </summary>
    public const string HealthNews = "/healthyNews";

    /// <summary>
    /// 权限管理（系统管理员）
    /// </summary>
    public const string PermissionManager = "/followupPremMgr";

    /// <summary>
    /// 运营管理（系统管理员）
    /// </summary>
    public const string OperationManager = "/operationMgr";

    /// <summary>
    /// 数据迁移（系统管理员）
    /// </summary>
    public const string DataMigration = "/data-migration";

    /// <summary>
    /// 医生管理（课题权限管理）
    /// </summary>
    public const string DoctorManager = "/projectPremMgr";

    /// <summary>
    /// 门诊管理
    /// </summary>
    public const string OutpatientManager = "/outpatientMgr";

    /// <summary>
    /// 排床管理
    /// </summary>
    public const string BedManager = "/waitingBedMgr";

    /// <summary>
    /// 构建带查询参数的URL
    /// </summary>
    /// <param name="basePath">基础路径</param>
    /// <param name="parameters">查询参数字典</param>
    /// <returns>完整URL</returns>
    public static string BuildUrl(string basePath, Dictionary<string, string> parameters)
    {
        if (parameters == null || !parameters.Any())
        {
            return basePath;
        }

        var queryString = string.Join("&", parameters.Select(kvp => $"{kvp.Key}={Uri.EscapeDataString(kvp.Value)}"));
        return $"{basePath}?{queryString}";
    }

    /// <summary>
    /// 构建医院主页URL
    /// </summary>
    /// <param name="hospitalId">医院ID</param>
    /// <returns>医院主页URL</returns>
    public static string BuildHospitalMainUrl(Guid hospitalId)
    {
        return BuildUrl(HospitalMain, new Dictionary<string, string> { { "hospitalId", hospitalId.ToString() } });
    }

    /// <summary>
    /// 构建科室主页URL
    /// </summary>
    /// <param name="departmentId">科室ID</param>
    /// <returns>科室主页URL</returns>
    public static string BuildDepartmentMainUrl(Guid departmentId)
    {
        return BuildUrl(DepartmentMain, new Dictionary<string, string> { { "departmentId", departmentId.ToString() } });
    }

    /// <summary>
    /// 构建课题主页URL
    /// </summary>
    /// <param name="projectId">课题ID</param>
    /// <returns>课题主页URL</returns>
    public static string BuildProjectMainUrl(Guid projectId)
    {
        return BuildUrl(ProjectMain, new Dictionary<string, string> { { "projectId", projectId.ToString() } });
    }
}

