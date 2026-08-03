# Domain Docs

How the engineering skills should consume this repo's domain documentation when exploring the codebase.

## Before exploring, read these

- **`CONTEXT.md`** at the repo root, or
- **`CONTEXT-MAP.md`** at the repo root if it exists — it points at one `CONTEXT.md` per context. Read each one relevant to the topic.
- **`docs/adr/`** — read ADRs that touch the area you're about to work in. In multi-context repos, also check `src/<context>/docs/adr/` for context-scoped decisions.
- **`docs/prd/`** — 按版本组织的需求文档,与同版本 ADR 对应;进入某版本功能前先读对应 PRD。

If any of these files don't exist, **proceed silently**. Don't flag their absence; don't suggest creating them upfront. The `/domain-modeling` skill (reached via `/grill-with-docs` and `/improve-codebase-architecture`) creates them lazily when terms or decisions actually get resolved.

## File structure

Single-context repo (most repos):

```
/
├── CONTEXT.md
├── docs/adr/
│   ├── 000101-01-lock-mewui-version.md
│   └── 000101-02-mewdock-docking-engine.md
├── docs/prd/
│   └── v0.1.1/
└── src/
```

Multi-context repo (presence of `CONTEXT-MAP.md` at the root):

```
/
├── CONTEXT-MAP.md
├── docs/adr/                          ← system-wide decisions
├── docs/prd/                          ← versioned requirements
└── src/
    ├── ordering/
    │   ├── CONTEXT.md
    │   └── docs/adr/                  ← context-specific decisions
    └── billing/
        ├── CONTEXT.md
        └── docs/adr/
```

## ADR/PRD 编号规则

- ADR 前缀 = 版本编码:6 位 = major/minor/patch 各 2 位补零。`000101` → v0.1.1,`010203` → v1.2.3(上限 99.99.99)。
- 同一版本多条 ADR 用序号:`000101-01-lock-mewui-version.md`、`000101-02-…`。
- PRD 按版本分目录:`docs/prd/v0.1.1/`,与同版本 ADR 一一对应。
- 新版本决策用新前缀,PRD 另开新目录。

## Use the glossary's vocabulary

When your output names a domain concept (in an issue title, a refactor proposal, a hypothesis, a test name), use the term as defined in `CONTEXT.md`. Don't drift to synonyms the glossary explicitly avoids.

If the concept you need isn't in the glossary yet, that's a signal — either you're inventing language the project doesn't use (reconsider) or there's a real gap (note it for `/domain-modeling`).

## Flag ADR conflicts

If your output contradicts an existing ADR, surface it explicitly rather than silently overriding:

> _Contradicts ADR-0007 (event-sourced orders) — but worth reopening because…_
