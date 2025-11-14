-- 清理脚本：移除已替换的随访相关表
-- 请在确认数据已成功迁移且不再使用旧表后执行
BEGIN;

DROP TABLE IF EXISTS followup.followup_record;
DROP TABLE IF EXISTS followup.followup_education_history;
DROP TABLE IF EXISTS followup.followup_patient_visit_behavior_record;
DROP TABLE IF EXISTS followup.scan_code_message;

COMMIT;




