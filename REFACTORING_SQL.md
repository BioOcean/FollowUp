# REFACTORING_SQL

> 目标：记录 FollowUp 重构过程中对数据库结构的改动，帮助后续功能迁移时快速了解新的字段、数据来源以及脚本执行顺序。

## 1. 结构变更概览

| 序号 | 变更内容 | 说明 |
|------|----------|------|
| 1 | `system.sys_hospital` / `system.sys_department` 新增 `scan_code_msg` | 扫码提示文案移动至医院/科室基础表，对应原 `followup.scan_code_message` |
| 2 | `care.patient_event` 扩展随访字段 | 合并原 `followup.followup_record` 字段：`create_time`、`audit_doctor`、`task_name`、`push_time`、`input_time`、`audit_time`、`stop_time`、`event_type_definition_id`、`audit_result`、`followup_education_id`、`file_list`、`scode`、`push_flag`；**不再新增** `followup_type`、`outpatient_date`、`hospitalized_*`，改由 `event_type_definition` 及 `care.patient_hospitalized` / `care.patient_outpatient` 维护 |
| 3 | `followup.followup_education_push` 新增 `read_time` 以及状态约定 | `status` 仅允许 `default`/`sent`，并吸收原 `followup.followup_education_history` 里的数据 |
| 4 | **保留** `followup.followup_patient_visit_behavior_record` | 继续用于活跃度统计（日活/月活），不迁移到 `patient` 表 |

结构脚本：`SQL/structure/001_add_core_columns.sql`

## 2. 数据迁移脚本

### 2.1 随访记录合并（`SQL/migration/010_merge_followup_record.sql`）

1. **住院/门诊补全**
   - 若 `followup_type = 1` 且存在住院起止日期，则在 `care.patient_hospitalized` 中补建记录。
   - 若 `followup_type = 2` 且存在 `outpatient_date`，在 `care.patient_outpatient` 中补建记录。
2. **事件字段回写**
   - 以 `patient_event_id` 对齐 `care.patient_event` 主键，仅回写仍保留的字段（`create_time`、`task_name`、`push_time` 等），住院/门诊明细保存在 `care.patient_hospitalized`、`care.patient_outpatient`。
   - `task_name` 通过 `care.event_type_definition` 的 `name` 字段统一更新。

执行顺序建议：先结构脚本 → 本脚本 → 校验 `care.patient_event` 数据 → 再执行清理脚本。

### 2.2 宣教推送合并（`SQL/migration/020_merge_education_push_history.sql`）

1. 分别对 `followup_education_push`、`followup_education_history` 做去重，维度：`(followup_education_id, patient_id, push_time)`。
2. 将 history 中的已推送记录写回 push：
   - `status` 更新为 `sent`。
   - `read_time` 取 history 中最新的已读时间。
3. 合并完成后，push 表即为唯一数据源，满足"待推送+已读"双状态管理。

> 正式环境已验证：history 中不存在 push 缺失的记录，无需额外补建。

### 2.3 扫码消息迁移（`SQL/migration/030_migrate_scan_code_message.sql`）

1. **医院级别扫码消息**
   - 将 `followup.scan_code_message` 表中 `hospital_id` 不为空且 `department_id` 为空的记录
   - 迁移到 `system.sys_hospital.scan_code_msg` 字段
   
2. **科室级别扫码消息**
   - 将 `followup.scan_code_message` 表中 `department_id` 不为空的记录
   - 迁移到 `system.sys_department.scan_code_msg` 字段

3. **数据统计**（当前数据量）
   - 医院级别：4 条记录
   - 科室级别：3 条记录
   - 总计：7 条记录

**回滚方案**：`SQL/migration/030_migrate_scan_code_message_rollback.sql`

执行顺序：结构脚本 → 本脚本 → 校验数据 → 应用代码验证 → 清理旧表

## 3. 清理脚本

脚本：`SQL/cleanup/900_remove_legacy_followup_tables.sql`

| 删除对象 | 说明 |
|----------|------|
| `followup.followup_record` | 随访任务信息已合并至 `care.patient_event` |
| `followup.followup_education_history` | 数据迁移至 `followup_education_push` 的 `read_time` 字段 |
| `followup.scan_code_message` | 医院/科室扫码文案改由 `system` schema 维护 |

**保留的表**：
- `followup.followup_patient_visit_behavior_record` - 继续用于日活月活统计

执行前务必确认：
- 迁移数据校验通过；
- 应用代码已切换至新字段；
- 已做好备份并在维护窗口内操作。

## 4. 数据读取与开发指引

1. **随访事件**
   - 随访任务请统一查 `care.patient_event`。
   - 状态字段仍为 `event_status`，新增的 `push_time`、`input_time`、`audit_time` 等可直接使用。
   - 对应住院/门诊详情请关联 `care.patient_hospitalized`、`care.patient_outpatient`（如仍为空，可参考迁移脚本逻辑补写）。

2. **扫码文案**
   - 医院页面：`system.sys_hospital.scan_code_msg`
   - 科室页面：`system.sys_department.scan_code_msg`
   - 项目页面仍使用 `form_project.scan_code_msg`（逻辑未变）。

3. **宣教推送/阅读**
   - `followup.followup_education_push`
     - `status = 'default'` 表示待推送；`'sent'` 表示已推送（无论是否已读）。
     - `read_time` 为空视为未读；非空记录最新阅读时间。
   - 去重维度与脚本一致，可复用 `(followup_education_id, patient_id, push_time)` 作为业务键。

4. **活跃度统计**
   - 使用 `followup.followup_patient_visit_behavior_record` 表统计日活/月活。
   - 查询建议：先筛选 `source_type = 'followup'` 且 `is_valid = true` 的患者ID，再关联 `visit_time` 按日期分组统计去重患者数。

5. **脚本执行顺序示例**

```
-- 1. 新增结构
\i SQL/structure/001_add_core_columns.sql

-- 2. 数据迁移
\i SQL/migration/010_merge_followup_record.sql
\i SQL/migration/020_merge_education_push_history.sql
\i SQL/migration/030_migrate_scan_code_message.sql

-- 3. 校验（示例）
SELECT COUNT(*) FROM care.patient_event WHERE push_time IS NOT NULL;
SELECT status, COUNT(*) FROM followup.followup_education_push GROUP BY status;
SELECT COUNT(*) FROM system.sys_hospital WHERE scan_code_msg IS NOT NULL;
SELECT COUNT(*) FROM system.sys_department WHERE scan_code_msg IS NOT NULL;

-- 4. 清理旧表
\i SQL/cleanup/900_remove_legacy_followup_tables.sql
```

如后续迁移其他模块，可根据以上变更快速定位数据来源和字段含义。
