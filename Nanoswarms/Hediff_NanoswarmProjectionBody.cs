using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using VREAndroids;

namespace Nanoswarms
{
    public class Hediff_NanoswarmProjectionBody : Hediff_AndroidReactor
    {
        public CompBuildingDigitalMind DigitalMindStorage;
        
        private int _healTicksSinceLastHit = 0;
        private Gene_NaniteSwarmBody _nanoswarmBodyGene;
        
        private static Dictionary<SkillDef, (HediffDef hediffDef, string part)> skill10HediffMap = new Dictionary<SkillDef, (HediffDef hediffDef, string part)>()
        {
            { SkillDefOf.Construction, (mytNSDefOf.mytNS_construction10, "lefthand") },
            { SkillDefOf.Artistic, (mytNSDefOf.mytNS_artistic10, "righthand") },
            { SkillDefOf.Crafting, (mytNSDefOf.mytNS_crafting10, "lefthand") },
            { SkillDefOf.Animals, (mytNSDefOf.mytNS_animals10, "righthand") },
            { SkillDefOf.Cooking, (mytNSDefOf.mytNS_cooking10, "lefthand") },
            { SkillDefOf.Intellectual, (mytNSDefOf.mytNS_intelligence10, "array") },
            { SkillDefOf.Medicine, (mytNSDefOf.mytNS_medical10, "righthand") },
            { SkillDefOf.Melee, (mytNSDefOf.mytNS_melee10, "lefthand") },
            { SkillDefOf.Mining, (mytNSDefOf.mytNS_mining10, "righthand") },
            { SkillDefOf.Plants, (mytNSDefOf.mytNS_plants10, "lefthand") },
            { SkillDefOf.Shooting, (mytNSDefOf.mytNS_shooting10, "opticalCluster") },
            { SkillDefOf.Social, (mytNSDefOf.mytNS_social10, "swarm") }
        };
        public override void Tick()
        {
            if (!pawn.IsHashIntervalTick(60) && _healTicksSinceLastHit <= 0) return;
            if (_nanoswarmBodyGene == null)
            {
                NanoswarmsHelper.WriteLog("Nanoswarm Body Gene not found attached to hediff.", NanoswarmsHelper.LogType.Warning);
                LinkBodyGene();
            }
            _nanoswarmBodyGene?.HealIfPossible();
            _healTicksSinceLastHit--;

            if (pawn.IsHashIntervalTick(6000))
            {
                AddSkillHediffs();
            }
        }

        private void AddSkillHediffs()
        {
            BodyPartRecord leftArm = null;
            BodyPartRecord rightArm =  null;
            BodyPartRecord opticalSensor = null;
            BodyPartRecord swarm = null;
            BodyPartRecord array = null;

            foreach (var part in pawn.health.hediffSet.GetNotMissingParts())
            {
                if (part.groups.Contains(BodyPartGroupDefOf.LeftHand))
                {
                    leftArm = part;
                    continue;
                }
                if (part.groups.Contains(BodyPartGroupDefOf.RightHand))
                {
                    rightArm = part;
                    continue;
                }

                if (part.def == mytNSDefOf.mytNS_opticalCluster)
                {
                    opticalSensor = part;
                    continue;
                }

                if (part.def == mytNSDefOf.mytNS_swarmCloud)
                {
                    swarm = part;
                    continue;
                }

                if (part.def == mytNSDefOf.mytNS_tunnelingArray)
                {
                    array = part;
                    continue;
                }
            }

            foreach (var skill in pawn.skills.skills)
            {
                if (skill.Level >= 10)
                {
                    switch (skill10HediffMap[skill.def].part)
                    {
                        case "lefthand":
                            NanoswarmsHelper.WriteLog($"{pawn.Name}'s {skill.def} is level {skill.Level}. Add {skill10HediffMap[skill.def].hediffDef} added to {leftArm}",NanoswarmsHelper.LogType.Debug);
                            pawn.health.GetOrAddHediff(skill10HediffMap[skill.def].hediffDef, leftArm);
                            break;
                        case "righthand":
                            NanoswarmsHelper.WriteLog($"{skill.def} is level {skill.Level}. Add {skill10HediffMap[skill.def].hediffDef} added to {rightArm}",NanoswarmsHelper.LogType.Debug);
                            pawn.health.GetOrAddHediff(skill10HediffMap[skill.def].hediffDef, rightArm);
                            break;
                        case "array":
                            NanoswarmsHelper.WriteLog($"{skill.def} is level {skill.Level}. Add {skill10HediffMap[skill.def].hediffDef} added to {array}",NanoswarmsHelper.LogType.Debug);
                            pawn.health.GetOrAddHediff(skill10HediffMap[skill.def].hediffDef, array);
                            break;
                        case "opticalCluster":
                            NanoswarmsHelper.WriteLog($"{skill.def} is level {skill.Level}. Add {skill10HediffMap[skill.def].hediffDef} added to {opticalSensor}",NanoswarmsHelper.LogType.Debug);
                            pawn.health.GetOrAddHediff(skill10HediffMap[skill.def].hediffDef, opticalSensor);
                            break;
                        case "swarm":
                            NanoswarmsHelper.WriteLog($"{skill.def} is level {skill.Level}. Add {skill10HediffMap[skill.def].hediffDef} added to {swarm}",NanoswarmsHelper.LogType.Debug);
                            pawn.health.GetOrAddHediff(skill10HediffMap[skill.def].hediffDef, swarm);
                            break;
                    }
                }
            }
        }

        private void LinkBodyGene()
        {
            var gene = pawn.genes.GetGene(mytNSDefOf.mytNS_NanobotSwarm);
            if (gene is Gene_NaniteSwarmBody swarmBody)
            {
                _nanoswarmBodyGene = swarmBody;
                swarmBody.ProjectionBody = this;
            }
        } 

        public override bool Visible => true;

        public void RefreshNanitePool()
        {
            if (_nanoswarmBodyGene == null)
            {
                NanoswarmsHelper.WriteLog("Nanoswarm body gene is null. Cannot reset nanite pool.", NanoswarmsHelper.LogType.Warning);
                return;
            }
            NanoswarmsHelper.WriteLog("Reset nanite pool to max.");
            _nanoswarmBodyGene.Resource.Value = _nanoswarmBodyGene.Resource.Max;
            AddSkillHediffs();
        }
        
        public override void Notify_PawnPostApplyDamage(DamageInfo dinfo, float totalDamageDealt)
        {
            base.Notify_PawnPostApplyDamage(dinfo, totalDamageDealt);

            if (_nanoswarmBodyGene?.Active != true) return;
            NanoswarmsHelper.WriteLog("Begin healing if possible.", NanoswarmsHelper.LogType.Debug);
            _nanoswarmBodyGene?.HealIfPossible();
            _healTicksSinceLastHit = 600;
        }

        public override void Notify_PawnKilled()
        {
            base.Notify_PawnKilled();
            DigitalMindStorage?.StopProjection();
        }

        public override void Notify_PawnDied(DamageInfo? dinfo, Hediff culprit = null)
        {
            base.Notify_PawnDied(dinfo, culprit);
            DigitalMindStorage?.StopProjection();
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref _nanoswarmBodyGene, "_nanoswarmBodyGene");
            Scribe_Values.Look(ref _healTicksSinceLastHit, "_healTicksSinceLastHit");
        }
    }
}