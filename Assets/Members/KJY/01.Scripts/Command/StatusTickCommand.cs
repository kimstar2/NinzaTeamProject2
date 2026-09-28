using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Members.KJY._01.Scripts.Agent;

namespace Members.KJY._01.Scripts.Command
{
    public class StatusTickCommand : ICommand
    {
        private const float TickDelay = 0.4f;
        private readonly Func<IEnumerable<AbstractSelector>> _getCombatants;
        private CancellationToken _token;

        public StatusTickCommand(Func<IEnumerable<AbstractSelector>> getCombatants)
        {
            _getCombatants = getCombatants;
        }

        public void SetNextSignal(CancellationToken token) => _token = token;

        public async UniTask ExecuteAction()
        {
            bool anyDamage = false;
            foreach (AbstractSelector selector in _getCombatants())
            {
                if (selector == null || selector.IsDead || selector.AgentData == null) continue;
                float damage = selector.Effects.TickTurn();
                if (damage <= 0f) continue;
                selector.ApplyDamage(damage); // 보호 무시
                anyDamage = true;
            }
            if (anyDamage)
                await UniTask.Delay(TimeSpan.FromSeconds(TickDelay), cancellationToken: _token);
        }

        public void MoveNext() { }
    }
}
