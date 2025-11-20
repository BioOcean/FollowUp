namespace FollowUp.Components.Modules.FollowUpManagement.Models
{
    /// <summary>
    /// 任务详情数据模型（基于 care.patient_event）
    /// </summary>
    public class TaskDetailDto
    {
        public Guid Id { get; set; } = Guid.Empty;
        public Guid PatientId { get; set; } = Guid.Empty;
        public Guid ProjectId { get; set; } = Guid.Empty;
        public Guid? EventTypeDefinitionId { get; set; }
        public Guid FormSetId { get; set; } = Guid.Empty;

        public Guid PatientEventId => Id;
        public Guid FollowupRecordId => Id;

        // 患者信息
        public string PatientName { get; set; } = string.Empty;
        public string CaseNumber { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string IdNumber { get; set; } = string.Empty;

        // 任务信息
        public string TaskName { get; set; } = string.Empty;
        public string EventStatus { get; set; } = string.Empty;
        public DateTime? PushTime { get; set; }
        public DateTime? InputTime { get; set; }
        public DateTime? AuditTime { get; set; }
        public DateTime? StopTime { get; set; }
        public DateTime CreateTime { get; set; } = DateTime.Now;
        public string AuditResult { get; set; } = string.Empty;

        public Guid? AuditDoctorId { get; set; }
        public string AuditDoctorName { get; set; } = string.Empty;

        // 宣教与附件
        public Guid? FollowupEducationId { get; set; }
        public IReadOnlyList<Guid> FileIds { get; set; } = Array.Empty<Guid>();
        public IReadOnlyList<string> FileNames { get; set; } = Array.Empty<string>();

        // 随访类型与时间
        public int? FollowupType { get; set; }
        public DateTime? HospitalizedInDate { get; set; }
        public DateTime? HospitalizedOutDate { get; set; }
        public DateTime? OutpatientDate { get; set; }
        public DateTime? PlanStartTime { get; set; }
        public DateTime? PlanEndTime { get; set; }

        public string FollowupTypeName => FollowupType switch
        {
            1 => "住院随访",
            2 => "门诊随访",
            _ => "-"
        };

        public string FollowupDateInfo
        {
            get
            {
                if (FollowupType == 1 && HospitalizedInDate != null && HospitalizedOutDate != null)
                {
                    return $"{HospitalizedInDate:yyyy-MM-dd} ~ {HospitalizedOutDate:yyyy-MM-dd}";
                }

                if (FollowupType == 2 && OutpatientDate != null)
                {
                    return OutpatientDate.Value.ToString("yyyy-MM-dd");
                }

                return "-";
            }
        }

        public string FollowupPeriod
        {
            get
            {
                if (PlanStartTime != null && PlanEndTime != null)
                {
                    return $"{PlanStartTime:yyyy-MM-dd}至{PlanEndTime:yyyy-MM-dd}";
                }

                return "-";
            }
        }
    }
}
