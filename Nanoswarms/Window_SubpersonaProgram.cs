using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using VREAndroids;

namespace Nanoswarms
{
    [HotSwappable]
    public class Window_SubpersonaProgram : Window_CreateAndroidBase
    {
        private CompBuildingDigitalMind _compBuildingDigitalMind;
        public Window_SubpersonaProgram(CompBuildingDigitalMind compBuildingDigitalMind, Action callback) : base(callback)
        {
            _compBuildingDigitalMind = compBuildingDigitalMind;
            this.selectedGenes = _compBuildingDigitalMind.StoredMind.genes.GenesListForReading.Where<Gene>(x => x.def.IsAndroidGene()).Select(x => x.def).ToList<GeneDef>();
            this.forcePause = true;
        }

        protected override string Header => "mytNS.mytNS_Reprogram".Translate();
        protected override string AcceptButtonLabel => "mytNS.mytNS_Reprogram".Translate();
        protected override void AcceptInner()
        {
            CustomXenotype customXenotype = new CustomXenotype();
            customXenotype.name = this.xenotypeName?.Trim();
            customXenotype.genes.AddRange((IEnumerable<GeneDef>) this.selectedGenes);
            customXenotype.inheritable = false;
            customXenotype.iconDef = this.iconDef;
            _compBuildingDigitalMind.TotalWorkAmount = (float) (_compBuildingDigitalMind.StoredMind.genes.GenesListForReading.Where<Gene>(x => x.def.IsAndroidGene() && !this.selectedGenes.Contains(x.def)).ToList<Gene>().Count * 2000 + this.selectedGenes.Where<GeneDef>(x => !_compBuildingDigitalMind.StoredMind.genes.GenesListForReading.Select<Gene, GeneDef>((Func<Gene, GeneDef>) (y => y.def)).Contains<GeneDef>(x)).ToList<GeneDef>().Count * 2000);
            this.station.currentWorkAmountDone = 0.0f;
            this.station.initModification = true;
        }

        
    }
}