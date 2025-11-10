// pages/patientLogin/patientLogin.js
// 这是一个登录页面的组件，用于患者登录
const config = require('../../config');
var realapi = require('../../utils/api')
import Toast from '@vant/weapp/toast/toast'

Component({
  properties: {
    isPatient: {
      type: Boolean,
      value: true
    },
    ProjectId:{
      type:String,
      value:''
    },
    DoctorId:{
      type:String,
      value:''
    },
    fromWhere:{
      type:String,
      value:''
    },
    IsEnrollment:{
      type:Boolean,
      value:'false'
    },
    ScanTime:{
      type:String,
      value:''
    },
    code:{
      type:String,
      value:''
    },
    patientMessage:{
      type:String,
      value:''
    }
  },

  /**
   * 组件的初始数据
   */
  data: {
    loginType: 'verifyCode', // 'verifyCode' 或 'password'
    phoneNumber: '',
    verifyCode: '',
    password: '',
    showPassword: false,
    agreeToTerms: false,
    errorMessage: '',
    phoneError: '',
    isCountingDown: false,
    countdown: 60,
    isPhoneValid:false,
    wxUserInfo:{},
    
    // 新增：订阅消息相关数据
    isProcessingAuth: false, // 是否正在处理授权流程
    subscribeStatus: {
      requested: false,    // 是否已请求订阅
      authorized: false,   // 是否已授权
      templateIds: []      // 模板ID列表
    },
    authStep: 'phone', // 当前授权步骤：phone -> subscribe -> complete
    
    // 新增：按钮文字状态
    authButtonText: '一键登录', // 直接用数据而不是方法
    
    // 新增：弹窗控制
    showSubscribeModal: false,
    pendingPhoneEvent: null,     // 暂存手机号授权事件
    pendingLoginAction: null,    // 暂存登录动作类型：'phone' 或 'password'
  },

  /**
   * 组件生命周期函数--监听组件加载
   */
  attached() {
    // 初始化订阅消息模板ID
    this.initSubscribeTemplates();
  },

  /**
   * 组件的方法列表
   */
  methods: {
    // 新增：初始化订阅消息模板
    initSubscribeTemplates() {
      // 根据你的业务需求配置模板ID
      const templateIds = [
        config.UNREAD_MESSAGE_TEMPLATE,      // 未读消息通知模板
      ];
      
      this.setData({
        'subscribeStatus.templateIds': templateIds
      });
    },

    // 新增：检查订阅状态（通过API）
    async checkSubscribeStatus() {
      try {
        const openId = wx.getStorageSync('openid');
        if (!openId) {
          console.log('openId不存在，跳过订阅检查');
          return false;
        }

        const param = { weixin_id: openId };
        const result = await new Promise((resolve, reject) => {
          realapi.fn_HasSubscription(param, resolve, reject);
        });

        if (result.success) {
          return result.hasSubscription;
        } else {
          console.error('查询订阅状态失败:', result.message);
          return false;
        }
      } catch (error) {
        console.error('检查订阅状态失败:', error);
        return false;
      }
    },

    // 新增：保存订阅状态到API
    async saveSubscribeStatusToAPI(subscribeResult) {
      try {
        const openId = wx.getStorageSync('openid');
        if (!openId) {
          console.error('缺少openId，无法保存订阅状态');
          return;
        }

        // 检查是否有模板被授权
        const templateIds = this.data.subscribeStatus.templateIds;
        const authorizedTemplates = templateIds.filter(function(id) {
          return subscribeResult[id] === 'accept';
        });
        const isSubscribed = authorizedTemplates.length > 0;

        const param = {
          weixin_id: openId,
          is_subscribe: isSubscribed
        };

        // 不需要保存至服务器
        // const result = await new Promise((resolve, reject) => {
        //   realapi.fn_SetSubscription(param, resolve, reject);
        // });

        // if (result.success) {
        //   console.log('订阅状态设置成功:', isSubscribed);
        // } else {
        //   console.error('设置订阅状态失败:', result.message);
        // }
      } catch (error) {
        console.error('保存订阅状态失败:', error);
      }
    },

    // 切换登录方式
    switchLoginType(e) {
      const type = e.currentTarget.dataset.type;
      this.setData({
        loginType: type,
        password:'',
        verifyCode:'',
        errorMessage: ''
      });
    },

    // 处理手机号输入
    handlePhoneInput(e) {
      const phone = e.detail.value;
      let phoneError = '';
      let isPhoneValid = false;
      if (!phone) {
        phoneError = '请输入手机号';
      } else if (!/^1[3-9]\d{9}$/.test(phone)) {
        phoneError = '请输入正确的手机号';
      }else{
        isPhoneValid = true;
      }

      this.setData({
        phoneNumber: phone,
        phoneError,
        isPhoneValid
      });
    },

    // 处理验证码输入
    handleVerifyCodeInput(e) {
      this.setData({
        verifyCode: e.detail.value,
        errorMessage: ''
      });
    },

    // 处理密码输入
    handlePasswordInput(e) {
      this.setData({
        password: e.detail.value,
        errorMessage: ''
      });
    },

    // 切换密码可见性
    togglePassword() {
      this.setData({
        showPassword: !this.data.showPassword
      });
    },

    // 处理用户协议勾选
    handleAgreementChange(e) {
      this.setData({
        agreeToTerms: e.detail.value.length > 0
      });
    },

    // 验证手机号格式
    isPhoneValid() {
      return /^1[3-9]\d{9}$/.test(this.data.phoneNumber);
    },

    // 发送验证码
    sendVerifyCode() {
      if (!this.isPhoneValid() || this.data.isCountingDown) {
        return;
      }
      try {
        var param={
          phone:this.data.phoneNumber
        };
        var that = this;
        // 调用发送验证码API
        realapi.fn_SendPhoneVerificationCode(param,
          function(result){
            if(result.success){
               // 开始倒计时
               that.setData({
                isCountingDown: true,
                countdown: 60
              });
              const timer = setInterval(() => {
                if (that.data.countdown <= 1) {
                  clearInterval(timer);
                  that.setData({
                    isCountingDown: false
                  });
                } else {
                  that.setData({
                    countdown: that.data.countdown - 1
                  });
                }
              }, 1000);
            }else{
              that.setData({
                errorMessage: result.message
              });
            }
          },function(err){
            console.error("API Error:", err);
            that.setData({
              errorMessage: '发送验证码请求失败'
            });
          });
      } catch (error) {
        console.error("Sync Error:", error);
        this.setData({
          errorMessage: '发送验证码失败，请稍后重试'
        });
      }
    },

    // 修改：普通登录处理 - 先通过API检查订阅状态
    async handleLogin() {
      // 验证表单
      if (!this.validateForm()) {
        return;
      }

      try {
        // 先检查订阅状态
        const hasSubscription = await this.checkSubscribeStatus();
        if (hasSubscription) {
          // 已订阅，直接登录
          this.executeLoginAction(this.data.loginType);
        } else {
          // 未订阅，显示订阅确认弹窗
          this.setData({
            showSubscribeModal: true,
            pendingLoginAction: this.data.loginType,
            pendingPhoneEvent: null
          });
        }
      } catch (error) {
        console.error('检查订阅状态失败，继续登录流程:', error);
        // 检查失败，也显示订阅弹窗以确保用户体验
        this.setData({
          showSubscribeModal: true,
          pendingLoginAction: this.data.loginType,
          pendingPhoneEvent: null
        });
      }
    },

    // 修改：微信登录处理 - 先通过API检查订阅状态
    async handleWeixinLogin(e) {
      if (this.data.isProcessingAuth) {
        return;
      }

      // 检查是否有手机号授权数据
      if (!e.detail || !e.detail.code) {
        Toast({
          type: 'fail',
          message: '获取手机号失败，请重试',
          duration: 2000
        });
        return;
      }

      try {
        // 先检查订阅状态
        const hasSubscription = await this.checkSubscribeStatus();
        if (hasSubscription) {
          // 已订阅，直接进行手机号登录
          this.processPhoneAuth(e);
        } else {
          // 未订阅，暂存手机号授权事件，显示订阅确认弹窗
          this.setData({
            pendingPhoneEvent: e,
            pendingLoginAction: 'phone',
            showSubscribeModal: true
          });
        }
      } catch (error) {
        console.error('检查订阅状态失败，继续登录流程:', error);
        // 检查失败，也显示订阅弹窗以确保用户体验
        this.setData({
          pendingPhoneEvent: e,
          pendingLoginAction: 'phone',
          showSubscribeModal: true
        });
      }
    },

    // 修改：隐藏订阅弹窗
    hideSubscribeModal() {
      this.setData({
        showSubscribeModal: false,
        pendingPhoneEvent: null,
        pendingLoginAction: null
      });
    },

    // 修改：跳过订阅授权
    skipSubscribeAuth() {
      this.setData({
        showSubscribeModal: false,
        'subscribeStatus.requested': true,
        'subscribeStatus.authorized': false
      });
      
      // 根据登录类型执行相应的登录流程
      this.executeLoginAction();
    },

    // 修改：确认订阅授权
    confirmSubscribeAuth() {
      const that = this;
      const templateIds = this.data.subscribeStatus.templateIds;

      // 隐藏弹窗
      this.setData({
        showSubscribeModal: false
      });

      if (templateIds.length === 0) {
        // 没有模板，直接执行登录
        this.executeLoginAction();
        return;
      }

      // 用户确认后立即请求订阅授权（这里是在用户点击事件中）
      wx.requestSubscribeMessage({
        tmplIds: templateIds,
        success: function(res) {
          console.log('订阅消息授权结果:', res);
          
          const authorizedTemplates = templateIds.filter(id => res[id] === 'accept');
          const authorizedCount = authorizedTemplates.length;
          
          that.setData({
            'subscribeStatus.requested': true,
            'subscribeStatus.authorized': authorizedCount > 0
          });

          // 保存订阅状态到API而不是本地存储
          that.saveSubscribeStatusToAPI(res);
          
          // 订阅完成后继续登录流程
          that.executeLoginAction();
        },
        fail: function(err) {
          console.log('订阅消息授权失败:', err);
          
          that.setData({
            'subscribeStatus.requested': true,
            'subscribeStatus.authorized': false
          });

          // 保存订阅状态到API
          that.saveSubscribeStatusToAPI({ skipped: true });

          // 即使订阅失败也继续登录流程
          that.executeLoginAction();
        }
      });
    },

    // 修改：根据登录类型执行相应的登录动作
    executeLoginAction(loginAction) {
      const actionType = loginAction || this.data.pendingLoginAction;
      
      switch(actionType) {
        case 'phone':
          // 微信手机号登录
          if (this.data.pendingPhoneEvent) {
            this.processPhoneAuth(this.data.pendingPhoneEvent);
          }
          break;
        case 'verifyCode':
          // 验证码登录
          this.passwordOrCodeLogin(false);
          break;
        case 'password':
          // 密码登录
          this.passwordOrCodeLogin(true);
          break;
        default:
          console.error('未知的登录类型:', actionType);
      }
    },

    // 修改：密码或验证码登录（添加订阅状态提示）
    passwordOrCodeLogin(isPassword){
      // 设置处理状态
      this.setData({
        isProcessingAuth: true,
        authStep: 'login'
      });
      this.updateAuthButtonText();

      //获取微信用户信息
      var param = {
        phone:this.data.phoneNumber,
        password:this.data.password,
        verifyCode:this.data.verifyCode,
        openid:wx.getStorageSync('openid'),
        isPatient:this.properties.isPatient,
        isPassword:isPassword
      }
      var that = this;
      realapi.fn_LoginByPasswordOrCode(param,
        function(result){
          if (!result.success) {
            that.setData({
              errorMessage: result.message || '登录失败',
              isProcessingAuth: false,
              authStep: 'login'
            });
            that.updateAuthButtonText();
            return;
          }

          // 登录成功
          that.setData({
            authStep: 'complete',
            isProcessingAuth: false
          });
          that.updateAuthButtonText();

          // 显示登录成功消息（包含订阅状态）
          const subscribeMsg = that.data.subscribeStatus.authorized ? 
            '登录成功！已开启消息通知' : 
            '登录成功！';
          
          Toast({
            type: 'success',
            message: subscribeMsg,
            duration: 2000
          });

          // 等待Toast显示后跳转
          setTimeout(() => {
            that.fn_LoginSuccess(that, result);
          }, 2000);
          
        },
        function(){
          that.setData({
            isProcessingAuth: false,
            authStep: 'login',
            errorMessage: '网络错误，请重试'
          });
          that.updateAuthButtonText();
        }
      );
    },

    // 修改：更新按钮文字（添加更多状态）
    updateAuthButtonText() {
      let buttonText = '一键登录';
      
      if (this.data.isProcessingAuth) {
        switch (this.data.authStep) {
          case 'phone':
            buttonText = '正在获取手机号...';
            break;
          case 'subscribe':
            buttonText = '正在请求消息授权...';
            break;
          case 'login':
            buttonText = '正在登录...';
            break;
          case 'complete':
            buttonText = '登录成功';
            break;
          default:
            buttonText = '处理中...';
        }
      }
      
      this.setData({
        authButtonText: buttonText
      });
    },

    // 修改：处理手机号授权（保持原有逻辑）
    processPhoneAuth(e) {
      const that = this;
      
      this.setData({
        isProcessingAuth: true,
        authStep: 'phone'
      });
      this.updateAuthButtonText();

      var param = {
        code: e.detail.code,
        openid: wx.getStorageSync('openid'),
        isPatient: this.properties.isPatient
      };

      realapi.fn_GetPhoneNum(param,
        function(result) {
          if (!result.success) {
            that.setData({
              errorMessage: result.message,
              isProcessingAuth: false,
              authStep: 'phone'
            });
            that.updateAuthButtonText();
            return;
          }

          // 登录成功
          that.setData({
            authStep: 'complete',
            isProcessingAuth: false
          });
          that.updateAuthButtonText();

          const subscribeMsg = that.data.subscribeStatus.authorized ? 
            '登录成功！已开启消息通知' : 
            '登录成功！';
          
          Toast({
            type: 'success',
            message: subscribeMsg,
            duration: 2000
          });

          setTimeout(() => {
            that.fn_LoginSuccess(that, result);
          }, 2000);
        },
        function() {
          console.log("获取手机号API调用失败");
          that.setData({
            isProcessingAuth: false,
            authStep: 'phone',
            errorMessage: '网络错误，请重试'
          });
          that.updateAuthButtonText();
        }
      );
    },

    //登录成功后统一跳转方法
    fn_LoginSuccess(that,result){
      if(that.properties.isPatient){ //患者端返回的是默认就诊人
        const patient = result.patient;

        // 调用 Bio.Core 认证接口设置 Cookie
        var bioAuthParam = {
          UnionId: wx.getStorageSync('openid'),
          Role: 'patient'
        };

        realapi.fn_BioAuthWechat(bioAuthParam,
          function(bioAuthResult){
            // 判断返回体中的Success字段
            if (bioAuthResult && (bioAuthResult.Success === true || bioAuthResult.success === true)) {
              console.log(`【调试 Bio.Core 患者认证】成功`);

              // 将AccessToken携带到webview
              var accessToken = bioAuthResult.AccessToken || bioAuthResult.accessToken || '';
              var refreshToken = bioAuthResult.RefreshToken || bioAuthResult.refreshToken || '';
              var tokenQuery = '';
              if (accessToken) tokenQuery += `&accessToken=${encodeURIComponent(accessToken)}`;
              if (refreshToken) tokenQuery += `&refreshToken=${encodeURIComponent(refreshToken)}`;

              var endUrl = `fromWhere=${that.properties.fromWhere}&ProjectId=${that.properties.ProjectId}&DoctorId=${that.properties.DoctorId}&IsEnrollment=${that.data.IsEnrollment}&ScanTime=${that.data.ScanTime}&openId=${wx.getStorageSync('openid')}&code=${that.properties.code}&patientMessage=${that.properties.patientMessage}` + tokenQuery;

              if((patient.sid_type==null || patient.sid_type=='' || patient.sid_type==undefined) &&
              (patient.sid_number==null || patient.sid_number=='' || patient.sid_number==undefined)){
                wx.reLaunch({
                  url: `../realNameAuth/realNameAuth?patientInfo=${JSON.stringify(patient)}&${endUrl}`
                })
              }else{
                wx.reLaunch({
                  url: `../webview/webview?defaultPatientId=${result.patient.id}&${endUrl}`,
                })
              }
            }
          },
          function(bioAuthError){
            console.error(`【调试 Bio.Core 患者认证】失败:`, bioAuthError);
            Toast({
              message:'认证失败，请重试',
              position: 'bottom'
            })
          }
        );
      }else{
        //医生端,跳转医生端首页
        // 调用 Bio.Core 认证接口设置 Cookie
        var bioAuthParam = {
          UnionId: wx.getStorageSync('openid'),
          Role: 'doctor'
        };

        realapi.fn_BioAuthWechat(bioAuthParam,
          function(bioAuthResult){
            // 判断返回体中的Success字段
            if (bioAuthResult && (bioAuthResult.Success === true || bioAuthResult.success === true)) {
              console.log(`【调试 Bio.Core 医生认证】成功`);

              // 将AccessToken携带到webview
              var accessToken = bioAuthResult.AccessToken || bioAuthResult.accessToken || '';
              var refreshToken = bioAuthResult.RefreshToken || bioAuthResult.refreshToken || '';
              var extra = '';
              if (accessToken) extra += `&accessToken=${encodeURIComponent(accessToken)}`;
              if (refreshToken) extra += `&refreshToken=${encodeURIComponent(refreshToken)}`;

              wx.reLaunch({
                url: `../webview/webview?doctorId=${result.doctorId}&openId=${wx.getStorageSync('openid')}` + extra,
              });
            }
          },
          function(bioAuthError){
            console.error(`【调试 Bio.Core 医生认证】失败:`, bioAuthError);
            Toast({
              message:'认证失败，请重试',
              position: 'bottom'
            })
          }
        );
      }
    },
  
    // 验证表单
    validateForm() {
      if (!this.isPhoneValid()) {
        this.setData({
          errorMessage: '请输入正确的手机号'
        });
        return false;
      }

      if (this.data.loginType === 'verifyCode' && !this.data.verifyCode) {
        this.setData({
          errorMessage: '请输入验证码'
        });
        return false;
      }

      if (this.data.loginType === 'password' && !this.data.password) {
        this.setData({
          errorMessage: '请输入密码'
        });
        return false;
      }

      if (!this.data.agreeToTerms) {
        this.setData({
          errorMessage: '请阅读并同意用户协议和隐私政策'
        });
        return false;
      }

      return true;
    },

    // 修改：保存订阅状态 - 移除本地存储逻辑，因为已经改为API方式
    saveSubscribeStatus(subscribeResult) {
      // 这个方法保留是为了兼容性，但实际功能已转移到 saveSubscribeStatusToAPI
      console.log('saveSubscribeStatus called, redirecting to API method');
      this.saveSubscribeStatusToAPI(subscribeResult);
    }
  },

  /**
   * 组件生命周期函数--监听组件初次渲染完成
   */
  ready() {

  },

  /**
   * 组件生命周期函数--监听组件显示
   */
  moved() {

  },

  /**
   * 组件生命周期函数--监听组件隐藏
   */
  detached() {

  }
})