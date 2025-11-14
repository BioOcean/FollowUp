namespace FollowUp.Components.Modules.ProjectManagement.Models;

public sealed class FollowupEventInfo
{
    public Guid Id { get; set; }
    public Guid PatientId { get; set; }
    public Guid ProjectId { get; set; }
    public string EventStatus { get; set; } = string.Empty;
    public int? FollowupType { get; set; }
    public DateTime? PushTime { get; set; }
    public DateTime? InputTime { get; set; }
    public DateTime? AuditTime { get; set; }
    public DateTime? StopTime { get; set; }
    public Guid? EventTypeDefinitionId { get; set; }
    public Guid? FollowupEducationId { get; set; }
    public string? AuditResult { get; set; }
    public string? Scode { get; set; }
    public string? PushFlag { get; set; }
    public DateTime? CreateTime { get; set; }
}
