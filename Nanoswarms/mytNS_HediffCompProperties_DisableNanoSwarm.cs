using Verse;

namespace Nanoswarms
{
    public class mytNS_HediffCompProperties_DisableNanoSwarm : HediffCompProperties
    {
        public mytNS_HediffCompProperties_DisableNanoSwarm() => this.compClass = typeof (mytNS_HediffComp_DisableNanoSwarm);
    }
}