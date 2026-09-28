using System;
using System.Collections.Generic;

namespace Members.PSW.Code.InventorySystem
{
    // Victory gold effects live for an adventure or a number of completed battles.
    public sealed class EventGoldModifiers
    {
        private readonly List<(float percent, int battles)> _effects = new();
        public event Action Changed;
        public IEnumerable<(float percent, int battles)> Effects => _effects.AsReadOnly();

        public void Add(float percent, int battles)
        {
            if (float.IsNaN(percent) || float.IsInfinity(percent) ||
                (battles != -1 && battles <= 0)) return;
            _effects.Add((percent, battles));
            Changed?.Invoke();
        }

        public int Apply(int gold)
        {
            double percent = 0;
            foreach (var effect in _effects) percent += effect.percent;
            return (int)Math.Min(int.MaxValue, Math.Floor(Math.Max(0, gold) * Math.Max(0, 1 + percent / 100)));
        }

        public void CompleteBattle()
        {
            for (int i = _effects.Count - 1; i >= 0; i--)
            {
                var effect = _effects[i];
                if (effect.battles == -1) continue;
                if (effect.battles == 1) _effects.RemoveAt(i);
                else _effects[i] = (effect.percent, effect.battles - 1);
            }
            Changed?.Invoke();
        }

        public void Clear() { _effects.Clear(); Changed?.Invoke(); }
    }
}
