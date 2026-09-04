using RimWorld;
using Verse;
using Verse.AI;

namespace Nanoswarms
{
    [DefOf]
    public class mytNSDefOf
    {
        public static NanoswarmSettings mytNS_NanoswarmSettings;
        public static BackstoryDef mytNS_PersonaCore, mytNS_SubPersonaCore;
        public static DamageDef mytNS_Damage_Nanodust;
        public static EffecterDef mytNS_Damage_HitSwarm;
        public static FleshTypeDef mytNS_NanoFlesh;
        public static NanoswarmAndroidGeneDef mytNS_NanobotSwarm;
        public static GeneCategoryDef mytNS_NanoSwarm_Hardware;
        public static HediffDef mytNS_NanoswarmProjectionBody;
        public static ThingDef mytSubpersonaNeuralArray, mytAINeuralArray, mytNS_DMNeuralArray, mytNS_Filth_Nanodust, mytNS_Filth_NanodustSmear;
        public static JobDef EnterDigitalMindArray;
        public static TaleDef EnteredDigitalMindArray;
    }
}