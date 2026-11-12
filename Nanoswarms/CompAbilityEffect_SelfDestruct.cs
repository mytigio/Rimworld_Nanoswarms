using RimWorld;
using UnityEngine;
using Verse;

namespace Nanoswarms
{
    public class CompAbilityEffect_SelfDestruct : CompAbilityEffect
    {
        private const float MaxDamage = 4000.0f;
        private const float MaxRange = 9.8f;

        private const float Divisor = 99.0f;
        private const float RangeExponent = 0.421f;
        public new CompProperties_AbilitySelfDestruct Props => (CompProperties_AbilitySelfDestruct)props;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);
            if (!parent.pawn.IsNanoswarmAndroid()) return;
            var gene = parent.pawn.genes?.GetFirstGeneOfType<Gene_NaniteSwarmBody>();
            if (gene == null) return;
            var availableNanites = (gene.Resource.Value * 100.0f);

            var armorPenetration = Props.damageDef.defaultArmorPenetration;
            
            var calculatedRange = MaxRange * Mathf.Pow(((availableNanites - 1.0f) / Divisor),RangeExponent);
            var calculatedDamage = (int)(((availableNanites - 1) * MaxDamage / Divisor));

            var position = parent.pawn.Position;
            var map = parent.pawn.MapHeld;
            var sound = Props.damageDef.soundExplosion;
            GenExplosion.DoExplosion(position, map, calculatedRange, Props.damageDef, parent.pawn, calculatedDamage,
                armorPenetration, sound, null, null, null, null, 0f, 0, null, null, 0, true, null, 0f, 0, 0f,
                true);
            NanoswarmsHelper.WriteLog($"Calculated Range and Damage: {calculatedRange} / {calculatedDamage}");
            
            parent.pawn.removeNanites(availableNanites);
        }
    }
}