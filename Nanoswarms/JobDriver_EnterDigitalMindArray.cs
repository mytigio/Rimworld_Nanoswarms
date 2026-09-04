using System;
using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;
using VREAndroids;

namespace Nanoswarms
{
    public class JobDriver_EnterDigitalMindArray : JobDriver
    {
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(job.targetA, job, errorOnFailed: errorOnFailed);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedOrNull(TargetIndex.A);
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell);
            Toil toil = Toils_General.Wait(500);
            toil.FailOnCannotTouch<Toil>(TargetIndex.A, PathEndMode.InteractionCell);
            toil.WithProgressBarToilDelay(TargetIndex.A);
            yield return toil;
            var enter = ToilMaker.MakeToil();
            enter.initAction = () =>
            {
                var actor = enter.actor;
                var pod = (mytNS_Building_DigitalMindCasket) actor.CurJob.targetA.Thing;
                if (actor.IsAndroid()) return;
                if (!pod.def.building.isPlayerEjectable)
                {
                    if (this.Map.mapPawns.FreeColonistsSpawnedOrInPlayerEjectablePodsCount <= 1)
                        Find.WindowStack.Add((Window) Dialog_MessageBox.CreateConfirmation("CasketWarning".Translate(actor.Named("PAWN")).AdjustedFor(actor), ConfirmedAct));
                    else
                        ConfirmedAct();
                }
                else
                    ConfirmedAct();

                return;

                void ConfirmedAct()
                {
                    var flag = actor.DeSpawnOrDeselect();
                    if (!(pod.TryAcceptThing((Thing)actor, true) & flag)) return;
                    Find.Selector.Select((object)actor, false, false);
                }
            };
            enter.defaultCompleteMode = ToilCompleteMode.Instant;
            yield return enter;
        }
    }
}