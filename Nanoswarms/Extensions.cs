using System.Collections.Generic;
using RimWorld;
using Verse;
using VREAndroids;

namespace Nanoswarms
{
    public static class Extensions
    {
        public static bool AllowedForSwarms(this GeneDef geneDef)
        {
            var geneCategories = NanoswarmsHelper.ExtraGeneCategories;
            geneCategories.Add(VREA_DefOf.VREA_Subroutine);
            geneCategories.Add(mytNSDefOf.mytNS_NanoSwarm_Hardware);
            if (!geneCategories.Contains(geneDef.displayCategory)) return false;
            var onDisallowedList = (mytNSDefOf.mytNS_NanoswarmSettings.disallowedSubroutines.Contains(geneDef?.defName));
            return !onDisallowedList;
        }
        
        public static bool AllowedForSwarmCosmetic(this GeneDef geneDef)
        {
            var geneCategories = NanoswarmsHelper.ExtraGeneCategories;
            if (!geneCategories.Contains(geneDef.displayCategory)) return false;
            var onDisallowedList = (mytNSDefOf.mytNS_NanoswarmSettings.disallowedSubroutines.Contains(geneDef?.defName));
            return !onDisallowedList;
        }

        public static bool IsNanoswarmHediff(this Hediff hediff)
        {
            var nanoswarmHediffs = new List<HediffDef>()
            {
                mytNSDefOf.mytNS_NanoswarmProjectionBody,
                mytNSDefOf.mytNS_Desynchronization,
                mytNSDefOf.mytNS_melee10,
                mytNSDefOf.mytNS_shooting10,
                mytNSDefOf.mytNS_construction10,
                mytNSDefOf.mytNS_mining10,
                mytNSDefOf.mytNS_cooking10,
                mytNSDefOf.mytNS_plants10,
                mytNSDefOf.mytNS_animals10,
                mytNSDefOf.mytNS_crafting10,
                mytNSDefOf.mytNS_artistic10,
                mytNSDefOf.mytNS_medical10,
                mytNSDefOf.mytNS_social10,
                mytNSDefOf.mytNS_intelligence10
            };
                
            return hediff != null && nanoswarmHediffs.Contains(hediff.def);
        }

        public static bool IsNanoswarmAndroid(this Pawn pawn)
        {
            return pawn.health.hediffSet.HasHediff(mytNSDefOf.mytNS_NanoswarmProjectionBody);
        }
    }
}