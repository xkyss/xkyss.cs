# 03 交互流程（导航站简版）

## 核心流程

```
点击 ActivityBar 的 ⚡ QuickLaunch
    ↓
进入 QuickLaunch 视图
    ↓
在 SideBar 选择二级分类
    ↓
ContentArea 展示对应卡片
    ↓
点击卡片
    ↓
启动目标（Web/App/Script）
```

---

## 场景 1：打开网页

```
用户进入 QuickLaunch
    ↓
SideBar 选择：工作 / 常用网站
    ↓
右侧出现卡片：[GitHub] [Gmail] [Calendar]
    ↓
用户点击 [GitHub]
    ↓
系统默认浏览器打开 https://github.com
```

---

## 场景 2：打开本地软件

```
用户进入 QuickLaunch
    ↓
SideBar 选择：开发 / 工具
    ↓
右侧出现卡片：[VS Code] [Git Bash]
    ↓
用户点击 [VS Code]
    ↓
系统启动本地程序 Code.exe
```

---

## 场景 3：执行脚本

```
用户进入 QuickLaunch
    ↓
SideBar 选择：开发 / 脚本
    ↓
右侧出现卡片：[npm list] [build-release]
    ↓
用户点击 [npm list]
    ↓
系统执行脚本命令
```

---

## 场景 4：搜索当前分类

```
用户进入：工作 / 常用网站
    ↓
在搜索框输入 "git"
    ↓
右侧卡片过滤为 [GitHub]
    ↓
点击 [GitHub] 启动
```

---

## 交互规则

1. 一级分类
- 仅负责展开/折叠二级分类

2. 二级分类
- 负责决定右侧显示哪一组卡片

3. 卡片
- 单击即启动
- 可选右键菜单（启动/编辑/删除）

4. 搜索
- 只过滤当前二级分类，不做全局检索

---

## 异常与反馈

1. 启动失败
- 状态栏提示：启动失败 + 原因

2. 目标不存在
- 卡片显示错误提示
- 引导用户编辑目标路径

3. 当前分类无卡片
- 显示空状态：该分类暂无启动项
