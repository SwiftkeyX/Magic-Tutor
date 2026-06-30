---
name: google-sheet
description: Read and write data to/from the Custom Auto-Battler Google Sheet.
---

# Google Sheet Sync Skill

This skill allows the agent to read and write to the Custom Auto-Battler Google Sheet in the background.

## Purpose of the Google Sheet
This Google Sheet acts as the **design database** for the game. It is used by designers to balance the game and contains the source of truth for:
* **Heroes**: All 10 champions, their costs, classes, origins, roles, and combat logic.
* **Origins (Vertical Classes)**: Vanguard, Striker, Elementalist, and Ranger breakpoints and effects.
* **Classes (Horizontal Classes)**: Kinetic, Dreadknight, Warden, and Trickster breakpoints and effects.
* **Dashboard**: An interactive team builder and trait tracker for checking synergies.

The code in [ChampionRoster.cs](file:///c:/Organized%20Files/Working/Unity/Unity%20Project/Magic%20School/Assets/Scripts/Battle/Traits/ChampionRoster.cs) and [TraitSystem.md](file:///c:/Organized%20Files/Working/Unity/Unity%20Project/Magic%20School/.claude/docs/production/gdd/TraitSystem.md) must remain in sync with this sheet.

## Sheet URL
[Custom Auto-Battler Google Sheet](https://docs.google.com/spreadsheets/d/1lz_WS8Vw0YjfmNZLI3JmRArfFBU2sPAk2qAG8AKN3jg/edit?usp=sharing)

## Steps to Read the Sheet
1. Spawn a `browser_subagent` task to navigate to the Google Sheet URL.
2. Wait for the page and spreadsheet interface to load.
3. Access specific tabs:
   * **Heroes**: Click tab pixel or select from "All sheets" menu.
   * **Origin**: Click tab pixel or select from "All sheets" menu.
   * **Class**: Click tab pixel or select from "All sheets" menu.
   * **Dashboard**: Click tab pixel or select from "All sheets" menu.
4. Read table cells from the DOM or screenshots and return them.

## Steps to Write to the Sheet
1. Navigate to the desired tab.
2. Select the cell by clicking its coordinates or targeting it in the grid.
3. Simulate typing the new value.
4. Verify that Google Sheets displays "Saved to Drive" in the header status bar before closing.
