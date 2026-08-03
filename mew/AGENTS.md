## Agent skills

### Issue tracker

Issues and specs for this repo live as local markdown files under `.scratch/<feature>/`.

`<feature>` = `<版本前缀>-<功能名>`,如 `v0.1.1-workbench-launcher`。每个目录下的 `spec.md` 即该版本的 PRD(不再使用 `docs/prd/`)。

See `docs/agents/issue-tracker.md`.

### Triage labels

Triage uses the default label vocabulary (`needs-triage`, `needs-info`, `ready-for-agent`, `ready-for-human`, `wontfix`). See `docs/agents/triage-labels.md`.

### Domain docs

Single-context: one `CONTEXT.md` + `docs/adr/` at the repo root.

`adr` with prefix of version, like `000101`.

版本编码规则:ADR 前缀 = 6 位版本号(major/minor/patch 各 2 位补零),同版本多条加 `-NN` 序号;ADR 与 issue tracker 目录按版本对应(如 `000101` ↔ `v0.1.1`)。

See `docs/agents/domain.md`.

### Git conventions

Commit messages follow Conventional Commits 1.0.0 with repo-local Chinese message rules. See `docs/agents/git-conventions.md`.
