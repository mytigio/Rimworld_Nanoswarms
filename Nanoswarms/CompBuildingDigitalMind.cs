using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;
using VREAndroids;
using Random = UnityEngine.Random;

namespace Nanoswarms
{
    public class CompBuildingDigitalMind : ThingComp
    {
        private int tickModifier = 250;
        //public variables.
        public Pawn StoredMind;
        private CompProps_DigitalMind Props => (CompProps_DigitalMind) props;
        public CompRefuelable _compRefuelable;
        
        //Private variables.
        private CompPowerTrader _compPower;
        private static readonly Color NanoswarmColor = Color.gray;
        //private static readonly DamageDef NanoDust = DefDatabase<DamageDef>.GetNamed(nameof(mytNS_Filth_Nanodust));

        public CustomXenotype reprogramingProject;

        public CustomXenotype storedCustomXenotype;

        public float TotalWorkAmount = 12000.0f;
        public float CurrentWorkAmountDone = 0.0f;
        public float PowerScale = 1.0f;

        private readonly float ticksToFormBody = 2500.0f;
        private float _bodyFormingCompletedTicks = 0.0f;
        private bool isBodyForming = false;
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
            _compPower = parent.TryGetComp<CompPowerTrader>();
            _compRefuelable = parent.GetComp<CompRefuelable>();
        }
        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            var gizmosExtra = new List<Gizmo>();
            gizmosExtra.AddRange(base.CompGetGizmosExtra());
            if (parent.Faction == Faction.OfPlayer && _compPower.PowerOn)
            {
                if (StoredMind == null && DebugSettings.godMode)
                {
                    var createAIMindDebug = new Command_Action
                    {
                        action = CreateAIMind,
                        defaultLabel = "mytNS_Debug_CreateAIMind".Translate(),
                        defaultDesc = "mytNS_Debug_CreateAIMindDesc".Translate(),
                    };
                    gizmosExtra.Add(createAIMindDebug);
                }
                
                if (DebugSettings.godMode && ReprogrammingJobReady() && ReprogrammingInProgress())
                {
                    var finishReprogramming = new Command_Action
                    {
                        action = SetCustomXenotype,
                        defaultLabel = "mytNS_Debug_CompleteReprogramming".Translate(),
                        defaultDesc = "mytNS_Debug_CompleteReprogrammingDesc".Translate(),
                    };
                    gizmosExtra.Add(finishReprogramming);
                }
                
                if (StoredMind != null && !StoredMind.Spawned && !StoredMind.InContainerEnclosed && StoredMind.CarriedBy == null && !ReprogrammingJobReady() && !isBodyForming)
                {
                    var formProjectionAction = new Command_Action
                    {
                        action = initializeFormation,
                        defaultLabel = "mytNS_SpawnProjection".Translate(),
                        defaultDesc = "mytNS_SpawnProjectionDesc".Translate(),
                        icon = ContentFinder<Texture2D>.Get("UI/Gizmos/FormProjection")
                    };
                    gizmosExtra.Add(formProjectionAction);
                } else if (StoredMind != null && StoredMind.Spawned && !ReprogrammingJobReady() && !isBodyForming)
                {
                    var endFormProjectionAction = new Command_Action
                    {
                        action = StopProjection,
                        defaultLabel = "mytNS_EndProjection".Translate(),
                        defaultDesc = "mytNS_EndProjectionDesc".Translate(),
                        icon = ContentFinder<Texture2D>.Get("UI/Gizmos/CancelProjection")
                    };
                    gizmosExtra.Add(endFormProjectionAction);
                    
                    var reFormProjectionAction = new Command_Action
                    {
                        action = initializeFormation,
                        defaultLabel = "mytNS_RespawnProjection".Translate(),
                        defaultDesc = "mytNS_RespawnProjectionDesc".Translate(),
                        icon = ContentFinder<Texture2D>.Get("UI/Gizmos/FormProjection")
                    };
                    gizmosExtra.Add(reFormProjectionAction);
                }

                if (StoredMind != null && !StoredMind.Spawned && !StoredMind.InContainerEnclosed &&
                    StoredMind.CarriedBy == null && Props.reprogrammable && !ReprogrammingJobReady() && !isBodyForming)
                {
                    var reProgramAction = new Command_Action
                    {
                        action = InitiateReprogram,
                        defaultLabel = "mytNS_Reprogram".Translate(),
                        defaultDesc = "mytNS_ReprogramDesc".Translate(),
                        icon = ContentFinder<Texture2D>.Get("UI/Gizmos/ModifyAnAndroid")
                    };
                    gizmosExtra.Add(reProgramAction);
                }
            }

            return gizmosExtra;
        }

        public override string CompInspectStringExtra()
        {
            var sb = new StringBuilder();
            sb.Append(base.CompInspectStringExtra());
            if (StoredMind != null)
            {
                sb.Append("mytNS_StoredMind".Translate() + ": " + StoredMind.Name);
                sb.AppendLine();
            }

            if (ReprogrammingInProgress())
            {
                sb.Append("mytNS_Reprogramming".Translate() + ": " +
                          (CurrentWorkAmountDone / TotalWorkAmount).ToStringPercent());
                sb.AppendLine();
            }

            if (isBodyForming)
            {
                sb.Append("mytNS_FormingBody".Translate() + ": " +
                          (_bodyFormingCompletedTicks / ticksToFormBody).ToStringPercent());
                sb.AppendLine();
            }
            return sb.ToString().Trim();
        }

        private void InitiateReprogram()
        {
            if (!_compPower.PowerOn || !Props.reprogrammable) return;
            var creationWindow = new Window_SubpersonaProgram(this, null)
                {
                    disableAndroidHardwareLimitation = false
                };

            Find.WindowStack.Add(creationWindow);
        }

        public override void CompTickRare()
        {
            base.CompTickRare();
            
            if (ReprogrammingJobReady())
            {
                NanoswarmsHelper.WriteLog("Reprogramming Project Name: " + reprogramingProject.name,NanoswarmsHelper.LogType.Debug);
                NanoswarmsHelper.WriteLog("Work: " + CurrentWorkAmountDone + " / " + TotalWorkAmount);

                CurrentWorkAmountDone += tickModifier;
                if (CurrentWorkAmountDone >= TotalWorkAmount)
                {
                    SetCustomXenotype();
                }
            }

            if (isBodyForming && _compRefuelable.Fuel >= tickModifier * 4)
            {
                _bodyFormingCompletedTicks += tickModifier;
                _compRefuelable.ConsumeFuel(4 * tickModifier);
                if (_bodyFormingCompletedTicks >= ticksToFormBody)
                {
                    FormProjection();
                    isBodyForming = false;
                }
            }
        }

        private void SetCustomXenotype()
        {
            storedCustomXenotype = reprogramingProject;
            reprogramingProject = null;
            TotalWorkAmount = 12000;
            CurrentWorkAmountDone = 0;
            NanoswarmsHelper.WriteLog("Set stored xenotype to " + storedCustomXenotype.name,NanoswarmsHelper.LogType.Debug);
            PreFormation();
            var metScore = StoredMind.genes.GenesListForReading.Where(gene => !gene.Overridden).Sum(gene => gene.def.biostatMet);
            var powerConsumption = -(_compPower.Props.PowerConsumption * AndroidStatsTable.PowerEfficiencyToPowerDrainFactorCurve.Evaluate(metScore));
            NanoswarmsHelper.WriteLog("Building power to " + powerConsumption,NanoswarmsHelper.LogType.Debug);
            _compPower.powerOutputInt = powerConsumption;
        }

        public virtual void CreateAIMind()
        {
            NanoswarmsHelper.WriteLog("Creating AI Mind",NanoswarmsHelper.LogType.Debug);
            var pawnKindDef = Props.SpawnType;
            var ofPlayer = Faction.OfPlayer;
            var pawnRequest = new PawnGenerationRequest(
                PawnKindDefOf.Colonist,
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
                null,
                null,
                null,
                null,
                null,
                18f,
                null,
                null,
                null,
                null,
                null,
                null,
                true,
                true,
                true,
                false,
                null,
                null,
                null,
                null,
                null,
                0.0f,
                DevelopmentalStage.Adult,
                null,
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
            while (pawn.story.traits.allTraits.Count > Props.numberOfTraits)
                pawn.story.traits.allTraits.RemoveLast();            
            StoredMind = pawn;
            applyXenotype();
            var passionsRemaining = Props.maxPassions;
            var burningPassionsRemaining = Props.maxBurningPassions;
            foreach (var skill in StoredMind.skills.skills)
            {
                var random = Random.Range(0,100);
                var passionToSet = Passion.None;
                if (random < Props.burningPassionChancePercent && burningPassionsRemaining > 0)
                {
                    passionToSet = Passion.Major;
                    burningPassionsRemaining--;
                } else if (random < Props.passionChancePercent && passionsRemaining > 0)
                {
                    passionToSet = Passion.Minor;
                    passionsRemaining--;
                }
                NanoswarmsHelper.WriteLog("Value for " + skill.LevelDescriptor + ": " + random,
                    NanoswarmsHelper.LogType.Debug);
                skill.passion = passionToSet;
                var skillLevel = Random.Range(Props.skillRangeMinimum, Props.skillRangeMaximum+1);
                skill.levelInt = skillLevel;
            }
            
            StoredMind.skills.Notify_SkillDisablesChanged();
            if (ModsConfig.IdeologyActive)
                StoredMind.ideo.SetIdeo(Faction.OfPlayer.ideos.PrimaryIdeo);
            pawn.apparel.DestroyAll();
        }

        
        public bool ReprogrammingJobReady()
        {
            return (reprogramingProject != null);
        }
        
        public bool ReprogrammingInProgress()
        {
            return (reprogramingProject != null && CurrentWorkAmountDone < TotalWorkAmount);
        }
        
        
        public virtual void PreFormation()
        {
            NanoswarmsHelper.WriteLog("Has Reprogramming Project: " + (reprogramingProject != null),NanoswarmsHelper.LogType.Debug);
            if (ReprogrammingJobReady())
            {
                if (ReprogrammingInProgress())
                    return;
            }
            
            NanoswarmsHelper.WriteLog("Form Projection Started", NanoswarmsHelper.LogType.Debug);
            if (StoredMind.Spawned || StoredMind.Corpse != null)
            {
                NanoswarmsHelper.WriteLog("Projection already formed. Destroy first.", NanoswarmsHelper.LogType.Debug);
                StopProjection();
            }
            
            // iterate over the hediffs and remove any we wouldn't want on a digital mind in a nanobot swarm body.
            // this will include almost everything.
            //copy the list so we can act on it.
            var hediffList = StoredMind.health.hediffSet.hediffs.ListFullCopyOrNull();
            foreach (var hediff in hediffList)
            {
                NanoswarmsHelper.WriteLog("Removing Hediff " + hediff.Label, NanoswarmsHelper.LogType.Debug);
                StoredMind.health.RemoveHediff(hediff);
            }
            var projectionBody =
                (mytNS_NanoswarmProjectionBody) StoredMind.health.GetOrAddHediff(mytNSDefOf.mytNS_NanoswarmProjectionBody);
            projectionBody.DigitalMindStorage = this;
            applyXenotype();
            StoredMind.ageTracker.ResetAgeReversalDemand(Pawn_AgeTracker.AgeReversalReason.ViaTreatment);
            
            StoredMind.forceNoDeathNotification = true;
            NanoswarmsHelper.WriteLog("PreFormation for "+StoredMind.Name+" Complete.", NanoswarmsHelper.LogType.Debug);
        }

        private void initializeFormation()
        {
            PreFormation();
            _bodyFormingCompletedTicks = 0;
            isBodyForming = true;
        }

        private void applyXenotype()
        {
            if (StoredMind == null) return;
            if (storedCustomXenotype == null)
            {
                NanoswarmsHelper.WriteLog("No custom xenotype. Apply default one from Swarmtype.", NanoswarmsHelper.LogType.Info);
                StoredMind?.genes?.Endogenes?.Clear();
                StoredMind?.genes?.Xenogenes?.Clear();
                storedCustomXenotype = new CustomXenotype
                {
                    name = Props.SpawnType.label,
                    inheritable = false
                };
                NanoswarmsHelper.WriteLog("Creating " + storedCustomXenotype.name + ".", NanoswarmsHelper.LogType.Debug);
                if (Props?.SpawnType?.hardwareGenes?.Count > 0)
                {
                    NanoswarmsHelper.WriteLog("Hardware genes:  " + Props.SpawnType.hardwareGenes.Count + ".", NanoswarmsHelper.LogType.Debug);
                    storedCustomXenotype.genes.AddRange(Props.SpawnType.hardwareGenes);
                }
                if (Props?.SpawnType?.defaultSubroutineGenes?.Count > 0)
                {
                    NanoswarmsHelper.WriteLog("Subroutine genes:  " + Props.SpawnType.defaultSubroutineGenes.Count + ".", NanoswarmsHelper.LogType.Debug);
                    storedCustomXenotype.genes.AddRange(Props.SpawnType.defaultSubroutineGenes);    
                }
                storedCustomXenotype.iconDef = new XenotypeIconDef()
                {
                    texPath = Props.SpawnType.iconPath
                };
                //we'll only set the hardware genes once when initially setting the base xenotype.
                foreach (var geneDef in storedCustomXenotype.genes.OrderByDescending(x => !x.CanBeRemovedFromAndroid())
                             .ToList().Where(geneDef => geneDef.IsHardware()))
                {
                    StoredMind.genes.AddGene(geneDef, true);
                }
            }
            if (StoredMind?.genes == null)
            {
                StoredMind.genes = new Pawn_GeneTracker();
            }
            NanoswarmsHelper.WriteLog("Resetting xenotype for " + StoredMind.Name + " to " + storedCustomXenotype.name + ".", NanoswarmsHelper.LogType.Debug);
            StoredMind.genes.xenotypeName = storedCustomXenotype.name;
            StoredMind.genes.iconDef = storedCustomXenotype.iconDef;
            foreach (var gene in Utils.allAndroidGenes
                         .Select(allAndroidGene => StoredMind.genes.GetGene(allAndroidGene))
                         .Where(gene => gene != null && gene.def.IsSubroutine()))
            {
                StoredMind.genes.RemoveGene(gene);
            }

            foreach (var geneDef in storedCustomXenotype.genes.OrderByDescending(x => !x.CanBeRemovedFromAndroid())
                         .ToList().Where(geneDef => geneDef.IsSubroutine()))
            {
                StoredMind.genes.AddGene(geneDef, true);
            }
        }

        protected virtual void FormProjection()
        {
            if (StoredMind.Dead)
            {
                NanoswarmsHelper.WriteLog("Attempt resurrection for "+StoredMind.Name+".", NanoswarmsHelper.LogType.Debug);
                ResurrectionUtility.TryResurrect(StoredMind);
                NanoswarmsHelper.WriteLog("Resurrection for "+StoredMind.Name+" complete.", NanoswarmsHelper.LogType.Debug);
            }
            
            NanoswarmsHelper.WriteLog("Try Place for "+StoredMind.Name+".", NanoswarmsHelper.LogType.Debug);
            GenPlace.TryPlaceThing(StoredMind, parent.Position, parent.Map, ThingPlaceMode.Near);
            StoredMind.Drawer.renderer.EnsureGraphicsInitialized();
            NanoswarmsHelper.WriteLog("Try Place for "+StoredMind.Name+" Complete.", NanoswarmsHelper.LogType.Debug);
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
                StoredMind.DeSpawn();
            }

            if (StoredMind.Corpse?.Map != null)
            {
                NanoswarmsHelper.WriteLog("Found StoredMind Corpse Map.  Trigger explosion and then despawn.", NanoswarmsHelper.LogType.Debug);
                GenExplosion.DoExplosion(StoredMind.Corpse.Position, StoredMind.Corpse.Map, 4.9f, mytNSDefOf.mytNS_Damage_Nanodust, StoredMind.Corpse, -1, -1f, null, null, null, null, ThingDefOf.Filth_Slime);
                StoredMind.Corpse.Destroy();
            }
            
        }
    }
}