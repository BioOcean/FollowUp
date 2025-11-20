using Bio.Models;
using Microsoft.EntityFrameworkCore;

namespace FollowUp.Components.Modules.FollowUpManagement.Services
{
    /// <summary>
    /// 任务审核服务 - 负责任务审核、更换审核人等操作
    /// </summary>
    public class TaskAuditService
    {
        private readonly IDbContextFactory<CubeDbContext> _contextFactory;
        private readonly ILogger<TaskAuditService> _logger;

        public TaskAuditService(
            IDbContextFactory<CubeDbContext> contextFactory,
            ILogger<TaskAuditService> logger)
        {
            _contextFactory = contextFactory;
            _logger = logger;
        }

        /// <summary>
        /// 审核任务（通过审核）
        /// </summary>
        /// <param name="taskId">任务ID（patient_event.id）</param>
        /// <param name="auditComment">审核意见（可选）</param>
        /// <returns>操作是否成功</returns>
        public async Task<bool> ApproveTaskAsync(Guid taskId, string? auditComment = null, Guid? followupEducationId = null, bool skipAuditMessage = false)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            try
            {
                var task = await context.patient_event.FindAsync(taskId);
                if (task == null)
                {
                    _logger.LogWarning("审核失败：任务不存在，任务ID：{TaskId}", taskId);
                    return false;
                }

                // 更新审核状态
                task.event_status = "已审核";
                task.audit_time = DateTime.Now;
                if (!skipAuditMessage || !string.IsNullOrWhiteSpace(auditComment))
                {
                    task.audit_result = string.IsNullOrWhiteSpace(auditComment) ? "通过" : auditComment;
                }

                if (followupEducationId.HasValue)
                {
                    task.followup_education_id = followupEducationId;
                }

                await context.SaveChangesAsync();

                _logger.LogInformation("任务审核成功，任务ID：{TaskId}，审核结果：{Result}", taskId, task.audit_result);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "审核任务失败，任务ID：{TaskId}", taskId);
                throw;
            }
        }

        /// <summary>
        /// 拒绝任务审核（需要患者重新填写）
        /// </summary>
        /// <param name="taskId">任务ID（patient_event.id）</param>
        /// <param name="rejectReason">拒绝原因（必填）</param>
        /// <returns>操作是否成功</returns>
        public async Task<bool> RejectTaskAsync(Guid taskId, string rejectReason)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            try
            {
                var task = await context.patient_event.FindAsync(taskId);
                if (task == null)
                {
                    _logger.LogWarning("拒绝审核失败：任务不存在，任务ID：{TaskId}", taskId);
                    return false;
                }

                // 更新为患者未提交状态，允许重新填写
                task.event_status = "患者未提交";
                task.audit_time = DateTime.Now;
                task.audit_result = $"拒绝：{rejectReason}";
                task.input_time = null; // 清空填写时间，等待重新填写

                await context.SaveChangesAsync();

                _logger.LogInformation("任务拒绝成功，任务ID：{TaskId}，拒绝原因：{Reason}", taskId, rejectReason);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "拒绝任务失败，任务ID：{TaskId}", taskId);
                throw;
            }
        }

        /// <summary>
        /// 更换任务审核人
        /// </summary>
        /// <param name="taskId">任务ID</param>
        /// <param name="newAuditorId">新审核人ID</param>
        /// <returns>操作是否成功</returns>
        public async Task<bool> ChangeAuditorAsync(Guid taskId, Guid newAuditorId)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            try
            {
                var task = await context.patient_event.FindAsync(taskId);
                if (task == null)
                {
                    _logger.LogWarning("更换审核人失败：任务不存在，任务ID：{TaskId}", taskId);
                    return false;
                }

                var oldAuditorId = task.audit_doctor;
                task.audit_doctor = newAuditorId;

                await context.SaveChangesAsync();

                _logger.LogInformation("更换审核人成功，任务ID：{TaskId}，原审核人：{OldAuditor}，新审核人：{NewAuditor}",
                    taskId, oldAuditorId, newAuditorId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更换审核人失败，任务ID：{TaskId}，新审核人：{NewAuditor}", taskId, newAuditorId);
                throw;
            }
        }

        /// <summary>
        /// 批量更换审核人
        /// </summary>
        /// <param name="taskIds">任务ID列表</param>
        /// <param name="newAuditorId">新审核人ID</param>
        /// <returns>操作是否成功</returns>
        public async Task<bool> BatchChangeAuditorAsync(List<Guid> taskIds, Guid newAuditorId)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            try
            {
                var tasks = await context.patient_event
                    .Where(pe => taskIds.Contains(pe.id))
                    .ToListAsync();

                if (!tasks.Any())
                {
                    _logger.LogWarning("批量更换审核人失败：未找到任何任务");
                    return false;
                }

                foreach (var task in tasks)
                {
                    task.audit_doctor = newAuditorId;
                }

                await context.SaveChangesAsync();

                _logger.LogInformation("批量更换审核人成功，任务数量：{Count}，新审核人：{NewAuditor}",
                    tasks.Count, newAuditorId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "批量更换审核人失败，任务数量：{Count}，新审核人：{NewAuditor}",
                    taskIds.Count, newAuditorId);
                throw;
            }
        }

        /// <summary>
        /// 取消任务
        /// </summary>
        /// <param name="taskId">任务ID</param>
        /// <param name="reason">取消原因</param>
        /// <returns>操作是否成功</returns>
        public async Task<bool> CancelTaskAsync(Guid taskId, string reason)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            try
            {
                var task = await context.patient_event.FindAsync(taskId);
                if (task == null)
                {
                    _logger.LogWarning("取消任务失败：任务不存在，任务ID：{TaskId}", taskId);
                    return false;
                }

                task.event_status = "已取消";
                task.stop_time = DateTime.Now;
                task.audit_result = string.IsNullOrWhiteSpace(reason) ? "取消" : $"取消：{reason}";

                await context.SaveChangesAsync();

                _logger.LogInformation("取消任务成功，任务ID：{TaskId}", taskId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取消任务失败，任务ID：{TaskId}", taskId);
                throw;
            }
        }

        /// <summary>
        /// 批量取消任务
        /// </summary>
        /// <param name="taskIds">任务ID列表</param>
        /// <returns>操作是否成功</returns>
        public async Task<bool> BatchCancelTasksAsync(List<Guid> taskIds)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            try
            {
                var tasks = await context.patient_event
                    .Where(pe => taskIds.Contains(pe.id))
                    .ToListAsync();

                if (!tasks.Any())
                {
                    _logger.LogWarning("批量取消任务失败：未找到任何任务");
                    return false;
                }

                var now = DateTime.Now;
                foreach (var task in tasks)
                {
                    task.event_status = "已取消";
                    task.stop_time = now;
                }

                await context.SaveChangesAsync();

                _logger.LogInformation("批量取消任务成功，任务数量：{Count}", tasks.Count);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "批量取消任务失败，任务数量：{Count}", taskIds.Count);
                throw;
            }
        }
    }
}
