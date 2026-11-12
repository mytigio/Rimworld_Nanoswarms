using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace Nanoswarms
{
    public class CompProperties_AbilityNaniteSculpting : CompProperties_AbilityEffect
    {
        public CompProperties_AbilityNaniteSculpting()
        {
            compClass = typeof(CompAbilityEffect_NaniteSculpting);
        }
    }
}