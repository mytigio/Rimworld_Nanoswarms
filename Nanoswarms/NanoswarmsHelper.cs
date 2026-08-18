using System;
using HarmonyLib;
using Verse;

namespace Nanoswarms
{
    [StaticConstructorOnStartup]
    public static class NanoswarmsHelper
    {
        private static readonly Harmony HarmonyInstance;
        private static readonly string ModName = "[Nanoswarms]";
        
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
