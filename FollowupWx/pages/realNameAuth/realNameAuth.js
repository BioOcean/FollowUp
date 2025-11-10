// pages/realNameAuth/realNameAuth.js
var realapi = require('../../utils/api')
import Toast from '@vant/weapp/toast/toast'
import utils from '../../utils/getPageInfo'
Page({

  /**
   * 页面的初始数据
   */
  data: {
    patientInfo:{},
    patientId:'',
    patientName: '',
    idType: '',
    idNumber: '',
    gender: '',
    birthDate: '',
    relation: '',
    phoneNumber: '',
    idTypeList: ['居民身份证', '签证', '护照', '港澳通行证', '社会保障卡'],
    relationList: ['本人', '父母', '夫妻','子女', '兄弟姐妹', '其他']
  },

  /**
   * 生命周期函数--监听页面加载
   */
  onLoad(options) {
    console.log(options)
    if(options.patientInfo !='' && options.patientInfo != undefined){
      var patient = JSON.parse(options.patientInfo);
      this.setData({
        patientInfo:patient,
        patientId:patient.id,
        patientName:patient.name,
        gender:patient.gender,
        birthDate:patient.birthday,
        relation:patient.weixin_relation && patient.weixin_relation.length > 0 ? patient.weixin_relation[0]  : '',
        phoneNumber:patient.weixin_phone_number && patient.weixin_phone_number.length > 0 ? patient.weixin_phone_number[0] : ''
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
  onShow(){
    // 隐藏返回首页按钮
    wx.hideHomeButton();
  },

  // 处理证件类型选择
  idTypeChange(e) {
    const index = e.detail.value;
    this.setData({
      idType: this.data.idTypeList[index]
    });
  },

  // 处理患者关系选择
  relationChange(e) {
    const index = e.detail.value;
    this.setData({
      relation: this.data.relationList[index]
    });
  },

  // 处理性别选择
  selectGender(e) {
    const gender = e.currentTarget.dataset.gender;
    this.setData({
      gender: gender
    });
  },

  // 处理出生日期选择
  birthDateChange(e) {
    this.setData({
      birthDate: e.detail.value
    });
  },

  // 扫描身份证
  scanIdCard() {
    var that = this;
    wx.chooseMedia({
      count:1,
      mediaType:['image'],
      sourceType:['album', 'camera'],
      camera:'back',
      success(res){
        console.log("拍照结果",res)
        var path = res.tempFiles[0].tempFilePath;
        var param={type:"mini"};
        realapi.fn_GetAccessToken(param,
          function(result){
            if(result.success){
              wx.uploadFile({
                url:`https://api.weixin.qq.com/cv/ocr/idcard?access_token=${result.token}`,
                filePath: path,
                name: 'img',
              success(res){
                const result = JSON.parse(res.data);
                if(result.errcode==0){
                  if(result.type=="Front"){
                    that.setData({
                      idNumber:result.id,
                      patientName:result.name,
                      gender:result.gender,
                      birthDate:result.birth
                    })
                  }else{
                    Toast({
                      message:'请上传身份证正面',
                      position:'bottom'
                    })
                  }
                }else{
                  Toast({
                    message:result.errmsg,
                    position:'bottom'
                  })
                }
              }
              })
            }
          })
      }
    })
  },


  // 下一步
  nextStep() {
    // 表单验证
    if (!this.data.patientName) {
      this.showError('请输入患者姓名');
      return;
    }
    // if (!this.data.idType) {
    //   this.showError('请选择证件类型');
    //   return;
    // }
    // if (!this.data.idNumber) {
    //   this.showError('请输入证件号码');
    //   return;
    // }
    if (this.data.idType=="居民身份证" && this.data.idNumber) {
      // 判断身份证号是否符合格式
      if (!/(^\d{15}$)|(^\d{18}$)|(^\d{17}(\d|X|x)$)/.test(this.data.idNumber)) {
        this.showError('请输入正确的身份证号码');
        return;
      }
    }

    if (!this.data.gender) {
      this.showError('请选择患者性别');
      return;
    }
    if (!this.data.birthDate) {
      this.showError('请选择出生日期');
      return;
    }
    if (!this.data.relation) {
      this.showError('请选择患者关系');
      return;
    }
    if (!this.data.phoneNumber) {
      this.showError('请输入手机号码');
      return;
    }
    
    // 提交表单数据
    console.log('表单数据：', this.data);
    
    //请求接口更新患者信息，并导航到下一页
    var param={
      patientId:this.data.patientId,
      name:this.data.patientName,
      birthday:this.data.birthDate,
      gender:this.data.gender,
      sid_type:this.data.idType,
      sid_number:this.data.idNumber,
      openid:wx.getStorageSync('openid'),
      phoneNumber:this.data.phoneNumber,
      relation:this.data.relation
    }
    var that = this;
    var pageParam = utils.getCurrentPageParam();
    console.log(pageParam);
    realapi.fn_UpdatePaitentInfo(param,
      function(res){
        if(res.success){
          // 构建完整的URL参数，包括token信息
          var tokenQuery = '';
          if (pageParam.accessToken) tokenQuery += `&accessToken=${encodeURIComponent(pageParam.accessToken)}`;
          if (pageParam.refreshToken) tokenQuery += `&refreshToken=${encodeURIComponent(pageParam.refreshToken)}`;

          var endUrl = `fromWhere=${pageParam.fromWhere}&ProjectId=${pageParam.ProjectId}&DoctorId=${pageParam.DoctorId}&IsEnrollment=${pageParam.IsEnrollment}&ScanTime=${pageParam.ScanTime}&openId=${pageParam.openId}&code=${pageParam.code}&patientMessage=${pageParam.patientMessage}` + tokenQuery;
          wx.reLaunch({
            url: `../webview/webview?defaultPatientId=${that.data.patientId}&${endUrl}`,
          })
        }else{
          Toast({
            message:res.errmsg,
            position:'bottom'
          })
        }
      },
      function(){

      })
  },
  
  // 显示错误提示
  showError(msg) {
    Toast({
      message:msg,
      position:'bottom'
    })
  },
  // 输入患者姓名
  inputPatientName(e) {
    this.setData({
      patientName: e.detail.value
    });
  },

  // 输入证件号码
  inputIdNumber(e) {
    const idNumberValue = e.detail.value;
    this.setData({
      idNumber: idNumberValue
    });
    if (/(^\d{15}$)|(^\d{18}$)|(^\d{17}(\d|X|x)$)/.test(this.data.idNumber)) {
      //自动回填，男女、出生日期
      if(this.data.idNumber.length==18){
        const gender = parseInt(idNumberValue.charAt(16), 10) % 2 === 0 ? '女' : '男';
        const year = idNumberValue.substring(6, 10);
        const month = idNumberValue.substring(10, 12);
        const day = idNumberValue.substring(12, 14);
        const birthDate = `${year}-${month}-${day}`;
        this.setData({
          gender: gender,
          birthDate: birthDate
        });
      }
      if(this.data.idNumber.length==15){
        const gender = parseInt(idNumberValue.charAt(14), 10) % 2 === 0 ? '女' : '男';
        // 15位身份证年份补全为 "19xx"
        const year = "19" + idNumberValue.substring(6, 8);
        const month = idNumberValue.substring(8, 10);
        const day = idNumberValue.substring(10, 12);
        const birthDate = `${year}-${month}-${day}`;
        this.setData({
          gender: gender,
          birthDate: birthDate
        });
      }
    }
  },

  // 输入手机号码
  inputPhoneNumber(e) {
    this.setData({
      phoneNumber: e.detail.value
    });
  },
})