namespace FollowUp.Components.Modules.FollowUpManagement.Models
{
    /// <summary>
    /// 任务列表项 DTO
    /// </summary>
    public class TaskListItemDto
    {
        /// <summary>
        /// 患者事件ID（主键）
        /// </summary>
        public Guid Id { get; set; } = Guid.Empty;

        /// <summary>
        /// 序号（用于显示）
        /// </summary>
        public int Index { get; set; }

        /// <summary>
        /// 患者ID
        /// </summary>
        public Guid PatientId { get; set; } = Guid.Empty;

        /// <summary>
        /// 患者姓名
        /// </summary>
        public string PatientName { get; set; } = string.Empty;

        /// <summary>
        /// 病案号
        /// </summary>
        public string CaseNumber { get; set; } = string.Empty;

        /// <summary>
        /// 手机号码
        /// </summary>
        public string PhoneNumber { get; set; } = string.Empty;

        /// <summary>
        /// 身份证号
        /// </summary>
        public string IdNumber { get; set; } = string.Empty;

        /// <summary>
        /// 任务名称
        /// </summary>
        public string TaskName { get; set; } = string.Empty;

        /// <summary>
        /// 事件状态（待审核、已超时、患者未提交等）
        /// </summary>
        public string EventStatus { get; set; } = string.Empty;

        /// <summary>
        /// 推送时间
        /// </summary>
        public DateTime? PushTime { get; set; }

        /// <summary>
        /// 填写时间
        /// </summary>
        public DateTime? InputTime { get; set; }

        /// <summary>
        /// 审核时间
        /// </summary>
        public DateTime? AuditTime { get; set; }

        /// <summary>
        /// 审核结果
        /// </summary>
        public string AuditResult { get; set; } = string.Empty;

        /// <summary>
        /// 审核医生ID
        /// </summary>
        public Guid? AuditDoctorId { get; set; }

        /// <summary>
        /// 审核医生姓名
        /// </summary>
        public string AuditDoctorName { get; set; } = string.Empty;

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateTime { get; set; } = DateTime.Now;

        // ===== 随访类型和住院/门诊信息（新增） =====

        /// <summary>
        /// 随访类型（1=住院随访，2=门诊随访，null=未知）
        /// </summary>
        public int? FollowupType { get; set; }

        /// <summary>
        /// 随访类型名称（显示用）
        /// </summary>
        public string FollowupTypeName => FollowupType switch
        {
            1 => "住院随访",
            2 => "门诊随访",
            _ => "-"
        };

        /// <summary>
        /// 住院入院日期
        /// </summary>
        public DateTime? HospitalizedInDate { get; set; }

        /// <summary>
        /// 住院出院日期
        /// </summary>
        public DateTime? HospitalizedOutDate { get; set; }

        /// <summary>
        /// 门诊日期
        /// </summary>
        public DateTime? OutpatientDate { get; set; }

        /// <summary>
        /// 访视期开始时间
        /// </summary>
        public DateTime? PlanStartTime { get; set; }

        /// <summary>
        /// 访视期结束时间
        /// </summary>
        public DateTime? PlanEndTime { get; set; }

        /// <summary>
        /// 访视期（格式化显示）
        /// </summary>
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

        /// <summary>
        /// 随访日期信息（格式化显示）
        /// </summary>
        public string FollowupDateInfo
        {
            get
            {
                if (FollowupType == 1 && HospitalizedInDate != null && HospitalizedOutDate != null)
                {
                    return $"{HospitalizedInDate:yyyy-MM-dd} ~ {HospitalizedOutDate:yyyy-MM-dd}";
                }
                else if (FollowupType == 2 && OutpatientDate != null)
                {
                    return OutpatientDate.Value.ToString("yyyy-MM-dd");
                }
                return "-";
            }
        }

        /// <summary>
        /// 是否选中（用于批量操作）
        /// </summary>
        public bool IsSelected { get; set; } = false;
    }
}
