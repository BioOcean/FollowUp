// pages/webview/webview.js

import utils from '../../utils/getPageInfo'

// 引入导航工具
const navigationUtils = require('../../utils/navigation.js');

// 引入配置文件
const config = require('../../config.js');

const CAMERA_RETURN_THRESHOLD = 2 * 60 * 1000; // 2分钟，相机返回阈值
const LONG_BACKGROUND_THRESHOLD = 5 * 60 * 1000; // 5分钟，长时间后台阈值

Page({

  /** 
   * 页面的初始数据
   */
  data: {
    baseUrl: wx.getStorageSync('bhglUrl'),
    url: '',
    isDoctor:false,
    showFloatButton: true, // 控制浮动按钮是否显示，默认显示
    buttonTop: 400,
    buttonLeft: 300,
    startX: 0,         // 将用于存储 pageX 的初始值
    startY: 0,         // 将用于存储 pageY 的初始值
    buttonStartLeft: 0,
    buttonStartTop: 0,
    windowHeight: 0,
    windowWidth: 0,
    buttonWidthPx: 0, // 按钮实际宽度 (px)
    buttonHeightPx: 0, // 按钮实际高度 (px)
    webViewReady: false,
    isDragging: false, // 添加一个拖拽状态标志
    hasMoved:false, //标志位判断是否真实发生过移动
    originalSrc:'',
    hideTimestamp: 0, // 记录onHide的时间
    lastActiveTimestamp: 0, // 记录页面最后一次“有效活动”的时间戳
  },
  /**
   * 生命周期函数--监听页面加载
   */
  onLoad(options) {
     // 获取屏幕尺寸
     try {
      const systemInfo = wx.getSystemInfoSync(); // 正确使用
      this.setData({
        windowHeight: systemInfo.windowHeight,
        windowWidth: systemInfo.windowWidth,
      });
    } catch (e) {
      this.setData({
        windowHeight: 600, // Default
        windowWidth: 375,  // Default
      });
    }

    // 检查APP_HOSPITAL配置，如果是'xw'则不显示浮动按钮
    const shouldShowFloatButton = config.APP_HOSPITAL !== 'xw';
    this.setData({
      showFloatButton: shouldShowFloatButton
    });
  var pageParam = utils.getCurrentPageParam();
  var navigatUrl ="";
  var tokenParam = pageParam.accessToken || pageParam.token || '';
  var refreshParam = pageParam.refreshToken || '';

  const appendAccessToken = (url) => {
    let out = url;
    if (tokenParam) out += (out.indexOf('?') >= 0 ? '&' : '?') + 'accessToken=' + encodeURIComponent(tokenParam);
    if (refreshParam) out += '&refreshToken=' + encodeURIComponent(refreshParam);
    return out;
  }

  console.log(`【调试】${this.data.baseUrl}${decodeURIComponent(pageParam.jump)}`)
  
  //直接跳转至指定url
  if(pageParam.jump != undefined){
    //统一跳转到blazor去做页面跳转
    navigatUrl = `${this.data.baseUrl}${decodeURIComponent(pageParam.jump)}`;
    navigatUrl = appendAccessToken(navigatUrl);
    
    // wx.showToast({
    //   title: navigatUrl,
    //   icon: 'none'
    // });
    // return;
  }
  //患者站内信跳转
  else if(pageParam.patientMessage != undefined && pageParam.patientMessage !=''){
    //统一跳转到blazor去做页面跳转
    navigatUrl = `${this.data.baseUrl}navigationPage?openId=${pageParam.openId}&patientMessage=${pageParam.patientMessage}`;
    navigatUrl = appendAccessToken(navigatUrl);
  }
  //患者端
  else if(pageParam.defaultPatientId != undefined){
    //统一跳转到blazor去做页面跳转
    navigatUrl = `${this.data.baseUrl}navigationPage?uniquePatientId=${pageParam.defaultPatientId}&ProjectId=${pageParam.ProjectId}&DoctorId=${pageParam.DoctorId}&FromWhere=${pageParam.fromWhere}&ScanTime=${pageParam.ScanTime}&openId=${pageParam.openId}&code=${pageParam.code}&IsEnrollment=${pageParam.IsEnrollment}`;
    navigatUrl = appendAccessToken(navigatUrl);
   

     //患者入组，点击入院或者出院信息的链接进来的、门诊报到；首次入组并且从公众号菜单进来的;去选择就诊人的页面
    // if(pageParam.fromWhere == 'beHospitalized' || pageParam.fromWhere=='discharge' || pageParam.fromWhere == 'outpatient' || 
    //   (pageParam.fromWhere=='menu' && pageParam.IsEnrollment != undefined)){
    //   this.setData({
    //     url: `${this.data.baseUrl}chooseUniquePatient?uniquePatientId=${pageParam.defaultPatientId}&ProjectId=${pageParam.ProjectId}&DoctorId=${pageParam.DoctorId}&FromWhere=${pageParam.fromWhere}&ScanTime=${pageParam.ScanTime}&openId=${pageParam.openId}`
    //   })
    // }  
    // else{ //其他的暂时去首页，如果有其他入口再增加
    //   this.setData({
    //     url: `${this.data.baseUrl}patientHome?uniquePatientId=${pageParam.defaultPatientId}&isMiniProgram=true&openId=${pageParam.openId}`
    //   })
    // }
  }else{ //医生端
     navigatUrl = `${this.data.baseUrl}FollowupDoctorTerminalHome?DoctorId=${pageParam.DoctorId}&openId=${pageParam.openId}`;
     navigatUrl = appendAccessToken(navigatUrl);
      this.setData({
        isDoctor:true
      })
  }
  const now = Date.now();
  console.log(`调试03:${navigatUrl}`);
  this.setData({
    originalSrc:navigatUrl,
    url:navigatUrl,
    hideTimestamp: 0,
    lastActiveTimestamp: now
  });
  },
  onShow() {
    wx.hideHomeButton();

    const currentTime = Date.now();
     // 如果hideTimestamp为0，说明是首次进入或应用重启
     if (this.data.hideTimestamp === 0) {
      // 检查是否应该重新从登录页开始
      // 如果lastActiveTimestamp也是初始值或者很久之前，认为是应用重启
      const timeSinceLastActive = currentTime - this.data.lastActiveTimestamp;
      
      if (timeSinceLastActive > LONG_BACKGROUND_THRESHOLD) {
        // 应用重启，跳转到登录页
        const targetPage = this.data.isDoctor
          ? '/pages/doctorLogin/doctorLogin'
          : '/pages/patientLogin/patientLogin';

        console.log('应用重启，跳转到登录页:', targetPage);

        // 重置时间戳，防止循环
        this.setData({
          hideTimestamp: 0,
          lastActiveTimestamp: currentTime
        });

        // 使用 reLaunch 重启应用（通常不会超时，但添加错误处理）
        wx.reLaunch({
          url: targetPage,
          success: () => {
            console.log('reLaunch 成功:', targetPage);
          },
          fail: (error) => {
            console.error('reLaunch 失败:', error);
            // 如果 reLaunch 失败，尝试使用 redirectTo
            navigationUtils.redirectToWithRetry(targetPage, 2, true)
              .catch((redirectError) => {
                console.error('redirectTo 也失败了:', redirectError);
              });
          }
        });
        return;
      }
      // 否则是正常的页面跳转，继续正常流程
    } else {
      // hideTimestamp不为0，说明是从后台恢复
      const hideToShowTime = currentTime - this.data.hideTimestamp;
      
      if (hideToShowTime > LONG_BACKGROUND_THRESHOLD) {
        // 长时间后台，需要刷新或重启
        console.log('长时间后台，重新从登录页开始');
        const targetPage = this.data.isDoctor
          ? '/pages/doctorLogin/doctorLogin'
          : '/pages/patientLogin/patientLogin';

        // 重置时间戳，防止循环
        this.setData({
          hideTimestamp: 0,
          lastActiveTimestamp: currentTime
        });

        // 使用 reLaunch 重启应用（通常不会超时，但添加错误处理）
        wx.reLaunch({
          url: targetPage,
          success: () => {
            console.log('reLaunch 成功:', targetPage);
          },
          fail: (error) => {
            console.error('reLaunch 失败:', error);
            // 如果 reLaunch 失败，尝试使用 redirectTo
            navigationUtils.redirectToWithRetry(targetPage, 2, true)
              .catch((redirectError) => {
                console.error('redirectTo 也失败了:', redirectError);
              });
          }
        });
        return;
      } else if (hideToShowTime > CAMERA_RETURN_THRESHOLD) {
        // 中等时间后台，刷新当前页面
        console.log('中等时间后台，刷新webview');
        this.setData({
          url: this.data.originalSrc + '&t=' + currentTime
        });
      } else {
        // 短时间后台，疑似相机返回，不刷新
        console.log('短时间后台，疑似相机返回，不刷新');
      }
    }
    
    // 更新最后活跃时间，重置hideTimestamp
    this.setData({
      lastActiveTimestamp: currentTime,
      hideTimestamp: 0
    });
  },

  onHide(){
    this.setData({
      hideTimestamp: Date.now()
    });
  },

  onReady() {
     // 获取按钮的实际尺寸 (px)
    // 建议在onReady中执行，确保元素已渲染，但对于cover-view，onLoad中通常也可以
    if(!this.data.isDoctor){
    const query = wx.createSelectorQuery().in(this); // 确保在当前页面或组件内查询
    query.select('.float-button').boundingClientRect((rect) => {
      if (rect) {
        this.setData({
          buttonWidthPx: rect.width,
          buttonHeightPx: rect.height,
          // 获取到按钮尺寸后，可以更精确地设置初始位置（例如右下角）
          buttonLeft: this.data.windowWidth - rect.width - 20, // 20px 边距
          buttonTop: this.data.windowHeight - rect.height - 100 // 100px 边距 (避开底部导航等)
        });
      } else {
        // 如果失败，使用一个基于rpx估算的px值，或者默认值
        // 假设平均 1px = 2rpx
        const defaultButtonSize = 50; // 100rpx / 2
        this.setData({
          buttonWidthPx: defaultButtonSize,
          buttonHeightPx: defaultButtonSize,
          buttonLeft: this.data.windowWidth - defaultButtonSize - 20,
          buttonTop: this.data.windowHeight - defaultButtonSize - 100
        });
      }
    }).exec();
  }
  },

  // web-view加载完成事件
  onWebViewLoad(e) {
    this.setData({ webViewReady: true });
  },
 
  // 按钮拖拽开始
  buttonStart: function(e) {
    if (!e.touches[0]) return;
    this.setData({
       // 使用 pageX 和 pageY
       startX: e.touches[0].pageX,
       startY: e.touches[0].pageY,
       buttonStartLeft: this.data.buttonLeft,
       buttonStartTop: this.data.buttonTop,
       isDragging: true,
       hasMoved: false
    });
  },

  // 按钮拖动中
  buttonMove: function(e) {
    if (!this.data.isDragging || !e.touches[0]){
      return;
    } 
     // 使用 pageX 和 pageY
     const currentX = e.touches[0].pageX;
     const currentY = e.touches[0].pageY;

    const moveX = currentX - this.data.startX;
    const moveY = currentY - this.data.startY;
    // 仅当实际移动超过一个小阈值时才认为是有效移动
    // 这可以帮助区分轻微抖动和真实拖拽意图，有时能改善安卓上的表现
    if (Math.abs(moveX) > 2 || Math.abs(moveY) > 2) {
      this.setData({ hasMoved: true });
  }
    let newLeft = this.data.buttonStartLeft + moveX;
    let newTop = this.data.buttonStartTop + moveY;
    // 拖动时不进行边界检查，让用户可以拖出一点，松手时再吸附
    this.setData({
      buttonLeft: newLeft,
      buttonTop: newTop
    });
  },

  // 拖拽结束
  buttonMoveEnd: function() {
    if (!this.data.isDragging) return; // 如果不是拖拽状态（例如只是点击），则不执行

    let currentLeft = this.data.buttonLeft;
    let currentTop = this.data.buttonTop;
    const buttonWidth = this.data.buttonWidthPx;
    const buttonHeight = this.data.buttonHeightPx;
    const windowWidth = this.data.windowWidth;
    const windowHeight = this.data.windowHeight;

    const effectiveButtonWidth = buttonWidth || 50; // 使用获取到的值，否则用50px保底
    const effectiveButtonHeight = buttonHeight || 50; // 使用获取到的值，否则用50px保底
    // 垂直方向边界限制 (按钮不能超出屏幕)
    // 最小top为0，最大top为屏幕高度 - 按钮高度
    currentTop = Math.max(0, currentTop);
    currentTop = Math.min(currentTop, windowHeight - effectiveButtonHeight);
    
    // 水平方向贴边逻辑
    // 判断按钮中心点在哪一侧
    if ((currentLeft + effectiveButtonWidth / 2) < windowWidth / 2) {
      currentLeft = 10; // 贴左边，留10px边距
    } else {
      currentLeft = windowWidth - effectiveButtonWidth - 10; // 贴右边，留10px边距
    }
    this.setData({
      buttonLeft: currentLeft,
      buttonTop: currentTop,
      isDragging: false // 真正结束拖拽状态
    });
  },

  // 点击事件，需要与拖拽区分
  handleButtonTap: function() {
    // 关键：如果手指有明显移动，则判断为拖拽，不执行点击事件
    if (this.data.hasMoved) {
      return; 
    }
    this.openCustomerService();
  },

//人工客服跳转
  openCustomerService: function() {
    wx.openCustomerServiceChat({
        extInfo: {
            url: `https://work.weixin.qq.com/kfid/kfc150e846aa131eab9`
        },
        corpId: 'wwa271d452a19b4dc0',
        success: (res) => {  },
        fail: (err) => { }
    });
  },

  // 添加webview消息监听,  //没调出来，未知原因接收不到
  onMessage(e) { 
    if (Array.isArray(e.detail.data) && e.detail.data.length > 0) {
      const message = e.detail.data[0];
      if (message && message.action === 'openCustomerService') {
        this.openCustomerService();
      }
    }
  }
  
})