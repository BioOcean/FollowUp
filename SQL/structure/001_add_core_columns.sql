-- 结构调整脚本：新增字段及默认值
-- 执行前请确认已在维护窗口，并做好备份
BEGIN;

-- 患者表新增最近登录时间（用于活跃度统计）
ALTER TABLE public.patient
    ADD COLUMN IF NOT EXISTS last_login_time timestamp without time zone;

-- 医院与科室表新增扫码文案字段
ALTER TABLE system.sys_hospital
    ADD COLUMN IF NOT EXISTS scan_code_msg text;

ALTER TABLE system.sys_department
    ADD COLUMN IF NOT EXISTS scan_code_msg text;

-- 先移除历史脚本遗留的冗余字段
ALTER TABLE care.patient_event
    DROP COLUMN IF EXISTS followup_type,
    DROP COLUMN IF EXISTS outpatient_date,
    DROP COLUMN IF EXISTS hospitalized_in_date,
    DROP COLUMN IF EXISTS hospitalized_out_date;

-- patient_event 扩展随访字段
ALTER TABLE care.patient_event
    ADD COLUMN IF NOT EXISTS create_time timestamp without time zone,
    ADD COLUMN IF NOT EXISTS audit_doctor uuid,
    ADD COLUMN IF NOT EXISTS task_name text,
    ADD COLUMN IF NOT EXISTS push_time timestamp without time zone,
    ADD COLUMN IF NOT EXISTS input_time timestamp without time zone,
    ADD COLUMN IF NOT EXISTS audit_time timestamp without time zone,
    ADD COLUMN IF NOT EXISTS stop_time timestamp without time zone,
    ADD COLUMN IF NOT EXISTS event_type_definition_id uuid,
    ADD COLUMN IF NOT EXISTS audit_result text,
    ADD COLUMN IF NOT EXISTS followup_education_id uuid,
    ADD COLUMN IF NOT EXISTS file_list uuid[],
    ADD COLUMN IF NOT EXISTS scode text,
    ADD COLUMN IF NOT EXISTS push_flag text;

-- 随访宣教推送表新增 read_time 字段，并统一默认状态
ALTER TABLE followup.followup_education_push
    ADD COLUMN IF NOT EXISTS read_time timestamp without time zone,
    ALTER COLUMN status SET DEFAULT 'default';

COMMIT;

