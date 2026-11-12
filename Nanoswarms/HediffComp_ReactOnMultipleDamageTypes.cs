using RimWorld;
using Verse;
using Verse.AI;

namespace Nanoswarms
{
    public class HediffComp_ReactOnMultipleDamageTypes : HediffComp
    {
        public HediffCompProperties_ReactOnMultipleDamageTypes Props => (HediffCompProperties_ReactOnMultipleDamageTypes) props;

        public override void Notify_PawnPostApplyDamage(DamageInfo dinfo, float totalDamageDealt)
        {
            if (Props.damageDefIncoming.Contains(dinfo.Def))
            {
                React();
            }
        }
        private void React()
        {
            if (Props.createHediff != null)
            {
                BodyPartRecord part = parent.Part;
                if (Props.createHediffOn != null)
                {
                    part = parent.pawn.RaceProps.body.AllParts.FirstOrFallback((BodyPartRecord p) =>
                        p.def == Props.createHediffOn);
                }

                parent.pawn.health.AddHediff(Props.createHediff, part);
            }

            if (Props.vomit)
            {
                parent.pawn.jobs.StartJob(JobMaker.MakeJob(JobDefOf.Vomit), JobCondition.InterruptForced, null, resumeCurJobAfterwards: true);
            }
        }
    }
}