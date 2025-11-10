// pages/message/message.ts

// 添加微信小程序类型声明
declare const wx: any;
declare const getApp: () => any;
declare const getCurrentPages: () => any[];

const api = require("../../utils/api");
const config = require("../../config");
// 引入导航工具
const navigationUtils = require('../../utils/navigation.js');

interface MessageItem {
  id: string;
  patientId: string;
  title: string;
  content: string;
  createTime: string;
  createTimeDisplay: string;
  contentPreview: string;
  type: string;
  url?: string;
  readTime?: string;
  isRead: boolean;
}

interface PageData {
  messageList: MessageItem[];
  loading: boolean;
  hasMore: boolean;
  currentPage: number;
  pageSize: number;
  showMessageDetail: boolean;
  currentMessage: MessageItem | null;
  openId: string;
  userType: string; // 新增：用户类型 doctor/patient
  subscribeStatus: {
    requested: boolean;
    authorized: boolean;
    templateIds: string[];
  };
  pendingMessageAction: MessageItem | null;
}

// 页面实例方法类型定义
interface PageMethods {
  refreshMessages(): Promise<void>;
  loadMessages(isRefresh?: boolean): Promise<void>;
  formatMessageList(messages: any[]): MessageItem[];
  formatTime(timeStr: string): string;
  getContentPreview(content: string): string;
  getTypeDisplay(type: string): string;
  onMessageClick(e: any): Promise<void>;
  markMessageAsRead(messageId: string): Promise<void>;
  onMarkAllRead(): Promise<void>;
  onLoadMore(): void;
  onCloseModal(): void;
  onOpenUrl(): void;
  onBack(): void;
  initSubscribeTemplates(): void;
  checkSubscribeStatus(): Promise<boolean>;
  showMessageDetail(message: MessageItem): void;
  saveSubscribeStatus(subscribeResult: any): Promise<void>;
  requestSubscribeMessage(message: MessageItem): Promise<void>;
  loadMessageById(messageId: string): Promise<void>;
  handleUrlParams(options: any): Promise<void>;
  initPageByUserType(userType: string): void; // 新增：根据用户类型初始化页面
}

// @ts-ignore
Page<PageData & PageMethods>({
  /**
   * 页面的初始数据
   */
  data: {
    messageList: [],
    loading: false,
    hasMore: true,
    currentPage: 1,
    pageSize: 10,
    showMessageDetail: false,
    currentMessage: null,
    openId: '',
    userType: 'patient', // 默认患者
    subscribeStatus: {
      requested: false,
      authorized: false,
      templateIds: []
    },
    pendingMessageAction: null
  },

  /**
   * 生命周期函数--监听页面加载
   */
  onLoad(options: any) {
    // 从URL参数中获取openId和userType
    const { openId, userType } = options;
    if (!openId) {
      wx.showToast({
        title: '缺少openId参数',
        icon: 'none'
      });
      return;
    }

    // 保存openId和userType到页面数据
    this.setData({
      openId,
      userType: userType || 'patient' // 默认为患者
    });

    // 根据用户类型初始化页面
    this.initPageByUserType(userType || 'patient');

    // 初始化订阅模板（医生和患者都需要订阅推送）
    this.initSubscribeTemplates();

    // 处理URL参数（包括mid参数）
    this.handleUrlParams(options);
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
    if (this.data.openId) {
      this.refreshMessages();
    }
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
    this.refreshMessages().finally(() => {
      wx.stopPullDownRefresh();
    });
  },

  /**
   * 页面上拉触底事件的处理函数
   */
  onReachBottom() {
    if (!this.data.loading && this.data.hasMore) {
      this.loadMessages(false);
    }
  },

  /**
   * 用户点击右上角分享
   */
  onShareAppMessage() {

  },

  /**
   * 刷新消息列表
   */
  async refreshMessages(): Promise<void> {
    this.setData({
      currentPage: 1,
      hasMore: true,
      messageList: []
    });
    await this.loadMessages(true);
  },

  /**
   * 加载消息列表
   */
  async loadMessages(isRefresh: boolean = false): Promise<void> {
    if (this.data.loading) return;

    if (!this.data.openId) {
      wx.showToast({
        title: '缺少openId参数',
        icon: 'none'
      });
      return;
    }

    this.setData({ loading: true });

    try {
      // 根据用户类型构造不同的参数
      const param = this.data.userType === 'doctor' ? {
        union_id: this.data.openId,
        pageNumber: isRefresh ? 1 : this.data.currentPage,
        pageSize: this.data.pageSize
      } : {
        weixin_id: this.data.openId,
        pageNumber: isRefresh ? 1 : this.data.currentPage,
        pageSize: this.data.pageSize
      };

      await new Promise<void>((resolve, reject) => {
        // 根据用户类型调用不同API
        const apiMethod = this.data.userType === 'doctor'
          ? api.fn_GetDoctorMessageList
          : api.fn_GetPatientMessageList;

        apiMethod.call(api, param,
          (res: any) => {
            try {
              if (res.success) {
                const newMessages = this.formatMessageList(res.items || []);

                this.setData({
                  messageList: isRefresh ? newMessages : this.data.messageList.concat(newMessages),
                  currentPage: isRefresh ? 2 : this.data.currentPage + 1,
                  hasMore: res.hasNextPage || false,
                  loading: false
                });
                resolve();
              } else {
                throw new Error(res.message || '获取消息失败');
              }
            } catch (error) {
              reject(error);
            }
          },
          (error: any) => {
            reject(new Error('网络请求失败'));
          }
        );
      });

    } catch (error: any) {
      console.error('加载消息失败:', error);
      wx.showToast({
        title: error.message || '加载失败',
        icon: 'none'
      });
      this.setData({ loading: false });
    }
  },

  /**
   * 格式化消息列表数据
   */
  formatMessageList(messages: any[]): MessageItem[] {
    const self = this;
    return messages.map(function(item) {
      return {
        id: item.id,
        patientId: item.patientId,
        title: item.title || '无标题',
        content: item.content || '',
        createTime: item.createTime,
        createTimeDisplay: self.formatTime(item.createTime),
        contentPreview: self.getContentPreview(item.content || ''),
        type: self.getTypeDisplay(item.type),
        url: item.url,
        readTime: item.readTime,
        isRead: item.isRead || false
      };
    });
  },

  /**
   * 格式化时间显示
   */
  formatTime(timeStr: string): string {
    if (!timeStr) return '';
    
    try {
      const messageTime = new Date(timeStr);
      const now = new Date();
      const diff = now.getTime() - messageTime.getTime();
      
      // 小于1分钟
      if (diff < 60 * 1000) {
        return '刚刚';
      }
      
      // 小于1小时
      if (diff < 60 * 60 * 1000) {
        return `${Math.floor(diff / (60 * 1000))}分钟前`;
      }
      
      // 小于24小时
      if (diff < 24 * 60 * 60 * 1000) {
        return `${Math.floor(diff / (60 * 60 * 1000))}小时前`;
      }
      
      // 小于7天
      if (diff < 7 * 24 * 60 * 60 * 1000) {
        return `${Math.floor(diff / (24 * 60 * 60 * 1000))}天前`;
      }
      
      // 超过7天显示具体日期，格式为【yyyy-MM-dd】
      const year = messageTime.getFullYear();
      const month = String(messageTime.getMonth() + 1).padStart(2, '0');
      const day = String(messageTime.getDate()).padStart(2, '0');
      return `${year}-${month}-${day}`;
      
    } catch (error) {
      return timeStr;
    }
  },

  /**
   * 获取内容预览
   */
  getContentPreview(content: string): string {
    if (!content) return '暂无内容';
    
    // 移除HTML标签和多余空白
    const cleanContent = content.replace(/<[^>]*>/g, '').replace(/\s+/g, ' ').trim();
    
    // 限制长度
    return cleanContent.length > 50 ? cleanContent.substring(0, 50) + '...' : cleanContent;
  },

  /**
   * 获取类型显示文本
   */
  getTypeDisplay(type: string): string {
    // 根据用户类型返回不同的类型映射
    const typeMap = this.data.userType === 'doctor' ? {
      'system': '系统通知',
      'patient_feedback': '患者反馈',
      'task_reminder': '任务提醒',
      'schedule': '日程提醒'
    } : {
      'followup': '随访',
      'education': '宣教',
      'system': '系统',
      'notice': '通知'
    };
    return typeMap[type] || type || '消息';
  },

  /**
   * 消息点击事件
   */
  async onMessageClick(e: any): Promise<void> {
    const item: MessageItem = e.currentTarget.dataset.item;
    if (!item) return;

    try {
      // 检查订阅状态
      const hasSubscription = await this.checkSubscribeStatus();
      if (!hasSubscription) {
        console.log('需要订阅授权');
        // 需要订阅授权，直接调用微信授权
        await this.requestSubscribeMessage(item);
      } else {
        console.log('无需订阅确认');
        // 无需订阅确认，直接显示消息详情
        this.showMessageDetail(item);
      }
    } catch (error: any) {
      console.error('处理消息点击失败:', error);
      wx.showToast({
        title: '操作失败',
        icon: 'none'
      });
    }
  },

  /**
   * 标记消息为已读
   */
  async markMessageAsRead(messageId: string): Promise<void> {
    try {
      const param = { messageId };
      const result: any = await new Promise((resolve, reject) => {
        // 根据用户类型调用不同API
        const apiMethod = this.data.userType === 'doctor'
          ? api.fn_ViewDoctorMessageContent
          : api.fn_ViewPatientMessageContent;

        apiMethod.call(api, param, resolve, reject);
      });

      if (result.success) {
        // 更新本地消息状态
        const messageList = this.data.messageList.map(function(msg) {
          if (msg.id === messageId) {
            const updatedMsg: MessageItem = Object.assign({}, msg);
            updatedMsg.isRead = true;
            updatedMsg.readTime = new Date().toISOString();
            return updatedMsg;
          }
          return msg;
        });

        this.setData({ messageList });
      }
    } catch (error) {
      console.error('标记消息已读失败:', error);
    }
  },

  /**
   * 全部已读点击事件
   */
  async onMarkAllRead(): Promise<void> {
    if (this.data.loading) return;

    const unreadMessages = this.data.messageList.filter(function(msg) {
      return !msg.isRead;
    });
    if (unreadMessages.length === 0) {
      wx.showToast({
        title: '暂无未读消息',
        icon: 'none'
      });
      return;
    }

    this.setData({ loading: true });

    try {
      const openId = this.data.openId;
      if (!openId) {
        wx.showToast({
          title: '用户标识不存在',
          icon: 'none'
        });
        return;
      }

      // 根据用户类型构造不同的参数和调用不同的API
      const param = this.data.userType === 'doctor' ? { union_id: openId } : { weixin_id: openId };
      const result: any = await new Promise((resolve, reject) => {
        const apiMethod = this.data.userType === 'doctor'
          ? api.fn_ViewDoctorMessageAll
          : api.fn_ViewPatientMessageAll;

        apiMethod.call(api, param, resolve, reject);
      });

      if (result.success) {
        // 更新所有消息为已读状态
        const messageList = this.data.messageList.map(function(msg) {
          const updatedMsg: MessageItem = Object.assign({}, msg);
          updatedMsg.isRead = true;
          updatedMsg.readTime = msg.readTime || new Date().toISOString();
          return updatedMsg;
        });

        this.setData({ messageList });

        wx.showToast({
          title: '全部消息已标记为已读',
          icon: 'success'
        });
      } else {
        wx.showToast({
          title: result.message || '操作失败',
          icon: 'none'
        });
      }
    } catch (error) {
      console.error('标记全部已读失败:', error);
      wx.showToast({
        title: '网络错误，请稍后重试',
        icon: 'none'
      });
    } finally {
      this.setData({ loading: false });
    }
  },

  /**
   * 滚动到底部加载更多
   */
  onLoadMore(): void {
    if (this.data.hasMore && !this.data.loading) {
      this.loadMessages(false);
    }
  },

  /**
   * 关闭弹窗
   */
  onCloseModal(): void {
    this.setData({
      showMessageDetail: false,
      currentMessage: null
    });
  },

  /**
   * 打开链接
   */
  onOpenUrl(): void {
    const currentMessage = this.data.currentMessage;
    if (!currentMessage?.url) {
      wx.showToast({
        title: '暂无链接',
        icon: 'none'
      });
      return;
    }

    // 关闭弹窗
    this.onCloseModal();

    // 对URL进行编码，确保特殊字符正确传递
    const encodedUrl = encodeURIComponent(currentMessage.url.replace(/^\//, ''));
    const webviewUrl = `/pages/webview/webview?jump=${encodedUrl}`;

    console.log('准备跳转到webview页面:', currentMessage.url);

    // 使用带重试机制的navigateTo跳转
    navigationUtils.navigateToWithRetry(webviewUrl, 3, true)
      .then(() => {
        console.log('跳转到webview页面成功:', currentMessage.url);
      })
      .catch((error: any) => {
        console.error('跳转到webview页面失败:', error);

        // 如果跳转失败，fallback到复制链接
        wx.setClipboardData({
          data: currentMessage.url,
          success: () => {
            wx.showToast({
              title: '跳转失败，链接已复制',
              icon: 'success'
            });
          },
          fail: () => {
            wx.showToast({
              title: '跳转失败',
              icon: 'none'
            });
          }
        });
      });
  },

  /**
   * 返回按钮点击事件
   */
  onBack(): void {
    // // 检查是否可以返回上一页
    // const pages = getCurrentPages();
    // if (pages.length > 1) {
    //   wx.navigateBack();
    // } else {
    //   // 如果没有上一页，跳转到首页
    //   wx.reLaunch({
    //     url: '/pages/patientLogin/patientLogin'
    //   });
    // }
    //默认跳转至患者首页
    var jump = '/pages/patientLogin/patientLogin';
    if(this.data.userType === 'doctor'){
      jump = '/pages/doctorLogin/doctorLogin';
    }
    wx.reLaunch({
      url: jump
    });
  },

  /**
   * 根据用户类型初始化页面
   */
  initPageByUserType(userType: string): void {
    // 设置页面标题
    const title = userType === 'doctor' ? '医生消息' : '消息中心';
    wx.setNavigationBarTitle({
      title: title
    });

    // 可以根据用户类型设置不同的样式或功能
    console.log('初始化页面，用户类型:', userType);
  },

  /**
   * 初始化订阅模板
   */
  initSubscribeTemplates(): void {
    const templateIds = [
      config.UNREAD_MESSAGE_TEMPLATE, // 未读消息通知模板
    ];
    
    this.setData({
      'subscribeStatus.templateIds': templateIds
    });
  },

  /**
   * 检查订阅状态
   */
  async checkSubscribeStatus(): Promise<boolean> {
    try {
      const openId = this.data.openId;
      if (!openId) {
        return false;
      }

      const param = { weixin_id: openId };
      const result: any = await new Promise((resolve, reject) => {
        api.fn_HasSubscription(param, resolve, reject);
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

  /**
   * 显示消息详情（从原onMessageClick中提取的核心逻辑）
   */
  showMessageDetail(message: MessageItem): void {
    // 如果消息未读，标记为已读
    if (!message.isRead) {
      this.markMessageAsRead(message.id);
    }

    // 显示消息详情弹窗
    this.setData({
      currentMessage: message,
      showMessageDetail: true
    });
  },

  /**
   * 保存订阅状态
   */
  async saveSubscribeStatus(subscribeResult: any): Promise<void> {
    try {
      const openId = this.data.openId;
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

      const result: any = await new Promise((resolve, reject) => {
        api.fn_SetSubscription(param, resolve, reject);
      });

      if (result.success) {
        console.log('订阅状态设置成功:', isSubscribed);
      } else {
        console.error('设置订阅状态失败:', result.message);
      }
    } catch (error) {
      console.error('保存订阅状态失败:', error);
    }
  },

  /**
   * 请求订阅授权
   */
  async requestSubscribeMessage(message: MessageItem): Promise<void> {
    const that = this;
    const templateIds = this.data.subscribeStatus.templateIds;

    // 暂存当前消息
    this.setData({
      pendingMessageAction: message
    });

    if (templateIds.length === 0) {
      // 没有模板，直接显示消息详情
      this.showMessageDetail(message);
      return;
    }

    // 直接请求订阅授权
    wx.requestSubscribeMessage({
      tmplIds: templateIds,
      success: function(res: any) {
        console.log('订阅消息授权结果:', res);
        
        // 检查授权结果
        const authorizedTemplates = templateIds.filter(function(id) {
          return res[id] === 'accept';
        });
        const authorizedCount = authorizedTemplates.length;
        
        that.setData({
          'subscribeStatus.requested': true,
          'subscribeStatus.authorized': authorizedCount > 0
        });

        that.saveSubscribeStatus(res);
        
        // 显示授权结果提示
        if (authorizedCount > 0) {
          wx.showToast({
            title: '订阅成功，将及时推送消息通知',
            icon: 'success',
            duration: 2000
          });
        }
        
        // 授权完成后显示消息详情
        that.showMessageDetail(message);
      },
      fail: function(err: any) {
        console.log('订阅消息授权失败:', err);
        
        that.setData({
          'subscribeStatus.requested': true,
          'subscribeStatus.authorized': false
        });

        that.saveSubscribeStatus({ skipped: true });
        
        // 即使订阅失败也继续显示消息详情
        that.showMessageDetail(message);
      }
    });
  },

  /**
   * 根据消息ID加载单个消息并显示详情
   */
  async loadMessageById(messageId: string): Promise<void> {
    if (!messageId) {
      console.error('消息ID为空');
      return;
    }

    try {
      const param = { messageId };
      const result: any = await new Promise((resolve, reject) => {
        // 根据用户类型调用不同API
        const apiMethod = this.data.userType === 'doctor'
          ? api.fn_ViewDoctorMessageContent
          : api.fn_ViewPatientMessageContent;

        apiMethod.call(api, param, resolve, reject);
      });

      if (result.success && result.messageData) {
        // 格式化消息数据（根据用户类型调整字段）
        const message: MessageItem = {
          id: result.messageData.id,
          patientId: this.data.userType === 'doctor' ? result.messageData.doctorId : result.messageData.patientId,
          title: result.messageData.title || '无标题',
          content: result.messageData.content || '',
          createTime: result.messageData.createTime,
          createTimeDisplay: this.formatTime(result.messageData.createTime),
          contentPreview: this.getContentPreview(result.messageData.content || ''),
          type: this.getTypeDisplay(result.messageData.type),
          url: result.messageData.url,
          readTime: result.messageData.readTime,
          isRead: result.messageData.isRead || false
        };

        // 直接显示消息详情
        this.showMessageDetail(message);
      } else {
        wx.showToast({
          title: result.message || '消息不存在',
          icon: 'none'
        });
      }
    } catch (error: any) {
      console.error('根据ID加载消息失败:', error);
      wx.showToast({
        title: '加载消息失败',
        icon: 'none'
      });
    }
  },

  /**
   * 处理URL参数，包括mid参数的处理
   */
  async handleUrlParams(options: any): Promise<void> {
    const mid = options.mid;

    if (mid) {
      // 如果有mid参数，先加载消息列表，然后直接显示对应的消息
      console.log('检测到mid参数:', mid);

      try {
        // 先加载消息列表
        await this.loadMessages(true);

        // 尝试在当前消息列表中查找对应的消息
        const targetMessage = this.data.messageList.find(function(msg) {
          return msg.id === mid;
        });

        if (targetMessage) {
          // 如果在当前列表中找到了消息，直接显示
          console.log('在当前消息列表中找到目标消息');
          this.showMessageDetail(targetMessage);
        } else {
          // 如果在当前列表中没有找到，通过API直接获取消息详情
          console.log('在当前消息列表中未找到目标消息，尝试通过API获取');
          await this.loadMessageById(mid);
        }
      } catch (error) {
        console.error('处理mid参数失败:', error);
        // 如果处理mid失败，仍然加载消息列表
        await this.loadMessages(true);
      }
    } else {
      // 没有mid参数，正常加载消息列表
      await this.loadMessages(true);
    }
  }
});