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
        public static HediffDef mytNS_NanoswarmProjectionBody,mytNS_Desynchronization, mytNS_melee10, mytNS_shooting10, 
            mytNS_construction10, mytNS_mining10, mytNS_cooking10, mytNS_plants10, mytNS_animals10, mytNS_crafting10, 
            mytNS_artistic10, mytNS_medical10, mytNS_social10, mytNS_intelligence10, mytNS_construction20, 
            mytNS_artistic20, mytNS_crafting20, mytNS_animals20, mytNS_cooking20, mytNS_intelligence20, mytNS_medical20,
            mytNS_melee20, mytNS_mining20, mytNS_plants20, mytNS_shooting20, mytNS_social20;
        public static ThingDef mytSubpersonaNeuralArray, mytAINeuralArray, mytNS_DMNeuralArray, mytNS_Filth_Nanodust, mytNS_Filth_NanodustSmear, mytNS_SwarmProjectionRace;
        public static JobDef EnterDigitalMindArray;
        public static PawnKindDef mytNS_SwarmColonist;
        public static TaleDef EnteredDigitalMindArray;

        public static BodyPartDef mytNS_swarmCloud,
            mytNS_manipulator,
            mytNS_locomotor,
            mytNS_projector,
            mytNS_arrayCasing,
            mytNS_tunnelingArray,
            mytNS_opticalCluster,
            mytNS_vibrationCluster;
    }
}