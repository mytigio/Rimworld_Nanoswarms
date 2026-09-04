using VREAndroids;

namespace Nanoswarms
{
    public static class Extensions
    {
        public static bool AllowedForSwarms(this AndroidGeneDef geneDef)
        {
            if (geneDef.displayCategory != VREA_DefOf.VREA_Subroutine) return false;
            var onDisallowedList = (mytNSDefOf.mytNS_NanoswarmSettings.disallowedSubroutines.Contains(geneDef?.defName));
            return !onDisallowedList;
        }
    }
}