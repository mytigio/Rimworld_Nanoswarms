using RimWorld;
using Verse;

namespace Nanoswarms
{
    public class CompProps_DigitalMind : CompProperties
    {
        public PawnKindDef SpawnType;

        public bool IsAIMind
        {
            get
            {
                return (SpawnType == PawnKindDef.Named("mytNS_SubpersonaAI") ||
                        SpawnType == PawnKindDef.Named("mytNS_PersonaAI"));
            }
        }
        
        public int BaseStats
        {
            get
            {
                int baseStat = 0;
                if (SpawnType == PawnKindDef.Named("mytNS_SubpersonaAI"))
                {
                    baseStat = 3;   
                } else if (SpawnType == PawnKindDef.Named("mytNS_PersonaAI"))
                {
                    baseStat = 8;
                }
                
                return baseStat;
            }
        }
        
        public BackstoryDef ChildhoodBackstory
        {
            get
            {
                BackstoryDef backstoryDef = null;
                if (SpawnType == PawnKindDef.Named("mytNS_SubpersonaAI"))
                {
                    backstoryDef = DefDatabase<BackstoryDef>.GetNamed("mytNS_SubPersonaCore");
                } else if (SpawnType == PawnKindDef.Named("mytNS_PersonaAI"))
                { 
                    backstoryDef = DefDatabase<BackstoryDef>.GetNamed("mytNS_PersonaCore");
                }
                
                return backstoryDef;
            }
        }
        public CompProps_DigitalMind() => this.compClass = typeof (CompBuildingDigitalMind);
    }
}