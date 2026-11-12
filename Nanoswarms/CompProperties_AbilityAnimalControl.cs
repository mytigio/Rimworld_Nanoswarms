using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace Nanoswarms
{
    public class CompProperties_AbilityAnimalControl : CompProperties_AbilityEffect
    {
        public CompProperties_AbilityAnimalControl()
        {
            compClass = typeof(CompAbilityEffect_AnimalControl);
        }
    }
}