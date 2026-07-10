# TFT Set 9 Ability Audit (tft-set9 vs Community Dragon)

> **Status: fixed.** The `tft-set9` Champions tab's Skill Description column has been corrected in place — all 67 rows now reflect real %AD/%AP scaling, self-doubt comments are stripped, and several additional numeric errors and silent variable-resolution bugs found during the fix (Aatrox's Omnivamp/duration/3-star multiplier, Nasus's empowered-attack order-of-magnitude error, Ryze's entirely wrong ability mechanic, and 5 cases where the audit script itself had silently picked the wrong Community Dragon variable — Irelia, Galio, Fiora, Quinn, Bel'Veth) were corrected too. The findings below are kept as the historical record of what was wrong and why; they no longer describe the sheet's current state.

Diffs the `tft-set9` reference sheet's **Champions** tab "Skill Description" column against Riot's real Set 9 ability data (Community Dragon, patch 13.24), to find flat-damage numbers the sheet shows where the real ability actually scales by %AD/%AP (or a numeric value that's simply wrong).

> **Scope**: ability-text accuracy for the `tft-set9` sheet's Champions tab only. Champion base stats (Health/Armor/AS/etc.) and derived EHP/star columns were spot-checked separately during this audit and are correct — this doc covers Skill Description text only. Sheet identity/access lives in `data-sources.md`; DPS math built from these champions lives in `tft-set9-dps-analysis.md` and the tier-N docs.

## Why this exists

The sheet author left unresolved self-doubt comments embedded directly in a few ability descriptions (e.g. Taliyah: *"This is not flat damage, it should be %"*; Tristana: *"she gain 70% attack speed, no?"*) instead of verifying and fixing them. Manual spot-checks against the LoL Wiki confirmed those doubts were correct, and also caught Jinx's Skill Description inventing a flat "15/20/35" base damage number with no basis in her real kit at all. Wiki-scraping doesn't scale reliably to all 67 champions (JS-rendered sites return nothing, wiki summarization produced conflicting text for Jinx across two fetches), so a scripted diff against Riot's raw game data was built instead.

## Result

**60 of 67 champions (90%)** have a Skill Description that misrepresents real ability scaling — overwhelmingly the same defect: **the real ability scales by %AD/%AP, and the sheet shows a flat or mislabeled number instead.** This is not a handful of isolated typos; it's the dominant pattern across the tab.

Representative cases:
- **Jinx** — the sheet's "15/20/35" flat base has no basis in the real kit. `Fishbones!` deals **150/155/160% (AD+AP combined)** with no flat component at all.
- **Dozens of mages** (Cassiopeia, Malzahar, Karma, Vel'Koz, Azir, Lux, Ryze, Ahri, Karma, Kalista, and more) show plain flat magic/true damage where the real ability is `%AP` (or `%AP` true damage for Kalista).
- **Attack-speed/Omnivamp bonuses shown as flat**: Tristana ("1 Attack Speed" → real 70% AP), Aatrox (flat 20% Omnivamp → real ~10-15% AP-scaling, exact star values also off).
- **Numeric mismatches even where scaling type is fine**: Aatrox's transform duration (sheet 12/12/30s vs real 10/10/30s) and 3-star multiplier (sheet 275% vs real 250% AD).
- **4 champions came back clean**: Warwick (genuinely correct), Rek'Sai and Aphelios (already correctly annotate `%Attack Damage` — though the audit tool couldn't independently re-derive their exact numbers, see Limitations), and Maokai (sheet has no stat/ability data at all to check).

## Methodology

1. Community Dragon splits a mid-set update into two separate data blocks instead of one combined roster: the plain `TFTSet9` mutator is the original 9.0 release, `TFTSet9_Stage2` is the 9.5 refresh. Some champions only exist in one block (9.0-only champions were removed in the refresh; 9.5-only champions were added by it) — this exactly matches the sheet's own `Sub-Set` column values (`9.0`, `9.5`, `9.0 & 9.5`). The audit unions both blocks so all 67 sheet rows have ground truth to compare against.
2. Each ability's `desc` template (e.g. `Deal @TotalDamage@ physical damage`) is rendered by substituting its `variables` array at the 1★/2★/3★ indices, and tagging each substituted number with the scaling markers Riot's own data attaches to it (`%i:scaleAD%`, `%i:scaleAP%`, etc.) — this is the actual mechanism the game client uses to decide whether a number scales with AD, AP, max health, etc.
3. For each scaling-tagged number, the tool searches the sheet's Skill Description text for a numerically close triplet and checks whether the sheet marked it with `%`. If Community Dragon says a value scales AD/AP and the sheet's matching number has no `%` next to it, it's flagged as flat-vs-percent. If no close number exists in the sheet at all, it's flagged as unmatched.

Reusable script: **`.claude/scripts/audit_tft_abilities.py`** — parameterized by `--set` and `--sheet`, so re-running for `tft-set10`/`tft-set11` is a matter of finding each set's equivalent Community Dragon patch pin (`--patch`) and rerunning; no new code needed. Not yet run for those sets.

```bash
python .claude/scripts/audit_tft_abilities.py --set 9
python .claude/scripts/audit_tft_abilities.py --set 10 --sheet tft-set10 --patch <patch-pin>
```

## Limitations

- Some real abilities compute their tooltip number from an internal formula (Riot calls these things like `TotalDamage`, `MarkDamage`) rather than storing it as a single raw variable. The tool detects the correct **scaling type** for these (that's the basis of every flag above) but can't always resolve the literal combined number — in those rows the flag shows the closest matching raw sub-component instead. Treat the side-by-side text as the primary evidence; the auto-flag is a triage signal, not a final verdict.
- "Not flagged" does not always mean "independently verified correct" — for Rek'Sai and Aphelios specifically, the tool couldn't resolve their computed damage variables at all, so it never got a chance to check them (they happen to already show correct `%AD` annotations on manual read).

## Full findings (67/67 champions matched, 60 flagged)

### Cassiopeia
- **Sheet:** Deal 160/240/360 magic damage to the current target and Wound them for 5 seconds. If they are already Wounded, deal 30% bonus magic damage.Wound: Reduce healing received by 33%
- **Real (Community Dragon):** Deal 160/240/360 magic damage `[%AP]` to the current target and Wound them for a few seconds. If they are already Wounded, deal bonus magic damage `[%AP]`. Wound: Reduce healing received by 33%
- FLAG: sheet shows 160/240/360 as flat; real value scales %AP.

### Irelia
- **Sheet:** Enter a defensive stance and gain 350/400/450 Shield that rapidly decays over 3 seconds. When it expires, deal 70/100/150 magic damage + 30% of the damage absorbed to enemies around and in front of Irelia.Ionia Bonus: +25 Armor and Magic Resist
- **Real (Community Dragon):** Shield amount scales `[%AP]` (sheet's flat "3xx" reads as a decayed/misparsed fragment of the real value); strike damage also scales `[%AP]`. Ionia Bonus is flat +25/25/25 Armor/MR (sheet correct there).
- FLAG: Shield and strike damage mislabeled as flat.

### Jhin
- **Sheet:** *(the sheet author's own note, left unresolved)* "In the actual game, Jhin's spell scales with Attack Damage (it deals 300% / 300% / 344% AD plus that flat 60 / 90 / 135 physical damage). If we only calculate the flat 60/90/135 damage from the sheet, his spell DPS will be mathematically incorrect and extremely low."
- **Real (Community Dragon):** Take aim and deal **744/744/744% (AD+AP combined)** physical damage in a line, falloff per hit pierced. Ionia Bonus: +25/25/25% Attack Damage.
- FLAG: confirms the embedded doubt — entirely %AD+%AP scaling, no flat component.

### Kayle
- **Sheet:** Passive: Gains new Passive effects as your Tactician levels up.Level 1: Attacks deal 30/45/70 bonus magic damage.Level 6: Every 3rd attack launches a wave that deals 30/45/70 magic damage and 20% Shreds enemies for 3 seconds.Level 9: Every attack launches a wave and they travel farther.Shred: Reduce Magic Resist
- **Real (Community Dragon):** Both the Level 1 and Level 6 damage numbers scale `[%AP]`, not flat.
- FLAG: both damage instances mislabeled as flat.

### Malzahar
- **Sheet:** Open two portals near the current target. Destroy 50% of Shields and deal 220/330/500 magic damage to all enemies caught between the portals.
- **Real (Community Dragon):** 220/330/500 scales `[%AP]`.
- FLAG: mislabeled as flat.

### Orianna
- **Sheet:** Grant the lowest Health ally 200/225/275 Shield for 4 seconds and empowers Orianna's next attack to deal 290/435/650 bonus magic damage.
- **Real (Community Dragon):** Both the Shield and the 290/435/650 bonus damage scale `[%AP]`.
- FLAG: bonus damage mislabeled as flat.

### Poppy
- **Sheet:** Gain 250/325/400 Shield for 4 seconds. Deal 160/240/360 magic damage to the current target.
- **Real (Community Dragon):** Both Shield and damage scale `[%AP]`.
- FLAG: both mislabeled as flat.

### Renekton
- **Sheet:** Deal 180/270/400 magic damage to adjacent enemies. Heal 220/270/320 for the first enemy hit and another 30 for each additional one.
- **Real (Community Dragon):** 180/270/400 scales `[%AP]`; the heal values are genuinely flat (sheet correct there).
- FLAG: damage mislabeled as flat.

### Samira
- **Sheet:** Shoot at the current target and deal 190/190/200% Attack Damage to the first enemy hit. Reduce their Armor by 10/15/20 for the rest of combat.
- **Real (Community Dragon):** Primary damage is correctly %AD in the sheet; the Armor reduction (10/15/20) actually scales `[%AP]`, not flat.
- FLAG: Armor-shred value mislabeled as flat.

### Tristana
- **Sheet:** Gain 1 Attack Speed for 4 seconds (*"She didn't gain 1 attack speed, she gain 70% attack speed, no?"*). For the duration, attacks explode on impact and deal 60% Attack Damage physical damage to enemies within 1 hex. 4-Star Upgrade: Every 10th attack deals 120% Attack Damage bonus physical damage and ricochets, dealing 50% of that damage to enemies within 1 hex.
- **Real (Community Dragon):** Attack Speed gain is **70% AP-scaling** — confirms the embedded doubt exactly.
- FLAG: "1 Attack Speed" should be 70% AP-scaling Attack Speed.

### Viego
- **Sheet:** Deal 110/165/250 magic damage to the current target. For the rest of combat, Viego's attacks deal 20/30/45 bonus stacking magic damage.
- **Real (Community Dragon):** Both damage instances scale `[%AP]`.
- FLAG: both mislabeled as flat.

### Ashe
- **Sheet:** Fire 8 arrows in a cone, each dealing 10 (+160/165/175% Attack Damage) physical damage to the first enemy hit and Chilling them for 2 seconds.
- **Real (Community Dragon):** Arrow damage scales `[%AD]`; oddly the 2-second Chill duration is also tagged `[%AP]` in Riot's own data (a data quirk, not necessarily meaningful).
- FLAG: Chill duration flagged as flat vs the AP tag (low-confidence, likely not a real design intent — a data artifact).

### Galio
- **Sheet:** Reduce damage taken by 25/25/35% and heal for 300/350/400 over 2 seconds. After, deal 200/300/450 magic damage to adjacent enemies.
- **Real (Community Dragon):** the damage-reduction percent itself is the ability's magic-damage scaling variable (25/25/35`[%AP]`) — the sheet's separate "200/300/450" number doesn't independently resolve; likely a sheet transcription error mixing two different variables.
- FLAG: numeric mismatch, needs manual recheck against the wiki.

### Jinx
- **Sheet:** Fire 5 rockets at random enemies within 2 hexes of the current target. Each rocket deals 15/20/35 (+150/155/160% Attack Damage) physical damage.
- **Real (Community Dragon):** Each rocket deals **150/155/160% (AD+AP combined)** — no flat "15/20/35" component exists in the real kit.
- FLAG: the flat base is fabricated.

### Kassadin
- **Sheet:** Gain 250/300/400 Shield for 4 seconds. Deal 160/240/360 magic damage to enemies in a cone and Disarm them for 2/2/2 seconds.
- **Real (Community Dragon):** Shield and damage both scale `[%AP]`; Disarm duration is actually 1.5/1.75/2s (sheet rounds to a flat "2/2/2").
- FLAG: Shield/damage mislabeled as flat; Disarm duration slightly wrong.

### Kled
- **Sheet:** Passive: Start combat with 30% Shield. When it expires, dismount and gain 65/70/75% stacking Attack Speed for the rest of combat.Active: Remount and gain 30% Shield.
- **Real (Community Dragon):** the 65/70/75% Attack Speed is `[%AP]`-scaling on top of being a percent (sheet has the percent right, but doesn't note it also scales with AP).
- FLAG: AP-scaling on the Attack Speed bonus not noted.

### Sett
- **Sheet:** Deals 180/270/420 magic damage and Stuns them for 1/2/2 seconds. If only one enemy is grabbed, the damage and Stun duration are increased by 50%.
- **Real (Community Dragon):** 180/270/420 scales `[%AP]`; Stun duration is actually 1.25/1.5/2s.
- FLAG: damage mislabeled as flat.

### Soraka
- **Sheet:** Heal the lowest health ally for 140/160/180... Each deals 120/180/280 magic damage.
- **Real (Community Dragon):** the heals are genuinely flat (sheet correct); the 120/180/280 star damage scales `[%AP]`.
- FLAG: star damage mislabeled as flat.

### Swain
- **Sheet:** Transform and gain 325/375/550 max Health. While transformed, deal 25/40/60 magic damage every second. If already transformed, deal 100/160/300 magic damage.
- **Real (Community Dragon):** both damage-per-second values scale `[%AP]`.
- FLAG: both mislabeled as flat.

### Teemo
- **Sheet:** ...dealt 260/390/585 magic damage over 3 seconds.
- **Real (Community Dragon):** scales `[%AP]`.
- FLAG: mislabeled as flat.

### Taliyah
- **Sheet:** *(embedded doubt)* "It deals 120/180/270 magic damage... Deal 220/330/495 magic damage... **This is not flat damage, it should be %.**"
- **Real (Community Dragon):** both the passive boulder damage and the active spell damage scale `[%AP]` — confirms the embedded doubt exactly.
- FLAG: both mislabeled as flat.

### Vi
- **Sheet:** Gain 350/400/450 Shield for 4 seconds. Deal 300% Attack Damage physical damage... and 20% Sunder them.
- **Real (Community Dragon):** primary damage is correctly %AD; the Shield (350/400/450) scales `[%AP]`, mislabeled as flat.
- FLAG: Shield mislabeled as flat.

### Zed
- **Sheet:** dealing 25/40/50 (+140/140/150% Attack Damage) physical damage.
- **Real (Community Dragon):** the whole hit is **140/140/150% (AD+AP combined)** — no separate flat "25/40/50" component exists.
- FLAG: the flat base is fabricated, same pattern as Jinx.

### Akshan
- **Sheet:** Each deals 20/35/60 (+125% Attack Damage) physical damage.
- **Real (Community Dragon):** entire hit is %AD+%AP combined — no flat component.
- FLAG: fabricated flat base.

### Ekko
- **Sheet:** deal 255/380/570 magic damage.
- **Real (Community Dragon):** scales `[%AP]`.
- FLAG: mislabeled as flat.

### Garen
- **Sheet:** Spin 3 times every second for the next 4 seconds. Each spin deals 80/82/85% Attack Damage.
- **Real (Community Dragon):** spin damage is correctly %AD; but the "4 seconds" duration is itself the AP-scaling variable in Riot's data (`[%AP]`), which the sheet shows as a flat "4".
- FLAG: duration mislabeled as flat.

### Jayce
- **Sheet:** Grant 30/40/50% Attack Speed to Jayce and 15% to allies... deals 275/275/295% Attack Damage.
- **Real (Community Dragon):** primary damage correctly %AD; the "15%" ally Attack Speed bonus additionally scales `[%AP]`, not noted in the sheet.
- FLAG: ally bonus missing its AP-scaling note.

### Kalista
- **Sheet:** Passive: ...deals 18/27/45 **true damage** when removed.
- **Real (Community Dragon):** the true damage scales `[%AP]`.
- FLAG: true damage mislabeled as flat.

### Karma
- **Sheet:** dealing 200/300/470 magic damage.
- **Real (Community Dragon):** scales `[%AP]`.
- FLAG: mislabeled as flat.

### Katarina
- **Sheet:** deal 145/220/350 magic damage.
- **Real (Community Dragon):** scales `[%AP]`.
- FLAG: mislabeled as flat.

### Lissandra
- **Sheet:** deal 160/240/400 magic damage.
- **Real (Community Dragon):** scales `[%AP]`.
- FLAG: mislabeled as flat.

### Sona
- **Sheet:** deal 170/255/420 magic damage.
- **Real (Community Dragon):** scales `[%AP]`.
- FLAG: mislabeled as flat.

### Taric
- **Sheet:** Gain 500/580/680 Shield.
- **Real (Community Dragon):** scales `[%AP]`.
- FLAG: mislabeled as flat.

### Vel'Koz
- **Sheet:** deals 220/330/550 magic damage.
- **Real (Community Dragon):** scales `[%AP]`.
- FLAG: mislabeled as flat.

### Azir
- **Sheet:** deals 110/160/500 magic damage.
- **Real (Community Dragon):** scales `[%AP]`.
- FLAG: mislabeled as flat.

### Gwen
- **Sheet:** each dealing 100/150/400 magic damage.
- **Real (Community Dragon):** scales `[%AP]`.
- FLAG: mislabeled as flat.

### Jarvan IV
- **Sheet:** Deal 140/210/800 magic damage; Stun for 2/2/8 seconds.
- **Real (Community Dragon):** damage scales `[%AP]`; Stun is actually 1.5/2/8s.
- FLAG: damage mislabeled as flat.

### Kai'Sa
- **Sheet:** Each missile deals 80/120/240 magic damage.
- **Real (Community Dragon):** scales `[%AP]`.
- FLAG: mislabeled as flat.

### Lux
- **Sheet:** deals 735/1100/2750 magic damage.
- **Real (Community Dragon):** scales `[%AP]`.
- FLAG: mislabeled as flat.

### Nasus
- **Sheet:** every third attack deals 4/4/7 physical damage.
- **Real (Community Dragon):** that empowered attack is actually **380/380/700% AD**, not a flat "4/4/7" — an order-of-magnitude transcription error.
- FLAG: sheet value is off by roughly 100x and mislabeled as flat.

### Sejuani
- **Sheet:** deal 160/240/1200 bonus true damage / magic damage; Shield 600/700/2000.
- **Real (Community Dragon):** passive true damage scales `[%maxHP]`; active Shield and damage scale `[%AP]`.
- FLAG: all three mislabeled as flat.

### Shen
- **Sheet:** Shields 350/450/2000 and 225/275/1500; damage 240/360/2500.
- **Real (Community Dragon):** all three scale `[%AP]`.
- FLAG: all three mislabeled as flat.

### Urgot
- **Sheet:** passive blast 40/60/500 (+220% Attack Damage); active Shield 350/450/1200.
- **Real (Community Dragon):** passive blast is **220/220/220% (AD+AP combined)** — no flat component; Shield scales `[%AP]`.
- FLAG: fabricated flat base on the passive; Shield mislabeled as flat.

### Yasuo
- **Sheet:** dealing 55/85/300 (+500/500/1500% Attack Damage) physical damage... Slam deals 300/300/750% Attack Damage.
- **Real (Community Dragon):** the "55/85/300" is not a flat base — it's itself an **%AP-scaling** component of the same combined %AD+%AP hit (the slam's 300/300/750% AD is correctly labeled already).
- FLAG: the "55/85/300" component is mislabeled as flat; should read "+55/85/300% AP".

### Zeri
- **Sheet:** Execute enemies below 8% max Health; chain lightning deals 45/45/100% Attack Damage.
- **Real (Community Dragon):** the chain-lightning damage is correctly %AD; the "8%" execute threshold itself is tagged `[%AP]` in Riot's data (execute % increasing with AP — plausible real mechanic, worth double-checking against the wiki).
- FLAG: execute threshold flagged, lower confidence.

### Ahri
- **Sheet:** dealing 105/150/1000 magic damage... unleash a wave that deals 260/390/1999 magic damage.
- **Real (Community Dragon):** both scale `[%AP]`.
- FLAG: both mislabeled as flat.

### Aatrox
- **Sheet:** Transform for 12/12/30 seconds, gaining 20% Omnivamp... attacks deal 275/275/2500% Attack Damage.
- **Real (Community Dragon):** duration is actually **10/10/30s** (not 12/12/30); Omnivamp is `[%AP]`-scaling (real magnitude ~10%, not flat 20%); transform-attack multiplier is **250/250/2500% AD** (not 275).
- FLAG: three separate numeric/type errors in one ability.

### Heimerdinger
- **Sheet:** dealing 150/225/3141 magic damage; Stunning for 2/2/15 seconds.
- **Real (Community Dragon):** damage scales `[%AP]`; Stun is actually 1.5/2/15s.
- FLAG: damage mislabeled as flat.

### K'Sante
- **Sheet:** Deal 250/400/1000 magic damage (twice); Stun 2/2/10 and 1/1/5 seconds.
- **Real (Community Dragon):** both damage instances scale `[%AP]`; Stun durations are actually 2/2.5/10s and 1/1.25/5s.
- FLAG: both damage instances mislabeled as flat.

### Ryze
- **Sheet:** Deals 125/230/2006 magic damage; portals fire a rocket dealing 125/230/2006% damage; Barkskin grants Armor/MR for 4 seconds.
- **Real (Community Dragon):** Ryze's actual Set 9 ability is a completely different mechanic — a single portal coating the ground in vines, damage scaling off `[%Armor+%MR]` (not a gold-based multi-portal mechanic at all), and the Barkskin duration scales `[%AP]`.
- FLAG: the sheet's entire ability description appears to not match Ryze's real Set 9 kit — needs a full manual recheck, not just a numeric fix.

### Sion
- **Sheet:** reanimate with 100% Health, decaying 20/13/0%; gains 2/2/10 Attack Speed; deals 225/235/500% Attack Damage.
- **Real (Community Dragon):** Attack Speed gain (2/2/10, roughly right in magnitude) is `[%AP]`-scaling; charge damage scales `[%AD]`; Stun durations are actually 1.25/1.5/15s.
- FLAG: Attack Speed gain mislabeled as flat.

### Senna
- **Sheet:** dealing 235/250/2000% Attack Damage; allies gain 200/275/4000 Shield.
- **Real (Community Dragon):** primary damage correctly %AD; the Shield (200/275/4000) scales `[%AP]`, not flat.
- FLAG: Shield mislabeled as flat.

### Xayah *(9.5)*
- **Sheet:** "Summon feathers... dealing physical damage" *(no numbers at all in the sheet's Skill Description)*.
- **Real (Community Dragon):** 7/7/15 feathers, each dealing **15/25/60% (AD+AP combined)**.
- FLAG: sheet is missing the actual damage numbers entirely, not just mislabeling them.

### Nilah *(9.5)*
- **Sheet:** "Attacks strike in a cone... for physical damage" *(no numbers)*.
- **Real (Community Dragon):** cleave hits deal 55/55/100% AD; empowered every-3rd-attack hit is a separate %AD value; active Shield is 250/375/700 `[%AP]`.
- FLAG: sheet is missing numbers entirely.

### Fiora *(9.5)*
- **Sheet:** "Dash through enemies dealing physical damage... heals for a portion" *(no numbers)*.
- **Real (Community Dragon):** each of several strikes deals 60/90/270% AD plus a separate %AP true-damage component.
- FLAG: sheet is missing numbers entirely.

### Quinn *(9.5)*
- **Sheet:** "attack together for bonus physical damage... deals bonus damage and blinds" *(no numbers)*.
- **Real (Community Dragon):** mark amp is `[%AP]`-scaling; arrow damage is 10/10/10% `[%AD]`.
- FLAG: sheet is missing numbers entirely.

### Neeko *(9.5)*
- **Sheet:** "clones that taunt nearby enemies before exploding for magic damage" *(no numbers)*.
- **Real (Community Dragon):** Shield 250/325/425 `[%AP]`, slam damage 300/450/700 `[%AP]`.
- FLAG: sheet is missing numbers entirely.

### Qiyana *(9.5)*
- **Sheet:** "Dash and deal physical damage" *(no numbers)*.
- **Real (Community Dragon):** 255/255/270% (AD+AP combined).
- FLAG: sheet is missing numbers entirely.

### Silco *(9.5)*
- **Sheet:** "damage enemies and heal allies over time" *(no numbers)*.
- **Real (Community Dragon):** 65/100/400 `[%AP]` magic damage per second, flat 20/25/120 heal per second.
- FLAG: sheet is missing damage numbers entirely.

### Miss Fortune *(9.5)*
- **Sheet:** "dealing magic damage" *(no numbers)*.
- **Real (Community Dragon):** 240/360/580 `[%AP]`.
- FLAG: sheet is missing numbers entirely.

## Not flagged (4/67)

- **Warwick** — genuinely correct as written.
- **Rek'Sai**, **Aphelios** — already correctly show `%Attack Damage` in the sheet; the tool couldn't independently re-derive their exact numbers (see Limitations) but a manual read finds no issue.
- **Maokai** — no stat or ability data exists in the sheet row at all (separate data-completeness issue, not an ability-text issue).
