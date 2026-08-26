using Verse;

namespace Nanoswarms
{
    public class mytNS_NanoswarmProjectionBody : Hediff
    {
        public CompBuildingDigitalMind DigitalMindStorage;

        

        public override void Notify_PawnKilled()
        {
            base.Notify_PawnKilled();
            DigitalMindStorage?.StopProjection();
        }

        public override void Notify_PawnDied(DamageInfo? dinfo, Hediff culprit = null)
        {
            base.Notify_PawnDied(dinfo, culprit);
            DigitalMindStorage?.StopProjection();
        }
    }
}