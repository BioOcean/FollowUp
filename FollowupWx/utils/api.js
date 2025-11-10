var request = require("../utils/request")

// 是否登陆
function fn_CheckUserExists(param,doSuccess,doFail) {
  return request.request("Wechat/CheckUserExists",param,doSuccess,doFail);
}

function fn_GetPhoneNum(param,success,fail){
  return request.request("Wechat/GetPhoneNum",param,success,fail);
}

function fn_LoginByPasswordOrCode(param,success,fail){
  return request.request("Wechat/LoginByPasswordOrCode",param,success,fail)
}

function fn_SendPhoneVerificationCode(param,success,fail){
  return request.request("Wechat/SendPhoneVerificationCode",param,success,fail)
}

function fn_UpdatePaitentInfo(param,success,fail){
  return request.request("Wechat/UpdatePaitentInfo",param,success,fail)
}

function fn_GetAccessToken(param,success,fail){
  return request.request("Wechat/GetAccessToken",param,success,fail)
}

function fn_GetPatientMessageList(param,success,fail){
  return request.request("Wechat/GetPatientMessageList",param,success,fail)
}

function fn_ViewPatientMessageContent(param,success,fail){
  return request.request("Wechat/ViewPatientMessageContent",param,success,fail)
}

function fn_ViewPatientMessageAll(param,success,fail){
  return request.request("Wechat/ViewPatientMessageAll",param,success,fail)
}

function fn_GetDoctorMessageList(param,success,fail){
  return request.request("Wechat/GetDoctorMessageList",param,success,fail)
}

function fn_ViewDoctorMessageContent(param,success,fail){
  return request.request("Wechat/ViewDoctorMessageContent",param,success,fail)
}

function fn_ViewDoctorMessageAll(param,success,fail){
  return request.request("Wechat/ViewDoctorMessageAll",param,success,fail)
}

function fn_HasSubscription(param,success,fail){
  return request.request("Wechat/HasSubscription",param,success,fail)
}

function fn_SetSubscription(param,success,fail){
  return request.request("Wechat/SetSubscription",param,success,fail)
}

// 获取课题列表
function fn_GetProjectList(param,success,fail){
  return request.request("Wechat/GetProjectList",param,success,fail)
}

// 获取Banner参数
function fn_GetCache(param,success,fail){
  return request.request("Wechat/GetCache",param,success,fail)
}

// 患者入组
function fn_PatientEnterGroup(param,success,fail){
  return request.request("Wechat/PatientEnterGroup",param,success,fail)
}

// Bio.Core 微信认证登录
function fn_BioAuthWechat(param,success,fail){
  // 直接POST到 wechat/{unionId}?role=xxx，role 由调用方传入（doctor|patient）
  const role = param.Role ? `?role=${encodeURIComponent(param.Role)}` : '';
  return request.request(`api/auth/wechat/${param.UnionId}${role}`,{},success,fail,'POST')
}

module.exports.fn_CheckUserExists = fn_CheckUserExists;
module.exports.fn_GetPhoneNum = fn_GetPhoneNum;
module.exports.fn_LoginByPasswordOrCode= fn_LoginByPasswordOrCode;
module.exports.fn_UpdatePaitentInfo = fn_UpdatePaitentInfo;
module.exports.fn_GetAccessToken = fn_GetAccessToken;
module.exports.fn_SendPhoneVerificationCode = fn_SendPhoneVerificationCode;
module.exports.fn_GetPatientMessageList = fn_GetPatientMessageList;
module.exports.fn_ViewPatientMessageContent = fn_ViewPatientMessageContent;
module.exports.fn_ViewPatientMessageAll = fn_ViewPatientMessageAll;
module.exports.fn_GetDoctorMessageList = fn_GetDoctorMessageList;
module.exports.fn_ViewDoctorMessageContent = fn_ViewDoctorMessageContent;
module.exports.fn_ViewDoctorMessageAll = fn_ViewDoctorMessageAll;
module.exports.fn_HasSubscription = fn_HasSubscription;
module.exports.fn_SetSubscription = fn_SetSubscription;
module.exports.fn_GetProjectList = fn_GetProjectList;
module.exports.fn_PatientEnterGroup = fn_PatientEnterGroup;
module.exports.fn_GetCache = fn_GetCache;
module.exports.fn_BioAuthWechat = fn_BioAuthWechat;