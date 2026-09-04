using System;
using System.Collections.Generic;
using HarmonyLib;
using Verse;
using VREAndroids;

namespace Nanoswarms
{
    [StaticConstructorOnStartup]
    public static class NanoswarmsHelper
    {
        private static readonly Harmony HarmonyInstance;
        private static readonly string ModName = "[Nanoswarms]";

        private static List<GeneCategoryDef> extraGeneCategories;

        public static List<GeneCategoryDef> ExtraGeneCategories
        {
            get
            {
                if (extraGeneCategories == null)
                {
                    extraGeneCategories = new List<GeneCategoryDef>();
                    foreach (var convGenes in DefDatabase<AndroidConvertableGenesDef>
                                 .AllDefsListForReading)
                    {
                        extraGeneCategories.AddRange(convGenes.geneCategories);
                    }
                    
                    WriteLog("extra gene categories are null. Set to convertable genes defined in androids.", LogType.Debug);
                    WriteLog("extra categories: " + extraGeneCategories?.Count, LogType.Debug);
                }
                
                return extraGeneCategories;
            }
        }
        public static bool IsDebugBuild
        {
            get
            {
                #if DEBUG
                    return true;
                #else
                    return false;
                #endif
            }
        }


        static NanoswarmsHelper()
        {
            NanoswarmsHelper.WriteLog("Starting Nanoswarms Helper Mod.");
            NanoswarmsHelper.HarmonyInstance = new Harmony("mytMods.Nanoswarms");
            NanoswarmsHelper.HarmonyInstance.PatchAll();
            NanoswarmsHelper.WriteLog("Harmony Started.");
        }
        
        public enum LogType { Debug, Info, Warning, Error };
        
        public static void WriteLog(string message, LogType type = LogType.Info)
        {
            if (!IsDebugBuild && type == LogType.Debug)
                return;
            var finalMessage = $"{NanoswarmsHelper.ModName} {message}";
            switch (type)
            {
                case LogType.Debug:
                case LogType.Info:
                    Log.Message(finalMessage);
                    break;
                case LogType.Warning:
                    Log.Warning(finalMessage);
                    break;
                case LogType.Error:
                    Log.Error(finalMessage);
                    break;
                default:
                    Log.Error(finalMessage);
                    Log.Error("Somehow hit default in WriteLog");
                    break;
            }
        }
    }
}
