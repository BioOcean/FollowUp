namespace FollowUp.Services;

/// <summary>
/// 权限服务接口
/// 用于统一管理用户角色和权限检查逻辑
/// </summary>
public interface IAuthorizationService
{
    /// <summary>
    /// 获取当前用户的所有角色名称
    /// </summary>
    /// <returns>角色名称列表</returns>
    List<string> GetUserRoles();

    /// <summary>
    /// 检查当前用户是否拥有指定角色
    /// </summary>
    /// <param name="roleName">角色名称（支持部分匹配）</param>
    /// <returns>是否拥有该角色</returns>
    bool HasRole(string roleName);

    /// <summary>
    /// 检查当前用户是否拥有任意一个指定角色
    /// </summary>
    /// <param name="roleNames">角色名称列表</param>
    /// <returns>是否拥有任意一个角色</returns>
    bool HasAnyRole(params string[] roleNames);

    /// <summary>
    /// 检查当前用户是否拥有所有指定角色
    /// </summary>
    /// <param name="roleNames">角色名称列表</param>
    /// <returns>是否拥有所有角色</returns>
    bool HasAllRoles(params string[] roleNames);

    /// <summary>
    /// 检查当前用户是否为系统管理员
    /// </summary>
    /// <returns>是否为系统管理员</returns>
    bool IsSystemAdmin();

    /// <summary>
    /// 检查当前用户是否为医院管理员
    /// </summary>
    /// <returns>是否为医院管理员</returns>
    bool IsHospitalAdmin();

    /// <summary>
    /// 检查当前用户是否为科室管理员
    /// </summary>
    /// <returns>是否为科室管理员</returns>
    bool IsDepartmentAdmin();

    /// <summary>
    /// 检查当前用户是否为课题管理员
    /// </summary>
    /// <returns>是否为课题管理员</returns>
    bool IsProjectAdmin();

    /// <summary>
    /// 检查当前用户是否为随访角色（随访医生、随访护士、随访组长）
    /// </summary>
    /// <returns>是否为随访角色</returns>
    bool IsFollowupRole();

    /// <summary>
    /// 检查当前URL是否为系统管理页面
    /// </summary>
    /// <param name="currentUrl">当前URL</param>
    /// <returns>是否为系统管理页面</returns>
    bool IsAdminPage(string currentUrl);
}

