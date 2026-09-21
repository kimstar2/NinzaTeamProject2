using System;
using System.Collections.Generic;
using System.Threading;
using _LumenLib.PoolingSystem.Runtime;
using _TevLib.Extension.DoT;
using Cysharp.Threading.Tasks;
using DevLib.ServiceLocator;
using Members.KJY._01.Scripts.Pool;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace Members.KJY._01.Scripts.Agent.SkillSystem.Skill
{
    // 이동, 복귀, 풀 반환은 다 똑같아서 여기 둠. 때리는 방식은 각 스킬에서 정하면 됨
    public abstract class AbstractParticleSkill : AbstractSkillLogic
    {
        [field:Header("Sequence Setting")]
        [field:SerializeField, FormerlySerializedAs("actionSeq")] public TransformTweenSequencer ActionSeq {get; private set;}
        [field:SerializeField, FormerlySerializedAs("returnSeq")] public TransformTweenSequencer ReturnSeq {get; private set;}
        [SerializeField] private Transform castPosition;
        [SerializeField] private Transform returnPosition;
        [SerializeField, Min(0f)] private float advanceDistance = 0.6f;
        [SerializeField, Min(0.1f)] private float animationTimeout = 3f;
        [SerializeField] protected Vector3 effectOffset = new(0f, 0.5f, 0f);

        public UnityEvent onCast; // 시전 파티클, Executor.PlayAnim 연결
        public UnityEvent onAnimEnd; // ReturnSeq.Sequence 연결

        private ObjectPool _pool;
        private readonly List<PoolingParticle> _particles = new();
        private CancellationTokenSource _cts;
        private CancellationTokenSource _animCts;
        private Transform _attackerTrm;
        private Vector3 _defaultPos;
        private bool _castStarted;
        private bool _attackStarted;
        private bool _attackEnd = true;
        private bool _animEnd;
        private bool _returnStarted;
        private bool _returnEnd;
        private bool _skillEnd;

        protected bool CanHit => Executor != null && Executor.Attacker != null && Executor.Target != null &&
                                 !Executor.Attacker.IsDead && !Executor.Target.IsDead &&
                                 Executor.Attacker != Executor.Target;
        protected bool CanApplyStat => CanHit && _attackStarted && !_attackEnd && !_skillEnd;

        public override void Execute()
        {
            _attackerTrm = Executor.Attacker.MyAgent.transform;
            _defaultPos = _attackerTrm.position;
            if (!CanHit) // 자기 자신이 타겟이거나 이미 죽었으면 그냥 다음 행동으로
            {
                EndSkill();
                return;
            }
            if (ActionSeq == null || ReturnSeq == null || castPosition == null || returnPosition == null)
            {
                Debug.LogError($"{name}: 시퀀서랑 위치 연결 확인해줘", this);
                EndSkill();
                return;
            }

            ServiceLocator.TryGet(out _pool);
            _cts = new CancellationTokenSource();
            float distance = Executor.Target.MyAgent.transform.position.x - _defaultPos.x;
            float direction = Mathf.Sign(distance);
            castPosition.position = _defaultPos + Vector3.right *
                (direction * Mathf.Min(advanceDistance, Mathf.Abs(distance) * 0.25f));
            returnPosition.position = _defaultPos; // 돌아갈 위치는 시전 전에 저장해둠
            ActionSeq.SetTargetTrm(_attackerTrm);
            ReturnSeq.SetTargetTrm(_attackerTrm);

            if (!ActionSeq.SequenceAndResult()) PlaySkillAnim();
        }

        public void PlaySkillAnim() // ActionSeq.onSeqComplete에 연결
        {
            if (_castStarted || _returnStarted || _skillEnd) return;
            _castStarted = true;
            if (!CanHit)
            {
                AnimEnd();
                return;
            }

            _animCts = new CancellationTokenSource();
            WaitAnimEnd(_animCts.Token).Forget();
            onCast?.Invoke();
        }

        public override void Attack() // 애니메이션 OnAttack은 얘한테 들어옴
        {
            if (!_castStarted || _attackStarted || _returnStarted || _skillEnd || !CanHit) return;
            _attackStarted = true;
            _attackEnd = false;
            ExecuteAttack(_cts.Token).Forget();
        }

        protected abstract UniTask AttackAsync(CancellationToken token);

        private async UniTask ExecuteAttack(CancellationToken token)
        {
            try
            {
                await AttackAsync(token);
            }
            catch (OperationCanceledException) when (token.IsCancellationRequested) { }
            catch (Exception error)
            {
                Debug.LogException(error, this);
                AnimEnd(); // 스킬에서 에러 나도 배틀이 여기서 멈추지는 않게
            }
            finally
            {
                _attackEnd = true;
                TryReturn();
            }
        }

        public override void AnimEnd()
        {
            if (_animEnd || _skillEnd) return;
            _animEnd = true;
            KillAnimTask();
            if (_attackerTrm != null && !Executor.Attacker.IsDead) Executor.PlayIdleAnim();
            TryReturn();
        }

        private async UniTask WaitAnimEnd(CancellationToken token)
        {
            bool canceled = await UniTask.Delay(TimeSpan.FromSeconds(Mathf.Max(0.1f, animationTimeout)),
                cancellationToken: token).SuppressCancellationThrow();
            if (canceled) return;
            Debug.LogWarning($"{name}: 애니메이션 끝 이벤트 안 와서 복귀함", this);
            AnimEnd();
        }

        private void TryReturn()
        {
            if (_skillEnd || _returnStarted || !_animEnd || !_attackEnd) return;
            _returnStarted = true;
            if (_attackerTrm == null)
            {
                HandleReturnEnd();
                return;
            }
            onAnimEnd?.Invoke();
            if (!ReturnSeq.HasTween) HandleReturnEnd();
        }

        public void HandleReturnEnd() // ReturnSeq.onSeqComplete에 연결
        {
            if (!_returnStarted || _returnEnd || _skillEnd) return;
            if (_attackerTrm != null) _attackerTrm.position = _defaultPos;
            _returnEnd = true;
            TryEndSkill();
        }

        protected float GetDamage(float damage)
        {
            var diceData = Executor.Attacker.DiceInventory.GetDiceData();
            return Mathf.Max(0f, damage + (diceData != null ? diceData.BaseDamage : 0f));
        }

        protected void PlayParticle(PoolItemSO item, Vector3 pos)
        {
            if (_pool == null || item == null)
            {
                Debug.LogWarning($"{name}: 파티클 풀 연결 확인해줘", this);
                return;
            }
            var pooled = _pool.Pop(item.ItemName);
            PoolingParticle particle = pooled as PoolingParticle;
            if (particle == null)
            {
                if (pooled != null) _pool.Push(pooled);
                Debug.LogWarning($"{name}: {item.ItemName} 풀 등록 확인해줘", this);
                return;
            }
            _particles.Add(particle);
            particle.OnParticleEnd += ReturnToPool;
            particle.GameObject.SetActive(true);
            particle.PlayParticle(pos);
        }

        private void ReturnToPool(PoolingParticle particle)
        {
            particle.OnParticleEnd -= ReturnToPool;
            _particles.Remove(particle);
            if (_pool != null) _pool.Push(particle);
            TryEndSkill();
        }

        private void TryEndSkill()
        {
            if (_returnEnd && _particles.Count == 0) EndSkill(); // 복귀랑 이펙트 둘 다 끝나야 다음으로 감
        }

        private void EndSkill()
        {
            if (_skillEnd) return;
            _skillEnd = true;
            KillTask();
            Executor.SkillFinished();
            Executor.Remove();
        }

        private void KillAnimTask()
        {
            if (_animCts == null) return;
            _animCts.Cancel();
            _animCts.Dispose();
            _animCts = null;
        }

        private void KillTask()
        {
            KillAnimTask();
            if (_cts == null) return;
            _cts.Cancel();
            _cts.Dispose();
            _cts = null;
        }

        protected virtual void OnDestroy()
        {
            _skillEnd = true;
            KillTask();
            if (_attackerTrm != null && !_returnEnd) _attackerTrm.position = _defaultPos;
            foreach (PoolingParticle particle in _particles)
            {
                if (particle == null) continue;
                particle.OnParticleEnd -= ReturnToPool;
                particle.StopParticle();
                particle.ClearParticle();
                if (_pool != null) _pool.Push(particle);
            }
            _particles.Clear();
        }
    }
}
