# Card 组件设计（MewUI 版）

## 1. 设计目标

参考 Ant Design Card 的能力模型，结合 MewUI 当前 DSL 与主题体系，设计一个可复用、可组合、可扩展的通用卡片组件。

目标：

- 同时支持信息展示、操作入口、容器嵌套三类场景
- 提供稳定的语义槽位（Header/Cover/Body/Actions/Tabs/Meta/Grid）
- 支持轻量默认用法，也支持深度自定义
- 与 MewUI 现有模式一致：组合优先、主题驱动、链式 API 友好

非目标：

- 本文不包含具体实现代码
- 本文不绑定业务样式，不内置业务字段模型

已确认决策：

- 第一阶段包含 `CardMeta`
- 第一阶段包含 `CardGrid`
- 第一阶段采用 Tabs 内建
- 动作区布局采用自适应（不强制等宽）
- 第一阶段变体限定为 `Outlined` + `Borderless`

---

## 2. 适用场景

- 概览页信息块（统计、摘要、状态）
- 启动入口卡片（图标 + 名称 + 描述 + 操作）
- 配置面板中的分区容器
- 列表项增强展示（媒体卡片、对象卡片）
- 嵌套信息结构（外层卡片 + Inner Card）

---

## 3. 组件家族

建议设计为以下组件族：

- `Card`：主容器
- `CardMeta`：头像/标题/描述语义块
- `CardActionItem`：底部动作项模型（可选）
- `CardTabItem`：页签项模型（可选）
- `CardGrid`：网格子卡片容器

说明：`CardGrid` 可以是 `Card` 的子类型，也可以是 `Card` 的静态子构造器；最终以 MewUI 现有代码风格为准。

---

## 4. 语义结构（Semantic Slots）

### 4.1 主结构

```text
Card(root)
+-- Header (optional)
|   +-- Title
|   +-- Extra
+-- Cover (optional)
+-- Tabs (optional)
+-- Body (required, default empty container)
+-- Actions (optional)
```

### 4.2 Meta 结构

```text
CardMeta(root)
+-- Avatar (optional)
+-- Section
    +-- Title
    +-- Description
```

### 4.3 Grid 结构

```text
Card(root)
+-- Body
    +-- UniformGrid / WrapPanel
        +-- CardGridItem
        +-- CardGridItem
        +-- ...
```

---

## 5. ASCII 布局图

### 5.1 典型卡片（标题 + 内容 + 底部操作）

```text
+------------------------------------------------------+
| Card Title                                [More]     |
|------------------------------------------------------|
| Body Content                                         |
| - text paragraph                                     |
| - key/value summary                                 |
| - embedded custom element                            |
|------------------------------------------------------|
| [Edit]                [Share]                [Open]  |
+------------------------------------------------------+
```


### 5.2 含封面与 Meta 的对象卡片

```text
+------------------------------------------------------+
| [                    Cover Image                   ] |
|------------------------------------------------------|
| (Avatar)  Meta Title                                 |
|           Meta description text...                   |
|------------------------------------------------------|
| [Like]                         [More]                |
+------------------------------------------------------+
```

### 5.3 内部卡片（Inner）

```text
+------------------------------------------------------+
| Parent Card Title                                    |
|------------------------------------------------------|
| +-----------------------------------------------+    |
| | Inner Title                           [More]  |    |
| |-----------------------------------------------|    |
| | Inner content                                 |    |
| +-----------------------------------------------+    |
+------------------------------------------------------+
```

### 5.4 带页签的卡片

```text
+------------------------------------------------------+
| Card Title                                  [Ops]    |
|------------------------------------------------------|
| [Tab A] [Tab B] [Tab C]                              |
|------------------------------------------------------|
| Active tab content                                   |
+------------------------------------------------------+
```

### 5.5 网格型卡片

```text
+------------------------------------------------------+
| Dashboard Card                                       |
|------------------------------------------------------|
| +----------------+ +----------------+ +------------+ |
| | Grid Item 1    | | Grid Item 2    | | Item 3     | |
| +----------------+ +----------------+ +------------+ |
| +----------------+ +----------------+ +------------+ |
| | Grid Item 4    | | Grid Item 5    | | Item 6     | |
| +----------------+ +----------------+ +------------+ |
+------------------------------------------------------+
```

### 5.6 Url卡片

```text
+------------------------------------------------------+
| Url名称                                     分类      |
|------------------------------------------------------|
| (Icon)    Url描述                                    |
|           Url地址                                    |
|------------------------------------------------------|
| [打开]                                       [编辑]  |
+------------------------------------------------------+
```

### 5.7 本地软件卡片

```text
+------------------------------------------------------+
| 软件名称                                     分类     |
|------------------------------------------------------|
| (Icon)    软件描述                                    |
|           软件启动命令                                |
|------------------------------------------------------|
| [打开]                                       [编辑]  |
+------------------------------------------------------+
```

---

## 6. API 设计草案

以下是建议 API 形态（非最终代码，仅用于评审接口通用性）。

### 6.1 Card 核心属性

| 属性 | 类型 | 默认值 | 说明 |
|---|---|---:|---|
| `Title` | `object?` | `null` | 头部标题，可为文本或任意元素 |
| `Extra` | `FrameworkElement?` | `null` | 右上角扩展区域 |
| `Cover` | `FrameworkElement?` | `null` | 封面区域 |
| `Body` | `FrameworkElement?` | `null` | 主体内容 |
| `Actions` | `IList<FrameworkElement>` | 空 | 底部动作集合 |
| `Tabs` | `IList<CardTabItem>` | 空 | 页签项 |
| `ActiveTabKey` | `string?` | `null` | 当前激活页签 |
| `DefaultActiveTabKey` | `string?` | 首项 | 非受控初始激活页签 |
| `Loading` | `bool` | `false` | 预加载占位状态 |
| `Hoverable` | `bool` | `false` | 是否悬浮抬升 |
| `Variant` | `CardVariant` | `Outlined` | 外观变体 |
| `Size` | `CardSize` | `Medium` | 尺寸规格 |
| `Type` | `CardType` | `Default` | `Default` / `Inner` |
| `Bordered` | `bool` | `true` | 与 `Variant` 协同，兼容旧语义 |
| `Disabled` | `bool` | `false` | 禁用视觉与交互 |

### 6.2 Card 事件

| 事件 | 参数 | 说明 |
|---|---|---|
| `TabChanged` | `string key` | 页签切换回调 |
| `ActionInvoked` | `int index` 或模型 | 动作被触发 |
| `Clicked` | `EventArgs` | 卡片主区域点击（可选） |

### 6.3 Fluent API 建议

```csharp
Card.Create()
    .Title("Card Title")
    .Extra(new Button { Content = new Label { Text = "More" } })
    .Cover(coverElement)
    .Body(bodyElement)
    .Actions(editButton, shareButton, openButton)
    .Tabs(tabItems, activeKey: "overview")
    .Hoverable(true)
    .Variant(CardVariant.Outlined)
    .Size(CardSize.Medium)
    .OnTabChange(key => { /* ... */ });
```

注：链式 API 与属性 API 应可混用，避免强制单一范式。

### 6.4 CardMeta API 建议

| 属性 | 类型 | 默认值 | 说明 |
|---|---|---:|---|
| `Avatar` | `FrameworkElement?` | `null` | 头像/图标 |
| `Title` | `object?` | `null` | 标题 |
| `Description` | `object?` | `null` | 描述 |

---

## 7. 变体与尺寸规范

### 7.1 视觉变体

- `Outlined`：默认描边卡片
- `Borderless`：无边框，用于灰底容器中
- `Filled`：轻填充背景强调（不进入第一阶段）

### 7.2 类型

- `Default`：普通卡片
- `Inner`：用于嵌套，头部高度与间距更紧凑

### 7.3 尺寸

- `Small`：紧凑信息密集场景
- `Medium`：默认
- `Large`（可选）：面向内容型展示

推荐基准（可由 Theme Token 覆盖）：

| Token | Small | Medium | 说明 |
|---|---:|---:|---|
| `HeaderHeight` | 38 | 56 | 头部高度 |
| `HeaderPadding` | 12 | 24 | 头部内边距 |
| `BodyPadding` | 12 | 24 | 主体内边距 |
| `HeaderFontSize` | 14 | 16 | 标题字号 |

---

## 8. 状态模型

建议状态集合：

- `Normal`
- `Hover`
- `Pressed`（可选）
- `Focused`
- `Selected`（当卡片可选）
- `Disabled`
- `Loading`

状态优先级建议（由高到低）：

`Disabled > Loading > Pressed > Hover > Focused > Normal`

ASCII 状态转移图：

```text
        +---------+
        | Normal  |
        +----+----+
             | pointer enter
             v
        +----+----+
        |  Hover  |
        +----+----+
             | press
             v
        +----+----+
        | Pressed |
        +----+----+
             | release
             v
        +----+----+
        | Focused |
        +----+----+
             |
             v
          Normal

Disabled/Loading 可从任意态切入并屏蔽交互。
```

---

## 9. 主题与 Design Tokens

与 MewUI 主题系统对齐，禁止在组件内部硬编码颜色。建议暴露 `CardTokens`（可挂接 ThemeManager）。

| Token | 说明 |
|---|---|
| `CardBackground` | 卡片背景 |
| `CardBorderColor` | 边框色 |
| `CardRadius` | 圆角 |
| `CardShadow` | 阴影（hoverable 时增强） |
| `HeaderBackground` | 头部背景 |
| `HeaderTextColor` | 头部文字色 |
| `BodyTextColor` | 主体文字色 |
| `ExtraTextColor` | Extra 区文字色 |
| `ActionsBackground` | 动作区背景 |
| `ActionsBorderColor` | 动作区上边框 |
| `TabsMarginBottom` | 内置 Tabs 与 Body 间距 |
| `LoadingSkeletonColor` | 骨架色 |

实现原则：

- 默认值从 `theme.Palette` 映射
- 组件级 token 可覆盖全局 token
- `WithTheme((t, c) => ...)` 仅做最终渲染绑定，不在业务逻辑层写颜色

---

## 10. 交互规则

1. 点击卡片主区：
- 若绑定 `Clicked`，触发回调
- 若存在可聚焦子控件，不抢占其输入焦点

2. 点击 Action：
- 仅触发对应 action，不冒泡为卡片主点击（默认）

3. Tab 切换：
- 更新 `ActiveTabKey`
- 触发 `TabChanged`
- 仅刷新 Body 内容，Header/Cover/Actions 保持稳定

4. Loading：
- 内容区显示骨架占位
- Action 与 Tab 可配置是否禁用

5. Hoverable：
- 鼠标移入时抬升/阴影增强
- 触屏环境自动降级为静态样式

### 10.1 Tabs 内建 vs 外部组合（说明）

`Tabs 内建`：页签条由 `Card` 自己渲染，使用 `Tabs`、`ActiveTabKey`、`TabChanged` 等属性。

优点：

- 接口更集中，业务接入更快
- 视觉与交互一致性更好
- 对常见“卡片 + 页签内容”场景更省代码

缺点：

- `Card` 职责会更重
- 与现有独立 Tabs 组件可能出现能力重复

`外部组合`：`Card` 只提供 Header/Body/Actions，页签由外部 Tabs 组件放在 `Body` 中。

优点：

- 组件职责更单一，复用性高
- 与现有 Tabs 生态保持一致，减少重复实现

缺点：

- 业务层拼装代码略多
- 需要额外规范，避免不同页面出现不一致样式

推荐策略（本项目）：

- 第一阶段直接提供内建 Tabs（`Tabs`、`ActiveTabKey`、`TabChanged`）
- 同时保留外部组合能力（允许在 `Body` 放置独立 Tabs）
- 视觉与交互以内建 Tabs 为默认规范

---

## 11. 可访问性与键盘导航

- 卡片可选时：支持 `Tab` 聚焦、`Enter/Space` 触发主点击
- Action 区按视觉顺序参与 Tab 顺序
- Tabs 使用左右方向键切换（若启用内置 Tabs）
- 标题/描述支持可读文本输出，避免纯图标无标签
- Loading 状态可选暴露语义提示（如 `aria-busy` 等等价机制）

---

## 12. 性能与复用建议

- 卡片主体采用延迟内容构建（首次激活 Tab 时创建）
- `Loading` 与 `Normal` 视图节点可复用，避免频繁重建
- 网格场景避免每个 item 都深层嵌套过多容器
- 批量卡片更新时，优先数据驱动 + 最小化 UI 变更

---

## 13. 与 Ant Card 的能力映射

| Ant Design 能力 | MewUI Card 对应方案 |
|---|---|
| `title` / `extra` | `Title` / `Extra` 槽位 |
| `cover` | `Cover` 槽位 |
| `actions` | `Actions` 集合 |
| `tabList` + `onTabChange` | `Tabs` + `TabChanged` |
| `loading` | `Loading` + 骨架模板 |
| `size` | `CardSize` |
| `variant` / `bordered` | `CardVariant` + 兼容属性 |
| `type=inner` | `CardType.Inner` |
| `Card.Meta` | `CardMeta` |
| `Card.Grid` | `CardGrid` |
| 语义化样式 hooks | MewUI `StyleName` + 语义槽位属性 |

---

## 14. 最小可行版本（MVP）与演进路线

### 14.1 MVP（第一阶段）

- `Card` 基础结构：Header/Body/Actions
- `Title`、`Extra`、`Body`、`Loading`、`Hoverable`
- `CardMeta`
- `CardGrid`
- 动作区布局：自适应
- 内建 Tabs（支持 `Tabs`、`ActiveTabKey`、`TabChanged`）
- `Outlined` 与 `Borderless` 两种变体（首版限定）
- `Small/Medium` 两种尺寸
- 主题 token 最小集合

### 14.2 第二阶段

- `CardType.Inner`
- `Filled` 变体（若业务确有强调分组需求）

### 14.3 第三阶段

- 语义槽位级样式覆盖（细粒度）
- 动画策略（出现/切换/悬浮）
- 更完整可访问性语义

---

## 15. 验收清单（用于后续实现）

- API 是否能覆盖 80% 常见卡片场景且不过度复杂
- 是否完全遵循 MewUI 主题机制（无硬编码颜色）
- Header/Cover/Body/Actions/Tabs 是否可独立组合
- Loading、Hoverable、Disabled 状态是否可预测
- 20+ 卡片并列渲染时滚动与交互是否稳定
- 文档示例是否可以直接映射到 QuickLaunch 与设置页场景

---

## 16. 待你确认的问题

- 无

已确认项（已写入本文）：

- 第一阶段包含 `CardMeta`
- 第一阶段包含 `CardGrid`
- 第一阶段采用 Tabs 内建
- 动作区采用自适应
- 第一阶段变体限定为 `Outlined` + `Borderless`

确认后可进入实现阶段，并按本设计逐步提交代码与示例。