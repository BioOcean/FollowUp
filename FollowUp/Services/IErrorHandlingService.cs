namespace FollowUp.Services;

/// <summary>
/// 错误处理服务接口
/// 统一管理异常处理和用户友好的错误提示
/// </summary>
public interface IErrorHandlingService
{
    /// <summary>
    /// 处理异常并显示用户友好的错误提示
    /// </summary>
    /// <param name="exception">异常对象</param>
    /// <param name="userMessage">用户友好的错误消息（可选）</param>
    /// <param name="logContext">日志上下文信息（可选）</param>
    Task HandleExceptionAsync(Exception exception, string? userMessage = null, string? logContext = null);

    /// <summary>
    /// 显示成功消息
    /// </summary>
    /// <param name="message">成功消息</param>
    Task ShowSuccessAsync(string message);

    /// <summary>
    /// 显示警告消息
    /// </summary>
    /// <param name="message">警告消息</param>
    Task ShowWarningAsync(string message);

    /// <summary>
    /// 显示错误消息
    /// </summary>
    /// <param name="message">错误消息</param>
    Task ShowErrorAsync(string message);

    /// <summary>
    /// 显示信息消息
    /// </summary>
    /// <param name="message">信息消息</param>
    Task ShowInfoAsync(string message);

    /// <summary>
    /// 获取用户友好的错误消息
    /// </summary>
    /// <param name="exception">异常对象</param>
    /// <returns>用户友好的错误消息</returns>
    string GetUserFriendlyMessage(Exception exception);
}

