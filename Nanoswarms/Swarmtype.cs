using System.Collections.Generic;
using RimWorld;
using Verse;

namespace Nanoswarms
{
    public class Swarmtype : Def
    {
        public string descriptionShort;
        public string iconPath;
        public List<GeneDef> hardwareGenes;
        public List<GeneDef> defaultSubroutineGenes;
        public bool isAI;
        public BackstoryDef backstory = null;
    }
}