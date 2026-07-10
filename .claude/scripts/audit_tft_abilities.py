#!/usr/bin/env python3
"""Diff a tft-set<N> reference sheet's Champions tab against Riot's real ability
data (Community Dragon), to find flat-damage numbers that should be %AD/%AP
scaling, and other numeric mismatches, in the sheet's Skill Description column.

Usage (from repo root):
    python .claude/scripts/audit_tft_abilities.py --set 9
    python .claude/scripts/audit_tft_abilities.py --set 10 --sheet tft-set10 --patch 14.10
"""
import argparse
import json
import re
import subprocess
import sys
from pathlib import Path

import requests

SCRATCH_DIR = Path(r"C:\Users\ad\AppData\Local\Temp\claude\C--Organized-Files-Working-Unity-Unity-Project-Magic-School\bc935aa5-550d-4381-b476-ee9d4d9a1c96\scratchpad")

# A patch known to contain each set's *full* champion roster (including any
# mid-set ".5" additions). Community Dragon keys its archive by client patch
# version, not TFT set number, so this has to be found per set by hand.
DEFAULT_PATCH = {
    9: "13.24",
}

SCALE_LABELS = {
    "AD": "AD",
    "AP": "AP",
    "Health": "maxHP",
    "MaxHealth": "maxHP",
    "Range": "range",
}

TAGS_OF_INTEREST = {"physicalDamage", "magicDamage", "trueDamage", "TFTBonus", "scaleHealth"}

# Computed tooltip placeholders (TotalDamage, ModifiedX, ...) aren't raw
# variables — Community Dragon expects the game client to compute them from a
# formula it doesn't expose. Names like "TotalDamage"/"ModifiedDamage" recur
# across many champions with a different correct raw variable each time, so
# they can't be resolved generically; each pairing here was confirmed by
# reading that champion's actual variable list (see Phase A of the tft-set9
# ability-audit plan).
COMPUTED_OVERRIDES = {
    ("Tristana", "BonusDamage"): "PercentAttackDamage",
    ("Tristana", "4StarShotDamage"): "EmpoweredBonus",
    ("Tristana", "ExplosionDamage"): "ExplosionPercent",
    ("Irelia", "ModifiedBaseStrikeDamage"): "StrikeBaseDamage",
    ("Aatrox", "GreatswordDamage"): "GreatswordADRatio",
    ("Ashe", "ArrowDamage"): "ADPercent",
    ("Azir", "MaxSoldierStrike"): "BonusRatio",
    ("Jayce", "ModifiedEnhancedAttackSpeed"): "SelfAttackSpeed",
    ("Orianna", "ModifiedShielding"): "ShieldHealth",
    ("Rek'Sai", "TotalDamage"): "PercentOfAD",
    ("Rek'Sai", "MarkDamage"): "PercentOfADMark",
    ("Senna", "TotalDamage"): "ADRatio",
    ("Sion", "ModifiedDamage"): "ADPercent",
    ("Yasuo", "ModifiedAOEDamage"): "AOEADPercent",
    ("Cassiopeia", "BonusDamage"): "DamageAmp",
    ("Vi", "TotalDamage"): "ADPercent",
    ("Samira", "TotalDamage"): "ADPercent",
    ("Aphelios", "TotalBlastDamage"): "BlastPercentAD",
    ("Aphelios", "TotalChakramDamage"): "ChakramPercentAD",
    ("Zeri", "TotalChainDamage"): "OverchargeChainRatio",
    ("Nilah", "ModifiedEmpoweredDamage"): "EmpoweredLineDamage",
    ("Fiora", "ModifiedBonusDamage"): "BonusTrueDamage",
    ("Swain", "ModifiedBonusHealth"): "BonusMaxHealth",
    ("Swain", "ModifiedSecondaryBonusHealth"): "SecondaryBonusMaxHealth",
    # These four were silent mis-picks by the generic substring fallback: it
    # returns the *first* raw variable containing the stripped placeholder
    # name, and for these four that happened to be the wrong variable (e.g.
    # Galio's "ModifiedDamage" matched "DamageResist" — a totally different
    # stat — before ever reaching "Damage"). Found by scanning every
    # AD/AP-scaling placeholder for multiple substring candidates and
    # checking each one's raw values/description context by hand.
    ("Irelia", "ModifiedShield"): "ShieldHealth",
    ("Galio", "ModifiedDamage"): "Damage",
    ("Fiora", "TotalDamage"): "PercentAD",
    ("Quinn", "ModifiedDamage"): "PercentAD",
    # Combined %AD+%AP abilities where the sheet already shows the %AD term
    # correctly (e.g. Darius's existing "+350% Attack Damage") but is
    # missing the second, %AP-scaling component entirely. Override points at
    # that missing component so it surfaces in the rendered text.
    ("Darius", "TotalDamage"): "AbilityScaleDamage",

    # --- Set 11 overrides (found via the same ambiguous-match scan, repeated
    # for every Set 11 champion per tft-set10-set11-ability-audit session) ---
    # Silent duplicate-value mis-picks: the placeholder shares a "Damage"/
    # "Shield" substring with an unrelated variable that happens to sit
    # earlier in the champion's raw variables dict, so the generic fallback
    # returned the WRONG number (confirmed by two different placeholders in
    # the same ability rendering identical values, which is never correct
    # for genuinely distinct effects).
    ("Ahri", "ModifiedSecondaryDamage"): "SecondaryDamage",
    ("Darius", "ModifiedDamageOnHit"): "DamageOnHit",
    ("Jax", "ModifiedAOEDamage"): "AOEDamage",
    ("Kindred", "ModifiedSecondaryDamage"): "SecondaryDamage",
    ("Soraka", "ModifiedAoEDamage"): "AoEDamage",
    ("Zoe", "ModifiedSecondarySpellDamage"): "SecondarySpellDamage",
    ("Sylas", "ModifiedAOEDamage"): "AOEDamage",
    ("Udyr", "ModifiedShurikenDamage"): "ShurikenDamage",
    ("Lillia", "ModifiedMegaDamage"): "MegaDamage",
    ("Lillia", "SecondaryDamage"): "SecondaryTargetDamage",
    ("Kobuko", "ModifiedHeal"): "Heal",  # was matching "PermanentHealthPerInterest" (contains "Heal" via "Health")
    ("Garen", "ModifiedShield"): "ShieldBase",  # was matching "PercentHealthShield" (wrong scale: %maxHP vs the AP marker shown)
    ("Yasuo", "ModifiedShield"): "BaseShield",  # was matching "ShieldDuration" — a duration value used as a shield amount
    ("Yasuo", "ModifiedShieldDamage"): "ShieldDamage",  # was matching the unrelated active-cast "Damage" variable
    # Fully unresolved (no raw variable name is a plausible substring/token
    # match) — resolved by reading the ability's raw variable list by hand.
    ("Garen", "AdditionalDamage"): "ADPercent",
    ("Kobuko", "ModifiedDamageAmount"): "DamageHPPercent",
    ("Shen", "ModifiedTrueDamage"): "PercentArmorDamage",
    ("Bard", "ModifiedPhysicalDamage"): "PercentAttackDamage",
    ("Bard", "ModifiedMagicDamage"): "APDamage",
    ("Yone", "TotalDamage"): "ADRatio",
    ("Galio", "ModifiedArmor"): "BaseResists",
    ("Galio", "ModifiedMagicResist"): "BaseResists",  # Galio grants Armor and MR 1:1 from the same pooled stat
    ("Galio", "ModifiedShieldAmount"): "ShieldPercent",
    ("Kai'Sa", "AmplifiedDamage"): "AmpedPercentAD",
    ("Kayn", "TotalSwipeDamage"): "SwipeADPercent",
    ("Irelia", "Damage_BladeHit"): "BladeHitAD",
    ("Irelia", "Damage_SpellHit"): "SpellHitAD",
    ("Wukong", "ModifiedStunDamage"): "StunPercentAttackDamage",
    ("Wukong", "ModifiedSpinDamage"): "SpinPercentAttackDamage",
    ("Sett", "TotalAoEDamage"): "PercentADSecondary",
    ("Tahm Kench", "BonusModifiedShield"): "BonusShield",
    ("Annie", "BonusHealth"): "APHealth",
    # Single AD-ratio marker on a "Total"-prefixed placeholder where the raw
    # data also carries an unused/leftover AP-flavored variable that isn't
    # actually referenced anywhere in the desc template — the AD ratio is
    # the one real, displayed number.
    ("Caitlyn", "TotalDamage"): "PercentAttackDamage",
    ("Kha'Zix", "TotalDamage"): "PercentAttackDamage",
    # Combined two-term abilities (see the derivation block below) — override
    # points at the primary/base term so rendering succeeds; the second term
    # is spliced into the composed sheet text by hand, never merged into one
    # number, per the plan's combined-%AD+%AP convention extended to
    # combined-%AP+%maxHP cases.
    ("Ornn", "ModifiedTotalShieldValue"): "BaseShieldValue",
    ("Rek'Sai", "ModifiedDamage"): "Damage",  # was matching "PercentHPDamage" — that's only the ability's second, %maxHP term
}

# Set 11 placeholders whose real value is a *computed* formula over two raw
# variables (base * multiplier), not a single raw variable lookup — the
# generic resolver cannot represent this, so these are computed by hand and
# spliced into the composed sheet text directly rather than rendered. Kept
# here as a record of the derivation, not consumed by resolve_variable().
# ("Tahm Kench", "BonusModifiedDamage"): Damage * BonusPercentDamage = 300*1.6/450*1.6/700*1.6 = 480/720/1120
# ("Qiyana", "SecondaryDamage"): Damage * SecondaryTargetPercentage = 20*0.75/30*0.75/45*0.75 = 15/22.5/33.75
# ("Ornn", "ModifiedTotalShieldValue"): BaseShieldValue (300/350/1200, AP-scaling) + PercentHealthShield (15% maxHP, constant) — combined two-term shield, not a single number
# ("Rek'Sai", "ModifiedDamage"): Damage (70/90/120, AP-scaling) + PercentHPDamage (15% maxHP, constant) — combined two-term magic damage, not a single number

_CAMEL_SPLIT_RE = re.compile(r"[A-Z]+(?=[A-Z][a-z])|[A-Z][a-z0-9]*|[a-z0-9]+")
_GENERIC_TOKENS = {"modified", "total", "tooltip", "damage", "amount", "bonus", "base", "percent"}


def _tokens(name: str) -> set[str]:
    return {t.lower() for t in _CAMEL_SPLIT_RE.findall(name)}


def fetch_set_data(set_number: int, patch: str) -> list[dict]:
    cache_path = SCRATCH_DIR / f"cd_tft_{patch}.json"
    if cache_path.exists():
        raw = cache_path.read_bytes()
    else:
        url = f"https://raw.communitydragon.org/{patch}/cdragon/tft/en_us.json"
        resp = requests.get(url, timeout=120)
        resp.raise_for_status()
        raw = resp.content
        SCRATCH_DIR.mkdir(parents=True, exist_ok=True)
        cache_path.write_bytes(raw)
    data = json.loads(raw)
    candidates = [sd for sd in data["setData"] if sd["number"] == set_number]
    if not candidates:
        raise SystemExit(f"No setData entries found for set {set_number} in patch {patch}")
    # Community Dragon splits a mid-set ("X.5") update into two separate
    # mutator blocks instead of one combined roster: the plain "TFTSet<N>"
    # block is the original release, and "TFTSet<N>_Stage2" is the refresh —
    # champions dropped in the refresh only exist in the plain block, and
    # champions added by the refresh only exist in the Stage2 block. Ignore
    # other mutators (game-mode variants like _TURBO/_PAIRS/_Event) and union
    # the two so both "X.0-only" and "X.5-only" champions are covered.
    by_mutator = {sd.get("mutator"): sd["champions"] for sd in candidates}
    base = by_mutator.get(f"TFTSet{set_number}", [])
    stage2 = by_mutator.get(f"TFTSet{set_number}_Stage2", [])
    merged = {c["name"]: c for c in base}
    merged.update({c["name"]: c for c in stage2})
    if not merged:
        # No stage-split naming found for this set; fall back to the largest block.
        merged = {c["name"]: c for c in max(candidates, key=lambda sd: len(sd["champions"]))["champions"]}
    return list(merged.values())


def read_sheet_rows(sheet: str) -> list[dict]:
    result = subprocess.run(
        [sys.executable, ".claude/scripts/sheet_sync.py", "--sheet", sheet, "read", "Champions", "--format", "json"],
        capture_output=True, text=True, check=True,
    )
    return json.loads(result.stdout)


def fmt_num(v: float) -> str:
    return str(int(v)) if float(v).is_integer() else f"{v:g}"


def resolve_variable(var_name: str, variables: dict[str, list[float]], champion: str | None = None) -> list[float] | None:
    if var_name in variables:
        return variables[var_name]
    if champion is not None:
        override = COMPUTED_OVERRIDES.get((champion, var_name))
        if override and override in variables:
            return variables[override]
    # Computed placeholders (TotalDamage, ModifiedSpellDamage, ...) aren't raw
    # variables. Best-effort: match a raw variable whose name is a substring
    # match once common computed prefixes are stripped.
    stripped = re.sub(r"^(Modified|Total)", "", var_name)
    for candidate_name, values in variables.items():
        if candidate_name == stripped or stripped in candidate_name or candidate_name in stripped:
            return values
    # Token-overlap fallback: split both names on camelCase boundaries and
    # compare word sets (handles reordered compounds like "BaseStrikeDamage"
    # vs "ModifiedBaseStrikeDamage"). Only trust a match if exactly one
    # candidate shares a non-generic token, to avoid guessing wrong.
    placeholder_tokens = _tokens(var_name) - _GENERIC_TOKENS
    if placeholder_tokens:
        matches = [
            (name, values) for name, values in variables.items()
            if placeholder_tokens & (_tokens(name) - _GENERIC_TOKENS)
        ]
        if len(matches) == 1:
            return matches[0][1]
    return None


def render_ability(ability: dict, champion: str | None = None) -> tuple[str, list[dict]]:
    """Returns (human-readable rendered text, list of {var, stars, scales} facts)."""
    desc = ability.get("desc") or ""
    variables = {v["name"]: v["value"] for v in ability.get("variables", [])}
    facts = []

    def render_tag(match: re.Match) -> str:
        tag, body = match.group(1), match.group(2)
        scale_markers = re.findall(r"%i:scale(\w+)%", body)
        var_matches = list(re.finditer(r"@(\w+)@", body))
        rendered_body = body
        for vm in var_matches:
            var_name = vm.group(1)
            values = resolve_variable(var_name, variables, champion)
            if values and len(values) > 3:
                stars = [values[1], values[2], values[3]]
                # Actual damage/heal/shield values in this data are sometimes
                # stored as a 0-1 ratio (e.g. 0.12 meaning "12% max health")
                # rather than pre-multiplied. Only rescale inside true damage
                # tags (not durations/misc TFTBonus values, which are small
                # flat numbers, not ratios) to avoid corrupting real numbers.
                is_damage_tag = tag in {"physicalDamage", "magicDamage", "trueDamage"}
                if is_damage_tag and max(abs(v) for v in stars) < 10:
                    stars = [v * 100 for v in stars]
                    rendered_num = "/".join(fmt_num(v) for v in stars) + "%"
                else:
                    rendered_num = "/".join(fmt_num(v) for v in stars)
                if tag in TAGS_OF_INTEREST and scale_markers:
                    facts.append({"var": var_name, "stars": stars, "scales": scale_markers, "tag": tag})
            else:
                rendered_num = f"?{var_name}"
            rendered_body = rendered_body.replace(f"@{var_name}@", rendered_num, 1)
        # strip the %i:scaleXXX% icon tokens from display text, append readable suffix
        rendered_body = re.sub(r"\s*%i:scale(\w+)%", "", rendered_body)
        if tag in TAGS_OF_INTEREST and scale_markers:
            labels = [SCALE_LABELS.get(s, s) for s in scale_markers]
            rendered_body = f"{rendered_body} [{'+'.join('%' + l for l in labels)}]"
        return rendered_body

    text = re.sub(r"<(\w+)>(.*?)</\1>", render_tag, desc, flags=re.S)

    # Duration/range placeholders are often outside any damage tag entirely
    # (no icon needed), so the pass above never touches them. Resolve
    # anything left standing, including "@Var*100@" inline-arithmetic forms.
    def render_bare(match: re.Match) -> str:
        var_name, multiplier = match.group(1), match.group(2)
        values = resolve_variable(var_name, variables, champion)
        if not values or len(values) <= 3:
            return match.group(0)
        stars = [values[1], values[2], values[3]]
        if multiplier:
            stars = [v * float(multiplier) for v in stars]
        return "/".join(fmt_num(v) for v in stars)

    text = re.sub(r"@(\w+)(?:\*([\d.]+))?@", render_bare, text)
    text = re.sub(r"<br\s*/?>", " ", text)
    text = re.sub(r"\s+", " ", text).strip()
    return text, facts


def find_matching_triplet(sheet_text: str, stars: list[float], tolerance: float = 0.3) -> tuple[str, bool] | None:
    """Search sheet_text for an 'N1/N2/N3' or single-N pattern close to `stars`.
    Returns (matched_substring, has_percent_marker) or None if nothing close enough.
    """
    # Explicit 3-group pattern: a repeated capturing group (e.g. `(?:/(\d+)){0,2}`)
    # only retains its *last* repetition's capture in Python's re, silently
    # dropping the middle number of any 3-part "N1/N2/N3" match. Three
    # separate optional groups avoid that.
    for m in re.finditer(r"(\d+(?:\.\d+)?)(?:/(\d+(?:\.\d+)?))?(?:/(\d+(?:\.\d+)?))?", sheet_text):
        nums = [float(g) for g in m.groups() if g is not None]
        if len(nums) == 1:
            nums = nums * len(stars)
        if len(nums) != len(stars):
            continue
        ok = all(
            (abs(a - b) <= max(1.0, tolerance * max(a, b)))
            for a, b in zip(nums, stars)
        )
        if ok:
            window_end = min(len(sheet_text), m.end() + 3)
            has_percent = sheet_text[m.start():window_end].strip().endswith("%") or (
                window_end < len(sheet_text) and sheet_text[m.end():window_end].lstrip().startswith("%")
            )
            return m.group(0), has_percent
    return None


def audit_champion(sheet_row: dict, cd_champion: dict) -> dict:
    rendered, facts = render_ability(cd_champion["ability"], cd_champion.get("name"))
    sheet_desc = sheet_row.get("Skill Description", "")
    flags = []
    for fact in facts:
        if not ({"AD", "AP"} & set(fact["scales"])):
            continue  # only flag AD/AP scaling facts; ranges/other scales are noise
        match = find_matching_triplet(sheet_desc, fact["stars"])
        if match is None:
            flags.append(
                f"No sheet number close to {'/'.join(fmt_num(v) for v in fact['stars'])} "
                f"({'/'.join('%' + SCALE_LABELS.get(s, s) for s in fact['scales'])}, var={fact['var']})"
            )
        elif not match[1]:
            flags.append(
                f"Sheet shows '{match[0]}' as flat damage, but real value scales "
                f"{'/'.join('%' + SCALE_LABELS.get(s, s) for s in fact['scales'])} (var={fact['var']})"
            )
    return {
        "name": sheet_row.get("Champion Name"),
        "sheet_desc": sheet_desc,
        "cd_rendered": rendered,
        "flags": flags,
    }


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--set", type=int, required=True, help="TFT set number, e.g. 9")
    parser.add_argument("--sheet", type=str, default=None, help="Registered sheet name, defaults to tft-set<N>")
    parser.add_argument("--patch", type=str, default=None, help="Community Dragon patch pin to fetch (overrides default)")
    args = parser.parse_args()

    sheet_name = args.sheet or f"tft-set{args.set}"
    patch = args.patch or DEFAULT_PATCH.get(args.set)
    if not patch:
        raise SystemExit(f"No default patch known for set {args.set}; pass --patch explicitly")

    print(f"Fetching Community Dragon patch {patch} for Set {args.set}...")
    cd_champions = fetch_set_data(args.set, patch)
    cd_by_name = {c["name"]: c for c in cd_champions}

    print(f"Reading sheet '{sheet_name}' Champions tab...")
    sheet_rows = read_sheet_rows(sheet_name)

    results = []
    unmatched = []
    for row in sheet_rows:
        name = row.get("Champion Name")
        cd_champ = cd_by_name.get(name)
        if not cd_champ:
            unmatched.append(name)
            continue
        results.append(audit_champion(row, cd_champ))

    flagged = [r for r in results if r["flags"]]

    print(f"\n{len(results)}/{len(sheet_rows)} sheet champions matched to Community Dragon Set {args.set} data.")
    if unmatched:
        print(f"Unmatched (no CD entry found, likely a different sub-set/patch): {unmatched}")
    print(f"{len(flagged)} champions flagged with likely flat-vs-percent or numeric mismatches.\n")

    report_lines = [f"# TFT Set {args.set} ability audit ({sheet_name} vs Community Dragon patch {patch})\n"]
    report_lines.append(f"Matched {len(results)}/{len(sheet_rows)} champions. Unmatched: {unmatched or 'none'}.\n")
    for r in results:
        if not r["flags"]:
            continue
        report_lines.append(f"## {r['name']}")
        report_lines.append(f"- **Sheet:** {r['sheet_desc']}")
        report_lines.append(f"- **Real (Community Dragon):** {r['cd_rendered']}")
        for f in r["flags"]:
            report_lines.append(f"  - FLAG: {f}")
        report_lines.append("")

    out_path = SCRATCH_DIR / f"tft_set{args.set}_ability_audit.md"
    out_path.write_text("\n".join(report_lines), encoding="utf-8")
    print(f"Full report written to {out_path}")


if __name__ == "__main__":
    main()
