# FollowUp 数据库升级脚本
# 一键执行结构变更、数据迁移、验证和清理

$ErrorActionPreference = "Stop"

# 设置控制台编码为 UTF-8
$OutputEncoding = [System.Text.Encoding]::UTF8
[Console]::OutputEncoding = [System.Text.Encoding]::UTF8
chcp 65001 | Out-Null

$SCRIPT_DIR = Split-Path -Parent $MyInvocation.MyCommand.Path
$SQL_DIR = $SCRIPT_DIR

# 查找 psql.exe
$psqlPath = $null
$possiblePaths = @(
    "C:\Program Files\PostgreSQL\18\bin\psql.exe",
    "C:\Program Files\PostgreSQL\17\bin\psql.exe",
    "C:\Program Files\PostgreSQL\16\bin\psql.exe",
    "C:\Program Files\PostgreSQL\15\bin\psql.exe"
)

foreach ($path in $possiblePaths) {
    if (Test-Path $path) {
        $psqlPath = $path
        break
    }
}

if (-not $psqlPath) {
    try {
        $null = Get-Command psql -ErrorAction Stop
        $psqlPath = "psql"
    } catch {
        Write-Host "未找到 psql 工具，请检查 PostgreSQL 是否已安装" -ForegroundColor Red
        exit 1
    }
}

Write-Host "使用 psql: $psqlPath" -ForegroundColor Gray
Write-Host ""

# 读取 appsettings.json 中的数据库连接配置
$appsettingsPath = Join-Path (Split-Path $SCRIPT_DIR -Parent) "FollowUp\appsettings.json"

if (-not (Test-Path $appsettingsPath)) {
    Write-Host "未找到配置文件：$appsettingsPath" -ForegroundColor Red
    exit 1
}

try {
    $config = Get-Content $appsettingsPath -Raw -Encoding UTF8 | ConvertFrom-Json
    $connStr = $config.ConnectionStrings.DefaultConnection
    
    # 解析连接字符串
    if ($connStr -match "Host=([^;:]+)(?::(\d+))?") {
        $DB_HOST = $matches[1]
        $DB_PORT = if ($matches[2]) { $matches[2] } else { "5432" }
    }
    if ($connStr -match "Database=([^;]+)") {
        $DB_NAME = $matches[1]
    }
    if ($connStr -match "Username=([^;]+)") {
        $DB_USER = $matches[1]
    }
    if ($connStr -match "Password=([^;]+)") {
        $env:PGPASSWORD = $matches[1]
    }
    
    # 设置 PostgreSQL 客户端编码
    $env:PGCLIENTENCODING = "UTF8"
    
    Write-Host "已从配置文件读取数据库连接参数：" -ForegroundColor Gray
    Write-Host "  Host: $DB_HOST" -ForegroundColor Gray
    Write-Host "  Port: $DB_PORT" -ForegroundColor Gray
    Write-Host "  Database: $DB_NAME" -ForegroundColor Gray
    Write-Host "  User: $DB_USER" -ForegroundColor Gray
    Write-Host ""
} catch {
    Write-Host "解析配置文件失败：$_" -ForegroundColor Red
    exit 1
}

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "FollowUp 数据库升级脚本" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

Write-Host "警告：此操作将修改数据库结构和数据，请确保已做好备份！" -ForegroundColor Red
Write-Host ""
$confirmation = Read-Host "是否继续执行？(输入 YES 继续)"
if ($confirmation -ne "YES") {
    Write-Host "操作已取消" -ForegroundColor Yellow
    exit 0
}

Write-Host ""
Write-Host "开始执行升级脚本..." -ForegroundColor Green
Write-Host ""

$psqlArgs = @(
    "-h", $DB_HOST,
    "-p", $DB_PORT,
    "-U", $DB_USER,
    "-d", $DB_NAME,
    "-f", "$SQL_DIR\upgrade_database.sql"
)

try {
    & $psqlPath $psqlArgs
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host ""
        Write-Host "========================================" -ForegroundColor Green
        Write-Host "数据库升级成功完成！" -ForegroundColor Green
        Write-Host "========================================" -ForegroundColor Green
        Write-Host ""
        
        Write-Host "是否立即清理旧表？(输入 YES 立即清理，按 Enter 跳过)" -ForegroundColor Yellow
        Write-Host "将删除：followup_record, education_history, scan_code_message" -ForegroundColor Gray
        Write-Host ""
        
        $cleanupChoice = Read-Host "确认"
        
        if ($cleanupChoice -eq "YES") {
            $cleanupArgs = @(
                "-h", $DB_HOST,
                "-p", $DB_PORT,
                "-U", $DB_USER,
                "-d", $DB_NAME,
                "-f", "$SQL_DIR\cleanup\900_remove_legacy_followup_tables.sql"
            )
            
            & $psqlPath $cleanupArgs
            
            if ($LASTEXITCODE -eq 0) {
                Write-Host ""
                Write-Host "旧表清理成功！数据库升级流程全部完成！" -ForegroundColor Green
                Write-Host ""
            } else {
                Write-Host ""
                Write-Host "清理失败，但数据迁移已完成" -ForegroundColor Yellow
                Write-Host ""
            }
        } else {
            Write-Host ""
            Write-Host "已跳过清理，稍后可手动执行清理脚本" -ForegroundColor Yellow
            Write-Host ""
        }
    } else {
        throw "psql 执行失败"
    }
} catch {
    Write-Host ""
    Write-Host "数据库升级失败：$_" -ForegroundColor Red
    Write-Host ""
    exit 1
}
