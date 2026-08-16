using RimWorld;
using Verse;

namespace Nanoswarms
{
    public class CompProps_DigitalMind : CompProperties
    {
        private XenotypeDef SpawnType;
        
        public CompProps_DigitalMind() => this.compClass = typeof (CompBuildingDigitalMind);
    }
}