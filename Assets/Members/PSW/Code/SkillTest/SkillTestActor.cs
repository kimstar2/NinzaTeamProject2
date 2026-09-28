using Members.KJY._01.Scripts.Agent;
using Members.KJY._01.Scripts.Agent.Enemy;
using Members.KJY._01.Scripts.Agent.Player;
using UnityEngine;

namespace Members.PSW.Code.SkillTest
{
    // 테스트 씬에서만 사용. 원본 캐릭터 데이터와 실제 전투의 사망 규칙은 변경하지 않는다.
    public sealed class SkillTestActor : AbstractSelector
    {
        [SerializeField] private AgentDataSO data;
        [SerializeField, Min(2f)] private float maxHealth = 1000f;
        public float SkillLevel { get; set; } = 1f;

        protected override void Start()
        {
            SetData(data);
            EnterBattle();
            ResetActor();
            base.Start();
        }

        public void SetData(AgentDataSO value)
        {
            AgentData = value;
            if (value is PlayerDataSO player)
            {
                MyAgent.AgentRenderer.SetSprite(player.PlayerImage);
                MyAgent.AgentRenderer.SetColor(player.ImageColor.GetColor());
                MyAgent.AnimCompo.SetController(player.AnimCon);
            }
            else if (value is EnemyDataSO enemy)
            {
                MyAgent.AgentRenderer.SetSprite(enemy.EnemyImage);
                MyAgent.AgentRenderer.SetColor(enemy.ImageColor.GetColor());
                MyAgent.AnimCompo.SetController(enemy.EnemyAc);
            }
        }

        public void ResetActor()
        {
            IsDead = false;
            Effects.Clear();
            // 회복 스킬도 바로 확인할 수 있도록 반피에서 시작한다.
            MyAgent.HealthModule.InitHealth(maxHealth, maxHealth * 0.5f);
            MyAgent.transform.position = DefaultPosition.position;
        }

        public override void ApplyDamage(float damage)
        {
            if (float.IsNaN(damage) || float.IsInfinity(damage)) return;
            var health = MyAgent.HealthModule;
            health.TakeDamage(Mathf.Clamp(damage, 0f, Mathf.Max(0f, health.CurrentHealth - 1f)));
        }

        public override void ApplyHeal(float heal) => MyAgent.HealthModule.Heal(heal);
        public override float GetLevel() => SkillLevel;
        protected override void Select() { IsSelect = true; }
        protected override void UnSelect() { IsSelect = false; }
        public override void OnAttackCommand() { }
    }
}
