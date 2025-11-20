using FollowUp.Components.Modules.FollowUpManagement.Models;
using Bio.Models;
using Microsoft.EntityFrameworkCore;

namespace FollowUp.Components.Modules.FollowUpManagement.Services
{
    /// <summary>
    /// 任务统计服务 - 负责计算各类任务统计数据
    /// </summary>
    public class TaskStatisticsService
    {
        private readonly IDbContextFactory<CubeDbContext> _contextFactory;
        private readonly ILogger<TaskStatisticsService> _logger;

        public TaskStatisticsService(
            IDbContextFactory<CubeDbContext> contextFactory,
            ILogger<TaskStatisticsService> logger)
        {
            _contextFactory = contextFactory;
            _logger = logger;
        }

        /// <summary>
        /// 计算任务统计数据（根据用户权限过滤）
        /// </summary>
        /// <param name="projectId">项目ID</param>
        /// <param name="currentUserId">当前用户ID</param>
        /// <param name="isAdminMode">是否为管理员模式</param>
        /// <param name="isGroupLeaderMode">是否为组长模式</param>
        /// <param name="managedNurseIds">组长管理的护士ID列表</param>
        /// <param name="dataRangeMode">数据范围模式（my/group/all）</param>
        /// <returns>统计数据</returns>
        public async Task<TaskStatisticsDto> CalculateStatisticsAsync(
            Guid projectId,
            Guid currentUserId,
            bool isAdminMode,
            bool isGroupLeaderMode,
            List<Guid> managedNurseIds,
            string dataRangeMode)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            try
            {
                // 构建基础查询（统计所有状态的任务，不仅限于可审核状态）
                var allTasksQuery = context.patient_event
                    .AsNoTracking()
                    .Where(pe => pe.project_id == projectId);

                // 应用权限过滤
                allTasksQuery = ApplyPermissionFilter(allTasksQuery, currentUserId, isAdminMode, isGroupLeaderMode, managedNurseIds, dataRangeMode);

                // 获取当前时间的周和月的起始时间
                var now = DateTime.Now;
                var weekStart = GetCurrentWeekStart(now); // 本周一
                var monthStart = new DateTime(now.Year, now.Month, 1); // 本月1号

                // 性能优化：单次查询获取必要字段，在内存中统计
                var tasks = await allTasksQuery
                    .Select(pe => new { pe.event_status, pe.create_time })
                    .ToListAsync();

                // 在内存中进行统计
                var statistics = new TaskStatisticsDto
                {
                    // 按状态统计
                    PendingReviewCount = tasks.Count(t => t.event_status == "待审核"),
                    OverdueCount = tasks.Count(t => t.event_status == "已超时"),
                    PatientNotSubmittedCount = tasks.Count(t => t.event_status == "患者未提交"),
                    CompletedCount = tasks.Count(t => t.event_status == "已随访"),
                    NotYetPushedCount = tasks.Count(t => t.event_status == "未到推送时间"),

                    // 按时间统计（所有状态）
                    CurrentWeekCount = tasks.Count(t => t.create_time >= weekStart),
                    CurrentMonthCount = tasks.Count(t => t.create_time >= monthStart),

                    // 总计（所有状态）
                    TotalCount = tasks.Count
                };

                // 计算患者总量（仅在需要时计算）
                statistics.PatientTotalCount = await context.patient
                    .AsNoTracking()
                    .Where(p => p.project_id == projectId)
                    .Where(p => context.patient_event.Any(pe => pe.patient_id == p.id))
                    .CountAsync();

                return statistics;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "计算任务统计数据失败，项目ID：{ProjectId}，用户ID：{UserId}", projectId, currentUserId);
                throw;
            }
        }

        /// <summary>
        /// 应用权限过滤（管理员、组长、医生）
        /// </summary>
        private IQueryable<patient_event> ApplyPermissionFilter(
            IQueryable<patient_event> query,
            Guid currentUserId,
            bool isAdminMode,
            bool isGroupLeaderMode,
            List<Guid> managedNurseIds,
            string dataRangeMode)
        {
            // 全部任务：仅管理员可访问
            if (dataRangeMode == "all")
            {
                if (isAdminMode)
                {
                    return query;
                }
                dataRangeMode = "group";
            }

            // 我组任务：组长 + 管辖的护士 + 自己
            if (dataRangeMode == "group" && isGroupLeaderMode && managedNurseIds.Any())
            {
                var allowedDoctorIds = new List<Guid>(managedNurseIds) { currentUserId };
                return query.Where(pe => pe.audit_doctor != null && allowedDoctorIds.Contains(pe.audit_doctor.Value));
            }

            // 普通医生模式：仅统计自己的任务
            return query.Where(pe => pe.audit_doctor == currentUserId);
        }

        /// <summary>
        /// 获取本周起始日期（周一）
        /// </summary>
        private static DateTime GetCurrentWeekStart(DateTime now)
        {
            var offset = now.DayOfWeek == DayOfWeek.Sunday ? -6 : (int)DayOfWeek.Monday - (int)now.DayOfWeek;
            return now.Date.AddDays(offset);
        }
    }
}
