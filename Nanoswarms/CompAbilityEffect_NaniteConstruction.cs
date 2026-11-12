using System;
using RimWorld;
using UnityEngine;
using Verse;

namespace Nanoswarms
{
    public class CompAbilityEffect_NaniteConstruction : CompAbilityEffect
    {
        public new CompProperties_AbilityNaniteConstruction Props => (CompProperties_AbilityNaniteConstruction)props;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);
            var thing = target.Thing;
            var isNanoswarm = parent.pawn.IsNanoswarmAndroid();
            var frame = thing as Frame;
            var frameIsComplete = frame?.IsCompleted();
            NanoswarmsHelper.WriteLog($"{parent.pawn.Name} is nanoswarm: {isNanoswarm}; Thing is frame: {frame != null}; Frame is complete: {frameIsComplete ?? false}");
            if (!isNanoswarm || (frame == null) || !(frameIsComplete ?? false)) return;
            var framePosition = frame.Position;
            frame.CompleteConstruction(parent.pawn);
            var createdThing = parent.pawn.Map.thingGrid.ThingAt<Thing>(framePosition);
            if (createdThing != null && createdThing.HasComp<CompQuality>())
            {
                var comp = createdThing.TryGetComp<CompQuality>();
                if (comp.Quality < QualityCategory.Masterwork)
                {
                    comp.SetQuality(QualityCategory.Masterwork, null);
                    QualityUtility.SendCraftNotification(createdThing, parent.pawn);
                }
            }
        }

        public override bool Valid(LocalTargetInfo target, bool throwMessages = false)
        {
            var thing = target.Thing;
            if (thing is Frame frame)
            {
                NanoswarmsHelper.WriteLog($"Frame: {frame.Label} is complete: {frame.IsCompleted()}");
                if (!frame.IsCompleted() && throwMessages)
                {
                    Messages.Message(new Message("mytNS_InvalidNaniteConstructionTarget".Translate(),
                        MessageTypeDefOf.RejectInput));
                    return false;
                }
                
                return base.Valid(target, throwMessages);
            }
            else
            {
                if (throwMessages)
                {
                    Messages.Message(new Message("mytNS_InvalidNaniteConstructionTarget".Translate(), MessageTypeDefOf.RejectInput));
                }
                return false;
            }
        }
    }
}