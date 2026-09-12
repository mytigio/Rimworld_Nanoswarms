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
		    (_compBuildingDigitalMind = _projectionBody?.DigitalMindStorage);

	    private mytNS_NanoswarmProjectionBody _projectionBody;

        public Gene_Resource Resource => this;

        public bool CanOffset => pawn.Spawned && Active;

        public float ResourceLossPerDay => def.resourceLossPerDay;

        public Pawn Pawn => pawn;

        public string DisplayLabel => def.resourceLabel;
		public override float InitialResourceMax => 1f;

        public override float MinLevelForAlert => 0.15f;
		protected override Color BarColor => Color.gray;

        protected override Color BarHighlightColor => new Color(84, 84, 84);

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
            Scribe_Deep.Look(ref _compBuildingDigitalMind, "_compBuildingDigitalMind");
            Scribe_Deep.Look(ref _projectionBody, "_projectionBody");
            
        }

        public void HealIfPossible()
        {
	        var hediffs = new List<Hediff>();
	        pawn.health.hediffSet.hediffs.CopyToList(hediffs, true);
	        foreach (var hediff in hediffs)
	        {
		        switch (hediff)
		        {
			        case Hediff_MissingPart _:
			        {
				        NanoswarmsHelper.WriteLog("Found missing part to heal. Part: " + hediff?.Part?.Label,NanoswarmsHelper.LogType.Debug);
				        var total = hediff.Part.def.hitPoints;
				        var naniteHealCost = (hediff.Part.def.hitPoints * 2) / 1000.0f;
				        NanoswarmsHelper.WriteLog("Total HP to restore: " + total + "; Nanite cost: " + naniteHealCost + "; Nanites available: " + Resource.Value,NanoswarmsHelper.LogType.Debug); 
			        
				        pawn.health.RestorePart(hediff.Part);
				        Resource.Value -= naniteHealCost;
				        break;
			        }
			        case Hediff_Injury _:
			        {
				        var naniteHealCost = (hediff.Severity * 2) / 1000.0f;
				        NanoswarmsHelper.WriteLog("Found injury to heal. Hediff: " + hediff?.Label + "; Severity: " + hediff?.Severity + "; Nanite cost to heal: " + naniteHealCost + "; Nanites available: " + Resource.Value,NanoswarmsHelper.LogType.Debug);
			        
				        hediff.Heal(hediff.Severity);
				        Resource.Value -= naniteHealCost;
				        break;
			        }
		        }
	        }
        }
    }
}