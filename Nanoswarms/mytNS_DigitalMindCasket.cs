using System;
using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace Nanoswarms
{
    public class mytNS_DigitalMindCasket : Building_Casket
    {
        private const int LongTick = 2000;
        private bool _digitizationBegun = false;
        private int _digitizationTicks = 180000;
        
        private CompBuildingDigitalMind _compBuildingDigitalMind;

        public CompBuildingDigitalMind CompBuildingDigitalMind =>
            _compBuildingDigitalMind ??
            (this._compBuildingDigitalMind = this.TryGetComp<CompBuildingDigitalMind>());

        public override bool TryAcceptThing(Thing thing, bool allowSpecialEffects = true)
        {
            if (!base.TryAcceptThing(thing, allowSpecialEffects) || (CompBuildingDigitalMind.StoredMind != null))
                return false;
            if (allowSpecialEffects)
                SoundDefOf.CryptosleepCasket_Accept.PlayOneShot((SoundInfo) new TargetInfo(this.Position, this.Map));
            
            _digitizationBegun = true;
            return true;
        }
        
        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (Gizmo gizmo in base.GetGizmos())
            {
                yield return gizmo;
            }

            if (_digitizationBegun && DebugSettings.godMode && CompBuildingDigitalMind.StoredMind == null)
            {
                
            }
        }

        public override void TickLong()
        {
            base.TickLong();
            var pawnToStore = (Pawn)ContainedThing;
            if (pawnToStore == null) return;
            _digitizationTicks -= LongTick;
            if (_digitizationTicks > 0) return;
            NanoswarmsHelper.WriteLog("Digitization Complete. Storing " + pawnToStore.Name);
            CompBuildingDigitalMind.StoredMind = pawnToStore;
            CompBuildingDigitalMind.PreFormation();
        }
        
        public override void EjectContents()
        {
            NanoswarmsHelper.WriteLog("Attempted to eject contents of digital mind array. Failing");
        }
    }
}