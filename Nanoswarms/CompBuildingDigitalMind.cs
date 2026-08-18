using System;
using System.Collections.Generic;
using UnityEngine;
using RimWorld;
using Verse;
using VREAndroids;
using Random = UnityEngine.Random;

namespace Nanoswarms
{
    public class CompBuildingDigitalMind : ThingComp
    {
        //public variables.
        public Pawn StoredMind;
        public CompProps_DigitalMind Props => (CompProps_DigitalMind) this.props;
        
        //Private variables.
        private CompPowerTrader _compPower;
        private static readonly Color NanoswarmColor = Color.gray;
        //private static readonly DamageDef NanoDust = DefDatabase<DamageDef>.GetNamed(nameof(mytNS_Filth_Nanodust));

        public override void Initialize(CompProperties props)
        {
            base.Initialize(props);
            if (Props.IsAIMind && StoredMind == null)
            {
                CreateAIMind();
            }
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            this._compPower = this.parent.TryGetComp<CompPowerTrader>();
        }
        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            List<Gizmo> gizmosExtra = new List<Gizmo>();
            gizmosExtra.AddRange(base.CompGetGizmosExtra());
            if (this.parent.Faction == Faction.OfPlayer && this._compPower.PowerOn)
            {
                if (this.StoredMind == null && DebugSettings.godMode)
                {
                    Command_Action createAIMindDebug = new Command_Action
                    {
                        action = CreateAIMind,
                        defaultLabel = (string) "mytNS_Debug_CreateAIMind".Translate(),
                        defaultDesc = (string) "mytNS_Debug_CreateAIMindDesc".Translate(),
                    };
                    gizmosExtra.Add(createAIMindDebug);
                }
                if (this.StoredMind != null && !this.StoredMind.Spawned && !this.StoredMind.InContainerEnclosed && this.StoredMind.CarriedBy == null)
                {
                    Command_Action formPrjectionAction = new Command_Action
                    {
                        action = FormProjection,
                        defaultLabel = (string) "mytNS_SpawnProjection".Translate(),
                        defaultDesc = (string) "mytNS_SpawnProjectionDesc".Translate(),
                        icon = (Texture) ContentFinder<Texture2D>.Get("UI/Gizmos/FormProjection")
                    };
                    gizmosExtra.Add(formPrjectionAction);
                } else if (this.StoredMind != null && this.StoredMind.Spawned)
                {
                    Command_Action reFormPrjectionAction = new Command_Action
                    {
                        action = FormProjection,
                        defaultLabel = (string) "mytNS_RespawnProjection".Translate(),
                        defaultDesc = (string) "mytNS_RespawnProjectionDesc".Translate(),
                        icon = (Texture) ContentFinder<Texture2D>.Get("UI/Gizmos/FormProjection")
                    };
                    gizmosExtra.Add(reFormPrjectionAction);
                }
            }

            return gizmosExtra;
        }

        public virtual void CreateAIMind()
        {
            NanoswarmsHelper.WriteLog("Creating AI Mind",NanoswarmsHelper.LogType.Debug);
            var pawnKindDef = this.Props.SpawnType;
            var ofPlayer = Faction.OfPlayer;
            var pawnRequest = new PawnGenerationRequest(
                pawnKindDef,
                ofPlayer,
                PawnGenerationContext.NonPlayer,
                -1,
                true,
                false,
                false,
                false,
                true,
                0.0f,
                false,
                true,
                false,
                false,
                false,
                false,
                false,
                false,
                false,
                0.0f,
                0.0f,
                null,
                1f,
                (Predicate<Pawn>)null,
                (Predicate<Pawn>)null,
                (IEnumerable<TraitDef>)null,
                (IEnumerable<TraitDef>)null,
                null,
                18f,
                null,
                null,
                (string)null,
                (string)null,
                null,
                null,
                true,
                true,
                true,
                false,
                (List<GeneDef>)null,
                (List<GeneDef>)null,
                null,
                null,
                (List<XenotypeDef>)null,
                0.0f,
                DevelopmentalStage.Adult,
                (Func<XenotypeDef, PawnKindDef>)null,
                null,
                null,
                false,
                true,
                false,
                -1,
                0,
                true);
            var pawn = PawnGenerator.GeneratePawn(pawnRequest);
            pawn.story.Childhood = Props.ChildhoodBackstory;
            pawn.Name = PawnBioAndNameGenerator.GeneratePawnName(pawn);
            pawn.Position = parent.Position;
            pawn.relations = new Pawn_RelationsTracker(pawn);
            pawn.interactions = new Pawn_InteractionsTracker(pawn);
            while (pawn.story.traits.allTraits.Count > 0)
                pawn.story.traits.allTraits.RemoveLast<Trait>();
            StoredMind = pawn;
            foreach (var skill in StoredMind.skills.skills)
            {
                var random = Random.Range(0,100);
                var passionToSet = Passion.None;
                if (random < 1)
                {
                    passionToSet = Passion.Major;
                } else if (random < 10)
                {
                    passionToSet = Passion.Minor;
                }
                NanoswarmsHelper.WriteLog("Value for " + skill.LevelDescriptor + ": " + random,
                    NanoswarmsHelper.LogType.Debug);
                skill.passion = passionToSet;
                skill.levelInt = this.Props.BaseStats;
                var totallyDisabled = skill.TotallyDisabled;
            }
            StoredMind.skills.Notify_SkillDisablesChanged();
            if (ModsConfig.IdeologyActive)
                StoredMind.ideo.SetIdeo(Faction.OfPlayer.ideos.PrimaryIdeo);
            pawn.apparel.DestroyAll();
        }

        public virtual void PreFormation()
        {
            // iterate over the hediffs and remove any we wouldn't want on a digital mind in a nanobot swarm body.
            // this will include almost everything.
            //copy the list so we can act on it.
            var hediffList = StoredMind.health.hediffSet.hediffs.ListFullCopyOrNull();
            foreach (var hediff in hediffList)
            {
                NanoswarmsHelper.WriteLog("Removing Hediff " + hediff.Label, NanoswarmsHelper.LogType.Debug);
                StoredMind.health.RemoveHediff(hediff);
            }

            StoredMind.ageTracker.ResetAgeReversalDemand(Pawn_AgeTracker.AgeReversalReason.ViaTreatment);
            StoredMind.story.HairColor = NanoswarmColor;
            StoredMind.story.skinColorOverride = NanoswarmColor;
        }
        public virtual void FormProjection()
        {
            NanoswarmsHelper.WriteLog("Form Projection Started", NanoswarmsHelper.LogType.Debug);
            if (StoredMind.Spawned || StoredMind.Corpse != null)
            {
                NanoswarmsHelper.WriteLog("Projection already formed. Destroy first.", NanoswarmsHelper.LogType.Debug);
                StopProjection();
            }

            PreFormation();

            if (StoredMind.Dead)
            {
                ResurrectionUtility.TryResurrect(StoredMind);
            }
                
            GenPlace.TryPlaceThing(StoredMind, parent.Position, parent.Map, ThingPlaceMode.Near);
            this.StoredMind.Drawer.renderer.EnsureGraphicsInitialized();
        }

        public virtual void StopProjection()
        {
            NanoswarmsHelper.WriteLog("Form Projection Stopped", NanoswarmsHelper.LogType.Debug);
            if (StoredMind.carryTracker?.CarriedThing != null)
            {
                NanoswarmsHelper.WriteLog("Form Projection Carrying stuff. Drop it.", NanoswarmsHelper.LogType.Debug);
                StoredMind.carryTracker.TryDropCarriedThing(StoredMind.Position, ThingPlaceMode.Near, out var resultingThing);
            }
                
            if (StoredMind.Spawned || StoredMind.Corpse != null)
            {
                NanoswarmsHelper.WriteLog("Form Projection currently spawned. Drop all of their things.", NanoswarmsHelper.LogType.Debug);
                StoredMind.apparel.DropAll(StoredMind.Position);
                StoredMind.inventory.DropAllNearPawn(StoredMind.Position);
            }

            if (StoredMind.Map != null)
            {
                NanoswarmsHelper.WriteLog("Found StoredMind Map.  Trigger explosion and then despawn.", NanoswarmsHelper.LogType.Debug);
                //GenExplosion.DoExplosion(StoredMind.Position, StoredMind.Map, 4.9f, NanoDust, (Thing) StoredMind, -1, -1f, (SoundDef) null, (ThingDef) null, (ThingDef) null, (Thing) null, RimWorld.ThingDefOf.Filth_Slime);
                StoredMind.DeSpawn();
            }

            if (StoredMind.Corpse?.Map != null)
            {
                NanoswarmsHelper.WriteLog("Found StoredMind Corpse Map.  Trigger explosion and then despawn.", NanoswarmsHelper.LogType.Debug);
                //GenExplosion.DoExplosion(StoredMind.Corpse.Position, StoredMind.Corpse.Map, 4.9f, NanoDust, (Thing) StoredMind.Corpse, -1, -1f, (SoundDef) null, (ThingDef) null, (ThingDef) null, (Thing) null, RimWorld.ThingDefOf.Filth_Slime);
                StoredMind.Corpse.Destroy();
            }
            
        }
    }
}