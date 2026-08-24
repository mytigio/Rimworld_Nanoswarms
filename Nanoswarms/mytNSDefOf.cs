using RimWorld;
using Verse;
using Verse.AI;

namespace Nanoswarms
{
    [DefOf]
    public class mytNSDefOf
    {
        public static BackstoryDef mytNS_PersonaCore, mytNS_SubPersonaCore;
        public static DamageDef mytNS_Damage_Nanodust;
        public static EffecterDef mytNS_Damage_HitSwarm;
        public static FleshTypeDef mytNS_NanoFlesh;
        public static VREAndroids.AndroidGeneDef mytNS_NanobotCirculation;

        public static XenotypeDef mytNS_NanoswarmDigitalMind,
            mytNS_NanoswarmAI,
            mytNS_NanoswarmBasic;
        public static HediffDef mytNS_NanoswarmProjectionBody;
        public static PawnKindDef mytNS_SubpersonaAI, mytNS_PersonaAI, mytNS_DigitizedMind;
        public static ThingDef mytSubpersonaNeuralArray, mytAINeuralArray, mytNS_DMNeuralArray, mytNS_Filth_Nanodust, mytNS_Filth_NanodustSmear;
        public static JobDef EnterDigitalMindArray;
        public static TaleDef EnteredDigitalMindArray;
    }
}