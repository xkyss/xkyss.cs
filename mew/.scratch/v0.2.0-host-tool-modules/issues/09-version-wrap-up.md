# 09 — 版本收尾与冒烟

**What to build:** 版本提升与发布收尾:AppVersion 常量提升为 v0.2.0(宿主内,单文件单行提交先例)、发布产物更新、全量手工冒烟(浮层、热键、托盘、设置迁移、主题、AOT 发布)并落 verification 记录。

**Blocked by:** 07,08

**Status:** ready-for-agent

- [ ] AppVersion 提升 v0.2.0,单行提交(先例:6e02e1a / ba2885e)
- [ ] artifacts 更新为 v0.2.0 产物
- [ ] 冒烟清单全过:浮层呼出/搜索/启动、呼出键改绑与冲突、托盘常驻、设置迁移无损、主题切换、AOT 发布
- [ ] verification 记录落盘 `.scratch/v0.2.0-host-tool-modules/`
