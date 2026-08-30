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

        public override List<GeneDef> SelectedGenes => this.selectedGenes;
        protected override string Header => "mytNS_Reprogram".Translate();
        protected override string AcceptButtonLabel => "mytNS_Reprogram".Translate();
        protected override void AcceptInner()
        {
            CustomXenotype customXenotype = new CustomXenotype();
            customXenotype.name = xenotypeName?.Trim();
            customXenotype.genes.AddRange(selectedGenes);
            customXenotype.inheritable = false;
            customXenotype.iconDef = iconDef;
            _compBuildingDigitalMind.reprogramingProject = customXenotype;
            var workToDo = _compBuildingDigitalMind.StoredMind.genes.GenesListForReading.Where(x => x.def.IsAndroidGene() && !selectedGenes.Contains(x.def)).ToList().Count * 2000 + selectedGenes.Where(x => !_compBuildingDigitalMind.StoredMind.genes.GenesListForReading.Select(y => y.def).Contains(x)).ToList().Count * 2000;
            NanoswarmsHelper.WriteLog("Work to complete " + customXenotype.name + ": " + workToDo, NanoswarmsHelper.LogType.Debug);
            _compBuildingDigitalMind.TotalWorkAmount = workToDo; 
            _compBuildingDigitalMind.CurrentWorkAmountDone = 0.0f;
        }

        public override bool GeneValidator(GeneDef x)
        {
            return ((x is AndroidGeneDef androidGeneDef) &&
                     androidGeneDef.AllowedForSwarms()
                    ) && base.GeneValidator(x);
        }
    }
}