// app.js
App({
  onLaunch() {
    // 中文说明：抑制开发者工具中的 reportRealtimeAction 警告
    // 这是微信开发者工具的已知问题，不影响实际功能
    const originalConsoleWarn = console.warn;
    console.warn = function(...args) {
      const message = args.join(' ');
      // 中文说明：过滤掉 reportRealtimeAction 相关的警告信息
      if (message.includes('reportRealtimeAction') || message.includes('not support')) {
        return; // 忽略此类警告
      }
      originalConsoleWarn.apply(console, args);
    };
  },
  globalData: {

  },
  onShow(options) {

  },
})
