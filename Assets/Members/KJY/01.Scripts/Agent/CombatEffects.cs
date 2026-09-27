using System;
using System.Collections.Generic;
using UnityEngine;

namespace Members.KJY._01.Scripts.Agent
{
    // 턴이 끝날 때마다 피해를 주는 상태. 값 순서는 에셋에 저장되므로 뒤에만 추가할 것
    public enum StatusType { None, Poison, Burn, Brand }

    public class CombatEffects
    {
        private struct DotState { public float Damage; public int Turns; }

        public int MarkedHits { get; private set; }
        public int GuardHits { get; private set; }
        public bool IsEmpowered { get; private set; }
        // 빗나감은 이 대상이 공격할 때 확률로 피해가 들어가지 않는다.
        public float MissChance { get; private set; }
        public int MissTurns { get; private set; }
        // 행동 불가: 남은 턴 동안 공격 명령이 넘어간다.
        public int StunTurns { get; private set; }
        public bool IsStunned => StunTurns > 0;
        public event Action Changed;

        // 반사: 남은 턴 동안 스킬로 받은 피해의 일부를 공격자에게 되돌린다.
        public float ReflectRatio { get; private set; }
        public int ReflectTurns { get; private set; }

        public void AddReflect(float ratio, int turns)
        {
            if (turns <= 0 || ratio <= 0f) return;
            ReflectRatio = Mathf.Max(ReflectRatio, ratio);
            ReflectTurns = Mathf.Max(ReflectTurns, turns);
            Changed?.Invoke();
        }

        // 해로운 효과(표식, 지속 피해, 빗나감, 행동 불가)만 지운다. 보호·격려·반사는 남는다.
        public void ClearDebuffs()
        {
            MarkedHits = 0;
            _dots.Clear();
            MissChance = 0f;
            MissTurns = StunTurns = 0;
            Changed?.Invoke();
        }

        public void AddStun(int turns)
        {
            if (turns <= 0) return;
            StunTurns = Mathf.Max(StunTurns, turns);
            Changed?.Invoke();
        }

        private readonly Dictionary<StatusType, DotState> _dots = new();
        private readonly List<StatusType> _tickKeys = new();

        public static string StatusName(StatusType status) => status switch
        {
            StatusType.Poison => "독",
            StatusType.Burn => "화상",
            StatusType.Brand => "낙인",
            _ => string.Empty
        };

        public bool HasStatus(StatusType status) => _dots.TryGetValue(status, out var dot) && dot.Turns > 0;

        // 같은 상태가 다시 걸리면 더 센 쪽, 더 긴 쪽으로 갱신한다.
        public void AddStatus(StatusType status, float damage, int turns)
        {
            if (status == StatusType.None || turns <= 0 || damage <= 0f) return;
            _dots.TryGetValue(status, out var dot);
            dot.Damage = Mathf.Max(dot.Damage, damage);
            dot.Turns = Mathf.Max(dot.Turns, turns);
            _dots[status] = dot;
            Changed?.Invoke();
        }

        public void AddMiss(float chance, int turns)
        {
            if (turns <= 0 || chance <= 0f) return;
            MissChance = Mathf.Max(MissChance, Mathf.Clamp01(chance));
            MissTurns = Mathf.Max(MissTurns, turns);
            Changed?.Invoke();
        }

        public bool RollMiss() => MissTurns > 0 && UnityEngine.Random.value < MissChance;

        // 턴 종료 시 한 번 호출. 이번 턴에 받을 지속 피해 총량을 돌려주고 남은 턴을 줄인다.
        public float TickTurn()
        {
            float damage = 0f;
            _tickKeys.Clear();
            _tickKeys.AddRange(_dots.Keys);
            foreach (StatusType status in _tickKeys)
            {
                var dot = _dots[status];
                damage += dot.Damage;
                if (--dot.Turns <= 0) _dots.Remove(status);
                else _dots[status] = dot;
            }
            if (MissTurns > 0 && --MissTurns == 0) MissChance = 0f;
            if (StunTurns > 0) StunTurns--;
            if (ReflectTurns > 0 && --ReflectTurns == 0) ReflectRatio = 0f;
            Changed?.Invoke();
            return damage;
        }

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
            MissChance = ReflectRatio = 0f;
            MissTurns = StunTurns = ReflectTurns = 0;
            _dots.Clear();
            Changed?.Invoke();
        }

        public string Description
        {
            get
            {
                string text = (MarkedHits > 0 ? $"표식 {MarkedHits}  " : "") +
                              (GuardHits > 0 ? $"보호 {GuardHits}  " : "") + (IsEmpowered ? "격려  " : "") +
                              (ReflectTurns > 0 ? $"반사 {ReflectTurns}  " : "");
                foreach (var pair in _dots) text += $"{StatusName(pair.Key)} {pair.Value.Turns}  ";
                return text + (MissTurns > 0 ? $"빗나감 {MissTurns}  " : "") + (StunTurns > 0 ? $"행동 불가 {StunTurns}" : "");
            }
        }
    }
}
