using System.Collections.Generic;
using Members.PSW.Code.Test;
using UnityEngine;

namespace Members.PSW.Code.DiceReroll_Lock
{
    public class Dice : MonoBehaviour, IReroll
    {
        [SerializeField] private List<SkillSO> skillSet;
        [field: SerializeField] public SkillSO CurrentSkill { get; private set; }
        public bool IsLocked { get; private set; }

        [ContextMenu("Check")]
        private void CheckSkill()
        {
            Debug.Log($"{CurrentSkill.skillName}인 상태");
        }
        
        public void Reroll()
        {
            CurrentSkill = skillSet[Random.Range(0, skillSet.Count)];
        }

        public void Lock()
        {
            IsLocked = !IsLocked;
        }
    }
}