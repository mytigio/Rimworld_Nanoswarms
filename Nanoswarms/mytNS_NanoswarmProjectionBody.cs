using Verse;

namespace Nanoswarms
{
    public class mytNS_NanoswarmProjectionBody : HediffWithComps
    {
        public CompBuildingDigitalMind DigitalMindStorage;
        
        private int _healTicksSinceLastHit = 0;
        private Gene_NaniteSwarmBody _nanoswarmBodyGene;
        public override void Tick()
        {
            base.Tick();
            if (!pawn.IsHashIntervalTick(60) || _healTicksSinceLastHit < 0) return;
            if (_nanoswarmBodyGene == null)
            {
                NanoswarmsHelper.WriteLog("Nanoswarm Body Gene not found attached to hediff.", NanoswarmsHelper.LogType.Warning);
                LinkBodyGene();
            }
            _nanoswarmBodyGene?.HealIfPossible();
            _healTicksSinceLastHit--;
        }

        private void LinkBodyGene()
        {
            var gene = pawn.genes.GetGene(mytNSDefOf.mytNS_NanobotSwarm);
            if (gene is Gene_NaniteSwarmBody swarmBody)
            {
                _nanoswarmBodyGene = swarmBody;
            }
        }

        public void RefreshNanitePool()
        {
            if (_nanoswarmBodyGene == null)
            {
                NanoswarmsHelper.WriteLog("Nanoswarm body gene is null. Cannot reset nanite pool.", NanoswarmsHelper.LogType.Warning);
                return;
            }
            NanoswarmsHelper.WriteLog("Reset nanite pool to max.");
            _nanoswarmBodyGene.Resource.Value = _nanoswarmBodyGene.Resource.Max;
        }

        public override void Notify_PawnPostApplyDamage(DamageInfo dinfo, float totalDamageDealt)
        {
            base.Notify_PawnPostApplyDamage(dinfo, totalDamageDealt);

            if (_nanoswarmBodyGene?.Active != true) return;
            NanoswarmsHelper.WriteLog("Begin healing if possible.", NanoswarmsHelper.LogType.Debug);
            _nanoswarmBodyGene?.HealIfPossible();
            _healTicksSinceLastHit = 600;
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

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref _nanoswarmBodyGene, "_nanoswarmBodyGene");
            Scribe_References.Look(ref DigitalMindStorage, "DigitalMindStorage");
            Scribe_Values.Look(ref _healTicksSinceLastHit, "_healTicksSinceLastHit");
        }
    }
}