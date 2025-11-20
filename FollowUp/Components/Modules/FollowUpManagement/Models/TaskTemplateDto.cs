namespace FollowUp.Components.Modules.FollowUpManagement.Models;

/// <summary>
/// 任务模板DTO
/// </summary>
public class TaskTemplateDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public Guid FormSetId { get; set; }
    public string FormSetName { get; set; } = string.Empty;
    public int OffsetDays { get; set; }
    public int OffsetMonths { get; set; }
    public int OffsetYears { get; set; }
}
