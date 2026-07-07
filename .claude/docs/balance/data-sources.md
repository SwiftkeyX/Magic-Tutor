# Balance Data Sources

Registry of every Google Sheet the balance workflow reads or writes, and how to refresh a local copy of each. This is the single place that answers "which sheet is this, what's it for, and how do I get fresh data from it" — read this before touching any balance data.

> **Scope**: sheet identity, access, and refresh procedure only. Balance math and rules live in `tft-balancing-analysis.md`/`tft-balancing-rules.md`; our own implementation lives in `balance-framework.md`; the research roadmap lives in `tft-balancing-research-plan.md`.

---

## Registry

Sheet identity and access rules live in `.claude/scripts/sheets_config.json` — this table mirrors it for readability, but the JSON file is the source of truth for the actual keys.

| Name | Role | Writable? | Purpose | Tabs | Consumed by |
|---|---|---|---|---|---|
| `auto-battler` | Canonical | ✅ Yes | Our own design database — the 30-champion roster | Heroes, Origin, Class, Dashboard | `ChampionRoster.cs` (canonical stats), `balance_report.py`, `push_stats_to_sheet.py`, `balance-framework.md` |
| `tft-set9` | Reference | ✅ Yes | TFT Set 9 Champions — basic reference roster (clean, with dashboard) | Champions, Origins, Classes, TFT Balancing Rules, Meta Comps, Dashboard | `tft-balancing-analysis.md`, `tft-balancing-rules.md` |
| `tft-set9-analysis` | Reference Analysis | ✅ Yes | TFT Set 9 Carry and Combat Efficiency Analysis sheet (separated) | Hero Data, CE Analysis, Carry DPS Analysis | `tft-balancing-analysis.md`, `tft-set9-dps-analysis.md`, `tier-1-carry-dps.md`, `tier-2-carry-dps.md`, `tier-3-carry-dps.md`, `tier-4-carry-dps.md`, `tier-5-carry-dps.md` |
| `tft-set10` | Reference | ✅ Yes | TFT Set 10 Champions — external reference, used to cross-check that TFT math (EHP, mana, star scaling) holds across sets | TFT Set 10 Champions - Master Roster Layout, TFT Balancing Rules, CE Analysis, Meta Comps, Origins, Classes | `tft-balancing-research-plan.md` (Topic cross-set validation) |
| `tft-set11` | Reference | ✅ Yes | TFT Set 11 Champions — external reference, basic champion data populated mirroring Set 9's format, for cross-set validation alongside Set 9/Set 10 | Champions, Origins, Classes (basic roster, matching Set 9's layout — not yet confirmed via live read, see note below) | TBD |

All four reference sheets (`tft-set9`, `tft-set9-analysis`, `tft-set10`, `tft-set11`) are now marked `writable: true` in `sheets_config.json` — the previous hard rail in `sheet_sync.py write` (which refused writes against `tft-set9`/`tft-set10`) no longer applies to any registered sheet. Only `auto-battler` is truly ours; the others are external/reference sheets now opened up for writes at the user's request.

## Direct links

For quick access from a browser (each URL opens to a specific tab via `gid`):

| Name | URL |
|---|---|
| `tft-set9` | https://docs.google.com/spreadsheets/d/1xj6em5XlvIN1gWHTKOPDsssmkgnS3jIsaLnAMjYyPUA/edit?gid=1947605367#gid=1947605367 |
| `tft-set9-analysis` | https://docs.google.com/spreadsheets/d/1pnbDKR55HxXFUExIXKyksJqaUSrNuw53IfnvBEt9zNY/edit?gid=1251636133#gid=1251636133 |
| `tft-set10` | https://docs.google.com/spreadsheets/d/1Eutkil9U8RJ4Lqo3l6wk9pbUeVVH80M4JmcRyMEqa2E/edit?gid=42463266#gid=42463266 |
| `tft-set11` | https://docs.google.com/spreadsheets/d/1BCFZRFebGd2O41ZvbJcNZ7LdVBWH3ssBliTQGMVN3cI/edit?usp=sharing |

## Reading a sheet

```bash
python .claude/scripts/sheet_sync.py --sheet <name> read <TabName>
```
`--sheet` defaults to `auto-battler`, so every existing invocation without the flag is unchanged.

## Refreshing the local offline mirror

Each reference sheet has a local, self-describing snapshot under `.claude/reference/json/` for offline/reproducible analysis:

```bash
python .claude/scripts/sheet_sync.py --sheet tft-set9 dump
python .claude/scripts/sheet_sync.py --sheet tft-set9-analysis dump
python .claude/scripts/sheet_sync.py --sheet tft-set10 dump
```

This writes `<name>_dump_<date>.json` (e.g. `tft-set9_dump_20260702.json`) with an embedded `_meta` block (`sheet`, `key`, `dumped_at`) so provenance is never a guess — unlike the old anonymous dumps this replaced. Current snapshots:

- `.claude/reference/json/tft-set9_dump_20260702.json`
- `.claude/reference/json/tft-set9-analysis_dump_20260706.json`
- `.claude/reference/json/tft-set10_dump_20260702.json`
- `.claude/reference/json/tft_14.1.json` — a separate, unrelated Riot Data-Dragon-style API export (items/traits/champions for Sets 1, 9, 10), not a sheet dump. Kept as a cross-check source for Pillar B research (see `tft-balancing-research-plan.md`).

Re-run the dump commands whenever the reference sheets change and the analysis docs need updating.

## Adding a new sheet

1. Add an entry to `.claude/scripts/sheets_config.json` (`key`, `role`, `writable`, `description`).
2. Add a row to the registry table above.
3. Confirm read access: `python .claude/scripts/sheet_sync.py --sheet <name> read <SomeTab>`.
   - Note: this requires reaching `oauth2.googleapis.com`. Claude's sandboxed shell has a restricted network allowlist and cannot reach that host, so this confirmation step must be run locally by a human (or from an environment with Google API network access) rather than by Claude.
