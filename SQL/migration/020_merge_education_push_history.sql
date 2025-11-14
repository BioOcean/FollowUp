-- 数据迁移脚本：整合宣教推送与阅读记录
-- 目标：followup.followup_education_push 作为唯一数据源
BEGIN;

-- 1. 对 push 表去重（保留最早创建的记录）
WITH ranked AS (
    SELECT id,
           ROW_NUMBER() OVER (
               PARTITION BY followup_education_id, patient_id, push_time
               ORDER BY id
           ) AS rn
    FROM followup.followup_education_push
)
DELETE FROM followup.followup_education_push p
USING ranked r
WHERE p.id = r.id
  AND r.rn > 1;

-- 2. 对 history 表去重（优先保留最新的 read_time）
WITH ranked AS (
    SELECT id,
           ROW_NUMBER() OVER (
               PARTITION BY followup_education_id, patient_id, push_time
               ORDER BY read_time DESC NULLS LAST, id
           ) AS rn
    FROM followup.followup_education_history
)
DELETE FROM followup.followup_education_history h
USING ranked r
WHERE h.id = r.id
  AND r.rn > 1;

-- 3. 将 history 数据合并回 push
UPDATE followup.followup_education_push p
SET status = 'sent',
    read_time = h.read_time
FROM followup.followup_education_history h
WHERE p.followup_education_id = h.followup_education_id
  AND p.patient_id = h.patient_id
  AND p.push_time = h.push_time
  AND (
        p.read_time IS DISTINCT FROM h.read_time
        OR p.status <> 'sent'
    );

COMMIT;




