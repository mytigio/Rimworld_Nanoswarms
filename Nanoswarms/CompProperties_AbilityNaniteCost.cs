using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace Nanoswarms
{
    public class CompProperties_AbilityNaniteCost : CompProperties_AbilityEffect
    {
        public float naniteCost;

        public CompProperties_AbilityNaniteCost()
        {
            compClass = typeof(CompAbilityEffect_NaniteCost);
        }

        public override IEnumerable<string> ExtraStatSummary()
        {
            yield return string.Concat("mytNS_NaniteCost".Translate() + ": ", Mathf.RoundToInt(naniteCost * 100f));
        }
    }
}