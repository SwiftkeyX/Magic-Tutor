using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace MagicSchool.Battle
{
    /// <summary>
    /// Renders the Battle scene HUD via UI Toolkit.
    ///
    /// Owns: combatant cards (student left / enemy right), live HP bars, floating
    /// damage numbers, outcome overlay, speed-up indicator.
    ///
    /// Pure visual listener — never writes game state. The only outbound call is
    /// RunManager.Instance.CompleteBattlePhase() from the "Continue" button, which
    /// is the handshake that allows RunManager to advance the phase after the player
    /// has seen the outcome.
    ///
    /// AutoBattleResolver has no static Instance, so the reference is Inspector-wired
    /// via [SerializeField] _resolver. No FindObjectOfType anywhere in this file.
    /// </summary>
    public class BattleHUD : MonoBehaviour
    {
        // ── Inspector reference ───────────────────────────────────────────────
        [SerializeField] private AutoBattleResolver _resolver;

        // ── UIDocument ────────────────────────────────────────────────────────
        private UIDocument    _document;
        private VisualElement _root;

        // ── Structural elements ───────────────────────────────────────────────
        private VisualElement _studentColumn;
        private VisualElement _enemyColumn;
        private Label         _speedIndicator;

        // ── Outcome overlay ───────────────────────────────────────────────────
        private VisualElement _outcomeOverlay;
        private Label         _outcomeTitle;
        private Label         _outcomeSub;
        private Button        _continueButton;

        // ── Per-combatant card tracking ───────────────────────────────────────
        /// <summary>id → card root VisualElement</summary>
        private readonly Dictionary<string, VisualElement> _cards         = new Dictionary<string, VisualElement>();
        /// <summary>id → HP fill bar element</summary>
        private readonly Dictionary<string, VisualElement> _hpFills       = new Dictionary<string, VisualElement>();
        /// <summary>id → HP text label</summary>
        private readonly Dictionary<string, Label>         _hpLabels      = new Dictionary<string, Label>();
        /// <summary>id → container for floating damage numbers</summary>
        private readonly Dictionary<string, VisualElement> _dmgContainers = new Dictionary<string, VisualElement>();
        /// <summary>id → true when unit uses MagicAttack flag (for damage color)</summary>
        private readonly Dictionary<string, bool>          _isMagic       = new Dictionary<string, bool>();
        /// <summary>id → MaxHP (cached so we don't call resolver per-frame)</summary>
        private readonly Dictionary<string, int>           _maxHps        = new Dictionary<string, int>();

        // ── Cards-built guard ─────────────────────────────────────────────────
        private bool _cardsBuilt;

        // ── Lifecycle ─────────────────────────────────────────────────────────

        private void Awake()
        {
            _document = GetComponent<UIDocument>();
            if (_document == null)
            {
                Debug.LogError("[BattleHUD] UIDocument component not found on this GameObject.");
                enabled = false;
                return;
            }

            _root = _document.rootVisualElement;

            // Cache element references — never call Q<> outside Awake
            _studentColumn  = _root.Q<VisualElement>("student-column");
            _enemyColumn    = _root.Q<VisualElement>("enemy-column");
            _speedIndicator = _root.Q<Label>("speed-indicator");
            _outcomeOverlay = _root.Q<VisualElement>("outcome-overlay");
            _outcomeTitle   = _root.Q<Label>("outcome-title");
            _outcomeSub     = _root.Q<Label>("outcome-sub");
            _continueButton = _root.Q<Button>("continue-button");

            // Wire Continue button in code — never rely on Inspector wiring
            if (_continueButton != null)
            {
                _continueButton.SetEnabled(false);
                _continueButton.clicked += OnContinueClicked;
            }

            // Initial display state
            SetDisplay(_outcomeOverlay, false);
            SetDisplay(_speedIndicator, false);
        }

        private void OnEnable()
        {
            if (_resolver == null)
            {
                Debug.LogWarning("[BattleHUD] _resolver not assigned; events not subscribed. " +
                                 "Assign AutoBattleResolver via Inspector.");
                return;
            }

            _resolver.OnCombatantActed    += HandleCombatantActed;
            _resolver.OnCombatantDefeated += HandleCombatantDefeated;
            _resolver.OnBattleComplete    += HandleBattleComplete;

            if (InputHandler.Instance != null)
            {
                InputHandler.Instance.OnSpeedUpStarted   += HandleSpeedUpStarted;
                InputHandler.Instance.OnSpeedUpCancelled += HandleSpeedUpCancelled;
            }
        }

        private void OnDisable()
        {
            if (_resolver != null)
            {
                _resolver.OnCombatantActed    -= HandleCombatantActed;
                _resolver.OnCombatantDefeated -= HandleCombatantDefeated;
                _resolver.OnBattleComplete    -= HandleBattleComplete;
            }

            if (InputHandler.Instance != null)
            {
                InputHandler.Instance.OnSpeedUpStarted   -= HandleSpeedUpStarted;
                InputHandler.Instance.OnSpeedUpCancelled -= HandleSpeedUpCancelled;
            }
        }

        private void Start()
        {
            // Wait two frames so RunManager's scene-setup coroutine (which yields null
            // once) has called SetCombatants() before we read GetCombatantSnapshots().
            StartCoroutine(BuildCardsDelayed());
        }

        private IEnumerator BuildCardsDelayed()
        {
            yield return null;
            yield return null;
            BuildCombatantCards();
        }

        // ── Card building ──────────────────────────────────────────────────────

        private void BuildCombatantCards()
        {
            if (_resolver == null) return;

            var snapshots = _resolver.GetCombatantSnapshots();
            if (snapshots == null || snapshots.Count == 0)
            {
                Debug.LogWarning("[BattleHUD] GetCombatantSnapshots() returned no combatants. " +
                                 "Ensure AutoBattleResolver has been initialized before BattleHUD builds cards.");
                return;
            }

            // Clear any previous build
            _studentColumn?.Clear();
            _enemyColumn?.Clear();
            _cards.Clear();
            _hpFills.Clear();
            _hpLabels.Clear();
            _dmgContainers.Clear();
            _isMagic.Clear();
            _maxHps.Clear();

            foreach (var snap in snapshots)
            {
                bool magicUnit = snap.Flags != null && snap.Flags.Contains(BattleBehaviorFlag.MagicAttack);
                _isMagic[snap.Id] = magicUnit;
                _maxHps[snap.Id]  = snap.MaxHP;

                var card = BuildCard(snap);

                if (snap.IsStudent)
                    _studentColumn?.Add(card);
                else
                    _enemyColumn?.Add(card);

                _cards[snap.Id] = card;
            }

            _cardsBuilt = true;
            Debug.Log($"[BattleHUD] Built {snapshots.Count} combatant cards " +
                      $"({snapshots.Count(s => s.IsStudent)} students, " +
                      $"{snapshots.Count(s => !s.IsStudent)} enemies).");
        }

        private VisualElement BuildCard(CombatantSnapshot snap)
        {
            var card = new VisualElement();
            card.name = $"card-{snap.Id}";
            card.AddToClassList("combatant-card");
            card.pickingMode = PickingMode.Ignore;

            // ── Name ──────────────────────────────────────────────────────────
            var nameLabel = new Label(snap.DisplayName);
            nameLabel.AddToClassList("card-name");
            nameLabel.pickingMode = PickingMode.Ignore;
            card.Add(nameLabel);

            // ── HP bar ────────────────────────────────────────────────────────
            var hpBarBg = new VisualElement();
            hpBarBg.AddToClassList("hp-bar-bg");
            hpBarBg.pickingMode = PickingMode.Ignore;

            var hpFill = new VisualElement();
            hpFill.AddToClassList("hp-bar-fill");
            hpFill.pickingMode = PickingMode.Ignore;
            // Full HP at start
            hpFill.style.width = Length.Percent(100f);
            hpBarBg.Add(hpFill);
            card.Add(hpBarBg);

            _hpFills[snap.Id] = hpFill;

            // ── HP label ──────────────────────────────────────────────────────
            var hpLabel = new Label($"{snap.MaxHP} / {snap.MaxHP}");
            hpLabel.AddToClassList("hp-label");
            hpLabel.pickingMode = PickingMode.Ignore;
            card.Add(hpLabel);

            _hpLabels[snap.Id] = hpLabel;

            // ── Behavior flag badges ──────────────────────────────────────────
            if (snap.Flags != null && snap.Flags.Count > 0)
            {
                var flagsRow = new VisualElement();
                flagsRow.AddToClassList("flags-row");
                flagsRow.pickingMode = PickingMode.Ignore;
                foreach (var flag in snap.Flags)
                {
                    var badge = new Label(FlagToShortName(flag));
                    badge.AddToClassList("flag-badge");
                    badge.pickingMode = PickingMode.Ignore;
                    flagsRow.Add(badge);
                }
                card.Add(flagsRow);
            }

            // ── Damage number container ───────────────────────────────────────
            var dmgContainer = new VisualElement();
            dmgContainer.AddToClassList("dmg-container");
            dmgContainer.pickingMode = PickingMode.Ignore;
            card.Add(dmgContainer);

            _dmgContainers[snap.Id] = dmgContainer;

            return card;
        }

        // ── Event handlers ────────────────────────────────────────────────────

        private void HandleCombatantActed(string actorId, string targetId, int damage, List<string> tags)
        {
            // Lazy card build safety: if cards weren't ready when battle started
            if (!_cardsBuilt)
            {
                Debug.LogWarning("[BattleHUD] OnCombatantActed fired before cards were built. Building now.");
                BuildCombatantCards();
                if (!_cardsBuilt) return; // if still failed, bail
            }

            // ── Update target's HP bar ────────────────────────────────────────
            if (!_hpFills.TryGetValue(targetId, out var hpFill))
            {
                Debug.LogWarning($"[BattleHUD] OnCombatantActed: target '{targetId}' not in card list.");
                return;
            }

            int currentHp = _resolver.GetCurrentHP(targetId);
            int maxHp     = _maxHps.TryGetValue(targetId, out var mh) ? mh : 1;
            float ratio   = Mathf.Clamp01((float)currentHp / maxHp);

            hpFill.style.width = Length.Percent(ratio * 100f);
            UpdateHpBarColor(hpFill, ratio);

            if (_hpLabels.TryGetValue(targetId, out var hpLabel))
                hpLabel.text = $"{Mathf.Max(0, currentHp)} / {maxHp}";

            // ── Spawn floating damage number ──────────────────────────────────
            bool isMagicAttack = _isMagic.TryGetValue(actorId, out var magic) && magic;
            bool isAbility     = tags != null && tags.Contains("ABILITY");
            // Use magic color for explicit magic attackers OR when the event carries ABILITY tag
            string dmgClass = (isMagicAttack || isAbility) ? "dmg-magic" : "dmg-physical";

            if (_dmgContainers.TryGetValue(targetId, out var container) && damage > 0)
                StartCoroutine(SpawnDamageNumber(container, damage, dmgClass));
        }

        private void HandleCombatantDefeated(string id)
        {
            if (!_cards.TryGetValue(id, out var card))
            {
                // Already removed or never existed — idempotent no-op per GDD Edge Case
                return;
            }

            // Collapse card (simple hide — fade+shrink is optional for this pass)
            SetDisplay(card, false);

            // Clean up tracking dictionaries
            _cards.Remove(id);
            _hpFills.Remove(id);
            _hpLabels.Remove(id);
            _dmgContainers.Remove(id);

            Debug.Log($"[BattleHUD] Card for '{id}' hidden on defeat.");
        }

        private void HandleBattleComplete(BattleResult result)
        {
            if (_outcomeOverlay == null) return;

            SetDisplay(_outcomeOverlay, true);

            if (_outcomeTitle != null)
                _outcomeTitle.text = result.Won ? "Victory!" : "Defeated";

            if (_outcomeSub != null)
            {
                bool showSub = result.TimedOut;
                _outcomeSub.text = "(Time Limit)";
                SetDisplay(_outcomeSub, showSub);
            }

            // Enable Continue only after battle is complete (GDD AC)
            if (_continueButton != null)
                _continueButton.SetEnabled(true);

            Debug.Log($"[BattleHUD] Outcome overlay shown — " +
                      $"{(result.Won ? "Victory" : "Defeated")}" +
                      $"{(result.TimedOut ? " (Time Limit)" : string.Empty)}");
        }

        // ── Speed-up indicator ────────────────────────────────────────────────

        private void HandleSpeedUpStarted()
        {
            SetDisplay(_speedIndicator, true);
        }

        private void HandleSpeedUpCancelled()
        {
            SetDisplay(_speedIndicator, false);
        }

        // ── Continue button ───────────────────────────────────────────────────

        private void OnContinueClicked()
        {
            if (RunManager.Instance == null)
            {
                Debug.LogError("[BattleHUD] RunManager.Instance is null. Cannot advance phase.");
                return;
            }

            // Hide overlay immediately so double-click does nothing visible
            if (_continueButton != null)
                _continueButton.SetEnabled(false);

            RunManager.Instance.CompleteBattlePhase();
        }

        // ── Damage number coroutine ───────────────────────────────────────────

        private IEnumerator SpawnDamageNumber(VisualElement container, int damage, string colorClass)
        {
            var label = new Label($"-{damage}");
            label.AddToClassList("dmg-number");
            label.AddToClassList(colorClass);
            label.pickingMode = PickingMode.Ignore;
            container.Add(label);

            const float duration = 0.8f;
            float       elapsed  = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);

                // Fade out in the second half of the animation
                float alpha = t < 0.5f ? 1f : 1f - ((t - 0.5f) / 0.5f);
                label.style.opacity = alpha;

                yield return null;
            }

            if (container.Contains(label))
                container.Remove(label);
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private static void UpdateHpBarColor(VisualElement fill, float ratio)
        {
            fill.RemoveFromClassList("hp-bar-high");
            fill.RemoveFromClassList("hp-bar-mid");
            fill.RemoveFromClassList("hp-bar-low");

            if (ratio > 0.6f)
                fill.AddToClassList("hp-bar-high");
            else if (ratio > 0.3f)
                fill.AddToClassList("hp-bar-mid");
            else
                fill.AddToClassList("hp-bar-low");
        }

        private static void SetDisplay(VisualElement el, bool visible)
        {
            if (el != null)
                el.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
        }

        private static string FlagToShortName(BattleBehaviorFlag flag) => flag switch
        {
            BattleBehaviorFlag.MagicAttack        => "Magic",
            BattleBehaviorFlag.AOEAttack          => "AOE",
            BattleBehaviorFlag.FirstHitDouble     => "×2 First Hit",
            BattleBehaviorFlag.TakesReducedDamage => "Tanky",
            BattleBehaviorFlag.ShadowSurge        => "Shadow",
            _                                     => flag.ToString(),
        };
    }
}
