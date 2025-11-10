# FollowUp 开发环境 HTTPS 证书配置脚本
# 此脚本用于自动配置和信任 .NET 开发 HTTPS 证书

Write-Host "================================================" -ForegroundColor Cyan
Write-Host "FollowUp 开发环境 HTTPS 证书配置工具" -ForegroundColor Cyan
Write-Host "================================================" -ForegroundColor Cyan
Write-Host ""

# 检查是否以管理员权限运行
$isAdmin = ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
if (-not $isAdmin) {
    Write-Host "警告: 建议以管理员权限运行此脚本以确保证书正确安装" -ForegroundColor Yellow
    Write-Host ""
}

# 步骤 1: 清理现有证书
Write-Host "[1/3] 清理现有的开发证书..." -ForegroundColor Green
try {
    dotnet dev-certs https --clean
    Write-Host "✓ 证书清理完成" -ForegroundColor Green
} catch {
    Write-Host "× 证书清理失败: $_" -ForegroundColor Red
    exit 1
}
Write-Host ""

# 步骤 2: 生成并信任新证书
Write-Host "[2/3] 生成并信任新的开发证书..." -ForegroundColor Green
Write-Host "注意: 即将弹出安全警告对话框，请点击【是】以信任证书" -ForegroundColor Yellow
try {
    dotnet dev-certs https --trust
    Write-Host "✓ 证书生成并信任成功" -ForegroundColor Green
} catch {
    Write-Host "× 证书配置失败: $_" -ForegroundColor Red
    exit 1
}
Write-Host ""

# 步骤 3: 验证证书状态
Write-Host "[3/3] 验证证书配置状态..." -ForegroundColor Green
try {
    $output = dotnet dev-certs https --check --trust 2>&1
    Write-Host $output
    Write-Host "✓ 证书验证完成" -ForegroundColor Green
} catch {
    Write-Host "× 证书验证失败: $_" -ForegroundColor Red
    exit 1
}
Write-Host ""

# 完成提示
Write-Host "================================================" -ForegroundColor Cyan
Write-Host "✓ HTTPS 证书配置完成！" -ForegroundColor Green
Write-Host "================================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "接下来请执行以下步骤:" -ForegroundColor Yellow
Write-Host "1. 完全关闭 Visual Studio（确保所有实例已关闭）" -ForegroundColor White
Write-Host "2. 关闭所有浏览器窗口" -ForegroundColor White
Write-Host "3. 重新打开 Visual Studio 并运行 FollowUp 项目" -ForegroundColor White
Write-Host "4. 浏览器应该不再显示'不安全'警告" -ForegroundColor White
Write-Host ""
Write-Host "如有问题，请联系团队成员协助解决。" -ForegroundColor Cyan
Write-Host ""

# 等待用户确认
Write-Host "按任意键退出..." -ForegroundColor Gray
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
