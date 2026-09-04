using UnityEngine;
using Verse;

namespace Nanoswarms
{
    public class CompNoStoredMindGraphic : ThingComp
    {
        private CompProps_NoStoredMindGraphic PropsNo => (CompProps_NoStoredMindGraphic) props;
        
        public bool ParentIsEmpty => parent is IThingHolder thingParent && thingParent.GetDirectlyHeldThings().NullOrEmpty<Thing>();

        public bool ParentHasNoStoredMind
        {
            get
            {
                var comp = parent.TryGetComp<CompBuildingDigitalMind>();
                return comp?.StoredMind == null;
            }
        }

        public bool ShouldDraw => (ParentHasNoStoredMind && ParentIsEmpty);

        public override bool DontDrawParent() => (ShouldDraw) && !PropsNo.alwaysDrawParent;
        
        public override void PostDraw()
        {
            if (!ShouldDraw || parent.def.drawerType == DrawerType.MapMeshOnly)
                return;
            Graphics.DrawMesh(PropsNo.graphicData.Graphic.MeshAt(parent.Rotation),
                parent.DrawPos + PropsNo.graphicData.drawOffset.RotatedBy(parent.Rotation),
                Quaternion.identity,
                PropsNo.graphicData.Graphic.MatAt(parent.Rotation),
                0);
        }

        public override void PostPrintOnto(SectionLayer layer)
        {
            if (!ShouldDraw)
                return;
            PropsNo.graphicData.Graphic.Print(layer, parent, 0.0f);
        }
    }
}