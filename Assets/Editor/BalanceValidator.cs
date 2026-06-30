// Assets/Editor/BalanceValidator.cs
// Magic School/Validate Balance — runs 1v1 simulations for every champion pair and
// prints a win-rate matrix to the Unity Console. Edit-mode only; never included in builds.

using System;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;
using MagicSchool.Battle;

namespace MagicSchool.Editor
{
    public static class BalanceValidator
    {
        private const int   Trials      = 200;
        private const float TickDelay   = 0.1f;   // matches AutoBattleResolver
        private const int   MaxTicks    = 1200;   // 120s max
        private const bool  IncludeCrit = false;  // false = pure base stats; true = with CRIT variance

        // Expected win-rate bands for higher-cost vs lower-cost (for pass/fail logging)
        private static readonly (int costDiff, float lo, float hi)[] _bands =
        {
            (1, 0.55f, 0.80f),  // 2c vs 1c
            (2, 0.70f, 0.92f),  // 3c vs 1c
            (3, 0.80f, 0.96f),  // 4c vs 1c
            (4, 0.88f, 0.98f),  // 5c vs 1c
        };

        [MenuItem("Magic School/Validate Balance")]
        public static void Validate()
        {
            var roster  = BuildRoster();
            int n       = roster.Count;
            var results = new float[n, n];

            for (int i = 0; i < n; i++)
            for (int j = 0; j < n; j++)
            {
                if (i == j) { results[i, j] = 0.5f; continue; }
                results[i, j] = WinRate(roster[i], roster[j], Trials);
            }

            PrintMatrix(roster, results, n);
            PrintOutliers(roster, results, n);
        }

        // ── Simulation ───────────────────────────────────────────────────────

        private static float WinRate(SimUnit a, SimUnit b, int trials)
        {
            int wins = 0;
            for (int t = 0; t < trials; t++)
                if (Simulate(a.Clone(), b.Clone())) wins++;
            return (float)wins / trials;
        }

        private static bool Simulate(SimUnit a, SimUnit b)
        {
            for (int tick = 0; tick < MaxTicks; tick++)
            {
                a.Progress += a.AS * TickDelay;
                b.Progress += b.AS * TickDelay;

                if (a.Progress >= 1.0f)
                {
                    b.HP -= Damage(a, b);
                    if (b.HP <= 0) return true;
                    a.Progress -= 1.0f;
                }

                if (b.Progress >= 1.0f)
                {
                    a.HP -= Damage(b, a);
                    if (a.HP <= 0) return false;
                    b.Progress -= 1.0f;
                }
            }
            // timeout: side with more HP wins
            return a.HP > b.HP;
        }

        private static int Damage(SimUnit attacker, SimUnit defender)
        {
            int raw = attacker.IsMagic ? attacker.MG : attacker.ATK;
            int def = attacker.IsMagic ? defender.MR : defender.DEF;

            if (IncludeCrit && UnityEngine.Random.Range(0, 100) < attacker.CRIT)
                raw *= 2;

            return Math.Max(1, (int)(raw * 100f / (100 + def)));
        }

        // ── Output ──────────────────────────────────────────────────────────

        private static void PrintMatrix(List<SimUnit> roster, float[,] results, int n)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"\n=== Balance Win-Rate Matrix ({Trials} trials, CRIT={IncludeCrit}) ===");
            sb.Append("Attacker\\Defender".PadRight(22));
            foreach (var u in roster) sb.Append(u.ShortName.PadRight(14));
            sb.AppendLine();

            for (int i = 0; i < n; i++)
            {
                sb.Append(roster[i].ShortName.PadRight(22));
                for (int j = 0; j < n; j++)
                    sb.Append(i == j ? " — ".PadRight(14) : $"{results[i,j]:P0}".PadRight(14));
                sb.AppendLine();
            }
            Debug.Log(sb.ToString());
        }

        private static void PrintOutliers(List<SimUnit> roster, float[,] results, int n)
        {
            var sb = new StringBuilder();
            sb.AppendLine("\n=== Outliers (outside expected band for cost difference) ===");
            bool any = false;

            for (int i = 0; i < n; i++)
            for (int j = 0; j < n; j++)
            {
                if (i == j) continue;
                int costDiff = roster[i].Cost - roster[j].Cost;
                if (costDiff <= 0) continue;   // only check higher vs lower

                float wr = results[i, j];
                foreach (var band in _bands)
                {
                    if (band.costDiff != costDiff) continue;
                    if (wr < band.lo || wr > band.hi)
                    {
                        string flag = wr < band.lo ? "❌ TOO WEAK" : "❌ TOO STRONG";
                        sb.AppendLine($"  {roster[i].ShortName} (c{roster[i].Cost}) vs {roster[j].ShortName} (c{roster[j].Cost}): {wr:P0}  expected {band.lo:P0}–{band.hi:P0}  {flag}");
                        any = true;
                    }
                }
            }

            if (!any) sb.AppendLine("  ✅ All matchups within expected bands.");
            Debug.Log(sb.ToString());
        }

        // ── Data ─────────────────────────────────────────────────────────────

        private static List<SimUnit> BuildRoster()
        {
            // Mirrors ChampionRoster.cs exactly. Update both together.
            // Columns: name, cost, HP, ATK, DEF, MG, MR, AS, CRIT, isMagic
            return new List<SimUnit>
            {
                new SimUnit("Ironclad",         1,  650, 50, 45,  0, 45, 0.60f,  2, false),
                new SimUnit("Bloodhound",        1,  500, 55, 20,  0, 20, 0.75f, 12, false),
                new SimUnit("Pyromancer",        1,  480, 40, 20, 65, 20, 0.70f,  8, true ),
                new SimUnit("Windrunner",        2,  600, 65, 20,  0, 20, 0.80f, 14, false),
                new SimUnit("Grove Keeper",      2,  600, 40, 25, 55, 25, 0.65f,  4, true ),
                new SimUnit("Shadowblade",       2,  580, 65, 25,  0, 25, 0.80f, 20, false),
                new SimUnit("Phalanx",           3,  800, 60, 50,  0, 50, 0.60f,  3, false),
                new SimUnit("Stormbringer",      3,  700, 50, 25, 65, 25, 0.70f,  7, true ),
                new SimUnit("Phantom Assassin",  4,  750, 65, 30, 90, 30, 0.80f, 22, true ),
                new SimUnit("Dread Overlord",    5, 1000, 80, 60,  0, 60, 0.65f,  4, false),
            };
        }

        // ── SimUnit ──────────────────────────────────────────────────────────

        private class SimUnit
        {
            public readonly string ShortName;
            public readonly int    Cost;
            public readonly int    MaxHP;
            public readonly int    ATK;
            public readonly int    DEF;
            public readonly int    MG;
            public readonly int    MR;
            public readonly float  AS;
            public readonly int    CRIT;
            public readonly bool   IsMagic;

            public int   HP;
            public float Progress;

            public SimUnit(string name, int cost, int hp, int atk, int def,
                           int mg, int mr, float aSpeed, int crit, bool magic)
            {
                ShortName = name; Cost = cost; MaxHP = hp;
                ATK = atk; DEF = def; MG = mg; MR = mr;
                AS = aSpeed; CRIT = crit; IsMagic = magic;
                HP       = MaxHP;
                Progress = 0f;
            }

            public SimUnit Clone()
            {
                var c    = (SimUnit)MemberwiseClone();
                c.HP       = MaxHP;
                c.Progress = 0f;
                return c;
            }
        }
    }
}
