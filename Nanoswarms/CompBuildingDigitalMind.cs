using System;
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
    public class CompBuildingDigitalMind : ThingComp, ILoadReferenceable
    {
        private const int TickModifier = 250;
        private const float TicksToFormBody = 2500.0f;

        //public variables.
        public Pawn StoredMind;
        public CustomXenotype ReprogrammingProject;
        public float TotalWorkAmount = 12000.0f;
        public float CurrentWorkAmountDone = 0.0f;
        
        //Private variables.
        private CompProps_DigitalMind Props => (CompProps_DigitalMind) props;
        private CompRefuelable _compRefuelable;
        private CompPowerTrader _compPower;
        private CustomXenotype _storedCustomXenotype;
        
        private float _bodyFormingCompletedTicks = 0.0f;
        private bool _isBodyForming = false;
        private bool _requiresDeepSave = false;

        public bool Reprogrammable => this.Props.reprogrammable;



        public override void PostPreApplyDamage(ref DamageInfo dinfo, out bool absorbed)
        {
            base.PostPreApplyDamage(ref dinfo, out absorbed);
            if (dinfo.Def == DamageDefOf.EMP)
            {
                NanoswarmsHelper.WriteLog("EMP hit. End projection.", NanoswarmsHelper.LogType.Debug);
                StopProjection();
            }
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            _compPower = parent.TryGetComp<CompPowerTrader>();
            _compRefuelable = parent.GetComp<CompRefuelable>();
            if (!respawningAfterLoad)
            {
                CreateAIMind();
            }
            else
            {
                GetLinkedHediff();
            }
            
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
                
                if (StoredMind != null && !StoredMind.Spawned && !StoredMind.InContainerEnclosed && StoredMind.CarriedBy == null && !ReprogrammingJobReady() && !_isBodyForming)
                {
                    var formProjectionAction = new Command_Action
                    {
                        action = InitializeFormation,
                        defaultLabel = "mytNS_SpawnProjection".Translate(),
                        defaultDesc = "mytNS_SpawnProjectionDesc".Translate(),
                        icon = ContentFinder<Texture2D>.Get("UI/Gizmos/FormProjection")
                    };
                    gizmosExtra.Add(formProjectionAction);
                } else if (StoredMind != null && StoredMind.Spawned && !ReprogrammingJobReady() && !_isBodyForming)
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
                        action = InitializeFormation,
                        defaultLabel = "mytNS_RespawnProjection".Translate(),
                        defaultDesc = "mytNS_RespawnProjectionDesc".Translate(),
                        icon = ContentFinder<Texture2D>.Get("UI/Gizmos/FormProjection")
                    };
                    gizmosExtra.Add(reFormProjectionAction);
                }

                if (StoredMind != null && !StoredMind.Spawned && !StoredMind.InContainerEnclosed &&
                    StoredMind.CarriedBy == null && !ReprogrammingJobReady() && !_isBodyForming)
                {
                    var reProgramAction = new Command_Action
                    {
                        action = InitiateReprogram,
                        defaultLabel = (Reprogrammable) ? "mytNS_Reprogram".Translate() : "mytNS_Customize".Translate(),
                        defaultDesc = (Reprogrammable) ? "mytNS_ReprogramDesc".Translate() : "mytNS_CustomizeDesc".Translate(),
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

            if (_isBodyForming)
            {
                sb.Append("mytNS_FormingBody".Translate() + ": " +
                          (_bodyFormingCompletedTicks / TicksToFormBody).ToStringPercent());
                sb.AppendLine();
            }
            return sb.ToString().Trim();
        }

        private void InitiateReprogram()
        {
            if (!_compPower.PowerOn) return;
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
                NanoswarmsHelper.WriteLog("Reprogramming Project for "+StoredMind.Name+" Name: " + ReprogrammingProject.name + "Work: " + CurrentWorkAmountDone + " / " + TotalWorkAmount,NanoswarmsHelper.LogType.Debug);
                CurrentWorkAmountDone += TickModifier;
                if (CurrentWorkAmountDone >= TotalWorkAmount)
                {
                    SetCustomXenotype();
                }
            }

            GetLinkedHediff();

            if (!_isBodyForming || !(_compRefuelable.Fuel >= TickModifier * 4)) return; //either we aren't growing a body atm or we dont have enough fuel to proceed.
            NanoswarmsHelper.WriteLog("Body Forming for "+StoredMind.Name + "Work: " + _bodyFormingCompletedTicks + " / " + TicksToFormBody,NanoswarmsHelper.LogType.Debug);
            _bodyFormingCompletedTicks += TickModifier;
            _compRefuelable.ConsumeFuel(4 * TickModifier);
            
            if (_bodyFormingCompletedTicks < TicksToFormBody) return;  //body formation not complete yet.
            FormProjection();
            _isBodyForming = false;
        }

        private mytNS_NanoswarmProjectionBody GetLinkedHediff()
        {
            if (StoredMind?.health?.hediffSet?.TryGetHediff(mytNSDefOf.mytNS_NanoswarmProjectionBody, out var bodyHediff) == true)
            {
                if (!(bodyHediff is mytNS_NanoswarmProjectionBody projectionBody)) return null;
                if (projectionBody.DigitalMindStorage == this) return projectionBody;
                NanoswarmsHelper.WriteLog("Set hediff digital mind storage to current digital mind storage comp", NanoswarmsHelper.LogType.Debug);
                projectionBody.DigitalMindStorage = this;
                return projectionBody;    
            }
            
            NanoswarmsHelper.WriteLog($"No nanoswarm body hediff found for {StoredMind?.Name}", NanoswarmsHelper.LogType.Debug);
            return null;                

        }

        private void SetCustomXenotype()
        {
            _storedCustomXenotype = ReprogrammingProject;
            ReprogrammingProject = null;
            TotalWorkAmount = 12000;
            CurrentWorkAmountDone = 0;
            NanoswarmsHelper.WriteLog("Set stored xenotype to " + _storedCustomXenotype.name,NanoswarmsHelper.LogType.Debug);
            ApplyXenotype();
            var metScore = StoredMind.genes.GenesListForReading.Where(gene => !gene.Overridden).Sum(gene => gene.def.biostatMet);
            var powerConsumption = -(_compPower.Props.PowerConsumption * AndroidStatsTable.PowerEfficiencyToPowerDrainFactorCurve.Evaluate(metScore));
            NanoswarmsHelper.WriteLog("Building power to " + (-1 * powerConsumption), NanoswarmsHelper.LogType.Debug);
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
            ApplyXenotype();
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
            return (ReprogrammingProject != null);
        }
        
        public bool ReprogrammingInProgress()
        {
            return (ReprogrammingProject != null && CurrentWorkAmountDone < TotalWorkAmount);
        }
        
        
        public virtual void PreFormation()
        {
            
            if (ReprogrammingJobReady())
            {
                if (ReprogrammingInProgress())
                {
                    NanoswarmsHelper.WriteLog("Has Reprogramming Project: " + (ReprogrammingProject != null),NanoswarmsHelper.LogType.Debug);
                    return;   
                }
            }
            
            NanoswarmsHelper.WriteLog("Form Projection Started", NanoswarmsHelper.LogType.Debug);
            if (StoredMind.Spawned || StoredMind.Corpse != null)
            {
                NanoswarmsHelper.WriteLog("Projection already formed. Destroy first.", NanoswarmsHelper.LogType.Debug);
                StopProjection();
            }
            
            // iterate over the hediffs and remove any we wouldn't want on a digital mind in a nanobot swarm body.
            // this will include almost everything.
            // copy the list so we can act on it.
            var hediffList = StoredMind.health.hediffSet.hediffs.ListFullCopyOrNull();
            foreach (var hediff in hediffList.Where(hediff => hediff.def != mytNSDefOf.mytNS_NanoswarmProjectionBody))
            {
                NanoswarmsHelper.WriteLog("Removing Hediff " + hediff.Label, NanoswarmsHelper.LogType.Debug);
                StoredMind.health.RemoveHediff(hediff);
                StoredMind.health.Notify_HediffChanged(hediff);
            }

            var projectionBody = GetLinkedHediff();
            projectionBody?.RefreshNanitePool();

            NanoswarmsHelper.WriteLog("Reset age reversal need.", NanoswarmsHelper.LogType.Debug);
            StoredMind.ageTracker.ResetAgeReversalDemand(Pawn_AgeTracker.AgeReversalReason.ViaTreatment);
            
            StoredMind.forceNoDeathNotification = true;
            NanoswarmsHelper.WriteLog("PreFormation for "+StoredMind.Name+" Complete.", NanoswarmsHelper.LogType.Debug);
        }

        public void CompleteDigitization(Pawn pawnToStore)
        {
            pawnToStore.forceNoDeathNotification = true;
            pawnToStore.equipment.DestroyAllEquipment();
            pawnToStore.apparel.DestroyAll();
            pawnToStore.inventory.DestroyAll();
            StoredMind = pawnToStore;
            ApplyXenotype();
            InitializeFormation();
        }

        private void InitializeFormation()
        {
            PreFormation();
            _bodyFormingCompletedTicks = 0;
            _isBodyForming = true;
        }

        private void ApplyXenotype()
        {
            if (StoredMind == null) return;
            if (_storedCustomXenotype == null)
            {
                NanoswarmsHelper.WriteLog("No custom xenotype. Create default one from Swarmtype.", NanoswarmsHelper.LogType.Info);
                StoredMind?.genes?.Endogenes?.Clear();
                StoredMind?.genes?.Xenogenes?.Clear();
                _storedCustomXenotype = new CustomXenotype
                {
                    name = Props.SpawnType.label,
                    inheritable = false
                };
                NanoswarmsHelper.WriteLog("Creating " + _storedCustomXenotype.name + ".", NanoswarmsHelper.LogType.Debug);
                if (Props?.SpawnType?.hardwareGenes?.Count > 0)
                {
                    NanoswarmsHelper.WriteLog("Hardware genes:  " + Props.SpawnType.hardwareGenes.Count + ".", NanoswarmsHelper.LogType.Debug);
                    _storedCustomXenotype.genes.AddRange(Props.SpawnType.hardwareGenes);
                }
                if (Props?.SpawnType?.defaultSubroutineGenes?.Count > 0)
                {
                    NanoswarmsHelper.WriteLog("Subroutine genes:  " + Props.SpawnType.defaultSubroutineGenes.Count + ".", NanoswarmsHelper.LogType.Debug);
                    _storedCustomXenotype.genes.AddRange(Props.SpawnType.defaultSubroutineGenes);
                }
                _storedCustomXenotype.iconDef = new XenotypeIconDef()
                {
                    texPath = Props.SpawnType.iconPath
                };
                
                //we'll only set the non-subroutine genes once when initially setting the base xenotype.
                foreach (var geneDef in _storedCustomXenotype.genes.OrderByDescending(x => !x.CanBeRemovedFromAndroid())
                             .ToList())
                {
                    if (StoredMind?.genes == null || StoredMind.genes.HasActiveGene(geneDef)) continue;
                    NanoswarmsHelper.WriteLog($"Adding {geneDef.defName} to {StoredMind?.Name}. IsAndroidGene: {geneDef.IsAndroidGene()}; IsHardware: {geneDef.IsHardware()}; IsSubroutine: {geneDef.IsSubroutine()}", NanoswarmsHelper.LogType.Debug);
                    StoredMind?.genes?.AddGene(geneDef, true);
                }
            }
            if (StoredMind != null && StoredMind.genes == null)
            {
                StoredMind.genes = new Pawn_GeneTracker();
            }
            NanoswarmsHelper.WriteLog("Resetting xenotype for " + StoredMind?.Name + " to " + _storedCustomXenotype.name + ".", NanoswarmsHelper.LogType.Debug);
            StoredMind.genes.xenotypeName = _storedCustomXenotype.name;
            StoredMind.genes.iconDef = _storedCustomXenotype.iconDef;
            
            var categoriesToShow = NanoswarmsHelper.ExtraGeneCategories;
            
            foreach (var gene in Utils.allAndroidGenes
                         .Select(allAndroidGene => StoredMind.genes.GetGene(allAndroidGene))
                         .Where(gene => gene != null && (gene.def.IsSubroutine() || categoriesToShow.Contains(gene.def.displayCategory))))
            {
                NanoswarmsHelper.WriteLog($"Removing {gene.def.defName} from {StoredMind.Name}", NanoswarmsHelper.LogType.Debug);
                StoredMind.genes.RemoveGene(gene);
            }

            foreach (var geneDef in _storedCustomXenotype.genes.ToList()
                         .Where(genedef => (genedef.IsSubroutine() || 
                                            categoriesToShow.Contains(genedef.displayCategory))))
            {
                if (StoredMind?.genes == null || StoredMind.genes.HasActiveGene(geneDef)) continue;
                NanoswarmsHelper.WriteLog($"Adding {geneDef.defName} to {StoredMind?.Name}. IsAndroidGene: {geneDef.IsAndroidGene()}; IsHardware: {geneDef.IsHardware()}; IsSubroutine: {geneDef.IsSubroutine()}", NanoswarmsHelper.LogType.Debug);
                StoredMind?.genes?.AddGene(geneDef, true);
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
                StoredMind.Strip(false);
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
        
        public override void PostExposeData()
        {
            base.PostExposeData();
            if (Scribe.mode == LoadSaveMode.Saving)
            {
                _requiresDeepSave = (StoredMind == null || StoredMind.Spawned || StoredMind.InContainerEnclosed ||
                                     StoredMind.CarriedBy != null || Find.WorldPawns.Contains(StoredMind));
                Scribe_Values.Look(ref _requiresDeepSave, "RequiresDeepSave", defaultValue: false);
                if (!_requiresDeepSave) 
                {
                    Scribe_References.Look(ref StoredMind, "StoredMind");
                }
                else
                {
                    Scribe_Deep.Look<Pawn>(ref StoredMind, "StoredMind");    
                }
            }
            else
            {
                Scribe_Values.Look(ref _requiresDeepSave, "RequiresDeepSave", defaultValue: false);
                if (_requiresDeepSave)
                {
                    Scribe_Deep.Look(ref StoredMind, "StoredMind");
                }
                else
                {
                    Scribe_References.Look(ref StoredMind, "StoredMind");
                }
            }
            
            Scribe_Deep.Look(ref ReprogrammingProject, "ReprogrammingProject");
            Scribe_Deep.Look(ref _storedCustomXenotype, "_storedCustomXenotype");
            
            Scribe_Values.Look(ref TotalWorkAmount, "TotalWorkAmount");
            Scribe_Values.Look(ref CurrentWorkAmountDone, "CurrentWorkAmountDone");
            Scribe_Values.Look(ref _bodyFormingCompletedTicks, "_bodyFormingCompletedTicks");
            Scribe_Values.Look(ref _isBodyForming, "_isBodyForming");
        }

        public string GetUniqueLoadID()
        {
            return "mytNS_digitalmindcomp_" + parent.GetUniqueLoadID();
        }
    }
    
}