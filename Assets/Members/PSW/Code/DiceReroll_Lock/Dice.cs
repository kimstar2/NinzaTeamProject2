using System.Collections.Generic;
using DevLib.ModuleSystem;
using Members.PSW.Code.Test;
using UnityEngine;

namespace Members.PSW.Code.DiceReroll_Lock
{
    public class Dice : MonoBehaviour, IReroll
    {
        public SkillSO CurrentSkill { get; private set; }
        public List<SkillSO> SkillSet { get; private set; }
        public bool IsLocked { get; private set; }

        public void Reroll()
        {
            CurrentSkill = SkillSet[Random.Range(0, SkillSet.Count)];
        }

        public void Lock()
        {
            IsLocked = !IsLocked;
        }
    }
}