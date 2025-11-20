namespace FollowUp.Components.Modules.FollowUpManagement.Models;

/// <summary>
/// 患者搜索DTO
/// </summary>
public class PatientSearchDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? MedicalRecordNumber { get; set; }
    public string? PhoneNumber { get; set; }
    public string? IdCard { get; set; }

    public override string ToString()
    {
        return $"{Name} - {MedicalRecordNumber}";
    }
}
