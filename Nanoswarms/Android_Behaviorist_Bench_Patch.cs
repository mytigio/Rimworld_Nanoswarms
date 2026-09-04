using System;
using System.Collections.Generic;
using HarmonyLib;
using Verse;
using VREAndroids;

namespace Nanoswarms
{
    [HarmonyPatch(typeof(Building_AndroidBehavioristStation), "CanAcceptPawn")]
    public static class Android_Behaviorist_Bench_Patch
    {
        public static void Postfix(Pawn selPawn, ref AcceptanceReport __result)
        {
            if (!selPawn.IsNanoswarmAndroid())
                return;
            __result = (AcceptanceReport) "mytNS_NanoswarmNotAllowed".Translate();
        }
    }
}