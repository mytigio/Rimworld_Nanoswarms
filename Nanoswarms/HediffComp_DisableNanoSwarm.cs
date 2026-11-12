using Verse;

namespace Nanoswarms
{
    public class HediffComp_DisableNanoSwarm : HediffComp
    {
        public HediffCompProperties_DisableNanoSwarm Props => (HediffCompProperties_DisableNanoSwarm) props;

        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);
            var projectionBody = (Hediff_NanoswarmProjectionBody) Pawn.health.hediffSet.GetFirstHediffOfDef(mytNSDefOf.mytNS_NanoswarmProjectionBody);
            projectionBody?.DigitalMindStorage.StopProjection();
        }
    }
}