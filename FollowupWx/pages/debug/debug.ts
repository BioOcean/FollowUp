// pages/debug/debug.ts
// 调试页面：提供输入小程序路径并跳转的能力

// 引入导航工具（带重试与错误提示）
// 中文说明：navigationUtils提供了格式化路径、智能跳转等方法
const navigationUtils = require('../../utils/navigation.js');

Page({
  /**
   * 页面的初始数据
   * 中文说明：这里保存用户在输入框中填写的跳转路径
   */
  data: {
    // 页签相关
    activeTab: 'pageNav', // 当前激活的页签，pageNav: 页面跳转，miniProgramNav: 小程序跳转

    // 页面跳转页签数据
    inputPath: '', // 中文说明：用户输入的小程序页面路径（可包含查询参数）

    // 小程序跳转页签数据
    appId: '', // 中文说明：目标小程序的AppID
    miniProgramPath: '', // 中文说明：目标小程序的页面路径
    envVersion: '' // 中文说明：小程序环境版本，可选值：develop（开发版）、trial（体验版）、release（正式版）
  },

  /**
   * 页签切换事件
   * 中文说明：切换调试页签（页面跳转/小程序跳转）
   * @param e 事件对象，包含切换的页签标识
   */
  onTabChange(e: any) {
    // 中文说明：从事件对象中取出页签标识
    const tab: string = (e && e.currentTarget && e.currentTarget.dataset && e.currentTarget.dataset.tab) ? e.currentTarget.dataset.tab : 'pageNav';
    // 中文说明：更新当前激活的页签
    this.setData({ activeTab: tab });
  },

  /**
   * 页面跳转输入框内容变更事件
   * 中文说明：当用户在页面跳转输入框输入内容时，更新data中的inputPath
   * @param e 事件对象，包含输入框的最新值
   */
  onPathInput(e: any) {
    // 中文说明：从事件对象中取出最新输入值
    const value: string = (e && e.detail && e.detail.value) ? e.detail.value : '';
    // 中文说明：更新页面数据，触发视图层刷新
    this.setData({ inputPath: value });
  },

  /**
   * 小程序AppID输入框内容变更事件
   * 中文说明：当用户在AppID输入框输入内容时，更新data中的appId
   * @param e 事件对象，包含输入框的最新值
   */
  onAppIdInput(e: any) {
    // 中文说明：从事件对象中取出最新输入值
    const value: string = (e && e.detail && e.detail.value) ? e.detail.value : '';
    // 中文说明：更新页面数据，触发视图层刷新
    this.setData({ appId: value });
  },

  /**
   * 小程序路径输入框内容变更事件
   * 中文说明：当用户在小程序路径输入框输入内容时，更新data中的miniProgramPath
   * @param e 事件对象，包含输入框的最新值
   */
  onMiniProgramPathInput(e: any) {
    // 中文说明：从事件对象中取出最新输入值
    const value: string = (e && e.detail && e.detail.value) ? e.detail.value : '';
    // 中文说明：更新页面数据，触发视图层刷新
    this.setData({ miniProgramPath: value });
  },

  /**
   * 环境版本选择器变更事件
   * 中文说明：当用户选择环境版本时，更新data中的envVersion
   * @param e 事件对象，包含选择器的最新值
   */
  onEnvVersionChange(e: any) {
    // 中文说明：从事件对象中取出最新选择值
    const value: string = (e && e.detail && e.detail.value) ? e.detail.value : 'release';
    // 中文说明：更新页面数据，触发视图层刷新
    this.setData({ envVersion: value });
  },

  /**
   * 页面跳转按钮点击事件
   * 中文说明：读取输入的路径，进行格式化校验后发起页面跳转
   */
  onNavigate() {
    // 中文说明：原始输入路径
    const rawPath: string = (this.data as any).inputPath || '';
    // 中文说明：去除首尾空格的路径
    const trimmedPath: string = rawPath.trim();

    // 中文说明：校验是否填写了路径
    if (!trimmedPath) {
      wx.showToast({ title: '请先输入路径', icon: 'none' });
      return;
    }

    // 中文说明：格式化路径，确保以'/'开头，便于小程序识别，例如pages/index/index -> /pages/index/index
    const url: string = navigationUtils.formatPath(trimmedPath);

    // 中文说明：使用智能跳转（优先navigateTo，失败自动尝试redirectTo），并内置重试与错误提示
    navigationUtils
      .smartNavigate(url, { retryCount: 2, showLoading: true, autoFallback: true, showModal: false })
      .then(() => {
        // 中文说明：跳转成功回调，可在此添加打点或提示
        console.log('调试跳转成功 ->', url);
      })
      .catch((err: any) => {
        // 中文说明：错误已在导航工具内处理（当showModal为false且重试后仍失败，会在工具内弹窗提示）
        console.error('调试跳转失败 ->', url, err);
      });
  },

  /**
   * 小程序跳转按钮点击事件
   * 中文说明：读取输入的小程序信息，调用wx.navigateToMiniProgram进行小程序跳转
   */
  onNavigateToMiniProgram() {
    // 中文说明：获取输入的小程序信息
    const data: any = this.data;
    const appId: string = (data.appId || '').trim();
    const path: string = (data.miniProgramPath || '').trim();
    const envVersion: string = data.envVersion || 'release';

    // 中文说明：校验AppID是否填写
    if (!appId) {
      wx.showToast({ title: '请输入目标小程序AppID', icon: 'none' });
      return;
    }

    // 中文说明：校验路径是否填写
    if (!path) {
      wx.showToast({ title: '请输入目标小程序页面路径', icon: 'none' });
      return;
    }

    // 中文说明：显示加载提示
    wx.showLoading({ title: '跳转中...' });

    // 中文说明：调用小程序跳转API
    wx.navigateToMiniProgram({
      appId: appId,  // 目标小程序的AppID
      path: path.startsWith('/') ? path : '/' + path,  // 目标小程序的页面路径，确保以'/'开头
      envVersion: envVersion,  // 小程序环境版本
      success(res: any) {
        // 中文说明：跳转成功回调
        wx.hideLoading();
        console.log('小程序跳转成功', res);
        wx.showToast({ title: '跳转成功', icon: 'success' });
      },
      fail(err: any) {
        // 中文说明：跳转失败回调
        wx.hideLoading();
        console.error('小程序跳转失败', err);
        wx.showModal({
          title: '跳转失败',
          content: `错误信息：${err.errMsg || '未知错误'}`,
          showCancel: false
        });
      }
    });
  },

  /**
   * 生命周期函数--监听页面加载
   * 中文说明：页面加载时的生命周期钩子，这里可根据需要设置默认示例路径
   */
  onLoad() {
    // 中文说明：可设置一个示例路径，便于快速测试
    // this.setData({ inputPath: 'pages/enter/enter?f=xw' });
  },

  /**
   * 生命周期函数--监听页面初次渲染完成
   */
  onReady() {
    // 中文说明：页面首次渲染完成的回调
  },

  /**
   * 生命周期函数--监听页面显示
   */
  onShow() {
    // 中文说明：页面显示时的回调
  },

  /**
   * 生命周期函数--监听页面隐藏
   */
  onHide() {
    // 中文说明：页面隐藏时的回调
  },

  /**
   * 生命周期函数--监听页面卸载
   */
  onUnload() {
    // 中文说明：页面卸载时的回调
  },

  /**
   * 页面相关事件处理函数--监听用户下拉动作
   */
  onPullDownRefresh() {
    // 中文说明：处理下拉刷新逻辑（本页面暂无相关逻辑）
  },

  /**
   * 页面上拉触底事件的处理函数
   */
  onReachBottom() {
    // 中文说明：处理触底加载逻辑（本页面暂无相关逻辑）
  },

  /**
   * 用户点击右上角分享
   */
  onShareAppMessage() {
    // 中文说明：配置页面分享内容（可按需实现）
  }
});