-- 数据迁移脚本：合并 followup.followup_record → care.patient_event
-- 依赖结构脚本：SQL/structure/001_add_core_columns.sql
-- 注意：执行前请确保已启用 pgcrypto（用于 gen_random_uuid）
BEGIN;

-- 1. 为住院随访补全 care.patient_hospitalized 记录
INSERT INTO care.patient_hospitalized (
    id,
    patient_id,
    patient_event_id,
    hospitalized_start_date,
    hospitalized_end_date,
    hospitalized_start_diagnosis,
    hospitalized_end_diagnosis
)
SELECT
    gen_random_uuid(),
    pe.patient_id,
    pe.id,
    fr.hospitalized_in_date,
    fr.hospitalized_out_date,
    NULL,
    NULL
FROM followup.followup_record fr
JOIN care.patient_event pe ON pe.id = fr.patient_event_id
WHERE fr.followup_type = 1
  AND (fr.hospitalized_in_date IS NOT NULL OR fr.hospitalized_out_date IS NOT NULL)
  AND NOT EXISTS (
        SELECT 1
        FROM care.patient_hospitalized ph
        WHERE ph.patient_event_id = fr.patient_event_id
    );

-- 2. 为门诊随访补全 care.patient_outpatient 记录
INSERT INTO care.patient_outpatient (
    id,
    patient_id,
    patient_event_id,
    outpatient_date,
    outpatient_type,
    outpatient_department,
    outpatient_symptom,
    outpatient_diagnosis,
    outpatient_treatment
)
SELECT
    gen_random_uuid(),
    pe.patient_id,
    pe.id,
    fr.outpatient_date,
    '普通门诊',
    NULL,
    NULL,
    NULL,
    NULL
FROM followup.followup_record fr
JOIN care.patient_event pe ON pe.id = fr.patient_event_id
WHERE fr.followup_type = 2
  AND fr.outpatient_date IS NOT NULL
  AND NOT EXISTS (
        SELECT 1
        FROM care.patient_outpatient po
        WHERE po.patient_event_id = fr.patient_event_id
    );

-- 3. 将随访字段写入 care.patient_event
UPDATE care.patient_event pe
SET audit_doctor = fr.audit_doctor,
    create_time = fr.create_time,
    push_time = fr.push_time,
    input_time = fr.input_time,
    audit_time = fr.audit_time,
    stop_time = fr.stop_time,
    event_type_definition_id = fr.event_type_definition_id,
    audit_result = fr.audit_result,
    followup_education_id = fr.followup_education_id,
    file_list = fr.file_list,
    scode = fr.scode,
    push_flag = fr.push_flag
FROM followup.followup_record fr
WHERE fr.patient_event_id = pe.id;

-- 4. 更新 task_name（与 event_type_definition.name 保持一致）
UPDATE care.patient_event pe
SET task_name = etd.name
FROM care.event_type_definition etd
WHERE pe.event_type_definition_id = etd.id
  AND etd.name IS NOT NULL;

COMMIT;


