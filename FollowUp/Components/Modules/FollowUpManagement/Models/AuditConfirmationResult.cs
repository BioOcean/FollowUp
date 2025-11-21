namespace FollowUp.Components.Modules.FollowUpManagement.Models
{
    public sealed class AuditConfirmationResult
    {
        public string AuditComment { get; set; } = string.Empty;
        public bool SkipMessage { get; set; }
        public bool RequireEducation { get; set; }
        public Guid? FollowupEducationId { get; set; }
    }
}
