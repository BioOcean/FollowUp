namespace FollowUp.Components.Modules.FollowUpManagement.Models;

/// <summary>
/// 审核人DTO
/// </summary>
public class AuditorDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? RoleName { get; set; }

    public override string ToString()
    {
        return string.IsNullOrEmpty(RoleName) ? Name : $"{Name} - {RoleName}";
    }
}
