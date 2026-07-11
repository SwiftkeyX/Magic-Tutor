# TFT Set 10 Ability Audit (tft-set10 vs Community Dragon)

> **Status: authored.** The `tft-set10` sheet's champion data lives in a worksheet named `"TFT Set 10 Champions - Master Roster Layout"` (not `"Champions"`), which had **no Skill Description column at all**. A new column was inserted at position E (between Design Role and Origin 1, mirroring `tft-set9`/`tft-set11`'s layout) and populated for all 60 rows from Riot's real Set 10 ability data. Every other column's data was verified byte-identical before and after the insertion.

Authors the `tft-set10` reference sheet's Master Roster Layout tab "Skill Description" column from Community Dragon Set 10 data, patch **14.13**.

> **Scope**: ability-text authoring for the `tft-set10` sheet's roster tab only. Champion base stats were not touched, and were verified unchanged by the column insertion (see Verification below).

## Patch pin

Set 10 ("Remix Rumble") data is present in Community Dragon from patch 13.24 through patch 14.13, holding steady at 76 champions from patch 14.8 onward; the set disappears from Community Dragon entirely at patch 14.14 (Set 11 launches at 14.6, so there's a brief overlap window before Set 10's data is dropped). **Patch 14.13** was chosen — the last patch with Set 10 data present.

## Sheet structure notes

- **Worksheet name mismatch**: the script's `read_sheet_rows()` hardcodes `"Champions"` as the worksheet name (correct for `tft-set9`/`tft-set11`, wrong for `tft-set10`). Since there was no existing Skill Description column to diff against anyway, this session used the script's *generation* half (`fetch_set_data` + `render_ability`) directly rather than its *diff* half — the worksheet-name mismatch was a non-issue rather than something that needed fixing.
- **60 sheet rows, but "Akali" appears twice** with different stats (Health 950 vs 1000, Origin "K/DA" vs "True Damage"). This reflects Set 10's real **Headliner** mechanic: a champion could be reassigned to a different origin/band with boosted stats, without changing their underlying kit. Community Dragon has only one Akali champion entry (`apiName: TFT10_Akali`, traits `K/DA`/`Executioner`/`Breakout`) — both sheet rows correctly received the **same** composed ability text, since they share one ability regardless of Headliner assignment.
- **Every ability's `desc` ends with `@TFTUnitProperty.:TFT10_Headliner_TRA@`** — a live in-game readout of the champion's current Headliner trait bonus, not part of the base kit. Dropped entirely from all 60 rows rather than rendered as a fabricated number.
- **Many abilities contain a `<spellActive enabled=TFT10_BlingActive alternate=rules>...</spellActive>` block** describing an alternate/upgraded effect unlocked by the "Bling" radiant item. These are item-conditional bonuses, not base-kit text, and were dropped entirely — consistent with a champion reference sheet describing base abilities, not item interactions.
- **Yorick's Community Dragon `ability.variables` array is empty** at every patch checked (14.8 through 14.13) — a genuine, persistent data gap for this one champion, not a patch-specific quirk. His numbers (9/10/25 ghouls, 150/150/1000% AD, BIG ghoul 900/1750/9001% AP Health / 250/250/2000% AD) came from the LoL Wiki's Set 10 patch-history section instead.
- **Olaf's Attack Speed-per-missing-Health scaling term did not resolve cleanly** from Community Dragon's raw data (the only remotely-matching variable, `BonusAttackSpeed = 0.0015`, doesn't produce a plausible tooltip number under any tested rescale), and the wiki's patch-history section for Olaf was truncated before reaching Set 10 numbers. Rather than fabricate a value, his passive heal (20/25/30 Ability Power Health, which resolved cleanly) is shown and the Attack Speed term is described qualitatively ("gain bonus Attack Speed that scales with max Health") without a specific number. Flagged here for a follow-up fix if a better source is found.

## Methodology

Same approach as `tft-set11` (see that doc for the general methodology, since this was a from-scratch authoring pass, not a diff-and-correct pass — see also `tft-set9-ability-audit.md` for the original diff methodology this all descends from):

1. All 59 unique champion names (Akali counted once) matched exact Community Dragon Set 10 entries — no coverage gaps.
2. Rendered every ability at 1★/2★/3★ (`variables` index 1/2/3), tagged with Riot's `%i:scaleAD%`/`%i:scaleAP%`/etc. markers.
3. Ran the same three-pass ambiguous-match methodology as Set 11: (a) leftover-placeholder scan, (b) multi-candidate substring/token scan, (c) same-ability duplicate-resolved-value scan. Found 23 ambiguous-candidate cases and 20 fully-unresolved placeholders; all were resolved by hand from the raw `variables` list and added to `SET10_OVERRIDES` in the composer.
4. **Set 10's naming convention is far less consistent than Set 9/11's** — placeholder and variable names mix `CamelCase`, `SNAKE_CASE`, and even all-lowercase (Ahri's `modifieddamage`/`damage`) within the same ability, and some are case-mismatched between the `desc` template and the `variables` list (Zac's `@stunduration@` vs. the raw variable `StunDuration`). The composer's resolver was extended with a case-insensitive fallback lookup to handle this.
5. **11 champions have genuinely combined two-term `%AD`+`%AP` (or `%AP`+`%maxHealth`) abilities** where Community Dragon's `desc` template only exposes one placeholder for what the tooltip shows as two separate scaling components: Caitlyn, Ezreal, Kai'Sa, Lucian, Urgot, Garen, Riven, Taric, Qiyana, K'Sante, Pantheon. Each was resolved by finding both raw variables by hand and composing the sheet text as two explicit terms, per the combined-scaling convention carried over from `tft-set9`/`tft-set11`.
6. Wiki spot-check (Yorick) confirmed the Community Dragon extraction methodology holds for Set 10 too, and reconfirmed the "absurd 3-star number" pattern is real and intentional (Yorick's BIG ghoul Health hits **9001% AP** at 3-star, matching the same joke-number pattern independently confirmed for Set 11's Sett).
7. Prose composed in the same natural-sentence convention as `tft-set9`/`tft-set11`.

Reusable script: `.claude/scripts/audit_tft_abilities.py`'s `resolve_variable`/`render_ability`/`fetch_set_data` functions were reused directly (imported, not modified for Set10-specific overrides — those live in the composer script used for this session, not in the shared `COMPUTED_OVERRIDES` table, since Set 10's override keys would collide with Set 9/11's naming conventions in ways that need case-insensitive handling the shared table doesn't have).

```bash
python -c "import audit_tft_abilities as A; A.fetch_set_data(10, '14.13')"  # generation half only — no diff path used
```

## Anomalies flagged for human review (not corrected — presented as extracted)

- **Sona**: `BaseShieldAmount` jumps from 550 (2-star) to 3333 (3-star) and `Duration` jumps from 2.5s to 30s — an unusually large discontinuity even by this set's "joke 3-star number" standard. Worth a sanity check.
- **Multiple champions show a large 3-star spike** consistent with the confirmed-intentional pattern from the Set 11 audit (Illaoi, Kayn, Ziggs, Thresh, Udyr-style tank/support kits): treat as real per the Yorick wiki confirmation, but a final human pass is still recommended before using these for balance math.
- **Olaf's Attack Speed-scaling term**: presented qualitatively without a number — see Sheet structure notes above.

## Full data source

All 60 rows' final composed Skill Description text is live in the `tft-set10` sheet's `TFT Set 10 Champions - Master Roster Layout` tab, column E, verified via a full re-read/diff after write (0 mismatches) and a full-row diff against the pre-insertion snapshot confirming every other column's data is unchanged. This doc is the audit trail, not a mirror of the sheet content.
