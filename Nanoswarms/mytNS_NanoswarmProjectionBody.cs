using Verse;

namespace Nanoswarms
{
    public class mytNS_NanoswarmProjectionBody : Hediff
    {
        public CompBuildingDigitalMind DigitalMindStorage;

        public override void Tick()
        {
            base.Tick();
            if (DigitalMindStorage != null) return;
            
            NanoswarmsHelper.WriteLog("Form Projection Xenotype with no linked digital mind building. Destroy", NanoswarmsHelper.LogType.Warning);
            this.DestroyUnlinkedSwarm();
        }

        private void DestroyUnlinkedSwarm()
        {
            
            if (pawn.carryTracker?.CarriedThing != null)
            {
                pawn.carryTracker.TryDropCarriedThing(pawn.Position, ThingPlaceMode.Near, out var resultingThing);
            }
                
            if (pawn.Spawned || pawn.Corpse != null)
            {
                pawn.apparel.DropAll(pawn.Position);
                pawn.inventory.DropAllNearPawn(pawn.Position);
            }

            if (pawn.Map != null)
            {
                pawn.DeSpawn();
            }

            if (pawn.Corpse?.Map == null) return;
            pawn.Corpse.Destroy();
        }

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