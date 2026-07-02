# Balance Data Sources

Registry of every Google Sheet the balance workflow reads or writes, and how to refresh a local copy of each. This is the single place that answers "which sheet is this, what's it for, and how do I get fresh data from it" — read this before touching any balance data.

> **Scope**: sheet identity, access, and refresh procedure only. Balance math and rules live in `tft-balancing-analysis.md`/`tft-balancing-rules.md`; our own implementation lives in `balance-framework.md`; the research roadmap lives in `tft-balancing-research-plan.md`.

---

## Registry

Sheet identity and access rules live in `.claude/scripts/sheets_config.json` — this table mirrors it for readability, but the JSON file is the source of truth for the actual keys.

| Name | Role | Writable? | Purpose | Tabs | Consumed by |
|---|---|---|---|---|---|
| `auto-battler` | Canonical | ✅ Yes | Our own design database — the 30-champion roster | Heroes, Origin, Class, Dashboard | `ChampionRoster.cs` (canonical stats), `balance_report.py`, `push_stats_to_sheet.py`, `balance-framework.md` |
| `tft-set9` | Reference | ❌ No | TFT Set 9 Champions — external reference roster/math, not ours to edit | TFT Set 9 Champions - Master Roster Layout, TFT Balancing Rules, CE Analysis, Meta Comps, Regional Origins, Combat Classes, Dashboard | `tft-balancing-analysis.md`, `tft-balancing-rules.md` |
| `tft-set10` | Reference | ❌ No | TFT Set 10 Champions — external reference, used to cross-check that TFT math (EHP, mana, star scaling) holds across sets | TFT Set 10 Champions - Master Roster Layout, TFT Balancing Rules, CE Analysis, Meta Comps, Origins, Classes | `tft-balancing-research-plan.md` (Topic cross-set validation) |

`writable: false` in `sheets_config.json` is enforced in code: `sheet_sync.py write` refuses outright against `tft-set9`/`tft-set10` — a hard rail against accidentally editing a sheet we don't own.

## Reading a sheet

```bash
python .claude/scripts/sheet_sync.py --sheet <name> read <TabName>
python .claude/scripts/sheet_sync.py --sheet <name> read <TabName> --format json
```
`--sheet` defaults to `auto-battler`, so every existing invocation without the flag is unchanged.

## Refreshing the local offline mirror

Each reference sheet has a local, self-describing snapshot under `.claude/reference/json/` for offline/reproducible analysis:

```bash
python .claude/scripts/sheet_sync.py --sheet tft-set9 dump
python .claude/scripts/sheet_sync.py --sheet tft-set10 dump
```

This writes `<name>_dump_<date>.json` (e.g. `tft-set9_dump_20260702.json`) with an embedded `_meta` block (`sheet`, `key`, `dumped_at`) so provenance is never a guess — unlike the old anonymous dumps this replaced. Current snapshots:

- `.claude/reference/json/tft-set9_dump_20260702.json`
- `.claude/reference/json/tft-set10_dump_20260702.json`
- `.claude/reference/json/tft_14.1.json` — a separate, unrelated Riot Data-Dragon-style API export (items/traits/champions for Sets 1, 9, 10), not a sheet dump. Kept as a cross-check source for Pillar B research (see `tft-balancing-research-plan.md`).

Re-run the dump commands whenever the reference sheets change and the analysis docs need updating.

## Adding a new sheet

1. Add an entry to `.claude/scripts/sheets_config.json` (`key`, `role`, `writable`, `description`).
2. Add a row to the registry table above.
3. Confirm read access: `python .claude/scripts/sheet_sync.py --sheet <name> read <SomeTab>`.
