using Verse;

namespace Nanoswarms
{
    public class CompProps_NoStoredMindGraphic : CompProperties
    {
        public GraphicData graphicData;
        public bool alwaysDrawParent;

        public CompProps_NoStoredMindGraphic() => compClass = typeof (CompNoStoredMindGraphic);
    }
}