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

        // ===== 표시용: 떠오르는 문구와 디버프 색 =====
        public enum DebuffKind { Mark, Poison, Burn, Brand, Miss, Stun }

        public event Action<string, Color> Popup;
        public event Action Cleared; // 전투 입장 등으로 모든 효과가 초기화될 때
        private readonly List<DebuffKind> _debuffOrder = new();
        private readonly Dictionary<DebuffKind, Color> _debuffColors = new();

        public void ShowPopup(string text, Color color) => Popup?.Invoke(text, color);

        public static Color DefaultColor(DebuffKind kind) => kind switch
        {
            DebuffKind.Poison => new Color(0.45f, 1f, 0.45f),
            DebuffKind.Burn => new Color(1f, 0.55f, 0.25f),
            DebuffKind.Brand => new Color(0.75f, 0.45f, 1f),
            DebuffKind.Miss => new Color(0.65f, 0.75f, 1f),
            DebuffKind.Stun => new Color(1f, 0.9f, 0.3f),
            _ => new Color(1f, 0.45f, 0.45f)
        };

        public static DebuffKind ToKind(StatusType status) =>
            status == StatusType.Burn ? DebuffKind.Burn : status == StatusType.Brand ? DebuffKind.Brand : DebuffKind.Poison;

        // 디버프가 걸릴 때 색을 기록한다. 가장 최근에 걸린, 아직 남아 있는 디버프 색이 캐릭터 색이 된다.
        public void RememberDebuff(DebuffKind kind, Color color)
        {
            _debuffOrder.Remove(kind);
            _debuffOrder.Add(kind);
            _debuffColors[kind] = color;
            Changed?.Invoke();
        }

        private bool IsActive(DebuffKind kind) => kind switch
        {
            DebuffKind.Mark => MarkedHits > 0,
            DebuffKind.Miss => MissTurns > 0,
            DebuffKind.Stun => StunTurns > 0,
            DebuffKind.Burn => HasStatus(StatusType.Burn),
            DebuffKind.Brand => HasStatus(StatusType.Brand),
            _ => HasStatus(StatusType.Poison)
        };

        public Color? CurrentTint
        {
            get
            {
                for (int i = _debuffOrder.Count - 1; i >= 0; i--)
                    if (IsActive(_debuffOrder[i])) return _debuffColors[_debuffOrder[i]];
                return null;
            }
        }
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

        private bool _stunBlockedThisTurn;

        // 행동 불가면 이번 행동을 막는다. 보스는 한 턴에 여러 번 공격하므로 턴마다 1회만 막는다.
        public bool TryBlockAction(bool isBoss)
        {
            if (!IsStunned) return false;
            if (!isBoss) return true;
            if (_stunBlockedThisTurn) return false;
            _stunBlockedThisTurn = true;
            return true;
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
            _stunBlockedThisTurn = false;
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
            _stunBlockedThisTurn = false;
            _dots.Clear();
            _debuffOrder.Clear();
            _debuffColors.Clear();
            Cleared?.Invoke();
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
