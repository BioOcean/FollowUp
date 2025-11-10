/**
 * 小程序页面跳转工具类
 * 提供带重试机制和错误处理的页面跳转功能
 */

/**
 * 带重试机制的 navigateTo 跳转
 * @param {string} url 跳转路径
 * @param {number} retryCount 重试次数，默认3次
 * @param {boolean} showLoading 是否显示加载提示，默认true
 * @returns {Promise} 跳转结果Promise
 */
function navigateToWithRetry(url, retryCount = 3, showLoading = true) {
  return new Promise((resolve, reject) => {
    if (showLoading) {
      wx.showLoading({
        title: '跳转中...',
        mask: true
      });
    }

    const attemptNavigate = (currentRetry) => {
      console.log(`尝试 navigateTo 跳转 (第${retryCount - currentRetry + 1}次):`, url);
      
      wx.navigateTo({
        url: url,
        success: (res) => {
          console.log('navigateTo 跳转成功:', url);
          if (showLoading) {
            wx.hideLoading();
          }
          resolve(res);
        },
        fail: (err) => {
          console.error(`navigateTo 跳转失败 (第${retryCount - currentRetry + 1}次):`, err);
          
          if (currentRetry > 0) {
            // 还有重试次数，延迟后重试
            setTimeout(() => {
              attemptNavigate(currentRetry - 1);
            }, 1000);
          } else {
            // 重试次数用完
            if (showLoading) {
              wx.hideLoading();
            }
            reject(err);
          }
        }
      });
    };

    attemptNavigate(retryCount);
  });
}

/**
 * 带重试机制的 redirectTo 跳转
 * @param {string} url 跳转路径
 * @param {number} retryCount 重试次数，默认3次
 * @param {boolean} showLoading 是否显示加载提示，默认true
 * @returns {Promise} 跳转结果Promise
 */
function redirectToWithRetry(url, retryCount = 3, showLoading = true) {
  return new Promise((resolve, reject) => {
    if (showLoading) {
      wx.showLoading({
        title: '跳转中...',
        mask: true
      });
    }

    const attemptRedirect = (currentRetry) => {
      console.log(`尝试 redirectTo 跳转 (第${retryCount - currentRetry + 1}次):`, url);
      
      wx.redirectTo({
        url: url,
        success: (res) => {
          console.log('redirectTo 跳转成功:', url);
          if (showLoading) {
            wx.hideLoading();
          }
          resolve(res);
        },
        fail: (err) => {
          console.error(`redirectTo 跳转失败 (第${retryCount - currentRetry + 1}次):`, err);
          
          if (currentRetry > 0) {
            // 还有重试次数，延迟后重试
            setTimeout(() => {
              attemptRedirect(currentRetry - 1);
            }, 1000);
          } else {
            // 重试次数用完
            if (showLoading) {
              wx.hideLoading();
            }
            reject(err);
          }
        }
      });
    };

    attemptRedirect(retryCount);
  });
}

/**
 * 智能跳转：先尝试 navigateTo，失败后自动尝试 redirectTo
 * @param {string} url 跳转路径
 * @param {Object} options 配置选项
 * @param {number} options.retryCount 重试次数，默认3次
 * @param {boolean} options.showLoading 是否显示加载提示，默认true
 * @param {boolean} options.autoFallback 是否自动降级到redirectTo，默认true
 * @param {boolean} options.showModal 失败时是否显示确认对话框，默认true
 * @returns {Promise} 跳转结果Promise
 */
function smartNavigate(url, options = {}) {
  const {
    retryCount = 3,
    showLoading = true,
    autoFallback = true,
    showModal = true
  } = options;

  return new Promise((resolve, reject) => {
    // 首先尝试 navigateTo
    navigateToWithRetry(url, retryCount, showLoading)
      .then(resolve)
      .catch((navigateError) => {
        console.log('navigateTo 失败，准备处理:', navigateError);
        
        if (autoFallback) {
          if (showModal) {
            // 显示确认对话框
            wx.showModal({
              title: '跳转提示',
              content: 'navigateTo 跳转失败，是否尝试使用 redirectTo 跳转？\n注意：这将替换当前页面',
              confirmText: '确定',
              cancelText: '取消',
              success: (res) => {
                if (res.confirm) {
                  // 用户确认，尝试 redirectTo
                  redirectToWithRetry(url, retryCount, showLoading)
                    .then(resolve)
                    .catch((redirectError) => {
                      showNavigationError(navigateError, redirectError);
                      reject(redirectError);
                    });
                } else {
                  // 用户取消
                  showNavigationError(navigateError);
                  reject(navigateError);
                }
              }
            });
          } else {
            // 直接尝试 redirectTo
            redirectToWithRetry(url, retryCount, showLoading)
              .then(resolve)
              .catch((redirectError) => {
                showNavigationError(navigateError, redirectError);
                reject(redirectError);
              });
          }
        } else {
          // 不自动降级，直接显示错误
          showNavigationError(navigateError);
          reject(navigateError);
        }
      });
  });
}

/**
 * 显示跳转错误信息
 * @param {Object} navigateError navigateTo 错误
 * @param {Object} redirectError redirectTo 错误（可选）
 */
function showNavigationError(navigateError, redirectError) {
  let errorMsg = '跳转失败';
  
  if (navigateError.errMsg.includes('timeout')) {
    errorMsg = '跳转超时，目标页面可能加载时间过长';
  } else if (navigateError.errMsg.includes('url not in app.json')) {
    errorMsg = '页面路径不存在，请检查路径是否正确';
  } else if (navigateError.errMsg.includes('exceed max page stack limit')) {
    errorMsg = '页面栈已满，请返回上级页面后重试';
  } else {
    errorMsg = `跳转失败: ${navigateError.errMsg}`;
  }

  if (redirectError) {
    errorMsg += `\nredirectTo 也失败: ${redirectError.errMsg}`;
  }

  wx.showModal({
    title: '跳转失败',
    content: errorMsg,
    showCancel: false,
    confirmText: '确定'
  });
}

/**
 * 格式化跳转路径，确保以 / 开头
 * @param {string} path 原始路径
 * @returns {string} 格式化后的路径
 */
function formatPath(path) {
  if (!path) return '';
  const trimmedPath = path.trim();
  return trimmedPath.startsWith('/') ? trimmedPath : '/' + trimmedPath;
}

// 导出函数
module.exports = {
  navigateToWithRetry,
  redirectToWithRetry,
  smartNavigate,
  showNavigationError,
  formatPath
};
