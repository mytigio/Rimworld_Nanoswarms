using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace Nanoswarms
{
    public class GeneGizmo_Resource_Nanites : GeneGizmo_Resource
    {
        private static bool draggingBar;
        private List<Pair<IGeneResourceDrain, float>> tmpDrainGenes = new List<Pair<IGeneResourceDrain, float>>();
        public GeneGizmo_Resource_Nanites(Gene_NaniteSwarmBody gene, List<IGeneResourceDrain> drainGenes, Color barColor, Color barhighlightColor) : base(gene, drainGenes, barColor, barhighlightColor)
        {

        }

        protected override bool DraggingBar {
            get
            {
                return draggingBar;
            }
            set
            {
                draggingBar = value;
            }
        }

        protected override string GetTooltip()
        {
            tmpDrainGenes.Clear();
            var text = $"{gene.ResourceLabel.CapitalizeFirst().Colorize(ColoredText.TipSectionTitleColor)}: {gene.ValueForDisplay} / {gene.MaxForDisplay}\n";
            if (!drainGenes.NullOrEmpty())
            {
                var num = 0f;
                foreach (var drainGene in drainGenes.Where(drainGene => drainGene.CanOffset))
                {
                    tmpDrainGenes.Add(new Pair<IGeneResourceDrain, float>(drainGene, drainGene.ResourceLossPerDay));
                    num += drainGene.ResourceLossPerDay;
                }
                if (num != 0f)
                {
                    string text2 = ((num < 0f) ? "RegenerationRate".Translate() : "DrainRate".Translate());
                    text = text + "\n\n" + text2 + ": " + "PerDay".Translate(Mathf.Abs(gene.PostProcessValue(num))).Resolve();
                    text = tmpDrainGenes.Aggregate(text, (current, tmpDrainGene) => current + "\n  - " + tmpDrainGene.First.DisplayLabel.CapitalizeFirst() + ": " + "PerDay".Translate(gene.PostProcessValue(0f - tmpDrainGene.Second).ToStringWithSign()).Resolve());
                }
            }
            if (!gene.def.resourceDescription.NullOrEmpty())
            {
                text = text + "\n\n" + gene.def.resourceDescription.Formatted(gene.pawn.Named("PAWN")).Resolve();
            }
            return text;
        }
    }
}