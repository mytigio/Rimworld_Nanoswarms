using Verse;

namespace Nanoswarms
{
    public class HediffCompProperties_DisableNanoSwarm : HediffCompProperties
    {
        public HediffCompProperties_DisableNanoSwarm() => this.compClass = typeof (HediffComp_DisableNanoSwarm);
    }
}