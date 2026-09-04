using RimWorld;
using Verse;

namespace Nanoswarms
{
    public class CompProps_DigitalMind : CompProperties
    {
        public Swarmtype SpawnType;
        public int passionChancePercent = 0;
        public int maxPassions = 0;
        public int burningPassionChancePercent = 0;
        public int maxBurningPassions = 0;
        public int skillRangeMinimum = 0;
        public int skillRangeMaximum = 0;
        public int numberOfTraits = 0;
        public bool reprogrammable = false;

        public bool IsAIMind => SpawnType.isAI;

        public BackstoryDef ChildhoodBackstory => SpawnType.backstory;
        public CompProps_DigitalMind() => this.compClass = typeof (CompBuildingDigitalMind);
    }
}