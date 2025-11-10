# 小程序导航工具使用说明

## 概述

`navigation.js` 提供了带重试机制和错误处理的页面跳转功能，可以有效解决小程序跳转超时问题。

## 主要功能

1. **自动重试机制** - 跳转失败时自动重试指定次数
2. **智能降级** - navigateTo 失败后自动尝试 redirectTo
3. **详细错误处理** - 针对不同错误类型提供具体提示
4. **统一接口** - 简化跳转代码，提高代码复用性

## 使用方法

### 1. 引入工具

```javascript
const navigationUtils = require('../../utils/navigation.js');
```

### 2. 智能跳转（推荐）

```javascript
// 最简单的用法
navigationUtils.smartNavigate('/pages/patientLogin/patientLogin')
  .then(() => {
    console.log('跳转成功');
  })
  .catch((err) => {
    console.error('跳转失败:', err);
  });

// 带参数的跳转
const url = '/pages/patientLogin/patientLogin?fromWhere=dpt_test';
navigationUtils.smartNavigate(url, {
  retryCount: 3,        // 重试次数
  showLoading: true,    // 显示加载提示
  autoFallback: true,   // 自动降级到redirectTo
  showModal: true       // 显示确认对话框
});
```

### 3. 指定跳转方式

```javascript
// 只使用 navigateTo
navigationUtils.navigateToWithRetry('/pages/index/index', 3, true)
  .then(() => console.log('navigateTo 成功'))
  .catch((err) => console.error('navigateTo 失败:', err));

// 只使用 redirectTo
navigationUtils.redirectToWithRetry('/pages/patientLogin/patientLogin', 3, true)
  .then(() => console.log('redirectTo 成功'))
  .catch((err) => console.error('redirectTo 失败:', err));
```

## 在现有页面中的应用示例

### enter.js 页面优化示例

**原始代码：**
```javascript
wx.redirectTo({
  url: `/pages/patientLogin/patientLogin?fromWhere=${fromWhereParam}`,
  success: () => {
    console.log('成功跳转到患者登录页面', fromWhereParam);
  },
  fail: (error) => {
    console.error('跳转到患者登录页面失败', error);
    wx.showToast({
      title: '页面跳转失败',
      icon: 'error',
      duration: 2000
    });
  }
});
```

**优化后代码：**
```javascript
const navigationUtils = require('../../utils/navigation.js');

// 在页面顶部引入工具

// 在跳转逻辑中使用
const url = `/pages/patientLogin/patientLogin?fromWhere=${fromWhereParam}`;
navigationUtils.redirectToWithRetry(url, 3, true)
  .then(() => {
    console.log('成功跳转到患者登录页面', fromWhereParam);
  })
  .catch((error) => {
    console.error('跳转到患者登录页面失败', error);
    // 错误处理已经在工具中统一处理，这里可以添加额外的业务逻辑
  });
```

## 错误类型处理

工具会自动识别以下错误类型并提供相应提示：

- **timeout** - 跳转超时，目标页面加载时间过长
- **url not in app.json** - 页面路径不存在
- **exceed max page stack limit** - 页面栈已满

## 最佳实践

1. **优先使用 smartNavigate** - 它会自动处理大部分跳转场景
2. **合理设置重试次数** - 通常3次重试足够，避免过多重试影响用户体验
3. **根据场景选择跳转方式**：
   - 需要返回的页面使用 navigateTo
   - 替换当前页面使用 redirectTo
4. **在关键跳转处添加日志** - 便于调试和问题排查

## 注意事项

1. 工具已经包含了加载提示和错误处理，无需重复添加
2. 使用 Promise 方式处理结果，便于异步操作
3. 路径会自动格式化，无需手动添加 `/` 前缀
