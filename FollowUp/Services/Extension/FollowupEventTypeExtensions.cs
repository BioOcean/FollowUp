using System.ComponentModel;

namespace FollowUp.Services.Extension;

/// <summary>
/// 随访事件类型枚举
/// </summary>
public enum FollowupEventType
{
    /// <summary>
    /// 住院随访
    /// </summary>
    [Description("住院随访")]
    InpatientFollowup,

    /// <summary>
    /// 门诊随访
    /// </summary>
    [Description("门诊随访")]
    OutpatientFollowup,

    /// <summary>
    /// 计划外随访
    /// </summary>
    [Description("计划外随访")]
    UnplannedFollowup
}

/// <summary>
/// 随访事件类型扩展方法
/// </summary>
public static class FollowupEventTypeExtensions
{
    /// <summary>
    /// 获取所有随访类型的字符串值
    /// </summary>
    /// <returns>随访类型数组</returns>
    public static string[] GetFollowupEventTypes()
    {
        return new[] { "住院随访", "门诊随访", "计划外随访" };
    }

    /// <summary>
    /// 判断是否为随访类型
    /// </summary>
    /// <param name="eventType">事件类型</param>
    /// <returns>是否为随访类型</returns>
    public static bool IsFollowupEvent(string? eventType)
    {
        if (string.IsNullOrWhiteSpace(eventType))
            return false;

        return eventType switch
        {
            "住院随访" => true,
            "门诊随访" => true,
            "计划外随访" => true,
            _ => false
        };
    }

    /// <summary>
    /// 获取枚举的描述
    /// </summary>
    /// <param name="value">枚举值</param>
    /// <returns>描述文本</returns>
    public static string GetDescription(this FollowupEventType value)
    {
        var field = value.GetType().GetField(value.ToString());
        if (field == null)
            return value.ToString();

        var attribute = (DescriptionAttribute?)Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute));
        return attribute?.Description ?? value.ToString();
    }
}

