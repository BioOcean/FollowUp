using Bio.Shared.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace FollowUp.Services.Extension
{
    public static class QrCodeDownloadHelper
    {
        public static async Task<bool> DownloadAsync(
            IWechatService wechatService,
            IJSRuntime jsRuntime,
            NavigationManager navigationManager,
            string sceneKey,
            string sceneValue,
            string fileName,
            bool isXuanWuProject,
            ILogger? logger = null)
        {
            try
            {
                var qrCodeResponse = await wechatService.CreateQrCodeAsync(sceneKey, sceneValue, isXuanWuProject);

                if (qrCodeResponse?.Ticket == null)
                {
                    logger?.LogWarning("生成二维码失败: Ticket 为空，sceneKey={SceneKey}, sceneValue={SceneValue}", sceneKey, sceneValue);
                    return false;
                }

                var fileUrl = await wechatService.GetQrCodeAndSaveAsync(qrCodeResponse.Ticket, fileName);
                var fullUrl = navigationManager.BaseUri + fileUrl;

                await jsRuntime.InvokeVoidAsync("eval", $@"
(function() {{
    const link = document.createElement('a');
    link.href = '{fullUrl}';
    link.download = '{fileName}';
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
}})();
");

                return true;
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "下载二维码失败，sceneKey={SceneKey}, sceneValue={SceneValue}", sceneKey, sceneValue);
                return false;
            }
        }
    }
}
