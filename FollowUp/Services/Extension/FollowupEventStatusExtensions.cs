using System.ComponentModel;

namespace FollowUp.Services.Extension;

/// <summary>
/// 随访事件状态枚举
/// </summary>
public enum FollowupEventStatus
{
    /// <summary>
    /// 未到推送时间
    /// </summary>
    [Description("未到推送时间")]
    NotPushed,

    /// <summary>
    /// 患者未提交
    /// </summary>
    [Description("患者未提交")]
    PatientNotSubmitted,

    /// <summary>
    /// 待审核
    /// </summary>
    [Description("待审核")]
    PendingAudit,

    /// <summary>
    /// 已审核
    /// </summary>
    [Description("已审核")]
    Audited,

    /// <summary>
    /// 已随访
    /// </summary>
    [Description("已随访")]
    Completed,

    /// <summary>
    /// 已超时
    /// </summary>
    [Description("已超时")]
    Timeout,

    /// <summary>
    /// 已停止
    /// </summary>
    [Description("已停止")]
    Stopped
}

/// <summary>
/// 随访事件状态扩展方法
/// </summary>
public static class FollowupEventStatusExtensions
{
    /// <summary>
    /// 获取枚举的描述（中文状态值）
    /// </summary>
    public static string GetDescription(this FollowupEventStatus value)
    {
        var field = value.GetType().GetField(value.ToString());
        if (field == null)
            return value.ToString();

        var attribute = (DescriptionAttribute?)Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute));
        return attribute?.Description ?? value.ToString();
    }

    /// <summary>
    /// 从字符串解析为枚举
    /// </summary>
    public static FollowupEventStatus? ParseFromString(string? status)
    {
        if (string.IsNullOrWhiteSpace(status))
            return null;

        return status switch
        {
            "未到推送时间" => FollowupEventStatus.NotPushed,
            "患者未提交" => FollowupEventStatus.PatientNotSubmitted,
            "待审核" => FollowupEventStatus.PendingAudit,
            "已审核" => FollowupEventStatus.Audited,
            "已随访" => FollowupEventStatus.Completed,
            "已超时" => FollowupEventStatus.Timeout,
            "已停止" => FollowupEventStatus.Stopped,
            _ => null
        };
    }

    /// <summary>
    /// 判断是否为已停止状态
    /// </summary>
    public static bool IsStopped(string? status)
    {
        return status == FollowupEventStatus.Stopped.GetDescription();
    }

    /// <summary>
    /// 判断是否为未到推送时间状态
    /// </summary>
    public static bool IsNotPushed(string? status)
    {
        return status == FollowupEventStatus.NotPushed.GetDescription();
    }

    /// <summary>
    /// 判断是否为已随访状态
    /// </summary>
    public static bool IsCompleted(string? status)
    {
        return status == FollowupEventStatus.Completed.GetDescription();
    }

    /// <summary>
    /// 判断是否为待审核状态
    /// </summary>
    public static bool IsPendingAudit(string? status)
    {
        return status == FollowupEventStatus.PendingAudit.GetDescription();
    }

    /// <summary>
    /// 判断是否为患者未提交状态
    /// </summary>
    public static bool IsPatientNotSubmitted(string? status)
    {
        return status == FollowupEventStatus.PatientNotSubmitted.GetDescription();
    }

    /// <summary>
    /// 判断是否为已超时状态
    /// </summary>
    public static bool IsTimeout(string? status)
    {
        return status == FollowupEventStatus.Timeout.GetDescription();
    }
}

