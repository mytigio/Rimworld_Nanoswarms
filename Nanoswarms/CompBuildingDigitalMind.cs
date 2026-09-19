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

        public bool Reprogrammable => this.Props.SpawnType.isReprogrammable;

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
            if (!respawningAfterLoad && Props.SpawnType.isAI && _compPower.PowerOn)
            {
                CreateAIMind();
            }
            else
            {
                GetLinkedHediff();
            }
            
        }

        public bool StoredMindSpawned()
        {
            if (StoredMind == null) return false;
            
            
            return (StoredMind.Spawned || StoredMind.InContainerEnclosed ||
                    StoredMind.CarriedBy != null);
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
                
                if (StoredMind != null && !StoredMindSpawned() && !ReprogrammingJobReady() && !_isBodyForming)
                {
                    var formProjectionAction = new Command_Action
                    {
                        action = InitializeFormation,
                        defaultLabel = "mytNS_SpawnProjection".Translate(),
                        defaultDesc = "mytNS_SpawnProjectionDesc".Translate(),
                        icon = ContentFinder<Texture2D>.Get("UI/Gizmos/FormProjection")
                    };
                    gizmosExtra.Add(formProjectionAction);
                } else if (StoredMindSpawned() && !ReprogrammingJobReady() && !_isBodyForming)
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

                if (StoredMind != null && !StoredMindSpawned() && !ReprogrammingJobReady() && !_isBodyForming)
                {
                    if (Reprogrammable)
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
            }

            return gizmosExtra;
        }

        private void InitiateStyling()
        {
            if (!ModLister.CheckIdeology("Styling station")) return;
            Find.WindowStack.Add(new Dialog_StylingStation(StoredMind, parent));
            //set the hair
            if (StoredMind.style.nextHairDef != null && StoredMind.style.nextHairDef != StoredMind.story.hairDef)
            {
                StoredMind.story.hairDef = StoredMind.style.nextHairDef;
            }

            //set the bear
            if (StoredMind.style.CanWantBeard && StoredMind.style.nextBeardDef != null &&
                StoredMind.style.nextBeardDef != StoredMind.style.beardDef)
            {
                StoredMind.style.beardDef =  StoredMind.style.nextBeardDef;
            }
        
            if (StoredMind.style.nextFaceTattooDef != null)
            {
                StoredMind.style.FaceTattoo = StoredMind.style.nextFaceTattooDef;
            }

            if (StoredMind.style.nextBodyTatooDef != null)
            {
                StoredMind.style.BodyTattoo = StoredMind.style.nextBodyTatooDef;
            }
            
            StoredMind.style.Notify_StyleItemChanged();
            StoredMind.style.ResetNextStyleChangeAttemptTick();
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

            if (StoredMind == null) return sb.ToString().Trim();
            var desync = StoredMind.health.hediffSet.GetFirstHediffOfDef(mytNSDefOf.mytNS_Desynchronization);
            if (desync != null && desync.Severity > 0.0f)
            {
                sb.Append("mytNS_DesyncDisplay".Translate() + ": " + desync.Severity.ToStringPercent());
            }

            sb.AppendLine();
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

            if (StoredMind == null && _compPower.PowerOn && Props.IsAIMind)
            {
                CreateAIMind();
            }
            
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

            if (StoredMind != null)
            {
                var desync = StoredMind.health.hediffSet.GetFirstHediffOfDef(mytNSDefOf.mytNS_Desynchronization);
                if (!_compPower.PowerOn)
                {
                    if (StoredMindSpawned())
                    {
                        StopProjection();    
                    }
                
                    if (desync == null)
                    {
                        NanoswarmsHelper.WriteLog($"Power is off. Add desync hediff to {StoredMind.Name}.");
                        desync = StoredMind.health.AddHediff(mytNSDefOf.mytNS_Desynchronization);
                        desync.Severity = 0.1f;
                    }
                    else if (desync.Severity < 1.0f)
                    {
                        NanoswarmsHelper.WriteLog($"Power is off. Increase severity of desync hediff for {StoredMind.Name}.");
                        desync.Severity += 0.1f;
                    }
                    else
                    {
                        NanoswarmsHelper.WriteLog($"Desync severity for {StoredMind.Name} is at max severity.");
                    }
                }
                else
                {
                    if (desync != null && !StoredMindSpawned())
                    {
                        NanoswarmsHelper.WriteLog($"Reduce desync Severity for {StoredMind.Name} while no projection spawned: {desync.Severity}");
                        desync.Severity -= 0.001f;
                    }
                }
            }

            if (!_isBodyForming || !(_compRefuelable.Fuel >= TickModifier * 4)) return; //either we aren't growing a body atm or we dont have enough fuel to proceed.
            NanoswarmsHelper.WriteLog("Body Forming for "+StoredMind.Name + "Work: " + _bodyFormingCompletedTicks + " / " + TicksToFormBody,NanoswarmsHelper.LogType.Debug);
            _bodyFormingCompletedTicks += TickModifier;
            _compRefuelable.ConsumeFuel(4 * TickModifier);
            
            if (_bodyFormingCompletedTicks < TicksToFormBody) return;  //body formation not complete yet.
            FormProjection();
            _isBodyForming = false;
        }

        public override void PostDestroy(DestroyMode mode, Map previousMap)
        {
            base.PostDestroy(mode, previousMap);
            if (StoredMind == null) return;
            
            if (StoredMindSpawned())
            {
                StopProjection();
            }
            if (StoredMind.Dead)
            {
                NanoswarmsHelper.WriteLog($"Attempt resurrection for {StoredMind.Name} so we can kill them for real.", NanoswarmsHelper.LogType.Debug);
                ResurrectionUtility.TryResurrect(StoredMind);
                NanoswarmsHelper.WriteLog($"Resurrection for {StoredMind.Name} complete.", NanoswarmsHelper.LogType.Debug);
            }

            StoredMind.forceNoDeathNotification = false;
            NanoswarmsHelper.WriteLog($"{parent.ThingID} destroyed. Killing {StoredMind.Name}.");
            StoredMind.Kill(null);

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
                mytNSDefOf.mytNS_SwarmColonist,
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

            handleTraits(pawn);
            
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

        private void handleTraits(Pawn pawn)
        {
            var traitCount = pawn?.story?.traits?.allTraits?.Count ?? -1;
            var keptTraits = 0;
            for (var i = traitCount; i > 0; i--)
            {
                var idx = i - 1;
                var trait = pawn.story.traits.allTraits[idx];
                NanoswarmsHelper.WriteLog($"Checking trait {trait.def.defName} for removal.",NanoswarmsHelper.LogType.Debug);
                if (VREA_DefOf.VREA_AndroidSettings.disallowedTraits.Contains(
                        trait.def.defName) || keptTraits >= Props.numberOfTraits)
                {
                    pawn.story.traits.allTraits.RemoveAt(idx);
                    continue;
                }

                keptTraits++;
            }

            if (Props.SpawnType.forcedTraits != null && pawn?.story?.traits != null)
            {
                foreach (var trait in Props.SpawnType.forcedTraits)
                {
                    NanoswarmsHelper.WriteLog($"Adding {trait.defName} to {pawn.Name}");
                    pawn.story.traits.GainTrait(new Trait(trait));
                }     
            }
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
            foreach (var hediff in hediffList.Where(hediff => (hediff.def != mytNSDefOf.mytNS_NanoswarmProjectionBody && hediff.def != mytNSDefOf.mytNS_Desynchronization)))
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
            StoredMind = clonePawnAsSwarm(pawnToStore);
            handleTraits(StoredMind);
            pawnToStore.Destroy();
            ApplyXenotype();
            InitializeFormation();
        }

        private Pawn clonePawnAsSwarm(Pawn originalPawn)
        {
            var newPawn = (Pawn) ThingMaker.MakeThing(mytNSDefOf.mytNS_SwarmColonist.race);
            newPawn.kindDef = mytNSDefOf.mytNS_SwarmColonist;
            newPawn.SetFactionDirect(originalPawn.Faction);
            PawnComponentsUtility.CreateInitialComponents(newPawn);
            newPawn.gender = originalPawn.gender;
            
            newPawn.ageTracker = originalPawn.ageTracker;
            newPawn.needs = originalPawn.needs;
            newPawn.skills = originalPawn.skills;

            newPawn.story = originalPawn.story;
            newPawn.Name = originalPawn.Name;
            
            newPawn.abilities = originalPawn.abilities;
            newPawn.connections = originalPawn.connections;

            newPawn.genes = originalPawn.genes;
            newPawn.health = originalPawn.health;
            newPawn.ideo = originalPawn.ideo;

            if (originalPawn?.learning != null)
            {

                newPawn.learning = originalPawn.learning;
            }
            
            
            newPawn.foodRestriction = originalPawn.foodRestriction;
            newPawn.drugs = originalPawn.drugs;
            newPawn.workSettings = originalPawn.workSettings;

            if (originalPawn?.mechanitor != null)
            {
                newPawn.mechanitor = originalPawn.mechanitor;    
            }

            newPawn.forceNoDeathNotification = originalPawn.forceNoDeathNotification;
            
            return newPawn;
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
        private bool _stoppingProjection = false;
        public virtual void StopProjection()
        {
            //mutex lock to prevent multiple stops from running at the same time.
            if (_stoppingProjection)
            {
                NanoswarmsHelper.WriteLog($"Stopping projection underway. Return.", NanoswarmsHelper.LogType.Debug);
            }
            _stoppingProjection = true;
            NanoswarmsHelper.WriteLog("Form Projection Stopped", NanoswarmsHelper.LogType.Debug);
            if (StoredMind.carryTracker?.CarriedThing != null)
            {
                NanoswarmsHelper.WriteLog("Form Projection Carrying stuff. Drop it.", NanoswarmsHelper.LogType.Debug);
                StoredMind.carryTracker.TryDropCarriedThing(StoredMind.Position, ThingPlaceMode.Near, out var resultingThing);
            }
                
            if (StoredMindSpawned() || StoredMind.Corpse != null && StoredMind.Corpse.Spawned)
            {
                NanoswarmsHelper.WriteLog($"Form Projection spawned: {StoredMindSpawned()} or is corpse {StoredMind.Corpse != null && StoredMind.Corpse.Spawned}. Drop all of their things.", NanoswarmsHelper.LogType.Debug);
                if (StoredMindSpawned())
                {
                    StoredMind.Strip(false);
                    StoredMind.equipment.DestroyAllEquipment();
                    StoredMind.apparel.DestroyAll();
                    StoredMind.inventory.DestroyAll();
                }

                if (StoredMind.Corpse != null)
                {
                    StoredMind.Corpse.Strip(false);
                    StoredMind.Corpse.InnerPawn.equipment.DestroyAllEquipment();
                    StoredMind.Corpse.InnerPawn.apparel.DestroyAll();
                    StoredMind.Corpse.InnerPawn.inventory.DestroyAll();
                }
                
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

            _stoppingProjection = false;
        }
        
        public override void PostExposeData()
        {
            base.PostExposeData();
            if (Scribe.mode == LoadSaveMode.Saving)
            {
                var StoredMindSpawned = StoredMind?.Spawned == true;
                var StoredMindInContainer = StoredMind?.InContainerEnclosed == true;
                var StoredMindCarried = StoredMind?.CarriedBy != null;
                var StoredMindInWorld = Find.WorldPawns.Contains(StoredMind);
                _requiresDeepSave = (!StoredMindSpawned && !StoredMindInContainer &&
                                     !StoredMindCarried && !StoredMindInWorld);
                NanoswarmsHelper.WriteLog($"{StoredMind?.Name} requires deep save: " + _requiresDeepSave, NanoswarmsHelper.LogType.Debug);
                NanoswarmsHelper.WriteLog($"{StoredMind?.Name} is spawned: {StoredMindSpawned}");
                NanoswarmsHelper.WriteLog($"{StoredMind?.Name} is in container: {StoredMindInContainer}");
                NanoswarmsHelper.WriteLog($"{StoredMind?.Name} is being carried: {StoredMindCarried}");
                NanoswarmsHelper.WriteLog($"{StoredMind?.Name} is in world: {StoredMindInWorld}");
                Scribe_Values.Look(ref _requiresDeepSave, "RequiresDeepSave", defaultValue: false);
                if (_requiresDeepSave) 
                {
                    NanoswarmsHelper.WriteLog($"Deep save stored mind {StoredMind?.Name}", NanoswarmsHelper.LogType.Debug);
                    Scribe_Deep.Look<Pawn>(ref StoredMind, "StoredMind");
                }
                else
                {
                    NanoswarmsHelper.WriteLog($"Save reference to {StoredMind?.Name}", NanoswarmsHelper.LogType.Debug);
                    Scribe_References.Look(ref StoredMind, "StoredMind");    
                }
            }
            else
            {
                Scribe_Values.Look(ref _requiresDeepSave, "RequiresDeepSave", defaultValue: false);
                if (_requiresDeepSave)
                {
                    NanoswarmsHelper.WriteLog($"Restore StoredMind from deep save.", NanoswarmsHelper.LogType.Debug);
                    Scribe_Deep.Look(ref StoredMind, "StoredMind");
                }
                else
                {
                    NanoswarmsHelper.WriteLog($"Restore StoredMind via reference");
                    Scribe_References.Look(ref StoredMind, "StoredMind");
                }

                if (StoredMind != null)
                {
                    GetLinkedHediff();
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