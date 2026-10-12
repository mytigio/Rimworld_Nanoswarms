using System.Collections.Generic;
using Verse;

namespace Nanoswarms
{
    public class HediffCompProperties_ReactOnMultipleDamageTypes : HediffCompProperties
    {
        public List<DamageDef> damageDefIncoming;
        public BodyPartDef createHediffOn;
        public HediffDef createHediff;
        public bool vomit;
        
        public HediffCompProperties_ReactOnMultipleDamageTypes() => this.compClass = typeof (HediffComp_ReactOnMultipleDamageTypes);
    }
}