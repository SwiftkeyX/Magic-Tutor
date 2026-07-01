---
name: google-sheet
description: Read and write data to/from the Custom Auto-Battler Google Sheet using the background sheet_sync.py script.
---

# Google Sheet Sync Skill (Background API)

This skill allows the agent to read and write to the Custom Auto-Battler Google Sheet directly in the background using the [sheet_sync.py](file:///c:/Organized%20Files/Working/Unity/Unity%20Project/Magic%20School/sheet_sync.py) command-line utility. This avoids launching a browser and interrupting the user.

## Purpose of the Google Sheet
This Google Sheet acts as the **design database** for the game. It contains the source of truth for:
* **Heroes**: All 10 champions, their costs, classes, origins, roles, and combat logic.
* **Origins (Vertical Classes)**: Vanguard, Striker, Elementalist, and Ranger breakpoints and effects.
* **Classes (Horizontal Classes)**: Kinetic, Dreadknight, Warden, and Trickster breakpoints and effects.
* **Dashboard**: An interactive team builder and trait tracker for checking synergies.

The code in [ChampionRoster.cs](file:///c:/Organized%20Files/Working/Unity/Unity%20Project/Magic%20School/Assets/Scripts/Battle/Traits/ChampionRoster.cs) and [TraitSystem.md](file:///c:/Organized%20Files/Working/Unity/Unity%20Project/Magic%20School/.claude/docs/production/gdd/TraitSystem.md) must remain in sync with this sheet.

## Credentials
The credentials for this connection are stored in the root credential file [google-service-credential.json](file:///c:/Organized%20Files/Working/Unity/Unity%20Project/Magic%20School/google-service-credential.json).

## Steps to Read the Sheet
To fetch data from any tab in the spreadsheet, use the `run_command` tool to execute `sheet_sync.py` in the workspace root.

* **Format as Table (Human-Readable):**
  ```bash
  python sheet_sync.py read <TabName>
  ```
* **Format as JSON (For parsing):**
  ```bash
  python sheet_sync.py read <TabName> --format json
  ```
* **Format as CSV:**
  ```bash
  python sheet_sync.py read <TabName> --format csv
  ```

*Available TabNames: `Heroes`, `Origin`, `Class`, `Dashboard`.*

## Steps to Write to the Sheet
To update a cell in the spreadsheet, run the script's `write` command:

```bash
python sheet_sync.py write <TabName> <Cell> "<Value>"
```
*Example:*
```bash
python sheet_sync.py write Heroes B2 "Ironclad"
```

## Steps to Dump All Sheet Content
To download all data from the spreadsheet into a single local JSON file (`sheet_dump.json`):

```bash
python sheet_sync.py dump
```
