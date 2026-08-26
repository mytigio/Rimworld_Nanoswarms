using System;
using System.Collections.Generic;
using HarmonyLib;
using Verse;
using VREAndroids;

namespace Nanoswarms
{
    [HarmonyPatch(typeof(Need_ReactorPower), "get_CurLevel")]
    public static class AndroidNeedReactorPowerPatch
    {
        //[HarmonyPostfix]
        public static void Postfix(ref float __result, Pawn ___pawn)
        {
            NanoswarmsHelper.WriteLog("Checking if" + ___pawn?.Name + " is a projection swarm and should have power set to 1");
            if (___pawn != null && ___pawn.IsAndroid() && ___pawn.health.hediffSet.HasHediff(mytNSDefOf.mytNS_NanoswarmProjectionBody))
            {
                __result = 1.0f;
            }
        }
    }
}