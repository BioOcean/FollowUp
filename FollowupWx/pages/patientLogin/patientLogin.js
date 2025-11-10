
var realapi = require('../../utils/api')
import Toast from '@vant/weapp/toast/toast'

Page({
  data: {
    baseUrl:wx.getStorageSync('bhglUrl'),
    showLoginView: false,
    ProjectId:'', //bd51a2a3-d8b3-432b-8ec8-fc2fe2d08429
    DoctorId:'',
    ScanTime:'', //门诊报到扫码时间
    fromWhere:'' ,//discharge 出院入组  beHospitalized入院入组  outpatient门诊入组  menu 公众号菜单[我是患者]:首次需要入组,
    IsEnrollment:true, //是否入组,与fromwhere = menu结合用
    code:'', //从别处来的要跳转到某个具体页面的
    patientMessage:'', //订阅消息跳转
  },
  
  onLoad: function(options) {
    console.log(options);
    if(options.fromWhere != undefined){
      this.setData({
        fromWhere :options.fromWhere
      });
    }
    if(options.ProjectId != undefined){
      this.setData({
        ProjectId :options.ProjectId
      });
    }
    if(options.DoctorId != undefined){
      this.setData({
        DoctorId :options.DoctorId
      });
    }
    if(options.ScanTime != undefined ){
      this.setData({
        ScanTime:options.ScanTime
      });
    }
    if(options.code != undefined){
      this.setData({
        code:options.code
      });
    }
    if(options.patientMessage != undefined){
      this.setData({
        patientMessage:options.patientMessage
      });
    }
    console.log(`【调试 this.data】${JSON.stringify(this.data)}`)
    this.fn_isLogin();
  },
  
  
  fn_isLogin(){
    var that = this;
    wx.login({ 
      success(res) {   
        if(res.code) {
          var param = {
            jscode:res.code,
            isPatient:true,
            fromWhere:that.data.fromWhere
          }
          console.log(param);
          realapi.fn_CheckUserExists(param,
            function(result){
              console.log(`【调试 result.success】${result.success} 【result.wxOpenId】${result.wxOpenId}`)
              if(result.success){
                wx.setStorageSync('openid', result.wxOpenId);
                
                // 第二步：调用 Bio.Core 认证接口设置 Cookie
                var bioAuthParam = {
                  UnionId: result.wxOpenId,
                  Role: 'patient'
                };
                
                realapi.fn_BioAuthWechat(bioAuthParam,
                  function(bioAuthResult){
                    // 判断返回体中的Success字段
                    if (bioAuthResult && (bioAuthResult.Success === true || bioAuthResult.success === true)) {
                      console.log(`【调试 Bio.Core 认证】成功`);
                      // Bio.Core 认证成功后，继续原来的业务逻辑
                      var patient = result.patientInfo;
                      var scanRecord = result.scanRecord;
                      if(that.data.fromWhere == 'menu' &&scanRecord != null && scanRecord.status=='pending'){ //菜单栏进入??(应该不一定是菜单过来的也有未入组情况) 已扫码且未入组的患者
                        that.setData({
                          ProjectId:scanRecord.ProjectId,
                          DoctorId:scanRecord.DoctorId,
                          IsEnrollment:false //还没入组
                        });
                      }
                      // 将AccessToken携带到webview（使用jump参数，webview直接使用完整路径）
                      var accessToken = bioAuthResult.AccessToken || bioAuthResult.accessToken || '';
                      var refreshToken = bioAuthResult.RefreshToken || bioAuthResult.refreshToken || '';
                      var tokenQuery = '';
                      if (accessToken) tokenQuery += `&accessToken=${encodeURIComponent(accessToken)}`;
                      if (refreshToken) tokenQuery += `&refreshToken=${encodeURIComponent(refreshToken)}`;
                      var endUrl = `fromWhere=${that.data.fromWhere}&ProjectId=${that.data.ProjectId}&DoctorId=${that.data.DoctorId}&IsEnrollment=${that.data.IsEnrollment}&ScanTime=${that.data.ScanTime}&openId=${result.wxOpenId}&code=${that.data.code}&patientMessage=${that.data.patientMessage}` + tokenQuery;
                      console.log(`【调试 endUrl】${endUrl} 【not patient】${patient==null}`)
                        
                      if(patient==null){ //没有用户，显示注册
                          that.setData({
                            showLoginView:true
                          });
                        }
                        else if((patient.name==null || patient.name=='' || patient.name==undefined)){ //没有实名，跳转认证
                          wx.reLaunch({
                            url: `../realNameAuth/realNameAuth?patientInfo=${JSON.stringify(patient)}&${endUrl}`,
                          });
                        }
                        else{ //已注册用户，跳转至web页
                          var jumpPath = `navigationPage?uniquePatientId=${patient.id}&${endUrl}`;
                          wx.reLaunch({
                            url: `../webview/webview?jump=${encodeURIComponent(jumpPath)}`,
                          });
                        }
                    }
                  },
                  function(bioAuthError){
                    console.error(`【调试 Bio.Core 认证】失败:`, bioAuthError);
                    Toast({
                      message:'认证失败，请重试',
                      position: 'bottom'
                    })
                  }
                );
                }else{
                  Toast({
                    message:result.message,
                    position: 'bottom'
                  })
                }
          },function(e){
            Toast({
              message:'服务器无响应',
              position:'bottom'
            })
          })
        } else {
          console.log("获取微信id失败 "+res.errMsg);
        }
      }
    });
}, 
 
});