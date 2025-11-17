# FollowUp 随访管理系统 UI 设计指南

> **版本**: 1.0
> **更新日期**: 2025-01-17
> **设计主题**: 深色医疗风 (Deep Medical Style)

---

## 📋 目录

1. [设计理念](#设计理念)
2. [色彩系统](#色彩系统)
3. [排版系统](#排版系统)
4. [间距系统](#间距系统)
5. [圆角规范](#圆角规范)
6. [阴影层次](#阴影层次)
7. [组件规范](#组件规范)
8. [布局原则](#布局原则)
9. [动画交互](#动画交互)
10. [响应式设计](#响应式设计)
11. [无障碍设计](#无障碍设计)

---

## 设计理念

### 核心原则

1. **专业医疗感**
   - 使用深色医疗绿作为主色调，传递专业、可靠的品牌形象
   - 高对比度设计，确保重要信息清晰可读
   - 简洁克制的视觉语言，避免过度装饰

2. **信息层次清晰**
   - 通过字号、字重、颜色建立清晰的视觉层级
   - 重要数据突出显示（大字号、深色）
   - 次要信息适度弱化（小字号、浅色）

3. **一致性优先**
   - 统一的间距、圆角、阴影规范
   - 所有组件遵循相同的设计语言
   - 颜色使用 CSS 变量，易于维护和主题切换

4. **交互友好**
   - 适度的动画和过渡效果提升体验
   - 明确的悬浮、激活状态反馈
   - 符合用户直觉的操作流程

---

## 色彩系统

### 主色调 - 深色医疗绿

```css
--primary-color: #0A6B58;       /* 主题色 - 深医疗绿 */
--primary-light: #18B797;       /* 浅色版 - 悬浮、高亮 */
--primary-lighter: #D6F2ED;     /* 极浅版 - 背景、标签 */
--primary-dark: #085A4A;        /* 深色版 - 按钮按下 */
--primary-darker: #064D3F;      /* 极深版 - 导航菜单 */
```

**使用场景**：
- 顶栏背景、侧边栏背景
- 主按钮、链接
- 重要数据数值
- 激活状态背景

**对比度要求**：
- 白色文字在主色调背景上：**7.5:1** (WCAG AAA)
- 主色调文字在白色背景上：**7.2:1** (WCAG AAA)

---

### 次要色 - 深蓝

```css
--secondary-color: #1E4A6C;     /* 次要色 - 深专业蓝 */
```

**使用场景**：
- 辅助性信息
- 图表配色
- 次要按钮

---

### 功能色 - 状态指示

```css
--success-color: #2F855A;       /* 成功 - 深绿 */
--warning-color: #DD6B20;       /* 警告 - 深橙 */
--error-color: #C53030;         /* 错误 - 深红 */
--info-color: #2B6CB0;          /* 信息 - 深蓝 */
```

**使用场景**：
- 成功：提交成功、审核通过
- 警告：待审核、即将超时
- 错误：提交失败、已超时
- 信息：系统提示、帮助文档

---

### 中性色 - 文字与背景

```css
/* 文字颜色 */
--text-primary: #1A202C;        /* 主文本 - 深灰（高对比度）*/
--text-secondary: #4A5568;      /* 次要文本 - 中灰 */
--text-tertiary: #A0AEC0;       /* 辅助文本 - 浅灰 */

/* 背景颜色 */
--bg-primary: #F5F7FA;          /* 主背景 - 淡灰 */
--bg-secondary: #FFFFFF;        /* 卡片背景 - 纯白 */
--bg-tertiary: #EDF2F7;         /* 次要背景 - 浅灰 */

/* 边框颜色 */
--border-color: #E2E8F0;        /* 标准边框 */
--border-light: #EDF2F7;        /* 浅色边框 */
```

**使用规则**：
- 标题使用 `text-primary`
- 正文使用 `text-secondary`
- 提示文字使用 `text-tertiary`
- 页面背景使用 `bg-primary`
- 卡片背景使用 `bg-secondary`
- 分组背景使用 `bg-tertiary`

---

### 色彩使用禁忌

❌ **禁止**：
- 不要直接使用十六进制颜色值（硬编码）
- 不要使用低对比度的颜色组合（< 4.5:1）
- 不要在医疗系统中使用鲜艳、刺眼的颜色

✅ **推荐**：
- 始终使用 CSS 变量定义的颜色
- 确保所有文字与背景对比度 ≥ 4.5:1
- 使用深色、沉稳的色调

---

## 排版系统

### 字体家族

```css
--font-family-base: -apple-system, BlinkMacSystemFont, "Segoe UI",
                     Roboto, "Helvetica Neue", Arial, sans-serif;
```

**说明**：使用系统默认字体栈，确保跨平台一致性和最佳性能。

---

### 字号标尺

```css
--font-size-xs: 12px;           /* 辅助信息、标签 */
--font-size-sm: 13px;           /* 次要文本 */
--font-size-base: 15px;         /* 正文基准 */
--font-size-md: 16px;           /* 强调文本 */
--font-size-lg: 18px;           /* 小标题 */
--font-size-xl: 20px;           /* 数据数值 */
--font-size-2xl: 22px;          /* 关键数据 */
--font-size-3xl: 24px;          /* 页面标题 */
```

**使用场景**：

| 字号 | 使用场景 | 示例 |
|------|---------|------|
| 12px | 表格辅助信息、时间戳 | "2025-01-17 10:30" |
| 13px | 数据标签、次要文本 | "今日新增" |
| 15px | 正文、列表项 | 段落文字 |
| 16px | 卡片标题 | "平台用户数据" |
| 18px | 统计数值（次要） | "123" |
| 20px | 统计数值（标准） | "456" |
| 22px | 统计数值（关键） | "789" |
| 24px | 页面主标题 | "课题概览" |

---

### 字重标尺

```css
--font-weight-normal: 400;      /* 正文 */
--font-weight-medium: 500;      /* 强调 */
--font-weight-semibold: 600;    /* 小标题 */
--font-weight-bold: 700;        /* 数据数值 */
```

**使用规则**：
- 正文使用 `400`
- 导航链接、按钮使用 `500`
- 卡片标题、激活状态使用 `600`
- 数据数值使用 `700`

---

### 行高标尺

```css
--line-height-tight: 1.25;      /* 标题 */
--line-height-base: 1.6;        /* 正文 */
--line-height-relaxed: 1.75;    /* 长文本 */
```

**使用规则**：
- 页面标题使用 `1.25`
- 段落文字使用 `1.6`
- 帮助文档使用 `1.75`

---

## 间距系统

### 间距标尺

```css
--spacing-xs: 4px;              /* 微小间距 */
--spacing-sm: 8px;              /* 小间距 */
--spacing-md: 16px;             /* 中等间距（基准）*/
--spacing-lg: 24px;             /* 大间距 */
--spacing-xl: 32px;             /* 超大间距 */
--spacing-2xl: 48px;            /* 巨大间距 */
```

### 使用规则

**组件内部间距**：
- 标签与数值：`4px` (xs)
- 数据项间距：`8px` (sm)
- 卡片内边距：`16px` (md)
- 分组间距：`24px` (lg)

**组件外部间距**：
- 卡片间距：`16px` (md)
- 模块间距：`24px` (lg)
- 区块间距：`32px` (xl)

**示例代码**：
```css
/* 推荐 */
.card {
    padding: var(--spacing-md);
    margin-bottom: var(--spacing-lg);
}

/* 不推荐 */
.card {
    padding: 16px;  /* 硬编码 */
    margin-bottom: 20px;  /* 非标准值 */
}
```

---

## 圆角规范

### 圆角标尺

```css
--radius-sm: 4px;               /* 小圆角 - 按钮、输入框 */
--radius-md: 6px;               /* 中圆角 - 卡片 */
--radius-lg: 8px;               /* 大圆角 - 对话框 */
--radius-full: 9999px;          /* 完全圆角 - 徽章、头像 */
```

### 使用规则

| 组件类型 | 圆角大小 | CSS 变量 |
|---------|---------|---------|
| 按钮 | 4px | `--radius-sm` |
| 输入框 | 4px | `--radius-sm` |
| 卡片 | 6px | `--radius-md` |
| 对话框 | 6px | `--radius-md` |
| 下拉菜单 | 6px | `--radius-md` |
| 工具提示 | 4px | `--radius-sm` |
| 徽章 | 圆形 | `--radius-full` |

**设计原则**：
- 小圆角（4-6px）营造专业、克制的医疗感
- 避免使用大圆角（> 12px），过于轻松不适合医疗场景
- 保持全局一致性

---

## 阴影层次

### 阴影标尺

```css
--shadow-sm: 0 1px 3px rgba(0, 0, 0, 0.04), 0 1px 2px rgba(0, 0, 0, 0.02);
--shadow-md: 0 4px 6px rgba(0, 0, 0, 0.05), 0 2px 4px rgba(0, 0, 0, 0.03);
--shadow-lg: 0 10px 15px rgba(0, 0, 0, 0.06), 0 4px 6px rgba(0, 0, 0, 0.04);
--shadow-xl: 0 20px 25px rgba(0, 0, 0, 0.08), 0 10px 10px rgba(0, 0, 0, 0.04);
--shadow-hover: 0 8px 16px rgba(10, 107, 88, 0.15);  /* 悬浮阴影 - 带主色调 */
```

### 使用规则

| 层级 | 阴影大小 | 使用场景 |
|-----|---------|---------|
| 1 | `shadow-sm` | 卡片默认状态 |
| 2 | `shadow-md` | 按钮、输入框 |
| 3 | `shadow-lg` | 悬浮卡片、下拉菜单 |
| 4 | `shadow-xl` | 对话框、抽屉 |
| 悬浮 | `shadow-hover` | 交互元素悬浮 |

**设计原则**：
- 阴影要轻、要浅，避免厚重感
- 悬浮状态使用带主色调的阴影，增强品牌感
- 层级越高，阴影越明显

---

## 组件规范

### 1. 顶栏 (AppBar)

**规范**：
```css
背景色: var(--primary-color)           /* 深医疗绿 */
文字颜色: #FFFFFF                      /* 纯白 */
高度: 64px
内边距: 0 16px
边框: 1px solid var(--primary-dark)   /* 底部边框 */
阴影: 0 2px 8px rgba(0, 0, 0, 0.15)
```

**按钮样式**：
```css
默认: color: #FFFFFF
悬浮: background: rgba(255, 255, 255, 0.2)
激活: background: rgba(255, 255, 255, 0.3)
```

---

### 2. 侧边栏 (Sidebar)

**规范**：
```css
背景色: linear-gradient(180deg, #0A6B58 0%, #043D32 100%)
文字颜色: #FFFFFF                      /* 纯白 */
图标颜色: #FFFFFF                      /* 纯白，使用 color 属性 */
宽度: 260px (展开) / 64px (收起)
内边距: 8px
边框: 1px solid #064D3F (右侧)
```

**导航链接样式**：
```css
默认: background: transparent
悬浮: background: rgba(255, 255, 255, 0.15)
激活: background: rgba(255, 255, 255, 0.25), font-weight: 600
```

**激活指示条**：
```css
位置: 左侧
宽度: 4px
颜色: #FFFFFF
```

---

### 3. 卡片 (Card)

**规范**：
```css
背景色: var(--bg-secondary)            /* 纯白 */
圆角: var(--radius-md)                /* 6px */
边框: 1px solid var(--border-light)
阴影: var(--shadow-sm)                /* 默认浅阴影 */
内边距: 16px - 24px (根据内容密度)
```

**悬浮效果**：
```css
阴影: var(--shadow-lg)
transform: translateY(-2px)
过渡: all 0.3s ease-out
```

---

### 4. 按钮 (Button)

**主按钮 (Primary)**：
```css
背景色: var(--primary-color)
文字颜色: #FFFFFF
圆角: var(--radius-sm)                /* 4px */
内边距: 8px 16px
高度: 36px
字号: 15px
字重: 500

悬浮: background: var(--primary-dark)
激活: background: var(--primary-darker)
```

**次要按钮 (Secondary)**：
```css
背景色: transparent
文字颜色: var(--primary-color)
边框: 1px solid var(--primary-color)

悬浮: background: var(--primary-lighter)
```

**文字按钮 (Text)**：
```css
背景色: transparent
文字颜色: var(--primary-color)

悬浮: background: var(--primary-lighter)
```

---

### 5. 输入框 (Input)

**规范**：
```css
背景色: #FFFFFF
圆角: var(--radius-sm)                /* 4px */
边框: 1px solid var(--border-color)
高度: 40px
内边距: 8px 12px
字号: 15px

焦点: border-color: var(--primary-color)
      box-shadow: 0 0 0 3px var(--primary-light)
```

---

### 6. 数据面板 (Data Panel)

**标题区**：
```css
内边距: 8px (pa-2)
字号: 16px (Typo.h6)
字重: 600
颜色: var(--text-primary)
```

**数据分组**：
```css
内边距: 8px 12px
圆角: var(--radius-sm)               /* 4px */
边框: 1px solid var(--border-color)
间距: 8px (组间)
```

**数据项**：
```css
/* 标签 */
字号: 12px - 13px
颜色: var(--text-secondary)
字重: 500

/* 数值 */
字号: 18px - 22px (根据重要性)
颜色: var(--primary-color)
字重: 700
```

**分组背景**：
```css
第一组（关键统计）: var(--bg-tertiary)    /* 浅灰 */
第二组（详细统计）: var(--bg-primary)     /* 极浅灰 */
```

---

### 7. 图表容器 (Chart Panel)

**规范**：
```css
背景色: var(--bg-secondary)
圆角: var(--radius-md)                /* 6px */
边框: 1px solid var(--border-light)
最小高度: 350px
内边距: 12px - 16px

布局: display: flex; flex-direction: column;
```

**头部（Tab + 时间选择器）**：
```css
最小高度: 40px
内边距: 8px 12px
flex-shrink: 0                       /* 固定高度 */
```

**主体（图表区域）**：
```css
flex: 1                              /* 占据剩余空间 */
最小高度: 260px
overflow: hidden                     /* 防止溢出 */
```

---

## 布局原则

### 1. 页面容器

```css
背景色: var(--bg-primary)             /* 淡灰 */
内边距: 16px - 20px
最小高度: calc(100vh - 64px)         /* 减去顶栏高度 */
```

---

### 2. 栅格系统

使用 Bootstrap 12 列栅格系统：

```html
<div class="row">
    <div class="col-4">左侧 (33%)</div>
    <div class="col-8">右侧 (67%)</div>
</div>
```

**常用比例**：
- `col-3` / `col-9` (25% / 75%) - 侧边栏 + 主内容
- `col-4` / `col-8` (33% / 67%) - 图表 + 数据面板
- `col-6` / `col-6` (50% / 50%) - 双列布局

---

### 3. 间距规则

**卡片间距**：
```css
.row {
    gap: 16px;  /* 或使用 Bootstrap 的 g-3 */
}

.card + .card {
    margin-top: 16px;
}
```

**模块间距**：
```css
.module + .module {
    margin-top: 24px;
}
```

---

### 4. 分组布局

**横向分组（数据面板）**：
```css
.data-group {
    display: flex;
    justify-content: space-between;
    gap: 12px;                       /* 项目间距 */
}

.data-item {
    flex: 1;                         /* 等宽分布 */
}
```

**纵向分组（表单）**：
```css
.form-group {
    display: flex;
    flex-direction: column;
    gap: 8px;                        /* 字段间距 */
}
```

---

## 动画交互

### 1. 过渡时长

```css
--transition-fast: 150ms;           /* 快速交互 - 按钮点击 */
--transition-base: 300ms;           /* 标准交互 - 悬浮、展开 */
--transition-slow: 500ms;           /* 慢速交互 - 页面切换 */
```

---

### 2. 缓动函数

```css
--ease-out: cubic-bezier(0.4, 0, 0.2, 1);      /* 减速 - 悬浮 */
--ease-in-out: cubic-bezier(0.4, 0, 0.6, 1);   /* 加速-减速 - 对话框 */
--ease-bounce: cubic-bezier(0.68, -0.55, 0.265, 1.55);  /* 弹性 - 按钮 */
```

---

### 3. 常用动画

**悬浮效果**：
```css
.card:hover {
    transform: translateY(-2px);
    box-shadow: var(--shadow-hover);
    transition: all var(--transition-base) var(--ease-out);
}
```

**按钮点击**：
```css
.button:active {
    transform: scale(0.95);
    transition: transform var(--transition-fast) var(--ease-out);
}
```

**页面进入**：
```css
@keyframes pageEnter {
    from {
        opacity: 0;
        transform: translateY(8px);
    }
    to {
        opacity: 1;
        transform: translateY(0);
    }
}

.page-content {
    animation: pageEnter var(--transition-slow) var(--ease-out);
}
```

**对话框弹出**：
```css
@keyframes scaleIn {
    from {
        opacity: 0;
        transform: scale(0.95);
    }
    to {
        opacity: 1;
        transform: scale(1);
    }
}

.dialog {
    animation: scaleIn var(--transition-base) var(--ease-out);
}
```

---

### 4. 动画使用原则

✅ **推荐**：
- 使用轻微、自然的动画
- 过渡时长控制在 150-500ms
- 使用缓动函数，避免线性动画

❌ **禁止**：
- 过度的动画效果（如旋转、缩放 > 1.1）
- 超长的过渡时长（> 1s）
- 复杂的关键帧动画（影响性能）

---

## 响应式设计

### 断点标准

```css
/* 移动设备 */
@media (max-width: 576px) { ... }

/* 平板设备 */
@media (min-width: 577px) and (max-width: 768px) { ... }

/* 桌面设备 */
@media (min-width: 769px) { ... }
```

---

### 适配原则

**移动端（< 576px）**：
- 侧边栏默认收起
- 卡片垂直堆叠（col-12）
- 字号适当缩小
- 间距减小（16px → 8px）

**平板端（577px - 768px）**：
- 侧边栏可切换
- 卡片 2 列布局（col-6）
- 保持桌面端字号

**桌面端（> 769px）**：
- 侧边栏默认展开
- 卡片多列布局（col-3, col-4）
- 完整的交互效果

---

## 无障碍设计

### 1. 对比度要求

**WCAG AAA 标准 (7:1)**：
- 主文本与背景：≥ 7:1
- 次要文本与背景：≥ 4.5:1

**检查工具**：
```
在线工具: https://webaim.org/resources/contrastchecker/
浏览器插件: axe DevTools
```

---

### 2. 焦点可见性

```css
*:focus-visible {
    outline: 2px solid var(--primary-color);
    outline-offset: 2px;
    border-radius: var(--radius-sm);
}
```

---

### 3. 键盘导航

**要求**：
- 所有交互元素可通过 Tab 键访问
- 导航菜单支持方向键
- 对话框支持 Esc 关闭

---

### 4. 屏幕阅读器

**语义化 HTML**：
```html
<!-- 推荐 -->
<nav aria-label="主导航">
    <button aria-label="打开菜单">...</button>
</nav>

<!-- 不推荐 -->
<div onclick="openMenu()">...</div>
```

---

## 附录：设计检查清单

### 新增组件时检查

- [ ] 是否使用 CSS 变量定义颜色？
- [ ] 对比度是否符合 WCAG 标准？
- [ ] 圆角是否统一（4px 或 6px）？
- [ ] 间距是否使用标准值（8px、16px、24px）？
- [ ] 阴影是否使用预定义变量？
- [ ] 是否添加悬浮和激活状态？
- [ ] 动画时长是否合理（150-500ms）？
- [ ] 是否支持键盘导航？
- [ ] 移动端是否适配？

---

### 代码审查时检查

- [ ] 是否有硬编码的颜色值？
- [ ] 是否有非标准的间距值？
- [ ] 是否有过长的过渡时长？
- [ ] 是否有低对比度的文字？
- [ ] 是否有过度复杂的动画？

---

## 结语

本设计指南是 FollowUp 系统 UI 的核心规范，旨在：

1. **确保一致性**：所有页面、组件遵循统一的设计语言
2. **提升效率**：设计师和开发者有明确的规范可循
3. **保证质量**：通过标准化流程，减少设计债务
4. **易于维护**：使用 CSS 变量，便于主题切换和迭代

**持续改进**：
- 定期审查设计规范的合理性
- 根据用户反馈优化交互细节
- 随着项目演进，及时更新文档

---

**文档维护者**: Claude Code
**最后更新**: 2025-01-17
**版本历史**:
- v1.0 (2025-01-17): 初始版本，定义深色医疗风设计系统
