using Microsoft.EntityFrameworkCore;
using FollowUp.Components.Modules.FollowUpManagement.Models;
using Bio.Models;

namespace FollowUp.Components.Modules.FollowUpManagement.Services;

/// <summary>
/// 任务创建服务
/// </summary>
public class TaskCreationService
{
    private readonly IDbContextFactory<CubeDbContext> _contextFactory;
    private readonly ILogger<TaskCreationService> _logger;

    public TaskCreationService(
        IDbContextFactory<CubeDbContext> contextFactory,
        ILogger<TaskCreationService> logger)
    {
        _contextFactory = contextFactory;
        _logger = logger;
    }

    /// <summary>
    /// 创建住院随访任务
    /// </summary>
    public async Task<Guid> CreateHospitalizedFollowupTaskAsync(
        Guid patientId,
        Guid eventTypeDefinitionId,
        Guid auditDoctorId,
        DateTime hospitalizedInDate,
        DateTime hospitalizedOutDate,
        Guid? followupEducationId = null)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        using var transaction = await context.Database.BeginTransactionAsync();

        try
        {
            // 1. 查询任务模板获取配置
            var template = await context.event_type_definition
                .Where(e => e.id == eventTypeDefinitionId)
                .FirstOrDefaultAsync();

            if (template == null)
            {
                throw new Exception("任务模板不存在");
            }

            // 2. 计算推送时间（基于出院日期）
            var pushTime = CalculatePushTime(
                hospitalizedOutDate,
                template.offset_days,
                template.offset_months,
                template.offset_years);

            // 3. 创建 patient_event 记录
            var patientEvent = new patient_event
            {
                id = Guid.NewGuid(),
                patient_id = patientId,
                event_type = "followup",
                event_status = pushTime <= DateTime.Now ? "患者未提交" : "未到推送时间",
                form_set_id = template.form_set_id,
                project_id = template.project_id,
                event_type_definition_id = eventTypeDefinitionId,
                task_name = template.name,
                audit_doctor = auditDoctorId,
                push_time = pushTime,
                create_time = DateTime.Now,
                followup_education_id = followupEducationId
            };

            context.patient_event.Add(patientEvent);

            // 4. 创建住院记录
            var hospitalized = new patient_hospitalized
            {
                id = Guid.NewGuid(),
                patient_id = patientId,
                patient_event_id = patientEvent.id,
                hospitalized_start_date = DateOnly.FromDateTime(hospitalizedInDate),
                hospitalized_end_date = DateOnly.FromDateTime(hospitalizedOutDate)
            };

            context.patient_hospitalized.Add(hospitalized);

            await context.SaveChangesAsync();
            await transaction.CommitAsync();

            _logger.LogInformation("成功创建住院随访任务，patient_event_id: {PatientEventId}", patientEvent.id);
            return patientEvent.id;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "创建住院随访任务失败");
            throw;
        }
    }

    /// <summary>
    /// 创建门诊随访任务
    /// </summary>
    public async Task<Guid> CreateOutpatientFollowupTaskAsync(
        Guid patientId,
        Guid eventTypeDefinitionId,
        Guid auditDoctorId,
        DateTime outpatientDate,
        Guid? followupEducationId = null)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        using var transaction = await context.Database.BeginTransactionAsync();

        try
        {
            // 1. 查询任务模板获取配置
            var template = await context.event_type_definition
                .Where(e => e.id == eventTypeDefinitionId)
                .FirstOrDefaultAsync();

            if (template == null)
            {
                throw new Exception("任务模板不存在");
            }

            // 2. 计算推送时间（基于门诊日期）
            var pushTime = CalculatePushTime(
                outpatientDate,
                template.offset_days,
                template.offset_months,
                template.offset_years);

            // 3. 创建 patient_event 记录
            var patientEvent = new patient_event
            {
                id = Guid.NewGuid(),
                patient_id = patientId,
                event_type = "followup",
                event_status = pushTime <= DateTime.Now ? "患者未提交" : "未到推送时间",
                form_set_id = template.form_set_id,
                project_id = template.project_id,
                event_type_definition_id = eventTypeDefinitionId,
                task_name = template.name,
                audit_doctor = auditDoctorId,
                push_time = pushTime,
                create_time = DateTime.Now,
                followup_education_id = followupEducationId
            };

            context.patient_event.Add(patientEvent);

            // 4. 创建门诊记录
            var outpatient = new patient_outpatient
            {
                id = Guid.NewGuid(),
                patient_id = patientId,
                patient_event_id = patientEvent.id,
                outpatient_date = DateOnly.FromDateTime(outpatientDate)
            };

            context.patient_outpatient.Add(outpatient);

            await context.SaveChangesAsync();
            await transaction.CommitAsync();

            _logger.LogInformation("成功创建门诊随访任务，patient_event_id: {PatientEventId}", patientEvent.id);
            return patientEvent.id;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "创建门诊随访任务失败");
            throw;
        }
    }

    /// <summary>
    /// 计算推送时间
    /// </summary>
    public DateTime CalculatePushTime(
        DateTime baseDate,
        int offsetDays,
        int offsetMonths,
        int offsetYears)
    {
        return baseDate
            .AddYears(offsetYears)
            .AddMonths(offsetMonths)
            .AddDays(offsetDays);
    }

    /// <summary>
    /// 获取任务模板列表（当前项目）
    /// </summary>
    public async Task<List<TaskTemplateDto>> GetTaskTemplatesAsync(Guid projectId)
    {
        using var context = await _contextFactory.CreateDbContextAsync();

        return await context.event_type_definition
            .Where(e => e.project_id == projectId && e.group == "followup")
            .Select(e => new TaskTemplateDto
            {
                Id = e.id,
                Name = e.name ?? string.Empty,
                FormSetId = e.form_set_id,
                FormSetName = e.form_set != null ? e.form_set.name : string.Empty,
                OffsetDays = e.offset_days,
                OffsetMonths = e.offset_months,
                OffsetYears = e.offset_years
            })
            .ToListAsync();
    }

    /// <summary>
    /// 搜索患者
    /// </summary>
    public async Task<List<PatientSearchDto>> SearchPatientsAsync(string keyword, Guid projectId)
    {
        if (string.IsNullOrWhiteSpace(keyword) || keyword.Length < 2)
        {
            return new List<PatientSearchDto>();
        }

        using var context = await _contextFactory.CreateDbContextAsync();

        var query = context.patient
            .Where(p => p.project_id == projectId)
            .AsQueryable();

        // 按姓名、病案号、手机号搜索
        query = query.Where(p =>
            (p.name != null && p.name.Contains(keyword)) ||
            (p.medical_record_number != null && p.medical_record_number.Contains(keyword)) ||
            (p.phone_number != null && p.phone_number.Contains(keyword)));

        return await query
            .Take(20)
            .Select(p => new PatientSearchDto
            {
                Id = p.id,
                Name = p.name ?? string.Empty,
                MedicalRecordNumber = p.medical_record_number,
                PhoneNumber = p.phone_number,
                IdCard = p.sid_number
            })
            .ToListAsync();
    }

    /// <summary>
    /// 获取可选审核医生
    /// </summary>
    public async Task<List<AuditorDto>> GetAvailableAuditorsAsync(Guid projectId)
    {
        using var context = await _contextFactory.CreateDbContextAsync();

        // 查询具有随访相关角色的用户
        return await context.sys_user
            .Where(u => u.is_valid &&
                       u.role.Any(r => r.name == "随访医生" || r.name == "随访护士" || r.name == "随访组长"))
            .Select(u => new AuditorDto
            {
                Id = u.id,
                Name = u.display_name ?? string.Empty,
                RoleName = u.role.FirstOrDefault() != null ? u.role.First().name : string.Empty
            })
            .ToListAsync();
    }

    /// <summary>
    /// 批量创建计划外/随访任务（仅写入 patient_event，带访视期）
    /// </summary>
    public async Task<int> CreateBatchFollowupTasksAsync(
        Guid projectId,
        Guid eventTypeDefinitionId,
        Guid auditorId,
        DateTime planStartDate,
        DateTime planEndDate,
        IReadOnlyCollection<Guid> patientIds,
        Guid? followupEducationId = null)
    {
        if (patientIds == null || patientIds.Count == 0)
        {
            throw new ArgumentException("缺少患者列表", nameof(patientIds));
        }

        await using var context = await _contextFactory.CreateDbContextAsync();
        await using var transaction = await context.Database.BeginTransactionAsync();

        try
        {
            var template = await context.event_type_definition
                .FirstOrDefaultAsync(e => e.id == eventTypeDefinitionId && e.project_id == projectId);

            if (template == null)
            {
                throw new Exception("任务模板不存在");
            }

            var pushTime = CalculatePushTime(
                planStartDate,
                template.offset_days,
                template.offset_months,
                template.offset_years);

            var distinctPatientIds = patientIds.Distinct().ToList();
            var now = DateTime.Now;

            foreach (var patientId in distinctPatientIds)
            {
                var patientEvent = new patient_event
                {
                    id = Guid.NewGuid(),
                    patient_id = patientId,
                    event_type = "followup",
                    event_status = pushTime <= now ? "患者未提交" : "未到推送时间",
                    form_set_id = template.form_set_id,
                    project_id = projectId,
                    event_type_definition_id = eventTypeDefinitionId,
                    task_name = template.name,
                    audit_doctor = auditorId,
                    push_time = pushTime,
                    create_time = now,
                    followup_education_id = followupEducationId,
                    plan_start_time = DateOnly.FromDateTime(planStartDate),
                    plan_end_time = DateOnly.FromDateTime(planEndDate)
                };

                context.patient_event.Add(patientEvent);
            }

            var count = await context.SaveChangesAsync();
            await transaction.CommitAsync();

            _logger.LogInformation("批量创建随访任务完成，数量：{Count}", distinctPatientIds.Count);
            return distinctPatientIds.Count;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "批量创建随访任务失败");
            throw;
        }
    }

    /// <summary>
    /// 按 ID 批量获取患者简要信息
    /// </summary>
    public async Task<List<PatientSearchDto>> GetPatientsByIdsAsync(IEnumerable<Guid> patientIds)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var ids = patientIds.Distinct().ToList();
        if (!ids.Any())
        {
            return new List<PatientSearchDto>();
        }

        return await context.patient
            .Where(p => ids.Contains(p.id))
            .Select(p => new PatientSearchDto
            {
                Id = p.id,
                Name = p.name ?? string.Empty,
                MedicalRecordNumber = p.medical_record_number,
                PhoneNumber = p.phone_number,
                IdCard = p.sid_number
            })
            .ToListAsync();
    }
}
