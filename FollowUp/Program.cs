using Bio.Shared.Services;
using Bio.Shared.Interfaces;
using Bio.Models;
using FollowUp.Components;
using FollowUp.Components.Modules.ProjectManagement.Services;
using FollowUp.Components.Modules.PatientManagement.Services;
using FollowUp.Components.Modules.FollowUpManagement.Services;
using FollowUp.Components.Modules.EducationManagement.Services;
using Microsoft.EntityFrameworkCore;
using MudBlazor.Services;
using NLog.Web;
using System.Globalization;
using Bio.Core.Authentication;
using Bio.Core.FormSetDC;
using Bio.Core.Services;
using FollowUp.Services;

var builder = WebApplication.CreateBuilder(args);

var cultureInfo = new CultureInfo("zh-CN");
CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// 配置 Circuit 选项以显示详细错误（开发环境）
builder.Services.Configure<Microsoft.AspNetCore.Components.Server.CircuitOptions>(options =>
{
    if (builder.Environment.IsDevelopment())
    {
        options.DetailedErrors = true;
    }
});

// 注册核心服务
builder.Services.AddScoped<IUserContextService, UserContextService>();
builder.Services.AddScoped<INavigationService, NavigationService>();
builder.Services.AddScoped<FollowUp.Services.IAuthorizationService, FollowUp.Services.AuthorizationService>();
builder.Services.AddScoped<IErrorHandlingService, ErrorHandlingService>(); 
builder.Services.AddScoped<ICacheService, MemoryCacheService>();


// 注册项目管理模块服务
builder.Services.AddProjectManagementServices();
// 注册领域统计服务
builder.Services.AddScoped<IPatientStatisticsService, PatientStatisticsService>();
builder.Services.AddScoped<IFollowupStatisticsService, FollowupStatisticsService>();
builder.Services.AddScoped<IEducationStatisticsService, EducationStatisticsService>();
builder.Services.AddSingleton<ZhiPuQingYanService>();

// 注册随访管理模块服务（任务管理器）
builder.Services.AddScoped<TaskQueryService>();
builder.Services.AddScoped<TaskStatisticsService>();
builder.Services.AddScoped<TaskAuditService>();
builder.Services.AddScoped<TaskCreationService>();
builder.Services.AddScoped<TaskAttachmentService>();

// 注册 Bio.Core 静态数据服务（表单集定义缓存）
builder.Services.AddSingleton<IStaticDataService, StaticDataService>();
builder.Services.AddSingleton<IFormsetStaticDataService, FormsetStaticDataService>();

// 注册 Bio.Core 表单服务（FormSetServiceFactory 等）
builder.Services.AddFormsetServices();
builder.Services.AddSingleton<IFormsetDelegateService, FormsetDelegateService>();
builder.Services.AddScoped<IEntityFactory, EntityFactory>();


// 清除现有的日志提供程序
builder.Logging.ClearProviders();
builder.Logging.SetMinimumLevel(LogLevel.Debug);

// 使用NLog作为日志提供程序
builder.Host.UseNLog();

// 配置
builder.Services.AddSingleton<IConfiguration>(builder.Configuration);

// 配置数据库
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContextFactory<CubeDbContext>(options => options
    .UseNpgsql(connectionString, npgsqlOptions =>
    {
        npgsqlOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
        npgsqlOptions.SetPostgresVersion(new Version(9, 6));
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
    })
    .EnableSensitiveDataLogging(false));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// 添加MudBlazor服务
builder.Services.AddMudServices();

// 添加BootstrapBlazor服务
builder.Services.AddBootstrapBlazor();

// 添加HttpContextAccessor上下文
builder.Services.AddHttpContextAccessor();

// 添加 HttpClient（IWechatService 需要）
builder.Services.AddHttpClient();

// 注册微信服务
builder.Services.AddScoped<Bio.Shared.Interfaces.IWechatService, Bio.Shared.Services.WechatService>();

// Bio.Core认证服务 - 配置JWT和Cookie认证
builder.Services.AddBioAuthentication(builder.Configuration);

//授权服务 - 支持 [Authorize] 特性
builder.Services.AddAuthorization();

// LocalStorageService - 用于在客户端存储用户ID等信息
builder.Services.AddScoped<LocalStorageService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment()) 
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseStaticFiles();

// Bio.Core认证中间件（必须在UseAuthentication之前）
app.UseBioAuthentication();

app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// 使用Bio.Core标准认证端点（登录、刷新、登出）
app.MapBioAuthEndpoints();

// 预加载静态数据（表单集定义等）
var staticDataService = app.Services.GetRequiredService<IStaticDataService>();
await staticDataService.PreloadAllDataAsync();

var formsetStaticDataService = app.Services.GetRequiredService<IFormsetStaticDataService>();
await formsetStaticDataService.PreloadAllDataAsync(staticDataService);

// 预热 Formset 委托缓存，避免首访获取属性失败
var formsetDelegateService = app.Services.GetRequiredService<IFormsetDelegateService>();
await formsetDelegateService.InitializeAsync();

app.Run();
