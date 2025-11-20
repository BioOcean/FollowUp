using MudBlazor;

namespace FollowUp.Components.Modules.FollowUpManagement.Models
{
    /// <summary>
    /// 任务搜索条件模型
    /// </summary>
    public class TaskSearchCriteria
    {
        // ===== 基础搜索字段 =====

        /// <summary>
        /// 患者姓名
        /// </summary>
        public string PatientName { get; set; } = string.Empty;

        /// <summary>
        /// 任务名称
        /// </summary>
        public string TaskName { get; set; } = string.Empty;

        /// <summary>
        /// 任务状态（待审核、已超时、患者未提交）
        /// </summary>
        public string EventStatus { get; set; } = string.Empty;

        /// <summary>
        /// 审核医生ID
        /// </summary>
        public Guid? AuditDoctorId { get; set; }

        // ===== 高级搜索字段 =====

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
        /// 推送时间范围
        /// </summary>
        public DateRange PushTimeRange { get; set; } = new DateRange();

        /// <summary>
        /// 填写时间范围
        /// </summary>
        public DateRange InputTimeRange { get; set; } = new DateRange();

        /// <summary>
        /// 审核时间范围
        /// </summary>
        public DateRange AuditTimeRange { get; set; } = new DateRange();

        /// <summary>
        /// 创建时间范围
        /// </summary>
        public DateRange CreateTimeRange { get; set; } = new DateRange();

        // ===== 分页参数 =====

        /// <summary>
        /// 当前页码（从 0 开始）
        /// </summary>
        public int PageIndex { get; set; } = 0;

        /// <summary>
        /// 每页记录数
        /// </summary>
        public int PageSize { get; set; } = 10;

        /// <summary>
        /// 排序字段
        /// </summary>
        public string SortBy { get; set; } = "create_time";

        /// <summary>
        /// 排序方向（true: 升序, false: 降序）
        /// </summary>
        public bool IsAscending { get; set; } = false;

        // ===== 权限过滤参数 =====

        /// <summary>
        /// 当前项目ID
        /// </summary>
        public Guid ProjectId { get; set; } = Guid.Empty;

        /// <summary>
        /// 当前用户ID
        /// </summary>
        public Guid CurrentUserId { get; set; } = Guid.Empty;

        /// <summary>
        /// 是否为管理员模式
        /// </summary>
        public bool IsAdminMode { get; set; } = false;

        /// <summary>
        /// 是否为组长模式
        /// </summary>
        public bool IsGroupLeaderMode { get; set; } = false;

        /// <summary>
        /// 组长管理的护士ID列表
        /// </summary>
        public List<Guid> ManagedNurseIds { get; set; } = new List<Guid>();

        /// <summary>
        /// 数据范围模式（my=我的任务, group=我组任务, all=全部任务）
        /// </summary>
        public string DataRangeMode { get; set; } = "my";
    }
}
