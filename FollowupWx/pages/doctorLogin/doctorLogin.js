
var realapi = require('../../utils/api')
Page({
  data: {
    baseUrl:wx.getStorageSync('bhglUrl'),
    showLoginView: false
  },
  
  onLoad: function(options) {
    this.fn_isLogin();
  },
  
  
  fn_isLogin(){
    var that = this;
    wx.login({ 
      success(res) {   
        if(res.code) {
          var param = {
            jscode:res.code,
            isPatient:false,
            fromWhere:''
          }
          realapi.fn_CheckUserExists(param,
            function(result){
              if(result.success){
                wx.setStorageSync('openid', result.wxOpenId);

                var doctor = result.doctorInfo;
                // 如果医生信息不存在，显示登录界面，不进行Bio.Core认证
                if(doctor==null){
                  that.setData({
                    showLoginView:true
                  });
                  return; // 直接返回，不调用Bio.Core认证
                }

                // 医生信息存在，调用 Bio.Core 认证接口设置 Cookie
                var bioAuthParam = {
                  UnionId: result.wxOpenId,
                  Role: 'doctor'
                };

                realapi.fn_BioAuthWechat(bioAuthParam,
                  function(bioAuthResult){
                    // 判断返回体中的Success字段
                    if (bioAuthResult && (bioAuthResult.Success === true || bioAuthResult.success === true)) {
                      console.log(`【调试 Bio.Core 医生认证】成功`);
                      // 跳转webview，使用jump参数承载完整导航路径
                      var accessToken = bioAuthResult.AccessToken || bioAuthResult.accessToken || '';
                      var refreshToken = bioAuthResult.RefreshToken || bioAuthResult.refreshToken || '';
                      var extra = '';
                      if (accessToken) extra += `&accessToken=${encodeURIComponent(accessToken)}`;
                      if (refreshToken) extra += `&refreshToken=${encodeURIComponent(refreshToken)}`;
                      var jumpPath = `FollowupDoctorTerminalHome?DoctorId=${doctor.id}&openId=${result.wxOpenId}` + extra;
                      wx.reLaunch({
                        url: `../webview/webview?jump=${encodeURIComponent(jumpPath)}`,
                      });
                    }
                  },
                  function(bioAuthError){
                    console.error(`【调试 Bio.Core 医生认证】失败:`, bioAuthError);
                    console.log("医生认证失败，请重试");
                    // 认证失败时也显示登录界面
                    that.setData({
                      showLoginView:true
                    });
                  }
                );
                }
          },function(){
            console.log("....");
          })
        } else {
          console.log("获取微信id失败 "+res.errMsg);
        }
      }
    });
}, 
 
});