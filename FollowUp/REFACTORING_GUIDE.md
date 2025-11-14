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
7. [UI 设计系统](#七ui-设计系统)
8. [附录](#八附录)
9. [迁移经验与常见问题](#九迁移经验与常见问题)
10. [版本历史](#十版本历史)

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
- ✅ **核心功能和交互逻辑**（保留业务本质）

**可优化的内容**：
- 🔄 数据库表结构和关系（可优化）
- 🔄 API 接口定义（可简化）
- 🔄 代码实现方式（可重新设计）
- 🔄 **UI 布局和样式**（可重新设计，采用现代化风格）

**优化空间**：
- ✅ 可以重新简化代码结构并重新实现
- ✅ 可以合并冗余字段、优化表结构
- ✅ 可以简化复杂的业务流程
- ✅ **可以重新设计 UI，采用统一的现代化风格**
- ⚠️ **数据库改动必须提供迁移方案**

**UI 设计规范**：
- 🎨 **主题色**：rgb(24, 183, 151) #18B797 - 医疗绿
- 🎨 **设计理念**：专业、清晰、值得信赖
- 🎨 **设计风格**：现代化、扁平化、响应式
- 🎨 **保留原则**：核心功能和交互逻辑不变，UI 可重新设计

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

### 2.3 深度分析与再设计原则

- **先剖析，后实施**：每次迁移 NTCare 功能前，必须完成对原实现的完整拆解，输出"业务流程 / 数据来源 / UI 布局 / 交互行为"四维分析，并经用户确认后再进入编码阶段。
- **结合新架构重新设计**：参考分析结论，但允许在 FollowUp 中对页面布局、服务接口、统计口径做针对性优化，保证功能不缺失且符合最新数据库结构。
- **服务按模块分目录**：全局 `Services/` 目录应按模块拆分（例如 `Services/Statistics/`、`Services/Wechat/`），组件引用通过模块级 `_Imports.razor` 管理，避免旧结构下的堆叠式服务。
- **复杂页面拆分组件**：如 `HospitalMain` 等页面，需根据职责拆分为多个子组件或功能区块，并保持每个组件 < 500 行，配套 `.razor.css`。

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

## 七、UI 设计系统

> **设计理念**：专业、清晰、值得信赖
> **适用场景**：医疗随访后台管理系统
> **更新时间**：2025-01-13

### 7.1 配色系统

#### 主题色系（医疗绿）

```css
:root {
    /* 主题色系 - 医疗绿 */
    --primary-color: #18B797;          /* 主题色 */
    --primary-light: #4ECDB3;          /* 浅色版本 - 用于悬停、高亮 */
    --primary-lighter: #E8F8F5;        /* 极浅版本 - 用于背景、标签 */
    --primary-dark: #0D9B7E;           /* 深色版本 - 用于按钮按下 */
    --primary-darker: #0A7A63;         /* 极深版本 - 用于导航菜单 */
}
```

**使用场景**：
- `--primary-color` - 主要按钮、链接、图标、数据高亮
- `--primary-light` - 悬停状态、次要高亮
- `--primary-lighter` - 背景色、标签、徽章
- `--primary-dark` - 按钮按下状态、深色边框
- `--primary-darker` - 侧边导航、深色背景

#### 中性色系（专业灰）

```css
:root {
    /* 背景色 */
    --bg-primary: #F7F9FC;             /* 主背景 - 淡蓝灰 */
    --bg-secondary: #FFFFFF;           /* 卡片背景 - 纯白 */
    --bg-tertiary: #EDF2F7;            /* 次要背景 - 浅灰 */

    /* 文本色 */
    --text-primary: #2D3748;           /* 主文本 - 深灰 */
    --text-secondary: #718096;         /* 次要文本 - 中灰 */
    --text-tertiary: #A0AEC0;          /* 辅助文本 - 浅灰 */

    /* 边框与分割线 */
    --border-color: #E2E8F0;           /* 边框色 */
    --divider-color: #EDF2F7;          /* 分割线 */
}
```

**使用场景**：
- `--bg-primary` - 页面主背景（减少视觉疲劳）
- `--bg-secondary` - 卡片、对话框、表单背景
- `--bg-tertiary` - 次要区域、禁用状态背景
- `--text-primary` - 标题、重要文本
- `--text-secondary` - 正文、标签
- `--text-tertiary` - 辅助信息、占位符

#### 功能色系

```css
:root {
    --success-color: #48BB78;          /* 成功 - 绿色 */
    --warning-color: #ED8936;          /* 警告 - 橙色 */
    --error-color: #F56565;            /* 错误 - 红色 */
    --info-color: #4299E1;             /* 信息 - 蓝色 */
}
```

**使用场景**：
- `--success-color` - 成功提示、完成状态
- `--warning-color` - 警告提示、待处理状态
- `--error-color` - 错误提示、失败状态
- `--info-color` - 信息提示、系统管理功能

#### 数据可视化配色

```css
/* 用于图表、统计数据展示 */
--chart-colors: [
    '#18B797',  /* 主题绿 - 随访完成 */
    '#4299E1',  /* 蓝色 - 待随访 */
    '#ED8936',  /* 橙色 - 逾期 */
    '#9F7AEA',  /* 紫色 - 其他 */
    '#48BB78',  /* 成功绿 */
    '#F56565'   /* 错误红 */
];
```

---

### 7.2 阴影系统

```css
:root {
    /* 分层阴影 - 增强层次感 */
    --shadow-sm: 0 1px 3px rgba(0, 0, 0, 0.04), 0 1px 2px rgba(0, 0, 0, 0.02);
    --shadow-md: 0 4px 6px rgba(0, 0, 0, 0.05), 0 2px 4px rgba(0, 0, 0, 0.03);
    --shadow-lg: 0 10px 15px rgba(0, 0, 0, 0.06), 0 4px 6px rgba(0, 0, 0, 0.04);
    --shadow-xl: 0 20px 25px rgba(0, 0, 0, 0.08), 0 10px 10px rgba(0, 0, 0, 0.04);
}
```

**使用场景**：
- `--shadow-sm` - 卡片默认状态、输入框
- `--shadow-md` - 按钮、下拉菜单
- `--shadow-lg` - 卡片悬停状态、对话框
- `--shadow-xl` - 模态框、重要提示

**设计原则**：
- 使用双层阴影增强立体感
- 阴影颜色统一为黑色半透明
- 阴影强度随层级递增

---

### 7.3 布局设计

#### MainLayout（主布局）

```
┌─────────────────────────────────────────────────────────┐
│  AppBar (高度 64px)                                      │
│  背景：白色 + 底部阴影                                    │
│  左：Logo + 系统名称（深灰色）                            │
│  右：用户信息 + 头像                                      │
├──────┬──────────────────────────────────────────────────┤
│      │                                                   │
│ 侧边 │  主内容区                                         │
│ 导航 │  背景：#F7F9FC（淡蓝灰）                          │
│      │  内边距：32px                                     │
│ 宽度 │                                                   │
│ 240px│                                                   │
│      │                                                   │
│ 背景 │                                                   │
│ 渐变 │                                                   │
│      │                                                   │
└──────┴──────────────────────────────────────────────────┘
```

**设计要点**：
- **AppBar 白色背景** - 更专业、更清爽
- **系统名称深灰色** - 提升可读性
- **侧边导航深色渐变** - 保持层次感
- **主内容区淡蓝灰背景** - 减少视觉疲劳

#### 卡片设计

```
┌─ 左侧色条（4px 主题色）
│  ┌────────────────────────────┐
│  │ 图标 + 标题                 │
│  ├────────────────────────────┤
│  │ 📊 标签        数据值      │
│  │ 👥 标签        数据值      │
│  │ 📋 标签        数据值      │
│  ├────────────────────────────┤
│  │ 操作按钮  →                │
│  └────────────────────────────┘
```

**设计要点**：
- **左侧色条** - 4px 宽，用主题色标识
- **白色背景** - 纯白 + 微妙阴影
- **图标圆形背景** - 主题色 10% 透明度
- **数据大号字体** - 24px，突出关键信息
- **悬停效果** - 轻微抬升 + 阴影加深

---

### 7.4 视觉层次与信息架构

#### 信息优先级原则

界面中的信息应该有明确的层次，通过**字体大小、颜色、粗细**建立视觉优先级：

**第一层级：关键数据**
- 用途：核心业务数据（患者数、任务数、金额等）
- 设计：大字号 + 主题色 + 粗体 + 专用数字字体
- 原则：一眼就能看到，数字清晰易读

**第二层级：标题和主体内容**
- 用途：页面标题、卡片标题、主要文字
- 设计：中等字号 + 深灰色 + 粗体（标题）或常规（正文）
- 原则：建立内容结构，引导阅读顺序

**第三层级：标签和辅助文字**
- 用途：字段标签、说明文字、次要信息
- 设计：较小字号 + 中灰色 + 常规字重
- 原则：提供上下文，但不抢夺注意力

**第四层级：装饰和分隔元素**
- 用途：图标、分割线、背景元素
- 设计：浅灰色 + 细线 + 低对比度
- 原则：辅助视觉组织，不干扰内容阅读

#### 字体尺寸比例关系

不要孤立地设置字体大小，而应建立**比例系统**：

- **关键数据** 应比 **标题** 大 1.5-2 倍
- **标题** 应比 **标签** 大 1.2-1.4 倍
- **标签** 应比 **最小可读字号** 大至少 2px

**设计思路**：当调整一个层级的字号时，检查其他层级是否需要同步调整，保持比例协调。

#### 字体系统

```css
/* 标题字体 - 优先系统默认字体，确保跨平台一致性 */
font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', 'PingFang SC', 'Hiragino Sans GB', 'Microsoft YaHei', sans-serif;

/* 数字字体 - 用于统计数据，等宽清晰 */
font-family: 'Segoe UI', -apple-system, system-ui;
```

**字体选择原则**：
- 优先使用系统字体，加载快、渲染好
- 数字使用等宽或专用数字字体，对齐整齐
- 中文和英文字体分开指定，确保最佳显示效果

---

### 7.5 组件样式规范

#### 按钮

```css
/* 主要按钮 */
.primary-button {
    background: var(--primary-color);
    color: white;
    border-radius: 8px;
    padding: 10px 20px;
    transition: all 0.2s ease;
}

.primary-button:hover {
    background: var(--primary-dark);
    transform: translateY(-1px);
    box-shadow: var(--shadow-md);
}

.primary-button:active {
    transform: translateY(0);
}
```

#### 输入框

```css
.input-field {
    border: 1px solid var(--border-color);
    border-radius: 8px;
    padding: 10px 12px;
    transition: all 0.2s ease;
}

.input-field:focus {
    border-color: var(--primary-color);
    box-shadow: 0 0 0 3px var(--primary-lighter);
}
```

#### 卡片

```css
.card {
    background: var(--bg-secondary);
    border-radius: 12px;
    padding: 28px;  /* 内边距应足够宽松，给内容呼吸空间 */
    box-shadow: var(--shadow-sm);
    border-left: 4px solid var(--primary-color);
    transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);
}

.card:hover {
    transform: translateY(-4px);
    box-shadow: var(--shadow-lg);
}
```

**卡片内部间距设计思路**：
- **内边距原则**：卡片内边距应明显大于内部元素间距，建立视觉边界
- **层次间距**：头部、主体、底部之间用间距和分割线区分
- **元素间距**：相关元素（如图标+文字）间距小，不相关元素间距大
- **图标尺寸**：图标应与文字大小协调，不过大也不过小

**间距比例建议**：
- 卡片内边距 : 区块间距 : 元素间距 ≈ 3 : 2 : 1
- 例如：内边距 24-32px，区块间距 16-20px，元素间距 8-12px

---

### 7.6 响应式设计策略

#### 核心理念：适配而非缩放

响应式设计不是简单地缩小或放大，而是根据设备特性**重新组织内容**：

- **移动端**：单列布局，优先显示核心信息，次要信息可折叠
- **平板**：双列或三列，平衡信息密度和可读性
- **桌面**：多列布局，但优先增加留白而非增加列数
- **大屏**：更宽松的间距，更大的字体，而非简单地显示更多内容

#### 断点选择原则

使用标准断点，但根据内容特性灵活调整：

```css
/* 移动设备 - 优先可读性 */
@media (max-width: 600px) {
    /* 间距可适当紧凑，但要保证点击目标足够大（44×44px） */
    /* 字体不能过小，最小 14px */
}

/* 平板设备 - 平衡信息量和舒适度 */
@media (max-width: 960px) {
    /* 开始利用横向空间，但不要过度密集 */
}

/* 桌面设备 - 充分利用空间 */
@media (max-width: 1280px) {
    /* 多列布局，但保持足够间距 */
}

/* 大屏设备 - 优先舒适度 */
@media (min-width: 1281px) {
    /* 增加间距和留白，而非增加列数 */
    /* 考虑限制最大宽度，避免内容过于分散 */
}
```

#### MudGrid 布局思路

```razor
<!-- 卡片网格布局 - 响应式列数控制 -->
<MudGrid Spacing="4">  <!-- Spacing 越大，卡片间距越宽松 -->
    <MudItem xs="12" sm="6" md="4" lg="3">
        <!--
        xs="12": 移动端单列，确保可读性
        sm="6":  平板双列，开始利用横向空间
        md="4":  中等屏幕三列，平衡信息量
        lg="3":  大屏四列，避免过度密集（不要超过 6 列）
        -->
    </MudItem>
</MudGrid>
```

**列数选择原则**：
- **信息卡片**：大屏最多 4-6 列，再多会难以浏览
- **数据表格**：可以更密集，但要保证可读性
- **仪表盘**：优先 2-4 列，突出关键指标

**间距选择原则**：
- `Spacing="2"`：紧凑布局，适合数据密集型页面
- `Spacing="4"`：标准布局，适合大多数场景
- `Spacing="6"`：宽松布局，适合强调视觉舒适度的页面

#### 响应式间距策略

屏幕越大，间距应该**按比例增加**，而非保持不变：

- **移动端**：间距紧凑，但保证可操作性
- **平板**：间距适中，平衡空间利用和舒适度
- **桌面**：间距宽松，充分利用大屏优势
- **大屏**：间距更宽松，避免内容过于分散

**反例**：所有屏幕使用相同间距，导致大屏显得拥挤，小屏显得空洞。

#### 字体响应式原则

字体大小也应该响应式调整：

- **移动端**：字体可适当小一些（但不低于 14px），节省空间
- **桌面端**：字体适当大一些，利用大屏优势提升可读性
- **关键数据**：在所有屏幕上都应该足够大，易于识别

**调整幅度**：通常在 ±2-4px 范围内，不要差异过大。

---

### 7.7 视觉舒适度设计原则（2025-01-13 新增）

#### 核心理念：呼吸感优先

医疗系统的用户长时间使用界面，**视觉舒适度**直接影响工作效率和用户体验。设计时应优先考虑：

1. **留白即价值**
   - 适当的留白不是浪费空间，而是提升信息可读性的关键
   - 密集的布局会增加认知负担，降低操作效率
   - 宁可少显示一些内容，也要保证每个元素都清晰可辨

2. **信息密度控制**
   - 避免在单屏内塞入过多信息
   - 大屏幕不等于可以显示更多内容，而是应该提供更舒适的浏览体验
   - 卡片式布局：大屏一行 3-4 个为宜，不超过 6 个

3. **层次感通过间距建立**
   - 相关元素靠近，不相关元素远离
   - 卡片间距应明显大于卡片内部间距
   - 通过间距的层次变化引导用户视线

#### 设计决策框架

当遇到"页面过于密集"的问题时，按以下优先级调整：

**优先级 1：调整布局密度**
- 减少单行显示的卡片数量（响应式断点调整）
- 增大卡片之间的间距（Grid Spacing）
- 评估是否真的需要在首屏显示所有信息

**优先级 2：优化内部间距**
- 增加卡片内边距，给内容更多呼吸空间
- 调整元素之间的垂直/水平间距
- 确保点击目标足够大（移动端尤其重要）

**优先级 3：调整字体和图标**
- 适度增大字体，提升可读性
- 图标与文字保持视觉平衡
- 关键数据使用更大字号突出显示

**优先级 4：简化内容**
- 评估是否所有信息都必须显示
- 考虑将次要信息折叠或延迟加载
- 用渐进式披露减少初始信息量

#### 响应式设计思路

不同屏幕尺寸的设计目标不同：

- **移动端（≤600px）**：单列布局，确保可读性，间距可适当紧凑
- **平板（600-960px）**：双列布局，平衡信息量和舒适度
- **桌面（960-1280px）**：3-4 列布局，充分利用空间但不过度密集
- **大屏（>1280px）**：优先增加间距和留白，而非增加列数

**关键原则**：屏幕越大，间距应越宽松，而非简单地塞入更多内容。

#### 视觉平衡原则

调整任何一个维度时，需同步考虑其他维度：

- **间距增大** → 字体可适度增大，保持视觉密度一致
- **卡片变大** → 图标和标题也应相应增大
- **列数减少** → 可以增加卡片内部细节，因为单个卡片有更多空间

**反例**：只增加间距但字体过小，会显得空洞；只增大字体但间距不变，会显得拥挤。

#### 实施检查清单

设计完成后，问自己：

- [ ] 用户能否在 3 秒内找到最重要的信息？
- [ ] 长时间浏览是否会感到视觉疲劳？
- [ ] 卡片之间的分隔是否清晰？
- [ ] 点击目标是否足够大（至少 44×44px）？
- [ ] 大屏幕上是否还有足够的留白？
- [ ] 移动端是否仍然易于操作？

#### 设计权衡

有时需要在"信息密度"和"视觉舒适"之间权衡：

- **仪表盘/概览页**：优先舒适度，宁可分页也不要过度密集
- **数据表格/列表**：可适当提高密度，但要保证可读性
- **详情页**：充分利用留白，突出关键信息

**核心原则**：当不确定时，选择更宽松的布局。

---

### 7.8 微交互设计

#### 悬停效果

```css
/* 卡片悬停 */
.card:hover {
    transform: translateY(-4px);
    box-shadow: var(--shadow-lg);
}

/* 按钮悬停 */
.button:hover {
    transform: translateY(-1px);
}

/* 链接悬停 */
.link:hover {
    color: var(--primary-dark);
}
```

#### 加载动画

```css
@keyframes pulse {
    0%, 100% { opacity: 1; }
    50% { opacity: 0.5; }
}

.loading {
    animation: pulse 1.5s ease-in-out infinite;
}
```

---

### 7.9 UI 设计核心原则

#### 1. 专业性 - 清晰、严谨、不花哨

**设计思路**：
- 医疗系统需要传递专业和可信赖的感觉
- 避免过度装饰，每个元素都应有明确功能
- 使用克制的动画和过渡效果

**实施要点**：
- 配色稳重，不使用过于鲜艳的颜色
- 布局规整，对齐精确
- 图标风格统一，使用线性或填充风格之一

#### 2. 可读性 - 信息层次分明，数据易读

**设计思路**：
- 用户需要快速找到关键信息
- 通过视觉层次引导用户注意力
- 重要信息突出，次要信息弱化

**实施要点**：
- 建立清晰的字体大小层次（标题 > 正文 > 标签）
- 关键数据使用大字号和主题色
- 合理使用留白分隔不同信息块

#### 3. 信任感 - 稳重的配色，减少视觉疲劳

**设计思路**：
- 用户长时间使用系统，视觉舒适度很重要
- 避免高对比度和刺激性配色
- 使用柔和的背景色和适中的对比度

**实施要点**：
- 主背景使用淡蓝灰而非纯白，减少眼睛疲劳
- 文字颜色使用深灰而非纯黑，降低对比度
- 阴影使用柔和的多层阴影，而非单一硬阴影

#### 4. 效率优先 - 减少干扰，突出关键信息

**设计思路**：
- 用户的目标是完成任务，而非欣赏界面
- 减少不必要的视觉元素和交互步骤
- 常用功能应该一目了然

**实施要点**：
- 首屏显示最重要的信息和操作
- 减少弹窗和模态对话框的使用
- 操作反馈及时明确

#### 5. 一致性 - 统一的配色、字体、间距、阴影

**设计思路**：
- 一致性降低学习成本，提升使用效率
- 相同功能在不同页面应该有相同的表现
- 建立设计系统，而非每个页面单独设计

**实施要点**：
- 使用 CSS 变量统一管理颜色和尺寸
- 建立组件库，复用而非重复创建
- 定期审查，确保新页面符合既有规范

#### 6. 可访问性 - 足够的对比度，清晰的焦点状态

**设计思路**：
- 考虑不同用户的视觉能力
- 支持键盘导航和屏幕阅读器
- 确保在不同设备和环境下都可用

**实施要点**：
- 文字与背景对比度至少 4.5:1
- 可交互元素有明确的悬停和焦点状态
- 点击目标至少 44×44px（移动端）

#### 7. 舒适性 - 适当的留白和间距，避免视觉密集

**设计思路**：
- 留白不是浪费空间，而是提升体验的关键
- 密集的布局增加认知负担，降低效率
- 宁可少显示一些内容，也要保证清晰易读

**实施要点**：
- 卡片之间有明显的间距和分隔
- 卡片内部有足够的内边距
- 大屏幕优先增加留白，而非增加内容密度
- 控制单屏信息量，避免信息过载

---

### 7.10 UI 设计自查清单

#### 配色与主题

- [ ] 是否使用 CSS 变量而非硬编码颜色？
- [ ] 主题色是否统一使用 `var(--primary-color)`？
- [ ] 文本颜色是否符合层次规范（深灰 > 中灰 > 浅灰）？
- [ ] 背景色是否使用淡蓝灰而非纯白？
- [ ] 功能色（成功/警告/错误）是否使用统一变量？

#### 布局与间距

- [ ] 页面是否有足够的外边距（容器 padding）？
- [ ] 卡片之间的间距是否明显（Grid Spacing 足够大）？
- [ ] 卡片内边距是否足够宽松（给内容呼吸空间）？
- [ ] 相关元素是否靠近，不相关元素是否远离？
- [ ] 大屏幕是否优先增加留白而非增加内容密度？

#### 视觉层次

- [ ] 关键数据是否使用大字号和主题色突出显示？
- [ ] 标题、正文、标签的字体大小是否有明确层次？
- [ ] 字体大小比例是否协调（不要跳跃过大）？
- [ ] 是否通过间距和分割线建立内容层次？
- [ ] 用户能否在 3 秒内找到最重要的信息？

#### 响应式设计

- [ ] 是否在移动端、平板、桌面都测试过？
- [ ] 移动端是否单列布局，确保可读性？
- [ ] 大屏幕列数是否合理（不超过 6 列）？
- [ ] 字体大小是否随屏幕调整（±2-4px）？
- [ ] 间距是否随屏幕按比例调整？
- [ ] 移动端点击目标是否足够大（≥44×44px）？

#### 视觉舒适度

- [ ] 长时间浏览是否会感到视觉疲劳？
- [ ] 卡片之间的分隔是否清晰？
- [ ] 文字和图标尺寸是否易于阅读？
- [ ] 整体布局是否避免信息过载？
- [ ] 是否有足够的留白和呼吸空间？
- [ ] 单屏信息量是否适中（不过多也不过少）？

#### 交互与反馈

- [ ] 可交互元素是否有明确的悬停状态？
- [ ] 按钮和链接是否有清晰的焦点状态（键盘导航）？
- [ ] 悬停效果是否流畅（使用 transition）？
- [ ] 加载状态是否有明确提示？
- [ ] 操作反馈是否及时（成功/失败提示）？

#### 一致性检查

- [ ] 是否使用统一的阴影系统（sm/md/lg/xl）？
- [ ] 圆角是否统一（按钮 8px，卡片 12px）？
- [ ] 图标风格是否统一（全部使用 Material Icons）？
- [ ] 相同功能在不同页面是否表现一致？
- [ ] 是否复用现有组件而非重复创建？

#### 可访问性检查

- [ ] 文字与背景对比度是否足够（≥4.5:1）？
- [ ] 是否支持键盘导航（Tab 键切换焦点）？
- [ ] 表单是否有清晰的标签和错误提示？
- [ ] 颜色是否不是唯一的信息传递方式（考虑色盲用户）？
- [ ] 图片是否有 alt 文本（如适用）？

---

## 八、附录

### 8.1 相关文档

- [CLAUDE.md](./CLAUDE.md) - 项目结构规范
- [极简开发规则](用户规则) - 开发原则和约束

### 8.2 快速参考

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

## 九、迁移经验与常见问题

> 本章节记录实际迁移过程中遇到的问题和解决方案，持续更新。

### 9.1 配置管理

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

### 9.2 架构简化决策

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

### 9.3 数据库查询优化

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

### 9.4 服务依赖处理

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

### 9.5 UI 实现的深度分析

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

### 9.6 配置完整性验证

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

### 9.7 问题分类与处理原则

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

### 9.8 常见错误模式

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

### 9.9 快速检查清单

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

## 十、版本历史

| 版本 | 日期 | 说明 |
|------|------|------|
| 1.0 | 2025-01-12 | 初始版本 |
| 1.1 | 2025-01-13 | 新增第九章：迁移经验与常见问题（基于 AdminMain 迁移） |
| 1.2 | 2025-01-13 | 新增第七章：UI 设计系统（医疗专业版配色与布局规范） |
| 1.3 | 2025-01-13 | **UI 设计指南重构**：从具体数值转向设计思路和原则，提升指导性和通用性 |

**1.3 版本详细更新**：
- **重构 7.4 视觉层次**：从具体字号改为层次原则和比例关系，强调设计思路
- **重构 7.5 卡片样式**：从固定数值改为间距比例建议（3:2:1），提供设计思路
- **重构 7.6 响应式设计**：从断点数值改为响应式策略和原则，强调适配而非缩放
- **新增 7.7 视觉舒适度设计原则**：提供完整的设计决策框架和优化思路
- **扩展 7.9 设计原则**：从简单列表扩展为详细的设计思路和实施要点
- **重构 7.10 检查清单**：从数值检查改为设计质量检查，覆盖 7 个维度

**设计理念转变**：
- ❌ 旧版：提供具体数值（如 padding: 28px, font-size: 26px）
- ✅ 新版：提供设计思路（如间距比例 3:2:1，字体层次原则）
- 🎯 目标：从"照抄数值"转向"理解原则，灵活应用"

---

**文档维护**：此文档应随着重构过程不断更新和完善。

