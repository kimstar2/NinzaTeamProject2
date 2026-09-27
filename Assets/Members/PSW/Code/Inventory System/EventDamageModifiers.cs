using System;
using System.Collections.Generic;
using Members.KJY._01.Scripts.Agent;
using Members.KJY._01.Scripts.Agent.Player;

namespace Members.PSW.Code.InventorySystem
{
    // One adventure's party-wide event effects, independent of scene combat objects.
    public sealed class EventDamageModifiers : IDamageModifiers
    {
        private sealed class Effect
        {
            public float OutgoingPercent;
            public float IncomingPercent;
            public int RemainingBattles;
        }

        private readonly List<Effect> _effects = new();

        // -1 lasts until a new adventure. Positive durations count completed battles.
        public bool TryAdd(float outgoingPercent, float incomingPercent, int battles)
        {
            if (float.IsNaN(outgoingPercent) || float.IsInfinity(outgoingPercent) ||
                float.IsNaN(incomingPercent) || float.IsInfinity(incomingPercent) ||
                (battles != -1 && battles <= 0) || (outgoingPercent == 0f && incomingPercent == 0f))
                return false;
            _effects.Add(new Effect
            {
                OutgoingPercent = outgoingPercent,
                IncomingPercent = incomingPercent,
                RemainingBattles = battles
            });
            return true;
        }

        public float GetOutgoingMultiplier(AgentDataSO attacker)
            => attacker is PlayerDataSO ? GetMultiplier(true) : 1f;

        public float GetIncomingMultiplier(AgentDataSO target)
            => target is PlayerDataSO ? GetMultiplier(false) : 1f;

        private float GetMultiplier(bool outgoing)
        {
            double percent = 0;
            foreach (var effect in _effects)
                percent += outgoing ? effect.OutgoingPercent : effect.IncomingPercent;
            return (float)Math.Max(0d, Math.Min(float.MaxValue, 1d + percent / 100d));
        }

        public void CompleteBattle()
        {
            for (int i = _effects.Count - 1; i >= 0; i--)
            {
                var effect = _effects[i];
                if (effect.RemainingBattles == -1) continue;
                if (--effect.RemainingBattles == 0) _effects.RemoveAt(i);
            }
        }

        public void Clear() => _effects.Clear();
    }
}
