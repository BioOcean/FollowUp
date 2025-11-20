using System.Linq;
using Bio.Models;
using Microsoft.EntityFrameworkCore;

namespace FollowUp.Components.Modules.FollowUpManagement.Services;

/// <summary>
/// 任务附件服务（出院证明）
/// </summary>
public class TaskAttachmentService
{
    private readonly IDbContextFactory<CubeDbContext> _contextFactory;
    private readonly ILogger<TaskAttachmentService> _logger;

    public TaskAttachmentService(
        IDbContextFactory<CubeDbContext> contextFactory,
        ILogger<TaskAttachmentService> logger)
    {
        _contextFactory = contextFactory;
        _logger = logger;
    }

    /// <summary>
    /// 获取任务的出院证明文件名列表（拼接为 /uploads/{文件名}）
    /// </summary>
    public async Task<IReadOnlyList<string>> GetDischargeFilesAsync(Guid patientEventId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        try
        {
            var fileIds = await context.patient_event
                .AsNoTracking()
                .Where(pe => pe.id == patientEventId)
                .Select(pe => pe.file_list)
                .FirstOrDefaultAsync();

            if (fileIds == null || !fileIds.Any())
            {
                _logger.LogInformation("任务 {PatientEventId} 未包含出院证明文件", patientEventId);
                return Array.Empty<string>();
            }

            var names = await context.followup_file_list
                .AsNoTracking()
                .Where(f => fileIds.Contains(f.id))
                .Select(f => f.file_new_name ?? f.file_old_name ?? string.Empty)
                .Where(n => !string.IsNullOrWhiteSpace(n))
                .ToListAsync();

            return names.Select(n => $"/uploads/{n}").ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取任务 {PatientEventId} 出院证明失败", patientEventId);
            throw;
        }
    }
}
