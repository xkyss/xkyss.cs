# MewDock 作为停靠引擎

Workbench 需要 VSCode 式停靠布局(标签页、拆分、自动隐藏、拖拽重组)。决定:使用官方扩展 **MewDock**(`DockingManager` + 类型化/JSON 布局模型)作为停靠引擎,v1 不自研;Workbench 在其上封装类型化五区布局 API。

## Considered Options

- **自研停靠引擎**:完全可控,但开发与调优成本高,难以在 v1 达到 MewDock 的成熟度(拖拽、自动隐藏、弹出等)。
- **MewDock(选定)**:与 MewUI 同源、AOT/Trim 友好、提供 JSON 序列化模型,契合「布局保存/恢复」需求。

## Consequences

- 停靠能力边界受 MewDock 约束;若后续遇到其无法满足的场景,再评估替换。
