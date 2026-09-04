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

        public static bool IsNanoswarmAndroid(this Pawn pawn)
        {
            return pawn.health.hediffSet.HasHediff(mytNSDefOf.mytNS_NanoswarmProjectionBody);
        }
    }
}