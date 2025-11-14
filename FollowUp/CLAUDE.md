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

**所有 Razor 组件必须继承 BasePage**（布局组件和 _Imports.razor 除外）

```razor
@inherits BasePage
```

BasePage 提供：
- `ContextFactory`：数据库上下文工厂
- `context`：当前数据库上下文（自动创建/释放）
- `OnPageInitializedAsync()`：初始化钩子

```csharp
protected override async Task OnPageInitializedAsync()
{
    // context 已自动创建
    var data = await context.YourEntities.ToListAsync();
}
```

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

## 七、性能优化

- `@key` 标识列表元素
- `ShouldRender()` 控制渲染
- `EventCallback` 替代 `Action`
- `<Virtualize>` 处理长列表
- `CancellationToken` 取消异步操作
