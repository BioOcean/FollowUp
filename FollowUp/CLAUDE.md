# FollowUp 随访管理系统项目结构规范

## 项目组成

本解决方案包含两个主要项目：

1. **FollowUp** - Blazor Web 应用（管理端）
2. **FollowupWx** - 微信小程序（患者端）

---

## 一、Blazor 项目目录结构 (FollowUp/)

```
Components/
├── Layout/              # 布局组件
├── Shared/              # 全局共享组件（含登录、首页等全局页面）
├── Dialogs/             # 全局对话框
├── Modules/
│   └── ModuleName/
│       ├── Pages/       # 模块页面（含 @page）
│       ├── Components/  # 模块内组件
│       ├── Dialogs/     # 模块内对话框
│       ├── Models/
│       ├── Services/
│       └── _Imports.razor
├── BasePage.razor
└── _Imports.razor
Services/                # 全局服务
├── Extension/           # 扩展方法和工具类
Models/                  # 全局模型
wwwroot/                 # 静态资源
```

## 二、微信小程序项目结构 (FollowupWx/)

```
FollowupWx/
├── app.js               # 小程序入口逻辑
├── app.json             # 小程序全局配置
├── app.wxss             # 小程序全局样式
├── config.js            # 配置文件（API地址等）
├── pages/               # 小程序页面
│   ├── index/           # 首页
│   ├── patientLogin/    # 患者登录
│   ├── doctorLogin/     # 医生登录
│   ├── message/         # 消息页面
│   └── ...
├── components/          # 自定义组件
│   └── loginview/       # 登录视图组件
├── utils/               # 工具类
│   ├── api.js           # API接口定义
│   ├── request.js       # 网络请求封装
│   ├── navigation.js    # 导航工具
│   └── util.js          # 通用工具函数
├── images/              # 图片资源
├── miniprogram_npm/     # npm依赖（如Vant Weapp）
├── package.json         # npm依赖配置
├── project.config.json  # 开发者工具配置
└── sitemap.json         # 微信搜索配置
```

---

## 三、Blazor 组件分类规则

| 类型 | 位置 | 命名 | 引用方式 |
|------|------|------|----------|
| 页面组件 | `Modules/{模块}/Pages/` | `UserList.razor` | `<UserList />` |
| 页面子组件 | `Modules/{模块}/Components/` | `UserList.FilterPanel.razor` | `<UserList_FilterPanel />` |
| 模块内组件 | `Modules/{模块}/Components/` | `DataGrid.razor` | `<DataGrid />` |
| 全局组件 | `Components/Shared/` | `LoadingSpinner.razor` | `<LoadingSpinner />` |
| 模块对话框 | `Modules/{模块}/Dialogs/` | `EditDialog.razor` | - |
| 布局组件 | `Components/Layout/` | `MainLayout.razor` | - |

**组件位置决策：**
- 有 `@page` → `Pages/`
- 页面子组件 → `Components/`，命名 `PageName.SubComponent.razor`
- 模块内复用 → `Modules/{模块}/Components/`
- 跨模块复用 → `Components/Shared/`

## 四、核心规则

### 1. 代码拆分原则（优先级最高）

**只有文件 > 500 行时才拆分 .razor 和 .razor.cs，否则保持单文件**

```razor
@page "/users"
@inherits BasePage

<div>HTML 内容</div>

@code {
    // C# 代码直接写在这里
    protected override async Task OnPageInitializedAsync()
    {
        var data = await context.Users.ToListAsync();
    }
}
```

### 2. 页面子组件引用规则

文件名中的点(`.`)转换为下划线(`_`)：

| 文件名 | 引用方式 |
|--------|----------|
| `Home.Header.razor` | `<Home_Header />` ✅ |
| `Home.Header.razor` | `<Home.Header />` ❌ 无法渲染 |

**页面子组件必须添加：**
```razor
@namespace ProjectName.Components.Modules.ModuleName.Components
```

### 3. 组件文件三件套

- 主文件：`ComponentName.razor`（PascalCase）
- 代码后置：`ComponentName.razor.cs`（仅当 > 500 行）
- 样式文件：`ComponentName.razor.css`（必须同名）

### 4. BasePage 继承规范

**所有 Razor 组件必须继承 BasePage**（以下情况除外）

**例外情况**：
- 布局组件（`Layout/` 目录下的组件）
- `_Imports.razor` 文件
- 登录页面（`Login.razor`）：使用 `FullScreenLayout`，不需要数据库上下文
- 重定向页面（`Home.razor`、`RedirectToSignIn.razor`）：仅做路由跳转，不需要数据库上下文
- 错误页面（`ErrorPage.razor`）：显示错误信息，不需要数据库上下文

```razor
@inherits BasePage
```

BasePage 提供：
- `ContextFactory`：数据库上下文工厂
- `_context`：当前数据库上下文（自动创建/释放）
- `OnPageInitializedAsync()`：初始化钩子
- 自动认证检查：未认证用户自动跳转到登录页（已标记 `[AllowAnonymous]`）

```csharp
protected override async Task OnPageInitializedAsync()
{
    // _context 已自动创建
    var data = await _context.YourEntities.ToListAsync();
}
```

**注意事项**：
- BasePage 已标记 `[AllowAnonymous]`，子类无需重复标记
- BasePage 会检查认证状态，未认证用户会被重定向到 `/login`
- 登录页面（`/login`）会被自动跳过认证检查
- 如果页面不需要数据库访问，可以不使用 `_context`，但仍需继承 BasePage 以保持架构一致性

### 5. _Imports.razor 规范

**全局文件**（`Components/_Imports.razor`）：
- 框架命名空间
- 全局 UI 库
- 全局服务注入
- 不含模块特定命名空间

**模块文件**（`Modules/{模块}/_Imports.razor`）：
- 仅包含模块特定命名空间
- 自动继承全局导入

### 6. 注入命名规则

使用 `inj_` 前缀：

```razor
@inject IDbContextFactory<DbContext> inj_dbContextFactory
@inject ILogger<CurrentComponent> inj_logger
@inject NavigationManager inj_navigationManager
```

### 7. 样式文件规范

- 组件样式：`ComponentName.razor.css`（作用域隔离）
- 全局样式：`wwwroot/app.css`

## 五、常见错误

| 错误 | 原因 | 解决方案 |
|------|------|----------|
| 页面子组件无法渲染 | 使用 `<Home.Header />` | 改为 `<Home_Header />` |
| CS0103 命名空间不一致 | 子组件未添加 @namespace | 添加 `@namespace ProjectName.Components.Modules.ModuleName.Components` |
| CS0122 访问 BasePage 属性错误 | 直接注入 | 通过继承访问 protected 属性 |
| Logger 泛型错误 | 使用 `ILogger<BasePage>` | 使用 `ILogger<当前组件类型>` |

## 六、创建新模块流程

1. 创建目录：`Pages/`、`Components/`、`Dialogs/`、`Models/`、`Services/`
2. 创建 `_Imports.razor`，包含模块命名空间
3. 创建页面组件，继承 `BasePage`
4. 创建同名 `.razor.css` 样式文件
5. 复杂页面拆分为 `PageName.SubComponent.razor` 子组件，放在 `Components/` 目录（使用 `PageName_SubComponent` 引用）

## 七、错误处理规范

### 1. 全局错误边界

系统已配置全局错误边界（`GlobalErrorBoundary`），自动捕获未处理的异常并显示用户友好的错误页面。

### 2. 统一错误处理服务

所有继承 `BasePage` 的组件都可以使用以下错误处理方法：

```csharp
// 处理异常并显示用户友好的错误提示
await HandleExceptionAsync(ex, "保存失败", "保存用户数据");

// 显示成功消息
await ShowSuccessAsync("保存成功");

// 显示警告消息
await ShowWarningAsync("数据可能不完整");

// 显示错误消息
await ShowErrorAsync("操作失败，请重试");

// 显示信息消息
await ShowInfoAsync("正在处理中...");
```

### 3. 标准错误处理模式

**推荐模式**：
```csharp
try
{
    // 业务逻辑
    await _context.SaveChangesAsync();
    await ShowSuccessAsync("保存成功");
}
catch (Exception ex)
{
    await HandleExceptionAsync(ex, "保存失败，请重试", "保存数据");
}
```

**避免模式**：
```csharp
// ❌ 不要直接显示异常消息给用户
catch (Exception ex)
{
    await ShowErrorAsync(ex.Message); // 技术细节对用户不友好
}

// ❌ 不要吞掉异常
catch (Exception ex)
{
    // 什么都不做
}
```

### 4. 自动错误消息映射

`ErrorHandlingService` 会自动将技术异常转换为用户友好的消息：

- **数据库异常**：
  - 唯一约束冲突 → "数据已存在，请检查后重试"
  - 外键约束冲突 → "无法删除，存在关联数据"
  - 非空约束冲突 → "必填字段不能为空"

- **网络异常**：
  - `HttpRequestException` → "网络请求失败，请检查网络连接"
  - `TaskCanceledException` → "操作超时，请重试"

- **权限异常**：
  - `UnauthorizedAccessException` → "您没有权限执行此操作"

## 八、通用组件库

系统提供了一组通用组件，用于统一 UI 风格和减少重复代码：

### 1. EmptyDataView（无数据展示组件）

用于统一显示"无数据"状态：

```razor
@* 基础用法 *@
<EmptyDataView Message="暂无数据" />

@* 自定义样式 *@
<EmptyDataView Message="该科室暂无课题信息"
               Padding="120px 0"
               ImageSize="150px" />

@* 不使用图片，使用图标 *@
<EmptyDataView Message="无数据" ImageUrl="" />

@* 带操作按钮 *@
<EmptyDataView Message="暂无数据">
    <MudButton Color="Color.Primary" OnClick="@LoadData">重新加载</MudButton>
</EmptyDataView>
```

**参数说明**：
- `Message`：显示的消息文本（默认："暂无数据"）
- `ImageUrl`：图片URL（默认："images/nodata.jpg"，设为空字符串则显示图标）
- `ImageSize`：图片/图标大小（默认："120px"）
- `Padding`：内边距（默认："60px 0"）
- `MessageTypo`：消息文本样式（默认：`Typo.body2`）
- `MessageColor`：消息文本颜色（默认：`Color.Secondary`）
- `ChildContent`：自定义内容（如操作按钮）

### 2. LoadingView（加载状态组件）

用于统一显示加载状态：

```razor
@* 基础用法 *@
<LoadingView />

@* 自定义高度和消息 *@
<LoadingView Height="400px" Message="正在加载图表数据..." />

@* 仅显示消息，不显示加载动画 *@
<LoadingView ShowSpinner="false" Message="处理中..." />

@* 自定义加载动画样式 *@
<LoadingView SpinnerColor="Color.Success"
             SpinnerSize="Size.Large"
             Message="正在生成报表..." />
```

**参数说明**：
- `Message`：加载提示消息（默认："正在加载数据..."）
- `ShowSpinner`：是否显示加载动画（默认：`true`）
- `SpinnerColor`：加载动画颜色（默认：`Color.Primary`）
- `SpinnerSize`：加载动画大小（默认：`Size.Medium`）
- `Height`：容器高度（默认："300px"）
- `MessageTypo`：消息文本样式（默认：`Typo.body1`）
- `MessageColor`：消息文本颜色（默认：`Color.Default`）
- `ChildContent`：自定义内容

### 3. StatisticItem（统计数据项组件）

用于统一显示统计数据块：

```razor
@* 基础用法 *@
<StatisticItem Label="患者数量" Value="@patientCount.ToString()" Unit="个" />

@* 自定义样式 *@
<StatisticItem Label="累计扫码上平台数量"
               Value="@totalPatients.ToString()"
               Unit="人"
               LabelTypo="Typo.button"
               ValueTypo="Typo.h5"
               ValueColor="Color.Success"
               Width="33%" />

@* 自定义数值样式 *@
<StatisticItem Label="随访率"
               Value="@followupRate.ToString("0.0")"
               Unit="%"
               ValueStyle="color:#18b797; font-size:1.5rem;" />
```

**参数说明**：
- `Label`：标签文本
- `Value`：数值
- `Unit`：单位（默认：空字符串）
- `LabelTypo`：标签样式（默认：`Typo.button`）
- `LabelColor`：标签颜色（默认：`Color.Default`）
- `LabelClass`：标签CSS类
- `ValueTypo`：数值样式（默认：`Typo.h6`）
- `ValueColor`：数值颜色（默认：`Color.Primary`）
- `ValueClass`：数值CSS类（默认："fw-bold"）
- `ValueStyle`：数值自定义样式
- `TextAlign`：容器对齐方式（默认："center"）
- `Width`：容器宽度（可选）
- `ChildContent`：自定义内容

### 4. 使用建议

- **优先使用通用组件**：避免重复编写相同的 UI 代码
- **保持一致性**：统一的 UI 风格提升用户体验
- **适度定制**：通过参数定制样式，避免过度抽象

## 九、性能优化

- `@key` 标识列表元素
- `ShouldRender()` 控制渲染
- `EventCallback` 替代 `Action`
- `<Virtualize>` 处理长列表
- `CancellationToken` 取消异步操作
