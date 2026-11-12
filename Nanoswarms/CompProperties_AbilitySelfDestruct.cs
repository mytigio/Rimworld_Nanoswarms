using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace Nanoswarms
{
    public class CompProperties_AbilitySelfDestruct : CompProperties_AbilityEffect
    {
        public DamageDef damageDef;

        public CompProperties_AbilitySelfDestruct()
        {
            compClass = typeof(CompAbilityEffect_SelfDestruct);
        }
    }
}