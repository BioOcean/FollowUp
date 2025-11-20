namespace FollowUp.Components.Modules.FollowUpManagement.Models
{
    /// <summary>
    /// 任务统计数据 DTO
    /// </summary>
    public class TaskStatisticsDto
    {
        /// <summary>
        /// 待审核任务数
        /// </summary>
        public int PendingReviewCount { get; set; } = 0;

        /// <summary>
        /// 已超时任务数
        /// </summary>
        public int OverdueCount { get; set; } = 0;

        /// <summary>
        /// 患者未提交任务数
        /// </summary>
        public int PatientNotSubmittedCount { get; set; } = 0;

        /// <summary>
        /// 本周任务数
        /// </summary>
        public int CurrentWeekCount { get; set; } = 0;

        /// <summary>
        /// 本月任务数
        /// </summary>
        public int CurrentMonthCount { get; set; } = 0;

        /// <summary>
        /// 总任务数（所有可审核状态的任务）
        /// </summary>
        public int TotalCount { get; set; } = 0;

        /// <summary>
        /// 患者总量（PendingReview布局专用）
        /// </summary>
        public int PatientTotalCount { get; set; } = 0;

        /// <summary>
        /// 已随访任务数（已完成的任务）
        /// </summary>
        public int CompletedCount { get; set; } = 0;

        /// <summary>
        /// 未到推送时间任务数
        /// </summary>
        public int NotYetPushedCount { get; set; } = 0;
    }
}
