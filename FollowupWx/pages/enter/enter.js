// pages/enter/enter.js
// 入口页面 - 用于参数转发和页面跳转

// 引入导航工具
const navigationUtils = require('../../utils/navigation.js');

Page({

  /**
   * 页面的初始数据
   */
  data: {

  },

  /**
   * 生命周期函数--监听页面加载
   * @param options 页面参数对象
   */
  onLoad(options) {
    // 获取页面参数f
    const fParam = options.f;
    
    if (fParam) {
      // 组合参数：将f参数前加上"dpt_"前缀
      const fromWhereParam = `dpt_${fParam}`;

      // 构建跳转URL
      const url = `/pages/patientLogin/patientLogin?fromWhere=${fromWhereParam}`;
      console.log('准备跳转到患者登录页面，参数:', fromWhereParam);

      // 使用带重试机制的redirectTo跳转
      navigationUtils.redirectToWithRetry(url, 3, true)
        .then(() => {
          console.log('成功跳转到患者登录页面', fromWhereParam);
        })
        .catch((error) => {
          console.error('跳转到患者登录页面失败', error);
          // 导航工具已经处理了错误提示，这里可以添加额外的业务逻辑
        });
    } else {
      // 如果没有f参数，直接跳转到患者登录页面（不带fromWhere参数）
      const url = '/pages/patientLogin/patientLogin';
      console.log('准备跳转到患者登录页面（无参数）');

      // 使用带重试机制的redirectTo跳转
      navigationUtils.redirectToWithRetry(url, 3, true)
        .then(() => {
          console.log('成功跳转到患者登录页面（无参数）');
        })
        .catch((error) => {
          console.error('跳转到患者登录页面失败', error);
          // 导航工具已经处理了错误提示，这里可以添加额外的业务逻辑
        });
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
   * 用户点击右上角分享
   */
  onShareAppMessage() {

  }
})
