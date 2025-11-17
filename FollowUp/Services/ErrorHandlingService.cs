using MudBlazor;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace FollowUp.Services;

/// <summary>
/// 错误处理服务实现
/// 极简实现：统一异常处理和用户友好的错误提示
/// </summary>
public class ErrorHandlingService : IErrorHandlingService
{
    private readonly ISnackbar _snackbar;
    private readonly ILogger<ErrorHandlingService> _logger;

    public ErrorHandlingService(
        ISnackbar snackbar,
        ILogger<ErrorHandlingService> logger)
    {
        _snackbar = snackbar;
        _logger = logger;
    }

    public async Task HandleExceptionAsync(Exception exception, string? userMessage = null, string? logContext = null)
    {
        // 记录日志
        var context = string.IsNullOrWhiteSpace(logContext) ? "操作失败" : logContext;
        _logger.LogError(exception, "{Context}: {Message}", context, exception.Message);

        // 显示用户友好的错误消息
        var message = userMessage ?? GetUserFriendlyMessage(exception);
        await ShowErrorAsync(message);
    }

    public Task ShowSuccessAsync(string message)
    {
        _snackbar.Add(message, Severity.Success);
        return Task.CompletedTask;
    }

    public Task ShowWarningAsync(string message)
    {
        _snackbar.Add(message, Severity.Warning);
        return Task.CompletedTask;
    }

    public Task ShowErrorAsync(string message)
    {
        _snackbar.Add(message, Severity.Error);
        return Task.CompletedTask;
    }

    public Task ShowInfoAsync(string message)
    {
        _snackbar.Add(message, Severity.Info);
        return Task.CompletedTask;
    }

    public string GetUserFriendlyMessage(Exception exception)
    {
        return exception switch
        {
            // 数据库相关异常
            DbUpdateException dbEx when dbEx.InnerException is PostgresException pgEx => pgEx.SqlState switch
            {
                "23505" => "数据已存在，请检查后重试",
                "23503" => "无法删除，存在关联数据",
                "23502" => "必填字段不能为空",
                "23514" => "数据格式不正确",
                _ => "数据库操作失败，请稍后重试"
            },
            DbUpdateConcurrencyException => "数据已被其他用户修改，请刷新后重试",
            DbUpdateException => "保存数据失败，请检查输入后重试",

            // PostgreSQL 异常
            PostgresException pgEx => pgEx.SqlState switch
            {
                "08000" => "数据库连接失败，请检查网络",
                "08003" => "数据库连接已断开，请重试",
                "08006" => "数据库连接失败，请稍后重试",
                "57P03" => "数据库正在启动，请稍后重试",
                "53300" => "数据库资源不足，请联系管理员",
                _ => "数据库错误，请稍后重试"
            },

            // 网络相关异常
            HttpRequestException => "网络请求失败，请检查网络连接",
            TaskCanceledException => "操作超时，请重试",
            TimeoutException => "操作超时，请重试",

            // 参数异常
            ArgumentNullException => "参数不能为空",
            ArgumentException => "参数格式不正确",

            // 未授权异常
            UnauthorizedAccessException => "您没有权限执行此操作",

            // 文件相关异常
            FileNotFoundException => "文件不存在",
            DirectoryNotFoundException => "目录不存在",
            IOException => "文件操作失败，请重试",

            // 默认消息
            _ => "操作失败，请稍后重试"
        };
    }
}

