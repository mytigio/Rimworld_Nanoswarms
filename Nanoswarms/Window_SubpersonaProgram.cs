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
        private readonly CompBuildingDigitalMind _compBuildingDigitalMind;
        public Window_SubpersonaProgram(CompBuildingDigitalMind compBuildingDigitalMind, Action callback) : base(callback)
        {
            _compBuildingDigitalMind = compBuildingDigitalMind;
            selectedGenes = _compBuildingDigitalMind.StoredMind.genes.GenesListForReading.Select(x => x.def).ToList();
            forcePause = true;
        }

        public override List<GeneDef> SelectedGenes => this.selectedGenes;
        protected override string Header => "mytNS_Reprogram".Translate();
        protected override string AcceptButtonLabel => "mytNS_Reprogram".Translate();
        protected override void AcceptInner()
        {
            var customXenotype = new CustomXenotype
            {
                name = xenotypeName?.Trim(),
                inheritable = false,
                iconDef = iconDef
            };
            customXenotype.genes.AddRange(selectedGenes);
            _compBuildingDigitalMind.ReprogrammingProject = customXenotype;
            var workToDo = _compBuildingDigitalMind.StoredMind.genes.GenesListForReading.Where(x => x.def.IsAndroidGene() && !selectedGenes.Contains(x.def)).ToList().Count * 2000 + selectedGenes.Where(x => !_compBuildingDigitalMind.StoredMind.genes.GenesListForReading.Select(y => y.def).Contains(x)).ToList().Count * 2000;
            foreach (var gene in customXenotype.genes)
            {
                NanoswarmsHelper.WriteLog("Gene in " + xenotypeName?.Trim() + ": " + gene.defName,NanoswarmsHelper.LogType.Debug);    
            }
            NanoswarmsHelper.WriteLog("Work to complete " + customXenotype.name + ": " + workToDo, NanoswarmsHelper.LogType.Debug);
            _compBuildingDigitalMind.TotalWorkAmount = workToDo; 
            _compBuildingDigitalMind.CurrentWorkAmountDone = 0.0f;
        }

        public override bool GeneValidator(GeneDef x)
        {
                return ((x.IsAndroidGene()) && 
                        ((_compBuildingDigitalMind.Reprogrammable) ? x.AllowedForSwarms() : x.AllowedForSwarmCosmetic()));
        }
    }
}