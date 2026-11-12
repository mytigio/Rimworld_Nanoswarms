using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace Nanoswarms
{
    public class CompProperties_AbilityNaniteConstruction : CompProperties_AbilityEffect
    {
        public CompProperties_AbilityNaniteConstruction()
        {
            compClass = typeof(CompAbilityEffect_NaniteConstruction);
        }
    }
}