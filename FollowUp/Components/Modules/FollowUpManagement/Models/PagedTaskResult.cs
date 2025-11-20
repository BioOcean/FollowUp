namespace FollowUp.Components.Modules.FollowUpManagement.Models
{
    /// <summary>
    /// 分页任务查询结果
    /// </summary>
    public class PagedTaskResult
    {
        /// <summary>
        /// 任务列表
        /// </summary>
        public List<TaskListItemDto> Items { get; set; } = new List<TaskListItemDto>();

        /// <summary>
        /// 总记录数
        /// </summary>
        public int TotalCount { get; set; } = 0;
    }
}
