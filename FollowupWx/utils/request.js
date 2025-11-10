var app = getApp();
var Base64 = require('../utils/base64');
//项目URL相同部分
//线上小程序

var host = 'https://localhost:7177/'  //本地调试

//var host = 'https://suifang.xwhosp.com.cn/' //宣武正式
//var host = 'https://xwxcx.bio-ocean.com/' //宣武线上测试


//var host = 'https://wxdebug.bio-ocean.com/'  //正式随访

// host 存入本地缓存 用于跳转
wx.setStorageSync('bhglUrl', host);

//请求头处理函数
function CreateHeader() {
  let header = {}
    header = {
      'content-type': 'application/json;charset=UTF-8'
    }
    let token = wx.getStorageSync('token');
    if(token){
      header['auth'] = token;
    }
  return header;
}
/**
 * POST请求，
 * URL：接口
 * postData：参数，json类型
 * doSuccess：成功的回调函数
 * doFail：失败的回调函数
 */
function request(url, postData, doSuccess, doFail, method = 'POST') {

  // wx.showToast({ title: '加载中', icon: 'loading', duration: 10000 });
let header = CreateHeader();
// postData = JSON.stringify({a: Base64.encode(JSON.stringify(postData))});
// console.log('postdata：   ',JSON.stringify(postData))
wx.request({

    //项目的真正接口，通过字符串拼接方式实现
    url: host + url,
    header: header,
    data: postData,
    method: method,
    success: function (res) {
      // 参数值为 res.data；若为字符串且是JSON尝试解析
      wx.hideToast();
      let data = res.data;
      if (typeof data === 'string') {
        try { data = JSON.parse(data); } catch(e) {}
      }
      doSuccess(data);
    },
    fail: function () {
      wx.hideToast();
      doFail();
    },
  })
}
function getData2(url,params){
  Base64.utf8encode = true;
  return host+url+"?a="+Base64.encode(JSON.stringify(params));
}
//GET请求，不需传参，直接URL调用，
function getData(url,postData, doSuccess, doFail) {
  let header = CreateHeader();
  wx.request({
    url: host + url,
    header:header,
	  data: postData,
    method: 'GET',
    success: function (res) {
      wx.hideToast();
      doSuccess(res.data);
      if(res.data.token){
        wx.setStorageSync('token',res.data.token)
      }
    },
    fail: function () {
      wx.hideToast();
      doFail();
    },
  })
}

/**
 * module.exports用来导出代码
 * js文件中通过var call = require("../util/request.js")  加载
 */
module.exports.request = request;
module.exports.get = getData;
module.exports.getData = getData;
module.exports.getData2 = getData2;

/**
 * wxb100d6983f6a6a99   研发小程序
 * wx5a1cd780e53dc305   测试小程序
 * 
 */