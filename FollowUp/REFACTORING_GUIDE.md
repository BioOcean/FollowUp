# FollowUp 重构指导文档

> **目标**：将 NTCare 系统重构为 FollowUp 系统，采用模块化架构，简化代码结构，提升可维护性。

---

## 📋 目录

1. [重构原则](#一重构原则)
2. [工作流程](#二工作流程)
3. [分析报告格式](#三分析报告格式)
4. [实施规范](#四实施规范)
5. [质量检查](#五质量检查)
6. [常见简化场景](#六常见简化场景)

---

## 一、重构原则

### 1.1 核心原则

| 原则 | 说明 |
|------|------|
| **只参考业务逻辑** | 不照搬 NTCare 的代码结构，只参考业务流程和数据库表 |
| **极简架构** | 能简化就简化，避免过度设计，优先使用最直接的实现方式 |
| **模块化组织** | 按功能模块组织文件，遵循 `CLAUDE.md` 中的文件结构规范 |
| **先分析后实施** | 必须先给出方案，用户确认后再实现，避免返工 |
| **深度理解业务** | 深度解析 NTCare 代码，完全理解要迁移的功能，不遗漏任何细节 |
| **数据库表名验证** | 必须通过 MCP 查询确认实际表名，严禁猜测或假设 |

### 1.2 参考程度

```
NTCare 实现
    ↓
只提取：业务逻辑 + 数据库表 + API 接口
    ↓
代码结构：完全按 FollowUp 新架构重写
```

**不参考的内容**：
- ❌ NTCare 的文件组织方式
- ❌ NTCare 的 Service/Repository 层次
- ❌ NTCare 的 DTO 转换逻辑
- ❌ NTCare 的复杂状态管理

**必须参考的内容**：
- ✅ 业务流程和规则（核心，完全保留）
- ✅ 关键算法和计算逻辑（必须保留）
- ✅ **UI 布局和样式**（完全参考，未经用户同意不得修改）

**可优化的内容**：
- 🔄 数据库表结构和关系（可优化）
- 🔄 API 接口定义（可简化）
- 🔄 代码实现方式（可重新设计）

**优化空间**：
- ✅ 可以重新简化代码结构并重新实现
- ✅ 可以合并冗余字段、优化表结构
- ✅ 可以简化复杂的业务流程
- ⚠️ **数据库改动必须提供迁移方案**

**严格约束**：
- ⛔ **UI 布局和样式必须完全参考 NTCare**
- ⛔ **未经用户明确同意，不得修改 UI 风格**
- ⛔ **如有 UI 优化建议，必须先征得用户同意**

---

## 二、工作流程

### 2.1 流程图

```
┌─────────────────┐
│ 用户提出功能需求 │
└────────┬────────┘
         ↓
┌─────────────────┐
│ MCP 查询数据库  │
│ - 确认实际表名  │
│ - 查看表结构    │
└────────┬────────┘
         ↓
┌─────────────────┐
│ AI 深度分析     │
│ NTCare 实现     │
│ - 页面文件      │
│ - 服务类        │
│ - 业务流程      │
│ - 算法逻辑      │
│ - 边界情况      │
└────────┬────────┘
         ↓
┌─────────────────┐
│ AI 输出分析报告 │
│ - NTCare 分析   │
│ - FollowUp 方案 │
│ - 差异说明      │
│ - 待确认事项    │
└────────┬────────┘
         ↓
    ┌────┴────┐
    │ 用户审查 │
    └────┬────┘
         ↓
    需要调整？
    ├─ 是 → 用户提出调整意见 → AI 修改方案 → 返回审查
    └─ 否 ↓
┌─────────────────┐
│ AI 按新架构实现 │
│ 1. 创建文件结构 │
│ 2. 实现代码     │
│ 3. 创建样式     │
└────────┬────────┘
         ↓
┌─────────────────┐
│ 编译测试        │
└────────┬────────┘
         ↓
    有错误？
    ├─ 是 → 修复错误 → 返回编译测试
    └─ 否 ↓
┌─────────────────┐
│ 完成 ✅         │
└─────────────────┘
```

### 2.2 阶段说明

| 阶段 | 负责人 | 输出 | 检查点 |
|------|--------|------|--------|
| 1. 需求提出 | 用户 | 功能描述 | 需求是否明确 |
| 2. 数据库表确认 | AI | 实际表名清单 | 通过 MCP 查询验证 |
| 3. NTCare 深度分析 | AI | 完整实现分析 | 是否找到所有相关文件和逻辑细节 |
| 4. 方案设计 | AI | 实现方案 | 是否符合新架构 |
| 5. 方案审查 | 用户 | 确认/调整意见 | 是否需要调整 |
| 6. 代码实施 | AI | 代码文件 | 是否遵循规范 |
| 7. 编译测试 | AI | 测试结果 | 是否有错误 |
| 8. 验收 | 用户 | - | 功能是否符合预期 |

---

## 三、分析报告格式

### 3.1 前置步骤：数据库表确认

**每个功能开始前，必须先执行 MCP 查询：**

```sql
-- 步骤1：根据功能找到相关的 schema 和表名
-- 例如：用户管理 → system.sys_user
--       随访记录 → followup.followup_record
--       患者数据 → public.patient

-- 步骤2：查询表结构（必须带 schema）
SELECT column_name, data_type, character_maximum_length, is_nullable, column_default
FROM information_schema.columns
WHERE table_schema = 'schema名' AND table_name = '表名'
ORDER BY ordinal_position;

-- 步骤3：查看表数据示例（了解实际内容）
SELECT * FROM schema名.表名 LIMIT 3;

-- 步骤4：查询关联表（外键关系）
SELECT 
    tc.table_schema, 
    tc.table_name, 
    kcu.column_name,
    ccu.table_schema AS foreign_table_schema,
    ccu.table_name AS foreign_table_name,
    ccu.column_name AS foreign_column_name 
FROM information_schema.table_constraints AS tc 
JOIN information_schema.key_column_usage AS kcu
    ON tc.constraint_name = kcu.constraint_name
    AND tc.table_schema = kcu.table_schema
JOIN information_schema.constraint_column_usage AS ccu
    ON ccu.constraint_name = tc.constraint_name
    AND ccu.table_schema = tc.table_schema
WHERE tc.constraint_type = 'FOREIGN KEY' 
AND tc.table_schema = 'schema名' 
AND tc.table_name = '表名';
```

### 3.2 标准模板

```markdown
## 📊 功能分析：{功能名称}

### 0️⃣ 数据库表确认（MCP 查询结果）

**涉及的表**：
| 表名 | 用途 | 关键字段 |
|------|------|---------|
| xxx | ... | id, name, ... |

（此处粘贴 MCP 查询的实际结果）

---

### 1️⃣ NTCare 深度分析

#### 📄 页面文件
| 文件路径 | 说明 | 关键功能 |
|---------|------|---------|
| NTCare/Components/Pages/XXX.razor | 主页面 | 列表展示、搜索、分页 |
| NTCare/Components/Pages/XXX.Dialog.razor | 编辑对话框 | 新增/编辑表单 |

#### 🔧 服务类
| 类名 | 路径 | 关键方法 |
|------|------|---------|
| XXXService | NTCare/Services/ | GetList(), Create(), Update(), Delete() |

#### 🗄️ 数据库表（通过 MCP 查询确认）

**MCP 查询策略**：

使用 `mcp_postgres_query` 工具，数据库架构包含多个 schema：
- **system** - 系统管理（用户、医院、科室、角色、权限）
- **form** - 表单系统（表单集、表单、问题、项目）
- **followup** - 随访管理（随访记录、宣教、患者消息）
- **care** - 护理管理（患者事件、住院、门诊）
- **target** - 临床数据（327 个评分量表、检查检验表）
- **public** - 患者数据（患者、唯一患者）

**查询命令**：
```sql
-- 1. 查询所有表（指定 schema）
SELECT table_schema, table_name 
FROM information_schema.tables 
WHERE table_schema IN ('system', 'form', 'followup', 'care', 'target', 'public')
AND table_type = 'BASE TABLE' 
ORDER BY table_schema, table_name;

-- 2. 查询表结构（必须指定 schema）
SELECT column_name, data_type, character_maximum_length, is_nullable, column_default
FROM information_schema.columns
WHERE table_schema = 'schema名' AND table_name = '表名'
ORDER BY ordinal_position;

-- 3. 查询表数据示例
SELECT * FROM schema名.表名 LIMIT 5;
```

**涉及的表**：
| 表名 | 关键字段 | 说明 |
|------|---------|------|
| xxx | id, name, ... | （实际表名，不猜测） |
| xxx | id, xxx_id, ... | （实际表名，不猜测） |

#### 🔄 业务流程
（流程图或步骤说明）

#### 💡 关键代码片段
（NTCare 中的关键实现代码，包括但不限于：）
- 数据查询和过滤逻辑
- 业务规则校验
- 数据转换和计算
- 特殊边界情况处理
- 权限控制逻辑
- 状态转换规则

#### 🎨 UI 布局和样式分析
（必须详细记录 NTCare 的 UI 实现）
- 页面布局结构（HTML 层次）
- 使用的 UI 组件类型
- 关键样式定义（颜色、字体、间距）
- 交互行为（点击、悬停、动画）
- 响应式设计处理

#### 🔍 深度分析要点
- [ ] 所有数据库表操作已完全理解
- [ ] 所有业务规则已完全提取
- [ ] 所有边界情况已识别
- [ ] 所有算法逻辑已理解
- [ ] **所有 UI 布局和样式已完整记录**
- [ ] **所有 UI 组件类型已识别**
- [ ] **所有交互行为已掌握**
- [ ] 所有异常处理逻辑已明确

---

### 2️⃣ FollowUp 实现方案

#### 📁 文件结构
```
Components/Modules/{模块名}/
├── Pages/
│   ├── XXXList.razor
│   └── XXXList.razor.css
├── Services/
│   └── XXXService.cs（可选）
└── _Imports.razor
```

#### 🎯 实现要点
1. 数据访问方式（可简化）
2. 业务逻辑处理（可重新设计）
3. **UI 组件选择（完全参考 NTCare）**
4. **样式方案（完全参考 NTCare）**

#### 🎨 UI/样式处理原则

**必须遵守**：
- UI 布局、风格、交互方式完全参考 NTCare
- 组件类型、位置、大小保持一致
- 颜色、字体、间距保持一致
- 动画、过渡效果保持一致

**如需修改 UI**：
1. 在分析报告中明确说明 UI 优化建议
2. 对比 NTCare 和建议方案的截图/描述
3. 说明优化原因和预期效果
4. 等待用户明确同意后才能实施

**实施策略**：
- 优先使用 MudBlazor 组件复现 NTCare 的 UI
- 样式文件（.razor.css）从 NTCare 复制后微调
- 布局结构完全按照 NTCare 的 HTML 结构

#### 🗄️ 数据库优化方案（如有）

**优化类型**：
- 字段合并：将多个冗余字段合并为一个
- 字段删除：移除未使用的字段
- 表结构调整：优化表关系、索引
- 数据类型优化：更合适的数据类型

**迁移方案**（必须提供）：
```sql
-- 迁移步骤1：添加新字段
ALTER TABLE schema.table_name ADD COLUMN new_column type;

-- 迁移步骤2：数据迁移
UPDATE schema.table_name SET new_column = ...;

-- 迁移步骤3：删除旧字段
ALTER TABLE schema.table_name DROP COLUMN old_column;
```

**回滚方案**（必须提供）：
```sql
-- 回滚步骤（逆向操作）
```

#### 📝 需要创建的文件
| 文件路径 | 说明 | 预计行数 |
|---------|------|---------|
| ... | ... | ... |

#### 🔄 业务流程（简化后）
（简化后的流程图）

---

### 3️⃣ 差异说明

#### 代码实现差异
| 方面 | NTCare | FollowUp | 原因 |
|------|--------|----------|------|
| 架构层次 | Service + Repository | 直接用 context | 减少层次，降低复杂度 |
| ... | ... | ... | ... |

#### UI/样式差异
| 方面 | NTCare | FollowUp | 说明 |
|------|--------|----------|------|
| 布局结构 | （描述） | **完全一致** | 完全参考 NTCare |
| 组件类型 | （描述） | **完全一致** | 使用 MudBlazor 复现 |
| 样式风格 | （描述） | **完全一致** | 复制并微调样式 |

**UI 优化建议**（可选，需用户同意）：
- 建议 1：...（说明原因和效果）
- 建议 2：...（说明原因和效果）
- **未经用户同意，不实施任何 UI 修改**

#### 数据库结构差异（如有优化）
| 方面 | NTCare 表结构 | FollowUp 优化方案 | 优化原因 |
|------|--------------|------------------|---------|
| 示例 | field1, field2, field3 | merged_field | 合并冗余字段 |
| ... | ... | ... | ... |

**数据迁移影响评估**：
- 数据量：约 XX 条记录
- 停机时间：预计 XX 分钟
- 风险等级：低/中/高
- 回滚时间：预计 XX 分钟

---

### 4️⃣ 工作量评估

- **预计步骤**：X 步
- **预计文件**：X 个
- **复杂度**：⭐⭐⭐ (低/中/高)

---

### 5️⃣ 待确认事项

❓ **问题 1**：...  
❓ **问题 2**：...

---

**请审查以上方案，确认后我将开始实施。**
```

### 3.3 报告要素说明

| 要素 | 必需 | 说明 |
|------|------|------|
| 数据库表确认 | ✅ | 通过 MCP 查询的实际表名和结构，严禁猜测 |
| NTCare 页面文件 | ✅ | 列出所有相关页面和对话框 |
| NTCare 服务类 | ✅ | 列出所有相关服务和关键方法 |
| **NTCare UI 布局和样式** | ✅ | **详细记录布局、组件、样式、交互** |
| 业务流程 | ✅ | 用流程图或步骤说明业务逻辑 |
| 关键代码片段 | ✅ | 展示所有关键算法、业务规则、边界情况处理 |
| 深度分析检查点 | ✅ | 确认所有逻辑细节（含 UI）已完全理解 |
| FollowUp 文件结构 | ✅ | 明确需要创建的文件和目录 |
| 实现要点 | ✅ | 说明关键技术决策和简化点 |
| **UI 实现方案** | ✅ | **说明如何用 MudBlazor 复现 NTCare 的 UI** |
| 差异说明 | ✅ | 对比代码、数据库、UI 的差异 |
| **UI 优化建议** | 可选 | **如有 UI 改进建议，需明确说明并等待用户同意** |
| 数据库优化方案 | 可选 | 如有表结构优化，必须提供迁移和回滚方案 |
| 工作量评估 | ✅ | 预估工作量（含数据迁移和 UI 复现时间） |
| 待确认事项 | 可选 | 需要用户决策的问题 |

---

## 四、实施规范

### 4.1 文件创建顺序

```
1. 创建模块文件夹结构
   Components/Modules/{模块名}/
   ├── Pages/
   ├── Components/
   ├── Dialogs/
   ├── Models/
   └── Services/

2. 创建 _Imports.razor
   @using FollowUp.Components.Modules.{模块名}.Pages
   @using FollowUp.Components.Modules.{模块名}.Components
   ...

3. 创建 Models（如果需要）
   视图模型、枚举等

4. 创建 Services（如果需要）
   复杂业务逻辑服务

5. 创建 Pages
   页面组件（.razor）

6. 创建样式文件
   每个组件的 .razor.css

7. 创建 Dialogs（如果需要）
   对话框组件
```

### 4.2 代码实现原则

| 原则 | 说明 | 示例 |
|------|------|------|
| **极简优先** | 能用 10 行代码解决的不用 20 行 | 简单查询直接用 LINQ |
| **直接操作 DbContext** | 简单 CRUD 不需要 Service 层 | `context.Users.ToListAsync()` |
| **复用 Bio.Models** | 不创建重复的实体类 | 直接使用 `Bio.Models.User` |
| **单文件优先** | <500 行不拆分 .razor.cs | 保持代码在 .razor 文件中 |
| **样式隔离** | 每个组件都有独立的 .razor.css | `UserList.razor.css` |
| **继承 BasePage** | 所有组件都继承 BasePage | `@inherits BasePage` |

### 4.3 命名规范

| 类型 | 命名规则 | 示例 |
|------|---------|------|
| 模块文件夹 | PascalCase | `UserManagement` |
| 页面组件 | PascalCase | `UserList.razor` |
| 页面子组件 | `PageName.SubName.razor` | `UserList.FilterPanel.razor` |
| 引用子组件 | `PageName_SubName` | `<UserList_FilterPanel />` |
| 服务类 | `XXXService` | `UserService.cs` |
| 视图模型 | `XXXViewModel` | `UserViewModel.cs` |
| 注入变量 | `inj_XXX` | `inj_logger` |

### 4.4 数据库操作规范

```csharp
// ✅ 推荐：简单查询直接在页面中
protected override async Task OnPageInitializedAsync()
{
    users = await context.Users
        .Where(u => u.IsActive)
        .OrderBy(u => u.Name)
        .ToListAsync();
}

// ✅ 推荐：复杂业务逻辑抽取到 Service
var result = await userService.CreateUserWithRoles(user, roleIds);

// ❌ 避免：简单 CRUD 也创建 Service（过度设计）
```

### 4.5 错误处理规范

```csharp
try
{
    await context.SaveChangesAsync();
    await ShowSuccessMessage("保存成功");
}
catch (Exception ex)
{
    inj_logger.LogError(ex, "保存失败");
    await ShowErrorMessage("保存失败，请重试");
}
```

---

## 五、质量检查

### 5.1 检查清单

每个功能实现完成后，必须通过以下检查：

- [ ] **架构检查**
  - [ ] 所有组件都继承了 `BasePage`
  - [ ] 文件组织符合模块化结构
  - [ ] 页面子组件使用下划线引用（`PageName_SubName`）

- [ ] **代码检查**
  - [ ] 文件 <500 行（未拆分 .razor.cs）
  - [ ] 注入变量使用 `inj_` 前缀
  - [ ] 数据库操作使用 `context`（来自 BasePage）
  - [ ] 错误处理完整（try-catch + 日志）

- [ ] **样式检查**
  - [ ] 每个组件都有对应的 `.razor.css` 文件
  - [ ] 样式命名符合规范
  - [ ] 无内联样式（除非必要）

- [ ] **编译检查**
  - [ ] 编译无错误
  - [ ] 编译无警告（或仅包版本警告）

- [ ] **功能检查**
  - [ ] 业务逻辑与 NTCare 一致
  - [ ] 数据库操作正确
  - [ ] UI 交互流畅

### 5.2 常见问题检查

| 问题 | 检查方法 | 解决方案 |
|------|---------|---------|
| 页面子组件无法渲染 | 查看引用方式 | 改为 `<PageName_SubName />` |
| CS0103 命名空间不一致 | 查看 @namespace | 添加与父页面一致的命名空间 |
| CS0122 访问 BasePage 属性错误 | 查看是否直接注入 | 通过继承访问 protected 属性 |
| Logger 泛型错误 | 查看 ILogger 泛型参数 | 使用 `ILogger<当前组件类型>` |

---

## 六、常见简化场景

### 6.1 代码简化对照表

| NTCare 实现 | FollowUp 简化方案 | 原因 |
|------------|------------------|------|
| Service + Repository | 直接使用 context | 减少层次，降低复杂度 |
| DTO 转换 | 直接使用 Bio.Models | 减少代码，避免重复定义 |
| 多个小文件 | 合并到单文件 | 减少文件数，提升可读性 |
| 复杂状态管理 | 简单的组件参数传递 | 降低复杂度 |

**⚠️ UI 组件特殊说明**：
- ❌ **不简化 UI 组件**：必须完全参考 NTCare 的 UI 实现
- ❌ **不修改布局风格**：保持与 NTCare 完全一致
- ✅ **技术栈替换**：使用 MudBlazor 复现相同的 UI 效果

### 6.2 数据库优化场景

| 优化类型 | 示例 | 迁移复杂度 |
|---------|------|-----------|
| 合并冗余字段 | `first_name` + `last_name` → `full_name` | 低 |
| 删除未使用字段 | 删除从未赋值的字段 | 低 |
| 数据类型优化 | `varchar(max)` → `varchar(100)` | 低 |
| 表结构重组 | 拆分大表为主表+扩展表 | 中 |
| 索引优化 | 添加常用查询字段的索引 | 低 |
| 外键约束 | 添加缺失的外键关系 | 中 |

**数据库优化原则**：
- 优先考虑性能和可维护性
- 避免破坏性变更（如删除有数据的字段）
- 必须提供完整的迁移脚本
- 必须提供回滚方案
- 必须评估数据迁移风险

### 6.3 代码简化示例

#### 示例 1：数据访问简化

```csharp
// ❌ NTCare：多层调用
public class UserService
{
    private readonly IUserRepository _repo;
    public async Task<List<User>> GetUsers()
    {
        return await _repo.GetAllAsync();
    }
}

public class UserRepository
{
    private readonly DbContext _context;
    public async Task<List<User>> GetAllAsync()
    {
        return await _context.Users.ToListAsync();
    }
}

// ✅ FollowUp：直接查询
protected override async Task OnPageInitializedAsync()
{
    users = await context.Users.ToListAsync();
}
```

#### 示例 2：DTO 简化

```csharp
// ❌ NTCare：创建 DTO
public class UserDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    // ... 与 User 实体完全相同的字段
}

// ✅ FollowUp：直接使用实体
List<User> users = await context.Users.ToListAsync();
```

### 6.4 数据库优化示例

#### 示例 1：合并冗余字段

```sql
-- NTCare 结构
CREATE TABLE system.sys_user (
    id uuid PRIMARY KEY,
    first_name varchar(50),
    last_name varchar(50),
    middle_name varchar(50)  -- 很少使用
);

-- FollowUp 优化方案
CREATE TABLE system.sys_user (
    id uuid PRIMARY KEY,
    full_name varchar(100)  -- 合并为一个字段
);

-- 迁移脚本
-- 步骤1：添加新字段
ALTER TABLE system.sys_user ADD COLUMN full_name varchar(100);

-- 步骤2：迁移数据
UPDATE system.sys_user 
SET full_name = CONCAT_WS(' ', first_name, middle_name, last_name);

-- 步骤3：删除旧字段（确认数据正确后）
ALTER TABLE system.sys_user 
DROP COLUMN first_name,
DROP COLUMN middle_name,
DROP COLUMN last_name;

-- 回滚方案
ALTER TABLE system.sys_user 
ADD COLUMN first_name varchar(50),
ADD COLUMN middle_name varchar(50),
ADD COLUMN last_name varchar(50);

UPDATE system.sys_user 
SET first_name = SPLIT_PART(full_name, ' ', 1),
    last_name = SPLIT_PART(full_name, ' ', -1);
    
ALTER TABLE system.sys_user DROP COLUMN full_name;
```

#### 示例 2：删除未使用字段

```sql
-- 检查字段是否有数据
SELECT COUNT(*) FROM system.sys_user WHERE unused_field IS NOT NULL;

-- 如果为 0，可以安全删除
ALTER TABLE system.sys_user DROP COLUMN unused_field;

-- 回滚方案
ALTER TABLE system.sys_user ADD COLUMN unused_field type DEFAULT default_value;
```

---

## 七、附录

### 7.1 相关文档

- [CLAUDE.md](./CLAUDE.md) - 项目结构规范
- [极简开发规则](用户规则) - 开发原则和约束

### 7.2 快速参考

#### 创建新模块命令

```bash
# 创建模块文件夹结构
mkdir -p Components/Modules/{模块名}/{Pages,Components,Dialogs,Models,Services}
```

#### _Imports.razor 模板

```razor
@using FollowUp.Components.Modules.{模块名}.Pages
@using FollowUp.Components.Modules.{模块名}.Components
@using FollowUp.Components.Modules.{模块名}.Models
```

#### 页面组件模板

```razor
@page "/{路径}"
@inherits BasePage

<PageTitle>{页面标题}</PageTitle>

<MudContainer MaxWidth="MaxWidth.Large">
    <!-- 页面内容 -->
</MudContainer>

@code {
    protected override async Task OnPageInitializedAsync()
    {
        // 初始化逻辑
    }
}
```

---

## 八、迁移经验与常见问题

> 本章节记录实际迁移过程中遇到的问题和解决方案，持续更新。

### 8.1 配置管理

#### ❌ 问题：重复或冗余的配置项

**案例**：AdminMain 迁移时，错误地新增了 `SystemName` 配置，与已有的 `loginHospitalTitle` 重复。

**根本原因**：
- 没有全面检查 NTCare 的配置使用场景
- 没有理解 `loginHospitalTitle` 在登录页和主界面的双重用途

**解决方案**：
1. 在迁移前，使用 `grep` 全面搜索配置项的所有使用位置
2. 分析配置项的实际用途和业务含义
3. 严格遵循 NTCare 的配置命名和结构，不随意新增

**正确做法**：
```json
// ✅ 统一使用 loginHospitalTitle
{
  "AppSettings": {
    "loginHospitalTitle": "健康管家"  // 登录页和主界面共用
  }
}

// ❌ 不要新增重复配置
{
  "AppSettings": {
    "loginHospitalTitle": "健康管家",
    "SystemName": "健康管家"  // 冗余！
  }
}
```

**检查清单**：
- [ ] 是否检查了配置项在 NTCare 中的所有使用位置？
- [ ] 是否理解了配置项的业务含义？
- [ ] 是否与现有配置项重复？

---

### 8.2 架构简化决策

#### ❌ 问题："临时方案" vs "正式实现" 的混淆

**案例**：在实现 MainLayout 时，将"从配置读取系统名称"标记为"临时方案"，期待后续迁移 `ICacheService`。

**根本原因**：
- 没有深入分析业务差异（NTCare 多系统 vs FollowUp 单系统）
- 误认为不同的实现方式就是"临时"的

**正确理解**：
| 场景 | 判断标准 |
|------|---------|
| **正式实现** | 基于业务差异做出的架构决策，无需后续调整 |
| **待实现功能** | 当前功能不完整，需要后续补充，使用 `TODO` 标记 |
| **绝不使用"临时"** | "临时"是模糊表述，必须明确是"正式"还是"待实现" |

**正确做法**：

```csharp
// ✅ 正式实现（基于业务差异的架构决策）
// 极简实现：从配置文件读取系统名称
// FollowUp 单系统设计，登录页和主界面显示相同的 loginHospitalTitle
systemName = inj_configuration["AppSettings:loginHospitalTitle"] ?? "健康管家";

// ✅ 待实现功能（明确标记 TODO）
// TODO: [导航菜单] - 添加侧边导航栏（NavMenu）
//       参考 NTCare 的实现，需要创建 NavMenuForFollowup.razor

// ❌ 错误表述
// 临时方案：从配置文件读取（等待迁移 ICacheService）
```

**决策流程**：
```
发现 NTCare 使用了某个服务/功能
    ↓
问：FollowUp 是否需要相同的业务能力？
    ├─ 是 → 是否可以用更简单的方式实现？
    │      ├─ 是 → 极简实现（正式方案）+ 注释说明决策原因
    │      └─ 否 → 完整迁移 NTCare 的服务
    └─ 否 → 不实现，或实现 FollowUp 特有的简化版本
```

---

### 8.3 数据库查询优化

#### ✅ 最佳实践：根据数据结构选择最优查询方式

**案例**：AdminMain 统计随访任务数量

**NTCare 原实现**：
```csharp
// 使用 followup_record 表（需要多表关联）
var followupCounts = await context.followup_record
    .Include(fr => fr.patient_event)
    .ThenInclude(pe => pe.patient)
    .Where(fr => hospitalIds.Contains(fr.patient_event.patient.hospital_id))
    .GroupBy(fr => fr.patient_event.patient.hospital_id)
    .Select(g => new { HospitalId = g.Key, Count = g.Count() })
    .ToListAsync();
```

**FollowUp 优化方案**：
```csharp
// 直接使用 patient_event 表 + 事件类型枚举
var followupEventTypes = FollowupEventTypeExtensions.GetFollowupEventTypes();

var followupCounts = await _context.patient_event
    .Join(_context.patient,
        pe => pe.patient_id,
        p => p.id,
        (pe, p) => new { pe.event_type, p.hospital_id })
    .Where(x => hospitalIds.Contains(x.hospital_id) 
        && followupEventTypes.Contains(x.event_type))
    .GroupBy(x => x.hospital_id)
    .Select(g => new { HospitalId = g.Key, Count = g.Count() })
    .ToListAsync();
```

**优化要点**：
1. **分析数据关系**：随访事件本质是 `patient_event` 的一种类型
2. **避免不必要的关联**：`followup_record` → `patient_event` → `patient` 简化为 `patient_event` → `patient`
3. **使用枚举管理类型**：创建 `FollowupEventTypeExtensions` 统一管理
4. **提升查询效率**：减少表关联层次，提高查询性能

**经验总结**：
- 优先分析数据表的实际关系和业务含义
- 选择最直接的数据源，避免多余的表关联
- 使用枚举或常量管理业务类型，便于维护

---

### 8.4 服务依赖处理

#### ✅ 原则：根据业务场景重新设计，不盲目照搬

**案例**：AdminMain 的权限检查和缓存清理

**NTCare 原实现**：
```csharp
// 1. 从缓存获取角色
private async Task<bool> CheckPermissionAsync()
{
    var roleNames = await inj_cacheService.GetForUserAsync<string>(
        MemoryCacheKeys.RoleNames.ToString()
    ) ?? string.Empty;
    return roleNames.Contains("系统");
}

// 2. 清理缓存
protected override async Task OnPageInitializedAsync()
{
    await inj_cacheService.RemoveForUserAsync(MemoryCacheKeys.HospitalId.ToString());
    await inj_cacheService.RemoveForUserAsync(MemoryCacheKeys.DepartmentId.ToString());
    // ... 清理更多缓存项
}
```

**FollowUp 简化方案**：
```csharp
// 1. 直接从 Claims 读取角色（无需缓存）
private Task<bool> CheckPermissionAsync()
{
    var user = inj_httpContextAccessor.HttpContext?.User;
    if (user?.Identity?.IsAuthenticated == true)
    {
        var roles = user.FindAll("roleName")
            .Select(c => System.Text.RegularExpressions.Regex.Unescape(c.Value))
            .ToList();
        var roleNames = string.Join(",", roles);
        return Task.FromResult(roleNames.Contains("系统"));
    }
    return Task.FromResult(false);
}

// 2. AdminMain 是全局视图，不需要清理缓存
protected override async Task OnPageInitializedAsync()
{
    // AdminMain 是系统管理员的全局视图，不需要清除缓存
    // （NTCare 中清除缓存是为了重置"当前选择的医院/科室/课题"状态）
    // FollowUp 采用更简单的设计：全局视图不维护"当前选择"状态
    await LoadData();
}
```

**决策依据**：
| 场景 | NTCare 设计 | FollowUp 简化 | 原因 |
|------|-----------|-------------|------|
| 角色信息 | 缓存到内存 | 直接读 Claims | Claims 本身就是请求级缓存 |
| AdminMain 状态 | 清理"当前选择"缓存 | 无状态设计 | 全局视图不需要记录"当前选择" |

**经验总结**：
- 不是所有 NTCare 的服务依赖都需要迁移
- 优先使用框架提供的能力（如 Claims）
- 根据 FollowUp 的架构重新设计状态管理

---

### 8.5 UI 实现的深度分析

#### ❌ 问题：UI 细节不一致

**案例**：MainLayout 的 AppBar 颜色和文本内容与 NTCare 不一致

**错误实现**：
```razor
<!-- ❌ 硬编码颜色 -->
<MudAppBar Style="background-color:#266F5E">
    <!-- ❌ 硬编码文本 -->
    <MudText>FollowUp 随访管理系统</MudText>
    <!-- ❌ 缺少动态问候 -->
    <MudText>你好, @userName</MudText>
</MudAppBar>
```

**正确实现**：
```razor
<!-- ✅ 使用 CSS 变量 -->
<MudAppBar Style="background-color:var(--bioo-main)">
    <!-- ✅ 从配置读取 -->
    <MudText Typo="Typo.h5" Color="Color.Inherit">@systemName</MudText>
    <MudSpacer />
    <!-- ✅ 动态问候 + 用户名 -->
    <MudText Style="cursor:pointer" @onclick="OnLogoutClick">@HelloMsg() @userName</MudText>
</MudAppBar>

@code {
    // ✅ 完整复制 NTCare 的 HelloMsg() 方法
    private string HelloMsg()
    {
        var msg = "";
        int hour = DateTime.Now.Hour;
        switch (hour)
        {
            case 0: case 1: case 2: case 3: case 4: 
            case 19: case 20: case 21: case 22: case 23: 
                msg = "晚上好！"; break;
            case 14: case 15: case 16: case 17: case 18: 
                msg = "下午好！"; break;
            case 11: case 12: case 13: 
                msg = "中午好！"; break;
            case 9: case 10: 
                msg = "上午好！"; break;
            default: 
                msg = "早上好！"; break;
        }
        return msg;
    }
}
```

**深度分析检查清单**：
- [ ] **颜色**：是否使用了 CSS 变量而非硬编码？
- [ ] **文本内容**：是否从配置读取？是否包含动态逻辑？
- [ ] **用户名显示**：Claims 优先级是否正确（displayName > GivenName > Name）？
- [ ] **问候语**：是否实现了时间段判断？
- [ ] **样式细节**：字体大小、颜色、间距是否与 NTCare 一致？

**经验总结**：
- 必须逐行对比 NTCare 的 UI 实现
- 不遗漏任何动态逻辑（如 `HelloMsg()`）
- 完整复制样式变量和 CSS 类名

---

### 8.6 配置完整性验证

#### ✅ 最佳实践：检查配置项的实际用途

**案例**：登录页面配置项的必要性验证

**用户疑问**：
> "这些配置是用来干什么的？是必要的吗？"
> - `hospitalLogo`
> - `loginBg`
> - `showLoginTitleLogo`
> - `loginHospitalTitle`
> - `subTitle`

**验证流程**：
1. 读取 NTCare 和 FollowUp 的 `Login.razor` 文件
2. 逐行查找配置项的使用位置
3. 确认配置项控制的 UI 元素
4. 检查对应的图片资源是否存在

**验证结果**：
| 配置项 | 代码位置 | 用途 | 必要性 |
|--------|---------|------|--------|
| `hospitalLogo` | 第14、32行 | 左上角logo + 登录框内logo | ✅ 必要 |
| `loginBg` | 第17行 | 登录页背景图 | ✅ 必要 |
| `showLoginTitleLogo` | 第26行 | 控制显示logo还是文本 | ✅ 必要 |
| `loginHospitalTitle` | 第28行 + MainLayout | 登录标题 + 主界面标题 | ✅ 必要 |
| `subTitle` | 第35-38行 | 登录框副标题 | ✅ 必要 |

**经验总结**：
- 不要凭感觉删除配置项，必须验证实际用途
- 使用 `grep` 搜索配置项的所有引用位置
- 检查配置项控制的 UI 元素是否在页面中使用

---

### 8.7 问题分类与处理原则

#### 分类标准

| 分类 | 判断标准 | 处理方式 |
|------|---------|---------|
| **A. 架构简化** | 基于业务差异的正式决策 | 实施 + 注释说明决策原因 |
| **B. 功能待实现** | 当前不完整，需后续补充 | 添加 `TODO` 标记 + 详细说明 |
| **C. 错误实现** | 与 NTCare 不一致或有 bug | 立即修正 |
| **D. 配置冗余** | 重复或未使用的配置 | 移除 + 更新相关代码 |

#### 处理流程

```
遇到问题
    ↓
判断问题类型（A/B/C/D）
    ↓
├─ A. 架构简化 → 在注释中说明：
│                "极简实现：基于 XXX 业务差异，采用 YYY 方案"
│
├─ B. 功能待实现 → 添加 TODO：
│                 "TODO: [模块名] - 功能描述
│                  参考 NTCare 的实现：xxx
│                  需要创建：xxx"
│
├─ C. 错误实现 → 深度分析 NTCare → 修正实现 → 测试验证
│
└─ D. 配置冗余 → 验证无引用 → 移除配置 → 清理相关代码
```

---

### 8.8 常见错误模式

#### 错误 1：硬编码配置值

```csharp
// ❌ 错误：硬编码
systemName = "健康管家";

// ✅ 正确：从配置读取
systemName = inj_configuration["AppSettings:loginHospitalTitle"] ?? "健康管家";
```

#### 错误 2：UI 细节不一致

```razor
<!-- ❌ 错误：缺少动态问候 -->
<MudText>你好, @userName</MudText>

<!-- ✅ 正确：动态问候 + 用户名 -->
<MudText>@HelloMsg() @userName</MudText>
```

#### 错误 3：颜色硬编码

```razor
<!-- ❌ 错误：硬编码颜色 -->
<MudAppBar Style="background-color:#266F5E">

<!-- ✅ 正确：使用 CSS 变量 -->
<MudAppBar Style="background-color:var(--bioo-main)">
```

#### 错误 4：新增冗余配置

```json
// ❌ 错误：新增重复配置
{
  "SystemName": "健康管家",
  "loginHospitalTitle": "健康管家"
}

// ✅ 正确：统一使用现有配置
{
  "loginHospitalTitle": "健康管家"
}
```

#### 错误 5：盲目照搬服务依赖

```csharp
// ❌ 错误：盲目迁移缓存服务
var roleNames = await inj_cacheService.GetForUserAsync<string>(...);

// ✅ 正确：根据业务选择最简方案
var roles = user.FindAll("roleName").Select(c => c.Value).ToList();
```

---

### 8.9 快速检查清单

**开始迁移前**：
- [ ] 是否阅读了 NTCare 的所有相关文件？
- [ ] 是否使用 MCP 查询了数据库表结构？
- [ ] 是否理解了业务流程和边界情况？
- [ ] 是否检查了所有配置项的用途？

**实施过程中**：
- [ ] 颜色是否使用 CSS 变量而非硬编码？
- [ ] 文本内容是否从配置读取？
- [ ] 是否完整复制了动态逻辑（如 `HelloMsg()`）？
- [ ] 是否根据业务差异做出了合理的简化决策？

**实施完成后**：
- [ ] 是否有使用"临时"这种模糊表述？（禁止使用）
- [ ] 所有简化决策是否有清晰的注释说明？
- [ ] 所有待实现功能是否有明确的 TODO 标记？
- [ ] UI 是否与 NTCare 完全一致？

---

## 九、版本历史

| 版本 | 日期 | 说明 |
|------|------|------|
| 1.0 | 2025-01-12 | 初始版本 |
| 1.1 | 2025-01-13 | 新增第八章：迁移经验与常见问题（基于 AdminMain 迁移） |

---

**文档维护**：此文档应随着重构过程不断更新和完善。

