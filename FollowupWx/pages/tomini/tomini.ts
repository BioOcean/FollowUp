// pages/tomini/tomini.ts
// 小程序跳转页面：用户确认后跳转到目标小程序

const api = require("../../utils/api");

Page({
  /**
   * 页面的初始数据
   */
  data: {
    // 跳转参数
    appId: '', // 目标小程序AppID
    path: '', // 目标小程序页面路径
    envVersion: 'release', // 环境版本（develop/trial/release）
    description: '', // 跳转目标描述

    // 页面状态
    isLoading: false, // 是否正在跳转中
    errorMessage: '', // 错误信息
    showError: false, // 是否显示错误信息
    showJumpButton: false, // 是否显示跳转按钮
    hasParams: false, // 是否已获取到跳转参数
  },

  /**
   * 生命周期函数--监听页面加载
   * 解析URL参数并准备跳转信息，等待用户确认
   */
  onLoad(options: any) {
    console.log('tomini页面加载，接收参数：', options);

    // 检查是否有key参数，如果有则从服务器获取参数
    if (options.a) {
      console.log('tomini页面加载，接收参数：options.a');
      this.loadParamsFromServer(options.a);
    } 
  },

  /**
   * 生命周期函数--监听页面初次渲染完成
   */
  onReady() {

  },

  /**
   * 生命周期函数--监听页面显示
   */
  onShow() {

  },

  /**
   * 生命周期函数--监听页面隐藏
   */
  onHide() {

  },

  /**
   * 生命周期函数--监听页面卸载
   */
  onUnload() {

  },

  /**
   * 页面相关事件处理函数--监听用户下拉动作
   */
  onPullDownRefresh() {

  },

  /**
   * 页面上拉触底事件的处理函数
   */
  onReachBottom() {

  },

  /**
   * 手动跳转到目标小程序
   * 用户点击确认跳转按钮后执行wx.navigateToMiniProgram API进行跳转
   */
  manualNavigateToMiniProgram() {
    const data: any = this.data;

    // 检查必要参数
    if (!data.appId || !data.path) {
      console.error('跳转参数不完整，无法执行跳转');
      return;
    }

    // 设置加载状态
    this.setData({ isLoading: true, showError: false });

    // 显示跳转提示
    wx.showLoading({ title: '正在跳转...' });

    // 执行小程序跳转
    wx.navigateToMiniProgram({
      appId: data.appId,
      path: data.path.startsWith('/') ? data.path : '/' + data.path, // 确保路径以'/'开头
      envVersion: data.envVersion,
      success: (res: any) => {
        // 跳转成功
        wx.hideLoading();
        this.setData({ isLoading: false });
        console.log('小程序跳转成功', res);
        wx.showToast({
          title: '跳转成功',
          icon: 'success',
          duration: 2000
        });
      },
      fail: (err: any) => {
        // 跳转失败
        wx.hideLoading();
        this.setData({ isLoading: false });
        console.error('小程序跳转失败', err);
        this.showError(`跳转失败：${err.errMsg || '未知错误'}`);
      }
    });
  },

  /**
   * 显示错误信息
   * @param message 错误信息内容
   */
  showError(message: string) {
    this.setData({
      errorMessage: message,
      showError: true,
      isLoading: false
    });

    // 显示错误弹窗
    wx.showModal({
      title: '跳转失败',
      content: message,
      showCancel: false,
      confirmText: '确定'
    });
  },

  /**
   * 从服务器加载参数
   * 通过key从服务器获取跳转参数
   * @param key 缓存键
   */
  loadParamsFromServer(key: string) {
    console.log('从服务器加载参数，key：', key);

    // 设置加载状态
    this.setData({ isLoading: true, showError: false, showJumpButton: false });
    wx.showLoading({ title: '加载参数中...' });

    const param = {
      key : key
    }

    var that = this;
    // 使用统一的API调用方式
    api.fn_GetCache(param,
      function(result){
        wx.hideLoading();
        if(result.success){
          console.log('tomini 返回Cache参数：', result);
          // 保存跳转参数
          that.setData({
            appId: result.data.appid,
            path: result.data.path,
            envVersion: result.data.env,
            description: result.data.desc,
            isLoading: false,
            hasParams: true,
            showJumpButton: true
          });
        } else {
          // 获取参数失败
          that.setData({
            isLoading: false,
            errorMessage: '获取跳转参数失败',
            showError: true
          });
          wx.showModal({
            title: '获取失败',
            content: '无法获取跳转参数，请重试',
            showCancel: false,
            confirmText: '确定'
          });
        }
    },function(){
      wx.hideLoading();
      console.log("接口请求失败");
      that.setData({
        isLoading: false,
        errorMessage: '网络请求失败',
        showError: true
      });
      wx.showModal({
        title: '网络错误',
        content: '网络请求失败，请检查网络连接',
        showCancel: false,
        confirmText: '确定'
      });
    });
  },

  /**
   * 用户点击确认跳转按钮
   * 执行手动跳转到目标小程序
   */
  onConfirmJump() {
    if (!this.data.hasParams) {
      wx.showModal({
        title: '参数缺失',
        content: '跳转参数未准备好，请稍后再试',
        showCancel: false,
        confirmText: '确定'
      });
      return;
    }
    this.manualNavigateToMiniProgram();
  },

  /**
   * 重试跳转
   * 用户手动触发重新跳转
   */
  onRetryJump() {
    this.setData({ showError: false });
    this.manualNavigateToMiniProgram();
  },

  /**
   * 用户点击右上角分享
   */
  onShareAppMessage() {
    // 不支持分享
  }
})