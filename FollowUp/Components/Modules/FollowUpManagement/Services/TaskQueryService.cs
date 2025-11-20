using FollowUp.Components.Modules.FollowUpManagement.Models;
using Bio.Models;
using Microsoft.EntityFrameworkCore;

namespace FollowUp.Components.Modules.FollowUpManagement.Services
{
    /// <summary>
    /// 任务查询服务 - 负责任务列表的分页查询和搜索
    /// </summary>
    public class TaskQueryService
    {
        private readonly IDbContextFactory<CubeDbContext> _contextFactory;
        private readonly ILogger<TaskQueryService> _logger;

        public TaskQueryService(
            IDbContextFactory<CubeDbContext> contextFactory,
            ILogger<TaskQueryService> logger)
        {
            _contextFactory = contextFactory;
            _logger = logger;
        }

        /// <summary>
        /// 分页查询任务列表
        /// </summary>
        /// <param name="criteria">搜索条件</param>
        /// <returns>分页任务结果</returns>
        public async Task<PagedTaskResult> QueryTasksAsync(TaskSearchCriteria criteria)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            try
            {
                // 构建基础查询
                var query = context.patient_event
                    .AsNoTracking()
                    .Where(pe => pe.project_id == criteria.ProjectId);

                // 应用权限过滤
                query = ApplyPermissionFilter(query, criteria);

                // 应用搜索条件（包括状态筛选，如果指定了状态）
                query = ApplySearchFilters(query, criteria, context);

                // 获取总记录数（在排序和分页之前）
                var totalCount = await query.CountAsync();

                // 应用排序
                query = ApplySorting(query, criteria);

                // 应用分页
                var pagedQuery = query
                    .Skip(criteria.PageIndex * criteria.PageSize)
                    .Take(criteria.PageSize);

                var items = await (
                    from pe in pagedQuery
                    join p in context.patient on pe.patient_id equals p.id into patientGroup
                    from p in patientGroup.DefaultIfEmpty()
                    join u in context.sys_user on pe.audit_doctor equals u.id into auditorGroup
                    from u in auditorGroup.DefaultIfEmpty()
                    join ph in context.patient_hospitalized on pe.id equals ph.patient_event_id into hospitalizedGroup
                    from ph in hospitalizedGroup.DefaultIfEmpty()
                    join po in context.patient_outpatient on pe.id equals po.patient_event_id into outpatientGroup
                    from po in outpatientGroup.DefaultIfEmpty()
                    select new
                    {
                        pe,
                        Patient = p,
                        Auditor = u,
                        HospitalizedInfo = ph,
                        OutpatientInfo = po
                    }).ToListAsync();

                // 转换为 DTO
                var dtoList = items.Select((item, index) => new TaskListItemDto
                {
                    Id = item.pe.id,
                    Index = criteria.PageIndex * criteria.PageSize + index + 1,
                    PatientId = item.pe.patient_id,
                    PatientName = item.Patient?.name ?? string.Empty,
                    CaseNumber = item.Patient?.medical_record_number ?? string.Empty,
                    PhoneNumber = item.Patient?.phone_number ?? string.Empty,
                    IdNumber = item.Patient?.sid_number ?? string.Empty,
                    TaskName = item.pe.task_name ?? string.Empty,
                    EventStatus = item.pe.event_status ?? string.Empty,
                    PushTime = item.pe.push_time,
                    InputTime = item.pe.input_time,
                    AuditTime = item.pe.audit_time,
                    AuditResult = item.pe.audit_result ?? string.Empty,
                    AuditDoctorId = item.pe.audit_doctor,
                    AuditDoctorName = item.Auditor?.display_name ?? string.Empty,
                    CreateTime = item.pe.create_time ?? DateTime.Now,
                    PlanStartTime = item.pe.plan_start_time.HasValue ? item.pe.plan_start_time.Value.ToDateTime(TimeOnly.MinValue) : (DateTime?)null,
                    PlanEndTime = item.pe.plan_end_time.HasValue ? item.pe.plan_end_time.Value.ToDateTime(TimeOnly.MinValue) : (DateTime?)null,

                    // 随访类型判断（住院优先）
                    FollowupType = item.HospitalizedInfo != null ? 1 : (item.OutpatientInfo != null ? 2 : (int?)null),
                    HospitalizedInDate = item.HospitalizedInfo?.hospitalized_start_date?.ToDateTime(TimeOnly.MinValue),
                    HospitalizedOutDate = item.HospitalizedInfo?.hospitalized_end_date?.ToDateTime(TimeOnly.MinValue),
                    OutpatientDate = item.OutpatientInfo != null ? item.OutpatientInfo.outpatient_date.ToDateTime(TimeOnly.MinValue) : (DateTime?)null
                }).ToList();

                return new PagedTaskResult
                {
                    Items = dtoList,
                    TotalCount = totalCount
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "查询任务列表失败，搜索条件：{@Criteria}", criteria);
                throw;
            }
        }

        /// <summary>
        /// 按任务ID获取详情（含患者、住院/门诊、文件名）
        /// </summary>
        public async Task<TaskDetailDto?> GetTaskDetailAsync(Guid taskId)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            try
            {
                var detail = await context.patient_event
                    .AsNoTracking()
                    .Where(pe => pe.id == taskId)
                    .Select(pe => new
                    {
                        pe.id,
                        pe.patient_id,
                        pe.project_id,
                        pe.event_type_definition_id,
                        pe.form_set_id,
                        pe.task_name,
                        pe.event_status,
                        pe.push_time,
                        pe.input_time,
                        pe.audit_time,
                        pe.audit_result,
                        pe.audit_doctor,
                        pe.create_time,
                        pe.plan_start_time,
                        pe.plan_end_time,
                        pe.file_list,
                        pe.followup_education_id,
                        pe.stop_time,
                        Patient = context.patient
                            .Where(p => p.id == pe.patient_id)
                            .Select(p => new
                            {
                                p.name,
                                p.medical_record_number,
                                p.phone_number,
                                p.sid_number
                            })
                            .FirstOrDefault(),
                        Auditor = context.sys_user
                            .Where(u => u.id == pe.audit_doctor)
                            .Select(u => u.display_name)
                            .FirstOrDefault(),
                        Hospitalized = context.patient_hospitalized
                            .Where(ph => ph.patient_event_id == pe.id)
                            .Select(ph => new
                            {
                                ph.hospitalized_start_date,
                                ph.hospitalized_end_date
                            })
                            .FirstOrDefault(),
                        Outpatient = context.patient_outpatient
                            .Where(po => po.patient_event_id == pe.id)
                            .Select(po => po.outpatient_date)
                            .FirstOrDefault()
                    })
                    .FirstOrDefaultAsync();

                if (detail == null)
                {
                    _logger.LogWarning("未找到任务详情：TaskId={TaskId}", taskId);
                    return null;
                }

                var followupType = detail.Hospitalized != null ? 1 : (detail.Outpatient != null ? 2 : (int?)null);
                var followupDateIn = detail.Hospitalized?.hospitalized_start_date?.ToDateTime(TimeOnly.MinValue);
                var followupDateOut = detail.Hospitalized?.hospitalized_end_date?.ToDateTime(TimeOnly.MinValue);
                var outpatientDate = detail.Outpatient != null ? detail.Outpatient.ToDateTime(TimeOnly.MinValue) : (DateTime?)null;

                var fileIds = detail.file_list ?? new List<Guid>();
                var fileNames = fileIds.Any()
                    ? await context.followup_file_list
                        .AsNoTracking()
                        .Where(f => fileIds.Contains(f.id))
                        .Select(f => f.file_new_name ?? f.file_old_name ?? string.Empty)
                        .Where(name => !string.IsNullOrWhiteSpace(name))
                        .ToListAsync()
                    : new List<string>();

                return new TaskDetailDto
                {
                    Id = detail.id,
                    PatientId = detail.patient_id,
                    ProjectId = detail.project_id,
                    EventTypeDefinitionId = detail.event_type_definition_id,
                    FormSetId = detail.form_set_id,
                    TaskName = detail.task_name ?? string.Empty,
                    EventStatus = detail.event_status ?? string.Empty,
                    PushTime = detail.push_time,
                    InputTime = detail.input_time,
                    AuditTime = detail.audit_time,
                    StopTime = detail.stop_time,
                    AuditResult = detail.audit_result ?? string.Empty,
                    AuditDoctorId = detail.audit_doctor,
                    AuditDoctorName = detail.Auditor ?? string.Empty,
                    CreateTime = detail.create_time ?? DateTime.Now,
                    FollowupEducationId = detail.followup_education_id,
                    FileIds = fileIds.ToList(),
                    FileNames = fileNames,
                    PatientName = detail.Patient?.name ?? string.Empty,
                    CaseNumber = detail.Patient?.medical_record_number ?? string.Empty,
                    PhoneNumber = detail.Patient?.phone_number ?? string.Empty,
                    IdNumber = detail.Patient?.sid_number ?? string.Empty,
                    FollowupType = followupType,
                    HospitalizedInDate = followupDateIn,
                    HospitalizedOutDate = followupDateOut,
                    OutpatientDate = outpatientDate,
                    PlanStartTime = detail.plan_start_time?.ToDateTime(TimeOnly.MinValue),
                    PlanEndTime = detail.plan_end_time?.ToDateTime(TimeOnly.MinValue)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取任务详情失败：TaskId={TaskId}", taskId);
                throw;
            }
        }

        /// <summary>
        /// 应用权限过滤（基于数据范围模式）
        /// </summary>
        private IQueryable<patient_event> ApplyPermissionFilter(
            IQueryable<patient_event> query,
            TaskSearchCriteria criteria)
        {
            // 根据数据范围模式进行过滤
            switch (criteria.DataRangeMode)
            {
                case "all":
                    // 全部任务：仅管理员可访问
                    if (criteria.IsAdminMode)
                    {
                        return query; // 无过滤，返回全部
                    }
                    // 非管理员降级到"group"模式
                    goto case "group";

                case "group":
                    // 我组任务：组长 + 管辖的护士的任务
                    if (criteria.IsGroupLeaderMode && criteria.ManagedNurseIds.Any())
                    {
                        var allowedDoctorIds = new List<Guid>(criteria.ManagedNurseIds) { criteria.CurrentUserId };
                        return query.Where(pe => pe.audit_doctor != null && allowedDoctorIds.Contains(pe.audit_doctor.Value));
                    }
                    // 非组长降级到"my"模式
                    goto case "my";

                case "my":
                default:
                    // 我的任务：仅查看自己的任务
                    return query.Where(pe => pe.audit_doctor == criteria.CurrentUserId);
            }
        }

        /// <summary>
        /// 应用搜索过滤条件
        /// </summary>
        private IQueryable<patient_event> ApplySearchFilters(
            IQueryable<patient_event> query,
            TaskSearchCriteria criteria,
            CubeDbContext context)
        {
            // 患者姓名（模糊搜索）
            if (!string.IsNullOrWhiteSpace(criteria.PatientName))
            {
                query = query.Where(pe =>
                    context.patient.Any(p => p.id == pe.patient_id && EF.Functions.Like(p.name, $"%{criteria.PatientName}%")));
            }

            // 任务名称（模糊搜索，使用 GIN 索引）
            if (!string.IsNullOrWhiteSpace(criteria.TaskName))
            {
                query = query.Where(pe => EF.Functions.Like(pe.task_name, $"%{criteria.TaskName}%"));
            }

            // 任务状态
            if (!string.IsNullOrWhiteSpace(criteria.EventStatus))
            {
                query = query.Where(pe => pe.event_status == criteria.EventStatus);
            }

            // 审核医生ID
            if (criteria.AuditDoctorId.HasValue)
            {
                query = query.Where(pe => pe.audit_doctor == criteria.AuditDoctorId.Value);
            }

            // 病案号
            if (!string.IsNullOrWhiteSpace(criteria.CaseNumber))
            {
                query = query.Where(pe =>
                    context.patient.Any(p => p.id == pe.patient_id && p.medical_record_number == criteria.CaseNumber));
            }

            // 手机号码
            if (!string.IsNullOrWhiteSpace(criteria.PhoneNumber))
            {
                query = query.Where(pe =>
                    context.patient.Any(p => p.id == pe.patient_id && p.phone_number == criteria.PhoneNumber));
            }

            // 身份证号
            if (!string.IsNullOrWhiteSpace(criteria.IdNumber))
            {
                query = query.Where(pe =>
                    context.patient.Any(p => p.id == pe.patient_id && p.sid_number == criteria.IdNumber));
            }

            // 推送时间范围
            if (criteria.PushTimeRange?.Start != null)
            {
                query = query.Where(pe => pe.push_time >= criteria.PushTimeRange.Start);
            }
            if (criteria.PushTimeRange?.End != null)
            {
                query = query.Where(pe => pe.push_time <= criteria.PushTimeRange.End);
            }

            // 填写时间范围
            if (criteria.InputTimeRange?.Start != null)
            {
                query = query.Where(pe => pe.input_time >= criteria.InputTimeRange.Start);
            }
            if (criteria.InputTimeRange?.End != null)
            {
                query = query.Where(pe => pe.input_time <= criteria.InputTimeRange.End);
            }

            // 审核时间范围
            if (criteria.AuditTimeRange?.Start != null)
            {
                query = query.Where(pe => pe.audit_time >= criteria.AuditTimeRange.Start);
            }
            if (criteria.AuditTimeRange?.End != null)
            {
                query = query.Where(pe => pe.audit_time <= criteria.AuditTimeRange.End);
            }

            // 创建时间范围
            if (criteria.CreateTimeRange?.Start != null)
            {
                query = query.Where(pe => pe.create_time >= criteria.CreateTimeRange.Start);
            }
            if (criteria.CreateTimeRange?.End != null)
            {
                query = query.Where(pe => pe.create_time <= criteria.CreateTimeRange.End);
            }

            return query;
        }

        /// <summary>
        /// 应用排序
        /// </summary>
        private IQueryable<patient_event> ApplySorting(
            IQueryable<patient_event> query,
            TaskSearchCriteria criteria)
        {
            // 根据排序字段和方向排序
            return criteria.SortBy.ToLower() switch
            {
                "create_time" => criteria.IsAscending
                    ? query.OrderBy(pe => pe.create_time)
                    : query.OrderByDescending(pe => pe.create_time),
                "push_time" => criteria.IsAscending
                    ? query.OrderBy(pe => pe.push_time)
                    : query.OrderByDescending(pe => pe.push_time),
                "input_time" => criteria.IsAscending
                    ? query.OrderBy(pe => pe.input_time)
                    : query.OrderByDescending(pe => pe.input_time),
                "audit_time" => criteria.IsAscending
                    ? query.OrderBy(pe => pe.audit_time)
                    : query.OrderByDescending(pe => pe.audit_time),
                _ => query.OrderByDescending(pe => pe.create_time) // 默认按创建时间降序
            };
        }
    }
}
