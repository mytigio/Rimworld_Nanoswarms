using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace Nanoswarms
{
    public class Gene_NaniteSwarmBody : Gene_Resource, IGeneResourceDrain
    {
	    private CompBuildingDigitalMind _compBuildingDigitalMind;
	    
	    public CompBuildingDigitalMind CompBuildingDigitalMind =>
		    _compBuildingDigitalMind ??
		    (_compBuildingDigitalMind = this.pawn.TryGetComp<CompBuildingDigitalMind>());
        private int lastConsumed;

        public Gene_Resource Resource => this;

        public bool CanOffset => pawn.Spawned && Active;

        public float ResourceLossPerDay => def.resourceLossPerDay;

        public Pawn Pawn => pawn;

        public string DisplayLabel => def.resourceLabel;
		public override float InitialResourceMax => 1f;

        public override float MinLevelForAlert => 0.15f;
		protected override Color BarColor => Color.gray;

        protected override Color BarHighlightColor => new Color(84, 84, 84);
        private bool addHediff = true;

        public override void Tick()
        {
	        base.Tick();
	        if (!addHediff) return;
	        if (pawn.health.hediffSet.HasHediff(mytNSDefOf.mytNS_NanoswarmProjectionBody)) return;
	        pawn.health.AddHediff(mytNSDefOf.mytNS_NanoswarmProjectionBody);
	        addHediff = false;
        }
        
		public bool ShouldConsumeNow()
		{
			return false;
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
			foreach (Gizmo gizmo in base.GetGizmos())
			{
				yield return gizmo;
			}
			foreach (Gizmo resourceDrainGizmo in GeneResourceDrainUtility.GetResourceDrainGizmos(this))
			{
				yield return resourceDrainGizmo;
			}
		}

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref lastConsumed, "lastConsumed");
        }
    }
}