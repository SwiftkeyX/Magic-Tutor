using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MagicSchool.Battle
{
    public class TraitHUDController : MonoBehaviour
    {
        [SerializeField] private TraitTracker  _tracker;
        [SerializeField] private RectTransform _panel;

        private static readonly Dictionary<TraitType, int[]> Thresholds = new Dictionary<TraitType, int[]>
        {
            { TraitType.Vanguard,     new[] { 2, 4, 6, 8 } },
            { TraitType.Striker,      new[] { 2, 4, 6, 8 } },
            { TraitType.Elementalist, new[] { 2, 4, 6, 8 } },
            { TraitType.Ranger,       new[] { 2, 4, 6    } },
            { TraitType.Kinetic,      new[] { 2, 4       } },
            { TraitType.Dreadknight,  new[] { 2, 4       } },
            { TraitType.Warden,       new[] { 2, 3, 4    } },
            { TraitType.Trickster,    new[] { 2, 4       } },
            { TraitType.Astral,       new[] { 2, 4, 6    } },
            { TraitType.Wild,         new[] { 2, 4, 6    } },
            { TraitType.Shadow,       new[] { 2, 4, 6    } },
            { TraitType.Oracle,       new[] { 2, 3       } },
            { TraitType.Guardian,     new[] { 2, 4       } },
            { TraitType.Tech,         new[] { 2, 4       } },
            { TraitType.Void,         new[] { 2, 4       } },
        };

        private readonly Dictionary<TraitType, Text> _labels = new Dictionary<TraitType, Text>();

        private void Awake()     { BuildLabels(); }
        private void OnEnable()  { if (_tracker != null) _tracker.OnTraitCountsChanged += OnCountsChanged; }
        private void OnDisable() { if (_tracker != null) _tracker.OnTraitCountsChanged -= OnCountsChanged; }

        private void BuildLabels()
        {
            if (_panel == null) return;
            var lg = _panel.GetComponent<VerticalLayoutGroup>();
            if (lg == null)
            {
                lg = _panel.gameObject.AddComponent<VerticalLayoutGroup>();
                lg.spacing                = 4f;
                lg.childAlignment         = TextAnchor.UpperLeft;
                lg.childForceExpandWidth  = true;
                lg.childForceExpandHeight = false;
            }

            foreach (TraitType t in Enum.GetValues(typeof(TraitType)))
            {
                var go  = new GameObject($"TraitLabel_{t}");
                go.transform.SetParent(_panel, false);
                var txt       = go.AddComponent<Text>();
                txt.font      = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                txt.fontSize  = 14;
                txt.color     = Color.gray;
                txt.text      = FormatLabel(t, 0, 0);
                var csf = go.AddComponent<ContentSizeFitter>();
                csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
                go.SetActive(false); // hidden until this trait has at least one placed unit
                _labels[t] = txt;
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
                kv.Value.gameObject.SetActive(count > 0);
                kv.Value.text  = FormatLabel(t, count, bp);
                kv.Value.color = bp > 0
                    ? new Color(1f, 0.85f, 0.2f)
                    : count > 0 ? Color.white : Color.gray;
            }
        }

        private static string FormatLabel(TraitType t, int count, int bp)
        {
            if (!Thresholds.TryGetValue(t, out var pts)) return t.ToString();
            int next = 0;
            foreach (int p in pts) { if (p > count) { next = p; break; } }
            if (next == 0) next = pts[pts.Length - 1];
            return $"{t} {count}/{next}";
        }
    }
}
