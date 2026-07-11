using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace MagicSchool.Battle
{
    [RequireComponent(typeof(UIDocument))]
    public class TraitHUDController : MonoBehaviour
    {
        [SerializeField] private TraitTracker _tracker;

        private readonly Dictionary<TraitType, Label> _labels = new Dictionary<TraitType, Label>();

        private UIDocument    _document;
        private VisualElement _root;

        private void Awake()
        {
            _document = GetComponent<UIDocument>();
            _document.sortingOrder = BattleUISortOrder.TraitHUD;
            _root = _document.rootVisualElement;
            BuildLabels();
        }

        private void OnEnable()  { if (_tracker != null) _tracker.OnTraitCountsChanged += OnCountsChanged; }
        private void OnDisable() { if (_tracker != null) _tracker.OnTraitCountsChanged -= OnCountsChanged; }

        private void BuildLabels()
        {
            var root = _root.Q<VisualElement>("trait-hud-root") ?? _root;

            foreach (TraitType t in Enum.GetValues(typeof(TraitType)))
            {
                var label = new Label(FormatLabel(t, 0, 0));
                label.AddToClassList("trait-label");
                label.style.display = DisplayStyle.None; // hidden until this trait has at least one placed unit
                root.Add(label);
                _labels[t] = label;
            }
        }

        private void OnCountsChanged(
            IReadOnlyDictionary<TraitType, int> counts,
            IReadOnlyDictionary<TraitType, int> breakpoints)
        {
            foreach (var kv in _labels)
            {
                var t = kv.Key;
                counts.TryGetValue(t, out int count);
                breakpoints.TryGetValue(t, out int bp);
                kv.Value.style.display = count > 0 ? DisplayStyle.Flex : DisplayStyle.None;
                kv.Value.text = FormatLabel(t, count, bp);
                kv.Value.style.color = bp > 0
                    ? new StyleColor(new Color(1f, 0.85f, 0.2f))
                    : new StyleColor(count > 0 ? Color.white : Color.gray);
            }
        }

        private static string FormatLabel(TraitType t, int count, int bp)
        {
            if (!TraitBreakpoints.All.TryGetValue(t, out var pts)) return t.ToString();
            int next = 0;
            foreach (int p in pts) { if (p > count) { next = p; break; } }
            if (next == 0) next = pts[pts.Length - 1];
            return $"{t} {count}/{next}";
        }
    }
}
