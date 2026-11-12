using System;
using System.Collections.Generic;
using HarmonyLib;
using Verse;
using VREAndroids;

namespace Nanoswarms
{
    [HarmonyPatch(typeof(Building_AndroidBehavioristStation), "CanAcceptPawn")]
    public static class mytNS_Patch_Android_Behaviorist_Bench
    {
        public static void Postfix(Pawn selPawn, ref AcceptanceReport __result)
        {
            if (!selPawn.IsNanoswarmAndroid())
                return;
            __result = (AcceptanceReport) "mytNS_NanoswarmNotAllowed".Translate();
        }
    }
}