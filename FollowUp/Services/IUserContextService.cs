using Bio.Models;

namespace FollowUp.Services;

/// <summary>
/// 用户上下文服务接口
/// 用于获取当前登录用户的医院、科室、课题等上下文信息
/// </summary>
public interface IUserContextService
{
    /// <summary>
    /// 获取用户所属的医院ID
    /// </summary>
    Task<Guid?> GetHospitalIdAsync();

    /// <summary>
    /// 获取用户所属的科室信息
    /// </summary>
    Task<sys_department?> GetDepartmentAsync();

    /// <summary>
    /// 获取用户所属的课题信息
    /// </summary>
    Task<form_project?> GetProjectAsync();
}

