using System;
using UnityEngine;

namespace Members.KJY._01.Scripts.Agent
{
    public class CombatEffects
    {
        public int MarkedHits { get; private set; }
        public int GuardHits { get; private set; }
        public bool IsEmpowered { get; private set; }
        public event Action Changed;

        public void Mark() { MarkedHits = 2; Changed?.Invoke(); }
        public void Guard() { GuardHits = 2; Changed?.Invoke(); }
        public void Empower() { IsEmpowered = true; Changed?.Invoke(); }

        public float UseMark(bool exploit)
        {
            if (MarkedHits == 0) return 1f;
            MarkedHits = exploit ? 0 : MarkedHits - 1;
            Changed?.Invoke();
            return exploit ? 1.5f : 1.25f;
        }

        public float UseEmpower()
        {
            if (!IsEmpowered) return 1f;
            IsEmpowered = false;
            Changed?.Invoke();
            return 1.35f;
        }

        public float ReduceDamage(float damage)
        {
            if (damage <= 0f || GuardHits == 0) return Mathf.Max(0f, damage);
            GuardHits--;
            Changed?.Invoke();
            return damage * 0.7f;
        }

        public void Clear()
        {
            MarkedHits = GuardHits = 0;
            IsEmpowered = false;
            Changed?.Invoke();
        }

        public string Description => (MarkedHits > 0 ? $"표식 {MarkedHits}  " : "") +
            (GuardHits > 0 ? $"보호 {GuardHits}  " : "") + (IsEmpowered ? "격려" : "");
    }
}
