// pages/patientMessage/patientMessage.js
const config = require('../../config');
import Toast from '@vant/weapp/toast/toast';

Page({

  /**
   * 页面的初始数据
   */
  data: {
    openId: '',
    isProcessing: false,
    authStep: 'waiting', // waiting, requesting, complete
    showCountdown: false,
    countdown: 3,
    subscribeResults: {},
    
    // 订阅消息配置
    templateIds: []
  },

  /**
   * 生命周期函数--监听页面加载
   */
  onLoad(options) {
    console.log('patientMessage页面参数:', options);
    
    // 获取openId参数
    if (options.openId) {
      this.setData({
        openId: options.openId
      });
    }
    
    // 初始化订阅消息模板
    this.initSubscribeTemplates();
    
    // 设置页面标题
    wx.setNavigationBarTitle({
      title: '消息订阅授权'
    });
  },

  /**
   * 初始化订阅消息模板
   */
  initSubscribeTemplates() {
    const templateIds = [config.UNREAD_MESSAGE_TEMPLATE];
    
    this.setData({
      templateIds
    });
  },

  /**
   * 处理同意授权
   */
  handleAccept() {
    console.log('用户同意授权');
    
    const { templateIds } = this.data;
    if (templateIds.length === 0) {
      console.log('没有配置模板ID，直接跳转');
      this.navigateToWebview();
      return;
    }
    
    this.setData({
      isProcessing: true,
      authStep: 'requesting'
    });
    
    // 请求订阅消息授权
    wx.requestSubscribeMessage({
      tmplIds: templateIds,
      success: (res) => {
        console.log('订阅授权结果:', res);
        this.handleAuthSuccess(res);
      },
      fail: (err) => {
        console.error('订阅授权失败:', err);
        this.handleAuthFailed(err);
      }
    });
  },

  /**
   * 处理拒绝授权
   */
  handleReject() {
    console.log('用户拒绝授权');
    
    wx.showModal({
      title: '确认拒绝',
      content: '拒绝授权可能无法接收重要的健康提醒，确定要拒绝吗？',
      success: (res) => {
        if (res.confirm) {
          // 保存拒绝状态
          this.saveSubscribeStatus({ rejected: true });
          
          Toast({
            type: 'none',
            message: '您已拒绝消息授权',
            duration: 2000
          });
          
          // 延迟跳转
          setTimeout(() => {
            this.navigateToWebview();
          }, 2000);
        }
      }
    });
  },

  /**
   * 处理授权成功
   */
  handleAuthSuccess(results) {
    const isAuthorized = results[config.UNREAD_MESSAGE_TEMPLATE] === 'accept';
    
    this.setData({
      subscribeResults: results,
      authStep: 'complete'
    });
    
    // 保存授权状态
    this.saveSubscribeStatus(results);
    
    // 显示结果
    if (isAuthorized) {
      Toast({
        type: 'success',
        message: '成功订阅消息通知',
        duration: 2000
      });
    } else {
      Toast({
        type: 'none',
        message: '您未授权消息通知',
        duration: 2000
      });
    }
    
    // 开始倒计时跳转
    this.startCountdownAndNavigate();
  },

  /**
   * 处理授权失败
   */
  handleAuthFailed(err) {
    console.error('授权失败:', err);
    
    this.setData({
      isProcessing: false,
      authStep: 'waiting'
    });
    
    Toast({
      type: 'fail',
      message: '授权请求失败，请重试',
      duration: 2000
    });
  },

  /**
   * 开始倒计时并跳转
   */
  startCountdownAndNavigate() {
    this.setData({
      showCountdown: true,
      countdown: 3
    });
    
    const timer = setInterval(() => {
      const newCountdown = this.data.countdown - 1;
      
      if (newCountdown <= 0) {
        clearInterval(timer);
        this.navigateToWebview();
      } else {
        this.setData({
          countdown: newCountdown
        });
      }
    }, 1000);
  },

  /**
   * 保存订阅状态
   */
  saveSubscribeStatus(results) {
    try {
      const subscribeData = {
        timestamp: Date.now(),
        results: results,
        templateIds: this.data.templateIds,
        openId: this.data.openId
      };
      wx.setStorageSync('patientSubscribeAuth', subscribeData);
    } catch (error) {
      console.error('保存授权记录失败:', error);
    }
  },

  /**
   * 跳转到webview页面
   */
  navigateToWebview() {
    const { openId } = this.data;
    console.log(`跳转到webview页面前 openId:${openId}`);
    
    wx.reLaunch({
      url: `../webview/webview?patientMessage=0&openId=${openId}`,
    });
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
   * 用户点击右上角分享
   */
  onShareAppMessage() {

  }
})