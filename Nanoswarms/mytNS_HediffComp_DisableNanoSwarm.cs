using Verse;

namespace Nanoswarms
{
    public class mytNS_HediffComp_DisableNanoSwarm : HediffComp
    {
        public mytNS_HediffCompProperties_DisableNanoSwarm Props => (mytNS_HediffCompProperties_DisableNanoSwarm) this.props;

        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);
            var projectionBody = (mytNS_NanoswarmProjectionBody) Pawn.health.hediffSet.GetFirstHediffOfDef(mytNSDefOf.mytNS_NanoswarmProjectionBody);
            projectionBody?.DigitalMindStorage.StopProjection();
        }
    }
}