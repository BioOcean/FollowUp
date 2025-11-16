-- 数据迁移脚本：将 followup.scan_code_message 表数据迁移到新字段
-- 目标：将扫码文案从独立表迁移到医院/科室基础表
-- 执行前提：已执行 SQL/structure/001_add_core_columns.sql

BEGIN;

-- 1. 迁移医院级别的扫码消息
-- 条件：hospital_id 不为空，department_id 为空
UPDATE system.sys_hospital h
SET scan_code_msg = scm.content
FROM followup.scan_code_message scm
WHERE h.id = scm.hospital_id
  AND scm.department_id IS NULL
  AND scm.content IS NOT NULL
  AND scm.content != '';

-- 2. 迁移科室级别的扫码消息  
-- 条件：department_id 不为空
UPDATE system.sys_department d
SET scan_code_msg = scm.content
FROM followup.scan_code_message scm
WHERE d.id = scm.department_id
  AND scm.content IS NOT NULL
  AND scm.content != '';

-- 3. 数据校验
-- 检查迁移结果
DO $$
DECLARE
    hospital_migrated INT;
    department_migrated INT;
    original_hospital_count INT;
    original_department_count INT;
BEGIN
    -- 统计迁移后的数据
    SELECT COUNT(*) INTO hospital_migrated
    FROM system.sys_hospital
    WHERE scan_code_msg IS NOT NULL AND scan_code_msg != '';
    
    SELECT COUNT(*) INTO department_migrated
    FROM system.sys_department
    WHERE scan_code_msg IS NOT NULL AND scan_code_msg != '';
    
    -- 统计原始数据
    SELECT COUNT(*) INTO original_hospital_count
    FROM followup.scan_code_message
    WHERE hospital_id IS NOT NULL AND department_id IS NULL;
    
    SELECT COUNT(*) INTO original_department_count
    FROM followup.scan_code_message
    WHERE department_id IS NOT NULL;
    
    -- 输出迁移结果
    RAISE NOTICE '========== 扫码消息迁移结果 ==========';
    RAISE NOTICE '医院级别：原始数据 % 条，已迁移 % 条', original_hospital_count, hospital_migrated;
    RAISE NOTICE '科室级别：原始数据 % 条，已迁移 % 条', original_department_count, department_migrated;
    
    -- 检查是否有迁移失败的情况
    IF hospital_migrated < original_hospital_count THEN
        RAISE WARNING '部分医院扫码消息未成功迁移，请检查医院 ID 是否存在于 sys_hospital 表';
    END IF;
    
    IF department_migrated < original_department_count THEN
        RAISE WARNING '部分科室扫码消息未成功迁移，请检查科室 ID 是否存在于 sys_department 表';
    END IF;
    
    RAISE NOTICE '========================================';
END $$;

-- 4. 查看迁移后的示例数据
SELECT 'HOSPITAL' as type, id, name, 
       CASE 
           WHEN scan_code_msg IS NULL THEN '(空)'
           WHEN scan_code_msg = '' THEN '(空字符串)'
           ELSE LEFT(scan_code_msg, 50) || '...'
       END as scan_code_msg_preview
FROM system.sys_hospital
WHERE scan_code_msg IS NOT NULL AND scan_code_msg != ''
LIMIT 3;

SELECT 'DEPARTMENT' as type, id, 
       COALESCE(display_name, name) as name,
       CASE 
           WHEN scan_code_msg IS NULL THEN '(空)'
           WHEN scan_code_msg = '' THEN '(空字符串)'
           ELSE LEFT(scan_code_msg, 50) || '...'
       END as scan_code_msg_preview
FROM system.sys_department
WHERE scan_code_msg IS NOT NULL AND scan_code_msg != ''
LIMIT 3;

COMMIT;

-- 执行后说明：
-- 1. 医院/科室的 scan_code_msg 字段已填充
-- 2. 原 followup.scan_code_message 表数据保留（在执行清理脚本前作为备份）
-- 3. 如需回滚，请执行对应的回滚脚本





