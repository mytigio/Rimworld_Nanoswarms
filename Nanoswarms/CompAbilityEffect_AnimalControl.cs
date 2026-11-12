using RimWorld;
using Verse;

namespace Nanoswarms
{
    public class CompAbilityEffect_AnimalControl : CompAbilityEffect
    {
        public new CompProperties_AbilityAnimalControl Props => (CompProperties_AbilityAnimalControl)props;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);
            if (!parent.pawn.IsNanoswarmAndroid()) return;
        }

        public override bool CanApplyOn(LocalTargetInfo target, LocalTargetInfo dest)
        {
            if (target.Thing.HasThingCategory(ThingCategoryDefOf.StoneChunks))
            {
                return base.CanApplyOn(target, dest);
            }

            return false;
        }
    }
}