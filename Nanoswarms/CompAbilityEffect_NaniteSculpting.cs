using System.Linq;
using RimWorld;
using Verse;

namespace Nanoswarms
{
    public class CompAbilityEffect_NaniteSculpting : CompAbilityEffect
    {
        public new CompProperties_AbilityNaniteSculpting Props => (CompProperties_AbilityNaniteSculpting)props;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);
            if (!parent.pawn.IsNanoswarmAndroid())
            {
                NanoswarmsHelper.WriteLog("[Nanoswarms.CompAbilityEffect_NaniteSculpting] Cast by non-swarm android.",NanoswarmsHelper.LogType.Warning);
                return;
            };
            var thing = target.Thing;
            if (thing == null || !thing.HasThingCategory(ThingCategoryDefOf.StoneChunks))
            {
                NanoswarmsHelper.WriteLog("[Nanoswarms.CompAbilityEffect_NaniteSculpting] Cast NaniteSculpting ability on non-stonechunk item.",NanoswarmsHelper.LogType.Warning);
                return;
            };
            var position = thing.Position;
            
            var firstRelevantProduct =
                thing.def.butcherProducts.Find(x =>
                    x.thingDef.thingCategories.Contains(ThingCategoryDefOf.StoneBlocks));

            var stuff = firstRelevantProduct.thingDef;
            if (stuff?.stuffProps == null)
            {
                NanoswarmsHelper.WriteLog($"[Nanoswarms.CompAbilityEffect_NaniteSculpting] Could not determine stuff for sculpture. Thing: {thing.Label}. ButcherProduct: {stuff?.defName ?? "None"}; Stuff Properties: {stuff?.stuffProps?.stuffAdjective ?? "None"}",NanoswarmsHelper.LogType.Warning);
                return;
            };
            thing.Destroy();
            var worker = parent.pawn;
            var sculptureDef = DefDatabase<ThingDef>.GetNamed("SculptureSmall");
            var sculpture = ThingMaker.MakeThing(sculptureDef, stuff);
            sculpture.SetFactionDirect(worker.Faction);
            var compQuality = sculpture.TryGetComp<CompQuality>();
            if (compQuality != null)
            {
                var isLegendary = Rand.Chance(0.25f);
                var qualityCreatedByPawn = (isLegendary ? QualityCategory.Legendary : QualityCategory.Masterwork);
                NanoswarmsHelper.WriteLog($"Set quality of new sculpture to {qualityCreatedByPawn}");
                compQuality.SetQuality(qualityCreatedByPawn, new ArtGenerationContext?(ArtGenerationContext.Colony));
                QualityUtility.SendCraftNotification(sculpture, worker);
            }
            var compArt = sculpture.TryGetComp<CompArt>();
            if (compArt != null)
            {
                if (compQuality == null)
                    compArt.InitializeArt(ArtGenerationContext.Colony);
                compArt.JustCreatedBy(worker);
            }
            GenSpawn.Spawn(sculpture, position, worker.Map, sculpture.Rotation, WipeMode.FullRefund,false,false);
        }

        public override bool Valid(LocalTargetInfo target, bool throwMessages = false)
        {
            var thing = target.Thing;
            var isChunk = (thing != null && thing.HasThingCategory(ThingCategoryDefOf.StoneChunks));
            var baseValid = base.Valid(target, throwMessages);
            var valid = isChunk && baseValid;
            if (!valid && throwMessages)
            {
                Messages.Message(new Message("mytNS_InvalidNaniteSculptingTarget".Translate(), MessageTypeDefOf.RejectInput));
            }
            return valid;
        }
    }
}