-- 数据迁移脚本：将 followup.followup_patient_visit_behavior_record 的行为时间迁移到 public.patient.last_login_time
-- 前置条件：已执行 SQL/structure/001_add_core_columns.sql，且尚未执行 SQL/cleanup/900_remove_legacy_followup_tables.sql
BEGIN;
WITH last_visit AS (
    SELECT patient_id, MAX(visit_time) AS last_time
    FROM followup.followup_patient_visit_behavior_record
    GROUP BY patient_id
)
UPDATE public.patient AS p
SET last_login_time = lv.last_time
FROM last_visit AS lv
WHERE p.id = lv.patient_id
  AND p.source_type = 'followup'
  AND p.is_valid = true
  AND (p.last_login_time IS NULL OR p.last_login_time < lv.last_time);
COMMIT;
-- 注意：如果行为表中的时间列名称不是 visit_time，请将上面的 visit_time 替换为实际“进入患者端/登录时间”字段名后再执行本脚本。

