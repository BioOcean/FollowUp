-- FollowUp 数据库升级脚本
-- 一键执行结构变更、数据迁移和验证

SET client_encoding = 'UTF8';
\set ON_ERROR_STOP on
\timing on

\echo ''
\echo '=========================================='
\echo 'FollowUp 数据库升级'
\echo '=========================================='
\echo ''

-- 检查 pgcrypto 扩展
CREATE EXTENSION IF NOT EXISTS pgcrypto;

\echo '步骤 1：结构变更'
\i structure/001_add_core_columns.sql

\echo ''
\echo '步骤 2：迁移随访记录'
\i migration/010_merge_followup_record.sql

\echo ''
\echo '步骤 3：合并宣教推送历史'
\i migration/020_merge_education_push_history.sql

\echo ''
\echo '步骤 4：迁移扫码消息'
\i migration/030_migrate_scan_code_message.sql

\echo ''
\echo '步骤 5：数据校验'


DO $$
DECLARE
    migrated_count INT;
    original_count INT;
BEGIN
    SELECT COUNT(*) INTO migrated_count FROM care.patient_event WHERE push_time IS NOT NULL;
    SELECT COUNT(*) INTO original_count FROM followup.followup_record;

    IF migrated_count = original_count THEN
        RAISE NOTICE '✓ 随访记录：% 条', migrated_count;
    ELSE
        RAISE WARNING '✗ 随访记录不匹配：原始 %，迁移 %', original_count, migrated_count;
    END IF;

    SELECT COUNT(*) INTO migrated_count FROM system.sys_hospital WHERE scan_code_msg IS NOT NULL;
    RAISE NOTICE '✓ 医院扫码消息：% 条', migrated_count;

    SELECT COUNT(*) INTO migrated_count FROM system.sys_department WHERE scan_code_msg IS NOT NULL;
    RAISE NOTICE '✓ 科室扫码消息：% 条', migrated_count;
END $$;

\echo ''
\echo '=========================================='
\echo '✅ 升级完成'
\echo '=========================================='
\echo ''

