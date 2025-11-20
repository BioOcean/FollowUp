-- FollowUp 随访管理系统 - 任务管理器性能优化索引
-- 创建日期：2025-01-18
-- 用途：优化待审核任务查询、统计和模糊搜索性能

-- ========================================
-- 索引 1：审核任务查询优化（部分索引）
-- ========================================
-- 用途：优化按项目和审核状态筛选待审核任务的查询
-- 预计性能提升：90-95%（仅索引待审核相关记录）
-- 使用场景：
--   - 待审核列表查询
--   - 按审核医生筛选任务
--   - 按审核状态统计
CREATE INDEX CONCURRENTLY IF NOT EXISTS idx_patient_event_task_audit
ON care.patient_event(project_id, event_status, audit_doctor)
WHERE event_status IN ('待审核', '已超时', '患者未提交');

-- ========================================
-- 索引 2：创建时间排序优化
-- ========================================
-- 用途：优化按创建时间倒序排列的查询
-- 预计性能提升：95-98%（避免全表扫描）
-- 使用场景：
--   - 任务列表默认排序（最新任务在前）
--   - 时间范围筛选（如"最近7天"）
CREATE INDEX CONCURRENTLY IF NOT EXISTS idx_patient_event_create_time
ON care.patient_event(create_time DESC);

-- ========================================
-- 索引 3：患者关联查询优化
-- ========================================
-- 用途：优化 patient_event 与 patient 表 JOIN 查询
-- 预计性能提升：90-95%（避免嵌套循环连接）
-- 使用场景：
--   - 查询患者姓名、身份证号
--   - 按患者信息筛选任务
CREATE INDEX CONCURRENTLY IF NOT EXISTS idx_patient_event_patient_id
ON care.patient_event(patient_id);

-- ========================================
-- 索引 4：任务名称模糊搜索优化（GIN 三元组索引）
-- ========================================
-- 用途：优化任务名称的模糊搜索（LIKE '%关键词%'）
-- 预计性能提升：95-98%（支持任意位置模糊匹配）
-- 前置条件：需要启用 pg_trgm 扩展
-- 使用场景：
--   - 按任务名称模糊搜索
--   - 高级搜索中的任务名称筛选

-- 确保 pg_trgm 扩展已启用
CREATE EXTENSION IF NOT EXISTS pg_trgm;

-- 创建 GIN 索引（使用三元组操作符类）
CREATE INDEX CONCURRENTLY IF NOT EXISTS idx_patient_event_task_name_gin
ON care.patient_event USING gin(task_name gin_trgm_ops);

-- ========================================
-- 索引创建完成
-- ========================================
-- 验证索引是否创建成功：
-- SELECT indexname, indexdef FROM pg_indexes WHERE tablename = 'patient_event' ORDER BY indexname;

-- 查看索引大小：
-- SELECT indexname, pg_size_pretty(pg_relation_size(indexrelid)) AS index_size
-- FROM pg_stat_user_indexes WHERE relname = 'patient_event';
