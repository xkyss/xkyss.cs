# 非功能需求

## 平台与技术栈

- Windows 10+,`net10.0-windows`,Direct2D 渲染后端(通过 `MewUIBackend=Direct2D` 收敛)。
- NuGet 依赖锁定 `Aprillz.MewUI` + `Aprillz.MewUI.MewDock` @ 0.19.1(见 ADR `000101-01`)。

## 性能

- 浮层从热键按下到可交互应接近瞬时(秒开)。
- 发布形态:NativeAOT 单文件 exe(见 ADR `000101-04`)。

## 兼容性约束

- 框架库保持 AOT/Trim 兼容:JSON 用 System.Text.Json 源生成,避免运行时反射。
- 布局能力基于 MewDock(见 ADR `000101-02`);扩展以编译期注册接入(见 ADR `000101-03`)。
