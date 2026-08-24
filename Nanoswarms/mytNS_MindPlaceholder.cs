using Verse;

namespace Nanoswarms
{
    public class mytNS_MindPlaceholder : Thing
    {
        public Pawn pawnPlaceholder;

        public override string Label => pawnPlaceholder?.Name.ToStringShort;
    }
}