# TFT Set 9 Skill Coverage — Full Table

Per-champion detail for [TFT Set 9 → SkillDefinition Coverage Audit](file:///c:/Organized%20Files/Working/Unity/Unity%20Project/Magic%20School/.claude/docs/hero-design/tft-set9-skill-coverage.md) — see that doc for methodology, the ✅/⚠️/❌ legend, and the recurring gap themes referenced in the Notes column below. Sorted by champion cost (1 → 5), then alphabetically within each cost tier.

| Champion | Cost | Design Role | Best-fit Archetype | Verdict | Notes |
|---|---|---|---|---|---|
| Cassiopeia | 1 | Carry | StandardProjectile | ⚠️ | Base hit works; Wound (heal reduction) status and the "if already Wounded, +30%" conditional aren't. |
| Cho'Gath | 1 | Tank | — | ❌ | Damage scales off target's max HP (no such damage field — only shields have `ShieldPctOfMaxHP`); execute-devour with permanent stat growth also unsupported. |
| Irelia | 1 | Tank | SelfBuff | ⚠️ | Shield grant works; the "explode into AoE damage when the shield expires, scaled by amount absorbed" reactive trigger isn't modeled. |
| Jhin | 1 | Carry | LaserBeam | ⚠️ | Line-pierce shape fits; per-hit damage falloff ("each hit reduces this damage by 56%") isn't supported. |
| Kayle | 1 | Carry | — | ❌ | Entirely passive, level-gated escalating attack modifiers — no active cast component at all. |
| Malzahar | 1 | Carry | GroundAoE | ⚠️ | AoE damage works; "destroy 50% of Shields" (enemy shield removal) has no equivalent — we can grant shields, not strip them. |
| Maokai | 1 | Tank | — | ❌ | Passive mana-on-enemy-cast trigger + "heal on next attack" (an attack-modifier state, not a direct skill effect) — neither exists in the schema. |
| Orianna | 1 | Support | StandardProjectile (Ally) | ⚠️ | Shield-lowest-HP-ally works via `TargetTeam.Ally` + `LowestHpPct`; "empower my next attack" is an attack-modifier state with no equivalent. |
| Poppy | 1 | Tank | SelfBuff / StandardProjectile | ⚠️ | Wants self-shield **and** enemy damage in one cast — no single archetype does both (gap #2); must pick one and lose the other. |
| Renekton | 1 | Tank | GroundAoE | ⚠️ | Adjacent AoE damage works; self-heal scaled by *how many enemies were hit* (a variable, hit-count-conditioned heal) isn't modeled. |
| Samira | 1 | Carry | StandardProjectile | ⚠️ | Base hit works; the permanent-for-combat Armor-shred debuff has no generic field (only the hardcoded per-hero `DreadZone` shreds, and only in an area, not single-target). |
| Tristana | 1 | Carry | SelfBuff | ⚠️ | `AttackSpeedBuffPct` covers the AS grant; "attacks explode in an AoE for the buff duration" is an attack-modifier-during-buff-window with no equivalent. |
| Viego | 1 | Carry | StandardProjectile | ⚠️ | Base hit works; the permanent-stacking bonus damage applied to *all future basic attacks* is a persistent passive modifier with no equivalent. |
| Ashe | 2 | Support | LaserBeam (approx.) | ⚠️ | No cone filter shape exists (only `Adjacent/WithinRange/Global/LinearPath`) — line approximates the cone; Chill (AS reduction) has no status field. |
| Galio | 2 | Tank | SelfBuff → GroundAoE (2-phase) | ⚠️ | No %-damage-reduction field (only shields), no heal-over-time (only instant), and the "heal now, AoE after a delay" two-phase sequencing needs two archetypes in one cast. |
| Jinx | 2 | Carry | GroundAoE | ⚠️ | Multi-target-in-range works; "5 rockets at *random* enemies" has no random targeting sort (only deterministic sorts exist). |
| Kassadin | 2 | Tank | SelfBuff → GroundAoE (2-phase) | ⚠️ | Shield and cone damage are each representable alone, but not in one cast (see gap #2); Disarm status also unsupported. |
| Kled | 2 | Tank | — | ❌ | Passive shield→dismount→stacking-AS state machine + 4-star execute threshold; only a bare "gain a shield" sliver would map, which isn't the kit. |
| Qiyana | 2 | Carry | LaserBeam | ⚠️ | Line-hit shape fits; terrain-based empowerment (no terrain system exists) and per-target-order CC duration differences aren't modeled. |
| Sett | 2 | Tank | StandardProjectile/GroundAoE | ⚠️ | Damage + Stun work; "if only one enemy grabbed, +50% damage and stun duration" conditional branch isn't modeled. |
| Soraka | 2 | Support | StandardProjectile (Ally) | ⚠️ | Base heal-lowest-HP-ally works; the conditional double-heal-if-below-50% and the delayed 5-second multi-target star barrage aren't. |
| Swain | 2 | Tank | — | ❌ | Transform + stacking double-transform escalation with an ongoing per-second AoE while transformed — no stance/transform mechanic exists. |
| Taliyah | 2 | Carry | StandardProjectile | ⚠️ | Active hit + Stun/knockup maps cleanly on its own; the passive ("whenever anything is knocked up/back anywhere, throw a boulder") is out of scope. |
| Teemo | 2 | Carry | GroundAoE | ⚠️ | AoE targeting/radius fits; the "over 3 seconds" DoT nature and Wound status collapse to one instant hit (matches the mushroom-density correction already logged in `.claude/docs/balance/calculations/`). |
| Vi | 2 | Tank | SelfBuff / GroundAoE | ⚠️ | Same self-buff-and-damage conflict as Poppy/Sejuani (gap #2); Sunder status also unsupported. |
| Warwick | 2 | Tank | SelfBuff (active only) | ⚠️ | AS buff via `SelfBuff` works, but it can't also Stun adjacent enemies in the same cast (gap #2 — `ExecuteSelfBuff` never applies CC to enemies); on-attack heal passive is out of scope. |
| Zed | 2 | Carry | — | ❌ | Core mechanic is a summoned shadow that doubles the hits — no summon mechanic exists. |
| Akshan | 3 | Carry | StandardProjectile | ✅ | Single target, `Sorts:[Furthest]`, flat physical hit — clean 1:1 fit. |
| Darius | 3 | Carry | StandardProjectile | ⚠️ | Base hit works; "if target dies, recast at reduced damage" on-kill chaining isn't modeled. |
| Ekko | 3 | Carry | StandardProjectile | ⚠️ | Damage to current target works; heal-based-on-damage-taken-in-last-4s (a rolling tracker) has no equivalent. |
| Garen | 3 | Carry | GroundAoE | ⚠️ | Approximated as one instant adjacent hit; the real kit is 12 ticking hits over 4s — no sustained-channel-with-repeated-damage model exists. |
| Jayce | 3 | Support | ExplodingProjectile | ⚠️ | The explosion-on-impact half works; the self-AS-buff + "AP buff to allies on my left/right" positional ally buff has no equivalent (no AP-buff field on ally support, no left/right targeting). |
| Kalista | 3 | Carry | BlinkStrike (approx.) | ⚠️ | Active "impale 6 spears" approximated via multi-hit loop; the defining on-attack passive (spear stacking) is entirely out of scope. |
| Karma | 3 | Carry | GroundAoE | ⚠️ | Base burst to adjacent enemies works; "every 3rd cast launches 3 bursts" cast-counter escalation isn't modeled. |
| Katarina | 3 | Carry | BouncingChain | ⚠️ | Multi-target teleport-and-hit approximated via chaining; Wound status isn't modeled. |
| Lissandra | 3 | Support | GroundAoE | ⚠️ | AoE damage + Stun work; the "+10% damage taken for 4s" vulnerability debuff has no field. |
| Miss Fortune | 3 | Carry | GroundAoE | ⚠️ | AoE damage works; shield-destruction and "reduce incoming Shields by 35% for 5s" have no equivalent. |
| Quinn | 3 | Carry | — | ❌ | Row/column targeting has no filter equivalent; mark-then-split-damage-among-marked is a two-phase mechanic with no equivalent. |
| Rek'Sai | 3 | Carry | — | ❌ | Conditional true-damage-below-threshold, mark-and-bonus-true-damage, and on-kill heal/cleave are all unsupported branches. |
| Sona | 3 | Support | LaserBeam | ⚠️ | Wave-to-largest-clump fits via `LargestCluster`; per-hit falloff (gap #6) and "allies hit gain AS for the rest of combat" (no AS-buff path for allies hit by an enemy-targeted beam) aren't modeled. |
| Taric | 3 | Tank | SelfBuff | ✅ | Shield self + `InterceptPct` (redirect % of damage taken by adjacent allies) — this is literally the field's design precedent; clean 1:1 fit. |
| Vel'Koz | 3 | Carry | LaserBeam | ⚠️ | Single-line pierce fits the first pass; "splits in two, 50% less damage per pass" (a branching beam with falloff) isn't modeled. |
| Aphelios | 4 | Carry | GroundAoE | ⚠️ | Initial blast via `LargestCluster` targeting works; the 7s stacking "equip Chakrams" attack-modifier passive is out of scope. |
| Azir | 4 | Carry | — | ❌ | Passive on-3rd-attack + active Sand Soldier summon; no summon mechanic exists. |
| Fiora | 4 | Carry | BlinkStrike | ⚠️ | Multi-hit-lowest-HP loop is close; untargetable state, true-damage portion, damage-based self-heal, and retarget-on-kill aren't. |
| Gwen | 4 | Carry | LaserBeam (approx.) | ⚠️ | Cone approximated as a line; "every 3 casts, gain Armor/MR" cast-counter escalation isn't modeled. |
| Jarvan IV | 4 | Tank | GroundAoE | ✅ | AoE damage + `CcType.Stun` around a hex near an enemy — maps cleanly (leap is presentation, not mechanics). |
| Kai'Sa | 4 | Carry | GroundAoE | ⚠️ | Approximated as an AoE hit; "split N missiles across exactly 4 nearest enemies" and the dash-away repositioning aren't modeled precisely. |
| Lux | 4 | Carry | LaserBeam | ⚠️ | `IsChannel=true` + beam damage fits the shape; per-second ticking MR shred and "shift to new target if they die" aren't modeled. |
| Nasus | 4 | Tank | — | ❌ | Core mechanic drains stats from multiple enemies into self; `SelfBuff` only grants flat/self-referential bonuses, never drains from targets. |
| Neeko | 4 | Tank | SelfBuff → GroundAoE (2-phase) | ⚠️ | Same two-phase issue as Galio/Irelia — shield-then-delayed-AoE can't be one cast. |
| Nilah | 4 | Carry | SelfBuff | ⚠️ | Active shield+dash maps to `SelfBuff`; the passive cone-attack + stacking AS (the majority of her kit) is out of scope. |
| Sejuani | 4 | Tank | SelfBuff / GroundAoE | ⚠️ | Same self-buff-and-damage conflict as Poppy/Vi (gap #2); Chill status also unsupported; passive on-ally-attack bonus is out of scope. |
| Shen | 4 | Tank | SelfBuff | ⚠️ | Self-shield + ally-shield via `SecondaryFilter`/`SecondaryHitsAll` is close, but "exactly the 2 lowest-HP allies" (a count-limited sort) isn't possible — only "all resolved" or "just the first"; the shield-refresh burst damage after is a second phase that can't chain from `SelfBuff`. |
| Silco | 4 | Support | — | ❌ | A persistent zone that simultaneously deals damage-over-time to enemies and heals allies inside it has no equivalent — `DreadZone` only carries a DEF/MR shred, not damage or healing. |
| Urgot | 4 | Tank | SelfBuff (active only) | ⚠️ | Active shield+dash maps to `SelfBuff`; cooldown-reset-on-cast and the defining passive (6 legs firing on enemies entering range) are both out of scope. |
| Yasuo | 4 | Carry | LaserBeam | ⚠️ | Beam-to-furthest-enemy + Stun covers the whirlwind; the following dash-slash-and-slam (a second, distinct AoE phase) can't chain from one archetype call. |
| Zeri | 4 | Carry | — | ❌ | Passive execute-below-8%-HP threshold + a sustained "attacks chain lightning for the next 8 seconds" buff-to-basic-attacks — neither an execute-threshold nor a duration-based attack modifier exists. |
| Aatrox | 5 | Carry | — | ❌ | Entire kit is a transform/stance state (AS→AD conversion, omnivamp while transformed); no stance mechanic exists. |
| Ahri | 5 | Carry | GroundAoE | ⚠️ | Base AoE around current target works; "every 2 casts" bonus wave and stacking Mana Reave debuff aren't modeled. |
| Bel'Veth | 5 | Carry | BlinkStrike | ⚠️ | Multi-hit-lowest-HP loop via `HitCount` is close; the true-damage/%-max-HP portion of each lash isn't representable. |
| Heimerdinger | 5 | Support | GroundAoE | ✅ | `LargestCluster` targeting + AoE damage + `CcType.Stun` — clean 1:1 fit. |
| K'Sante | 5 | Tank | — | ❌ | Core mechanic is forced displacement/knockback + collision-chain damage; no positional-push mechanic exists. |
| Ryze | 5 | Support | GroundAoE | ⚠️ | AoE damage + Stun fits the shape; dual DEF+MR scaling (only one `SecondaryMultiplier` off DEF exists) and the ally Armor/MR buff (no such field on ally support) aren't. |
| Senna | 5 | Support | LaserBeam | ⚠️ | Beam-to-furthest-enemy fits via `Sorts:[Furthest]` + `LinearPath`; `LaserBeam`'s executor never applies Ally-Support Resolution to allies caught in the beam, so the ally shield-on-hit is lost. |
| Sion | 5 | Tank | GroundAoE (active only) | ⚠️ | The active charge/knockup/stun maps reasonably to `GroundAoE` + `CcType.Stun`; the defining on-death revive-with-decaying-HP passive has no equivalent at all. |
| Xayah | 5 | Carry | BlinkStrike (approx.) | ⚠️ | Multi-hit loop approximates the feather barrage, but it re-picks the lowest-HP% target each hit rather than staying locked on the original target; permanent-until-combat Armor-shred-per-feather and the mana-on-attack passive aren't modeled. |
