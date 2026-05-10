# QuickLaunch 设计文档（简版）

本版方案已收敛为：

1. ActivityBar：QuickLaunch 只有一个入口按钮
2. SideBar：两级分类导航
3. ContentArea：卡片展示与启动

不再引入复杂 Settings 后台流程。

---

## 文档索引

1. [总览](00-overview.md)
- 目标、范围、最小功能、验收标准

2. [界面设计](02-ui-design-v2.md)
- ASCII 布局图
- SideBar 两级结构
- ContentArea 卡片网格

3. [交互流程](03-interactions.md)
- 打开网页/软件/脚本
- 当前分类搜索
- 异常反馈

4. [数据模型](01-functionality.md)
- LaunchItem / LaunchCategory / LaunchConfig

5. [配置示例](05-config-example.json)
- 默认分类与启动项示例

---

## MVP 边界

必须做：
1. 两级分类导航
2. 卡片启动（Web/App/Script）
3. 当前分类搜索
4. 配置持久化

先不做：
1. 复杂设置页面
2. 拖拽排序
3. 使用统计
4. 全局命令面板

---

## 一句话定义

QuickLaunch 就是一个插件化导航站：左侧选分类，右侧点卡片，立即启动。
