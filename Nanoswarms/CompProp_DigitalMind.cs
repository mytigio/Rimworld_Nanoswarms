using RimWorld;
using Verse;

namespace Nanoswarms
{
    public class CompProps_DigitalMind : CompProperties
    {
        public PawnKindDef SpawnType;
        public int passionChancePercent = 0;
        public int maxPassions = 0;
        public int burningPassionChancePercent = 0;
        public int maxBurningPassions = 0;
        public int skillRangeMinimum = 0;
        public int skillRangeMaximum = 3;
        public int numberOfTraits = 0;
        public bool reprogrammable = false;

        public bool IsAIMind
        {
            get
            {
                return (SpawnType == PawnKindDef.Named("mytNS_SubpersonaAI") ||
                        SpawnType == PawnKindDef.Named("mytNS_PersonaAI"));
            }
        }
        
        public BackstoryDef ChildhoodBackstory
        {
            get
            {
                BackstoryDef backstoryDef = null;
                if (SpawnType == PawnKindDef.Named("mytNS_SubpersonaAI"))
                {
                    backstoryDef = mytNSDefOf.mytNS_SubPersonaCore;
                } else if (SpawnType == PawnKindDef.Named("mytNS_PersonaAI"))
                { 
                    backstoryDef = mytNSDefOf.mytNS_PersonaCore;
                }
                
                return backstoryDef;
            }
        }
        public CompProps_DigitalMind() => this.compClass = typeof (CompBuildingDigitalMind);
    }
}