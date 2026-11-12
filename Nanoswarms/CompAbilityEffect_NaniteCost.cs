using RimWorld;
using Verse;

namespace Nanoswarms
{
    public class CompAbilityEffect_NaniteCost : CompAbilityEffect
    {
        public new CompProperties_AbilityNaniteCost Props => (CompProperties_AbilityNaniteCost)props;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);
            if (!parent.pawn.IsNanoswarmAndroid()) return;
            
            parent.pawn.removeNanites(Props.naniteCost);
        }

        public override bool GizmoDisabled(out string reason)
        {
            var gene = parent.pawn.genes?.GetFirstGeneOfType<Gene_NaniteSwarmBody>();
            if (gene == null)
            {
                NanoswarmsHelper.WriteLog("no nanoswarm body gene.", NanoswarmsHelper.LogType.Debug);
                reason = "mytNS_AbilityNoNanoswarm".Translate(parent.pawn);
                return true;
            }

            if (!HasEnoughNanites)
            {
                NanoswarmsHelper.WriteLog("Does not have enough nanites.", NanoswarmsHelper.LogType.Debug);
                reason = "mytNS_AbilityDisabledNoNanites".Translate(parent.pawn);
                return true;
            }

            reason = null;
            return false;
        }

        public override bool AICanTargetNow(LocalTargetInfo target)
        {
            return HasEnoughNanites;
        }
        
        private bool HasEnoughNanites
        {
            get
            {
                var gene = parent.pawn.genes?.GetFirstGeneOfType<Gene_NaniteSwarmBody>();
                return gene != null && (gene.Resource.Value >= Props.naniteCost);
            }
        }
        
    }
}