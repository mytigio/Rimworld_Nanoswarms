using Verse;

namespace Nanoswarms
{
    public class CompBuildingDigitalMind : ThingComp
    {
        public Pawn StoredMind;
        public CompProps_DigitalMind Props => (CompProps_DigitalMind) this.props;

    }
}