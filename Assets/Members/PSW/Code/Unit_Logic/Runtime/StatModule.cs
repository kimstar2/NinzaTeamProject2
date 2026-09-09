using DevLib.ModuleSystem;
using UnityEngine;

namespace Members.PSW.Code.Unit_Logic.Runtime
{
    public class StatModule : MonoModule
    {
        public CalculateStat CurrentStat { get; private set; }
        
        public override void Initialize(ModuleOwner owner)
        {
            base.Initialize(owner);
            var unit = owner as UnitController;
            Debug.Assert(unit != null, "UnitController is null");

            CurrentStat = unit.UnitData.baseStat;
        }

        public void ChangeStat(CalculateStat newStat)
        {
            CurrentStat = newStat;
        }
    }
}