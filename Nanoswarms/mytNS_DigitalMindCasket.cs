using System;
using System.Collections.Generic;
using System.Text;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Noise;
using Verse.Sound;

namespace Nanoswarms
{
    public class mytNS_DigitalMindCasket : Building, ISuspendableThingHolder 
    {
        protected ThingOwner innerContainer;

        private const int LongTick = 2000;
        private static readonly int DigitizationTicksMax = 180000;
        private int _digitizationTicks = DigitizationTicksMax;
        
        private CompBuildingDigitalMind _compBuildingDigitalMind;

        public mytNS_DigitalMindCasket()
        {
            innerContainer = new ThingOwner<Thing>(this, false);
        }
        public CompBuildingDigitalMind CompBuildingDigitalMind =>
            _compBuildingDigitalMind ??
            (_compBuildingDigitalMind = this.TryGetComp<CompBuildingDigitalMind>());

        public Thing ContainedThing => innerContainer.Count != 0 ? innerContainer[0] : null;

        public virtual bool Accepts(Thing thing) => this.innerContainer.CanAcceptAnyOf(thing);

        public virtual bool TryAcceptThing(Thing thing, bool allowSpecialEffects = true)
        {
            if (!Accepts(thing) || _compBuildingDigitalMind.StoredMind != null)
                return false;
            bool flag;
            if (thing.holdingOwner != null)
            {
                thing.holdingOwner.TryTransferToContainer(thing, this.innerContainer, thing.stackCount);
                flag = true;
            }
            else
                flag = this.innerContainer.TryAdd(thing);

            if (flag)
            {
                DigitizationBegun = true;
            }
            return flag;
        }

        public bool DigitizationBegun { get; private set; } = false;

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (Gizmo gizmo in base.GetGizmos())
            {
                yield return gizmo;
            }

            if (DigitizationBegun && DebugSettings.godMode && CompBuildingDigitalMind.StoredMind == null)
            {
                Command_Action completeDigitization = new Command_Action
                {
                    action = CompleteDigitization,
                    defaultLabel = (string) "mytNS_Debug_CompleteDigitization".Translate(),
                    defaultDesc = (string) "mytNS_Debug_CompleteDigitizationDesc".Translate(),
                };
                yield return completeDigitization;
            }
        }

        public override string GetInspectString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            string inspectString = base.GetInspectString();
            if (!inspectString.NullOrEmpty())
            {
                stringBuilder.AppendLine(inspectString);
            }

            if (DigitizationBegun == true && _compBuildingDigitalMind?.StoredMind == null)
            {
                var smallerMax = (float) (DigitizationTicksMax / 1000.0);
                var smallerCurrent = (float) (smallerMax - (_digitizationTicks / 1000.0));
                stringBuilder.Append("mytNS_DigitizingMind".Translate() + ": ");
                stringBuilder.Append((smallerCurrent / smallerMax).ToStringPercent() + " " + "mytNS_Complete".Translate());
            } else if (_compBuildingDigitalMind?.StoredMind != null)
            {
                stringBuilder.Append("mytNS_StoredMind".Translate() + ": " + _compBuildingDigitalMind.StoredMind.Name);
            }

            return stringBuilder.ToString().TrimEndNewlines();
        }

        private void CompleteDigitization()
        {
            _digitizationTicks = 0;
            this.TickLong();
        }
        
        public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Pawn myPawn)
        {
            var casket = this;
            if (myPawn.IsQuestLodger())
            {
              yield return new FloatMenuOption((string) "CannotUseReason".Translate((NamedArgument) "CryptosleepCasketGuestsNotAllowed".Translate()), (Action) null);
            }
            else
            {
              foreach (var floatMenuOption in base.GetFloatMenuOptions(myPawn))
                yield return floatMenuOption;
              if (casket.innerContainer.Count != 0 || casket.CompBuildingDigitalMind.StoredMind != null) yield break;
              if (!myPawn.CanReach((LocalTargetInfo) (Thing) casket, PathEndMode.InteractionCell, Danger.Deadly))
              {
                  yield return new FloatMenuOption((string) "CannotUseNoPath".Translate(), (Action) null);
              }
              else
              {
                  var jobDef = mytNSDefOf.EnterDigitalMindArray;
                  yield return FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption((string) "mytNS_EnterDigitalMindArray".Translate(), (Action) (() =>
                  {
                      if (ModsConfig.BiotechActive)
                      {
                          if (!(myPawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.PsychicBond) is Hediff_PsychicBond firstHediffOfDef2) || !ThoughtWorker_PsychicBondProximity.NearPsychicBondedPerson(myPawn, firstHediffOfDef2))
                              myPawn.jobs.TryTakeOrderedJob(JobMaker.MakeJob(jobDef, (LocalTargetInfo) (Thing) this));
                          else
                              Find.WindowStack.Add((Window) Dialog_MessageBox.CreateConfirmation("PsychicBondDistanceWillBeActive_Cryptosleep".Translate(myPawn.Named("PAWN"), ((Pawn) firstHediffOfDef2.target).Named("BOND")), (Action) (() => myPawn.jobs.TryTakeOrderedJob(JobMaker.MakeJob(jobDef, (LocalTargetInfo) (Thing) this))), true));
                      }
                      else
                          myPawn.jobs.TryTakeOrderedJob(JobMaker.MakeJob(jobDef, (LocalTargetInfo) (Thing) this));
                  })), myPawn, (LocalTargetInfo) (Thing) casket);
              }
            }
        }

        public override void TickLong()
        {
            NanoswarmsHelper.WriteLog("Casket long tick.", NanoswarmsHelper.LogType.Debug);
            base.TickLong();
            if (DigitizationBegun && _compBuildingDigitalMind.StoredMind == null)
            {
                var pawnToStore = (Pawn)ContainedThing;
                if (pawnToStore == null) return;
                _digitizationTicks -= LongTick;
                NanoswarmsHelper.WriteLog(_digitizationTicks + " remaining until digitization complete.", NanoswarmsHelper.LogType.Debug);
                if (_digitizationTicks > 0) return;
                NanoswarmsHelper.WriteLog("Digitization Complete. Storing " + pawnToStore.Name, NanoswarmsHelper.LogType.Debug);
                innerContainer.Clear();
                pawnToStore.forceNoDeathNotification = true;
                pawnToStore.apparel.DestroyAll();
                pawnToStore.inventory.DestroyAll();
                CompBuildingDigitalMind.StoredMind = pawnToStore;
                CompBuildingDigitalMind.StoredMind.genes.SetXenotype(
                    mytNSDefOf.mytNS_NanoswarmDigitalMind);
                CompBuildingDigitalMind.StopProjection();
                CompBuildingDigitalMind.PreFormation();
            }
            else
            {
                string message = (_compBuildingDigitalMind?.StoredMind != null ? "Digitization complete. Casket holds mind of " + _compBuildingDigitalMind?.StoredMind?.Name : "Digitization has not begun.");
                NanoswarmsHelper.WriteLog(message, NanoswarmsHelper.LogType.Debug);
            }
        }

        public ThingOwner GetDirectlyHeldThings() => innerContainer;

        public void GetChildHolders(List<IThingHolder> outChildren)
        {
            ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, GetDirectlyHeldThings());
        }

        public bool IsContentsSuspended => true;
    }
}