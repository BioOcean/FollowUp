namespace FollowUp.Components.Modules.ProjectManagement.Models;

public sealed record DepartmentOverviewItem(Guid DepartmentId, string DepartmentName);
public sealed record ProjectOverviewItem(Guid ProjectId, string ProjectName);
public sealed record DepartmentWithProjectsDto(Guid DepartmentId, string DepartmentName, IReadOnlyList<ProjectOverviewItem> Projects);
public sealed record DepartmentProjectMap(Guid DepartmentId, string DepartmentName, IReadOnlyList<Guid> ProjectIds);

public sealed record HospitalOverviewDto(
    Guid hospital_id,
    string hospital_name,
    string? scan_code_msg,
    IReadOnlyList<DepartmentWithProjectsDto> departments);

public enum ActivityDimension
{
    Daily,
    Monthly
}

public sealed record ActivityStatsDto(
    ActivityDimension Dimension,
    DateTime TargetDate,
    IReadOnlyList<int> ActiveCounts,
    IReadOnlyList<int> ActivePercents,
    IReadOnlyList<string> Labels);

public sealed record FollowupTrendDto(int Year, IReadOnlyList<double> MonthlyRates);
public sealed record EducationTrendDto(int Year, IReadOnlyList<double> MonthlyRates);

public sealed record DepartmentSummaryDto(
    Guid DepartmentId,
    string DepartmentName,
    int PatientCount,
    int FollowupTaskCount,
    double FollowupRate,
    IReadOnlyDictionary<string, int> StatusCounts);

public sealed record ProjectSummaryDto(
    Guid ProjectId,
    string ProjectName,
    int PatientCount,
    int FollowupTaskCount,
    double FollowupRate,
    IReadOnlyDictionary<string, int> StatusCounts);

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
