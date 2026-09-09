using DevLib.ModuleSystem;

namespace Members.PSW.Code.Unit_Logic.Runtime
{
    public class StatModule : MonoModule
    {
        public CalculateStat CurrentStat { get; private set; }
        
        public override void Initialize(ModuleOwner owner)
        {
            base.Initialize(owner);
            CurrentStat = new CalculateStat()
            {
                attackValue = 0,
                damageValue = 0
            };
        }

        public void ChangeStat(CalculateStat newStat)
        {
            CurrentStat = newStat;
        }
    }
}