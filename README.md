# FollowUp

## 开发环境配置

### HTTPS 证书配置（必需）

首次克隆项目或遇到浏览器提示"不安全"时，需要配置本地开发 HTTPS 证书。

#### 方法一：自动配置（推荐）

1. 以**管理员权限**打开 PowerShell
2. 进入项目根目录
3. 执行配置脚本：

```powershell
.\setup-dev-cert.ps1
```

4. 当弹出安全警告对话框时，点击【是】信任证书
5. 完全关闭 Visual Studio 和浏览器
6. 重新启动 Visual Studio 并运行项目

#### 方法二：手动配置

如果自动脚本无法运行，可以手动执行以下命令：

```powershell
# 1. 清理现有证书
dotnet dev-certs https --clean

# 2. 生成并信任新证书（会弹出对话框，点击"是"）
dotnet dev-certs https --trust

# 3. 验证证书状态
dotnet dev-certs https --check --trust
```

执行完成后，同样需要重启 Visual Studio 和浏览器。

#### 验证配置成功

配置成功后，访问 `https://localhost:xxxx` 时：
- ✓ 浏览器地址栏显示 🔒 锁形图标
- ✓ 不再出现"您的连接不是私密连接"等警告
- ✓ 可以正常访问项目

#### 常见问题

**Q: 为什么每个开发者都需要配置？**
A: HTTPS 证书存储在本地系统证书存储区，不会通过 Git 同步，因此每个开发者需要在自己的机器上配置一次。

**Q: 配置后仍然提示不安全？**
A: 请确保已完全关闭并重启 Visual Studio 和浏览器。某些情况下可能需要重启计算机。

**Q: 脚本执行失败？**
A: 请确保以管理员权限运行 PowerShell，并且已安装 .NET SDK。

## 项目运行

1. 使用 Visual Studio 2022 打开解决方案
2. 确保已完成 HTTPS 证书配置（见上文）
3. 按 F5 或点击运行按钮启动项目

---

更多帮助和文档请联系团队成员。
