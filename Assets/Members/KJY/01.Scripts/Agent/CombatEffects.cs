using System;
using System.Collections.Generic;
using UnityEngine;

namespace Members.KJY._01.Scripts.Agent
{
    // 에셋 저장값: 뒤에만 추가
    public enum StatusType { None, Poison, Burn, Brand }

    public class CombatEffects
    {
        private struct DotState { public float Damage; public int Turns; }

        public int MarkedHits { get; private set; }
        public int GuardHits { get; private set; }
        public bool IsEmpowered { get; private set; }
        public float MissChance { get; private set; }
        public int MissTurns { get; private set; }
        public int StunTurns { get; private set; }
        public bool IsStunned => StunTurns > 0;

        public enum DebuffKind { Mark, Poison, Burn, Brand, Miss, Stun, Weaken }

        public event Action<string, Color> Popup;
        public event Action Cleared;
        private readonly List<DebuffKind> _debuffOrder = new();
        private readonly Dictionary<DebuffKind, Color> _debuffColors = new();

        public void ShowPopup(string text, Color color) => Popup?.Invoke(text, color);

        public static readonly Color HealColor = new(0.45f, 1f, 0.55f);

        public static Color DefaultColor(DebuffKind kind) => kind switch
        {
            DebuffKind.Poison => new Color(0.45f, 1f, 0.45f),
            DebuffKind.Burn => new Color(1f, 0.55f, 0.25f),
            DebuffKind.Brand => new Color(0.75f, 0.45f, 1f),
            DebuffKind.Miss => new Color(0.65f, 0.75f, 1f),
            DebuffKind.Stun => new Color(1f, 0.9f, 0.3f),
            DebuffKind.Weaken => new Color(0.55f, 0.85f, 1f),
            _ => new Color(1f, 0.45f, 0.45f)
        };

        public static DebuffKind ToKind(StatusType status) =>
            status == StatusType.Burn ? DebuffKind.Burn : status == StatusType.Brand ? DebuffKind.Brand : DebuffKind.Poison;

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
            DebuffKind.Weaken => WeakenTurns > 0,
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

        public int TauntTurns { get; private set; }
        public float ResistRatio { get; private set; }
        public int ResistTurns { get; private set; }
        public float WeakenRatio { get; private set; }
        public int WeakenTurns { get; private set; }
        public float PowerUpRatio { get; private set; }
        public int PowerUpTurns { get; private set; }
        public float OutgoingMultiplier =>
            (WeakenTurns > 0 ? 1f - WeakenRatio : 1f) * (PowerUpTurns > 0 ? 1f + PowerUpRatio : 1f);

        public void AddPowerUp(float ratio, int turns)
        {
            if (turns <= 0 || ratio <= 0f) return;
            PowerUpRatio = Mathf.Max(PowerUpRatio, ratio);
            PowerUpTurns = Mathf.Max(PowerUpTurns, turns);
            Changed?.Invoke();
        }

        public void AddTaunt(int turns)
        {
            if (turns <= 0) return;
            TauntTurns = Mathf.Max(TauntTurns, turns);
            Changed?.Invoke();
        }

        public void AddResist(float ratio, int turns)
        {
            if (turns <= 0 || ratio <= 0f) return;
            ResistRatio = Mathf.Max(ResistRatio, Mathf.Clamp01(ratio));
            ResistTurns = Mathf.Max(ResistTurns, turns);
            Changed?.Invoke();
        }

        public void AddWeaken(float ratio, int turns)
        {
            if (turns <= 0 || ratio <= 0f) return;
            WeakenRatio = Mathf.Max(WeakenRatio, Mathf.Clamp01(ratio));
            WeakenTurns = Mathf.Max(WeakenTurns, turns);
            Changed?.Invoke();
        }

        public int InvulnerableTurns { get; private set; }

        public void AddInvulnerable(int turns)
        {
            if (turns <= 0) return;
            InvulnerableTurns = Mathf.Max(InvulnerableTurns, turns);
            Changed?.Invoke();
        }

        public float ReflectRatio { get; private set; }
        public int ReflectTurns { get; private set; }

        public void AddReflect(float ratio, int turns)
        {
            if (turns <= 0 || ratio <= 0f) return;
            ReflectRatio = Mathf.Max(ReflectRatio, ratio);
            ReflectTurns = Mathf.Max(ReflectTurns, turns);
            Changed?.Invoke();
        }

        public void ClearDebuffs()
        {
            MarkedHits = 0;
            _dots.Clear();
            MissChance = WeakenRatio = 0f;
            MissTurns = StunTurns = WeakenTurns = 0;
            Changed?.Invoke();
        }

        private bool _stunBlockedThisTurn;

        // 보스는 턴당 1회만 막음
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
            if (InvulnerableTurns > 0) InvulnerableTurns--;
            if (TauntTurns > 0) TauntTurns--;
            if (ResistTurns > 0 && --ResistTurns == 0) ResistRatio = 0f;
            if (WeakenTurns > 0 && --WeakenTurns == 0) WeakenRatio = 0f;
            if (PowerUpTurns > 0 && --PowerUpTurns == 0) PowerUpRatio = 0f;
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
            if (damage > 0f && InvulnerableTurns > 0)
            {
                ShowPopup("무효", new Color(0.85f, 0.9f, 1f));
                return 0f;
            }
            if (damage > 0f && ResistTurns > 0) damage *= 1f - ResistRatio;
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
            MissTurns = StunTurns = ReflectTurns = InvulnerableTurns = TauntTurns = ResistTurns = WeakenTurns = PowerUpTurns = 0;
            ResistRatio = WeakenRatio = PowerUpRatio = 0f;
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
                              (ReflectTurns > 0 ? $"반사 {ReflectTurns}  " : "") + (InvulnerableTurns > 0 ? $"무적 {InvulnerableTurns}  " : "") +
                              (TauntTurns > 0 ? $"도발 {TauntTurns}  " : "") + (ResistTurns > 0 ? $"피해 감소 {ResistTurns}  " : "") +
                              (WeakenTurns > 0 ? $"둔화 {WeakenTurns}  " : "") + (PowerUpTurns > 0 ? $"공격력 증가 {PowerUpTurns}  " : "");
                foreach (var pair in _dots) text += $"{StatusName(pair.Key)} {pair.Value.Turns}  ";
                return text + (MissTurns > 0 ? $"빗나감 {MissTurns}  " : "") + (StunTurns > 0 ? $"행동 불가 {StunTurns}" : "");
            }
        }
    }
}
