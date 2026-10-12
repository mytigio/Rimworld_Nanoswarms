using System;
using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.Noise;
using VREAndroids;

namespace Nanoswarms
{
    [HarmonyPatch(typeof(QualityUtility), "GenerateQualityCreatedByPawn", new Type[]
    {
        typeof(Pawn),
        typeof(SkillDef),
        typeof(bool)
    })]
    public static class mytNS_QualityUtility_GenerateQualityCreatedByPawn
    {
        public static void Postfix(Pawn pawn, SkillDef relevantSkill, ref QualityCategory __result)
        {
            if (pawn == null) return;
            var hasCraftingHediff = pawn.health.hediffSet.HasHediff(mytNSDefOf.mytNS_crafting10);
            var hasArtisticHediff = pawn.health.hediffSet.HasHediff(mytNSDefOf.mytNS_artistic10);
            var numericalQuality = (int)__result;
            NanoswarmsHelper.WriteLog($"{pawn.Name} completed project with skill {relevantSkill.defName}. calculated quality: {__result}", NanoswarmsHelper.LogType.Debug);
            if (relevantSkill == SkillDefOf.Crafting && hasCraftingHediff && numericalQuality < 5)
            {
                //we only improve crafting projects up to masterwork.
                __result = (QualityCategory)(byte)numericalQuality + 1;
                NanoswarmsHelper.WriteLog($"{pawn.Name} completed project with skill {relevantSkill.defName}. updated calculated quality: {__result}", NanoswarmsHelper.LogType.Debug);
            } else if ((relevantSkill == SkillDefOf.Artistic || relevantSkill == SkillDefOf.Construction) && hasArtisticHediff && numericalQuality < 6)
            {
                //we improve art and constructions projects up to legendary and by 1 point based on having the artistic level 10 power.
                __result = (QualityCategory)(byte)numericalQuality + 1;
                NanoswarmsHelper.WriteLog($"{pawn.Name} completed project with skill {relevantSkill.defName}. updated calculated quality: {__result}", NanoswarmsHelper.LogType.Debug);
            }
        }
    }
}