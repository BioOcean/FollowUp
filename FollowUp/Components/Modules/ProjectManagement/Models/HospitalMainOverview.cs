namespace FollowUp.Components.Modules.ProjectManagement.Models;

/// <summary>
/// 科室与课题映射关系
/// </summary>
public sealed record DepartmentProjectMap(Guid DepartmentId, string DepartmentName, IReadOnlyList<Guid> ProjectIds);

/// <summary>
/// 活跃度统计维度
/// </summary>
public enum ActivityDimension
{
    Daily,   // 日维度
    Monthly  // 月维度
}

/// <summary>
/// 活跃度统计数据
/// </summary>
public sealed record ActivityStatsDto(
    ActivityDimension Dimension,
    DateTime TargetDate,
    IReadOnlyList<int> ActiveCounts,
    IReadOnlyList<int> ActivePercents,
    IReadOnlyList<string> Labels);

/// <summary>
/// 随访率趋势数据
/// </summary>
public sealed record FollowupTrendDto(int Year, IReadOnlyList<double> MonthlyRates);

/// <summary>
/// 宣教阅读率趋势数据
/// </summary>
public sealed record EducationTrendDto(int Year, IReadOnlyList<double> MonthlyRates);

/// <summary>
/// 科室统计摘要
/// </summary>
public sealed record DepartmentSummaryDto(
    Guid DepartmentId,
    string DepartmentName,
    int PatientCount,
    int FollowupTaskCount,
    double FollowupRate,
    IReadOnlyDictionary<string, int> StatusCounts);

/// <summary>
/// 课题统计摘要
/// </summary>
public sealed record ProjectSummaryDto(
    Guid ProjectId,
    string ProjectName,
    int PatientCount,
    int FollowupTaskCount,
    double FollowupRate,
    IReadOnlyDictionary<string, int> StatusCounts,
    int TodayNewCount = 0,
    int YearlyTaskCount = 0,
    int YearlyPatientCount = 0,
    int NotPushedCount = 0);

/// <summary>
/// 医院用户统计数据
/// </summary>
public sealed record HospitalUserStatsDto(
    int TotalPatients,
    int TotalFollowupTasks,
    int TotalFollowupPatients,
    int TodayNewPatients,
    IReadOnlyList<string> AgeLabels,
    IReadOnlyList<double> AgeValues,
    IReadOnlyList<string> DepartmentLabels,
    IReadOnlyList<double> DepartmentPatientValues,
    IReadOnlyList<string> TodayLabels,
    IReadOnlyList<double> TodayPatientValues);
