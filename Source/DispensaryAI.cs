using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Verse.AI;

namespace DispensaryMod
{

    public class JobGiver_GetDrugsFromDispensary : ThinkNode_JobGiver
    {

        private ThingDef DispensaryDef { get { return DefDatabase<ThingDef>.GetNamed("Building_Dispensary"); } }
        private JobDef GetDrug { get { return DefDatabase<JobDef>.GetNamed("GetDrugFromDispensary"); } }


        public override float GetPriority(Pawn pawn)
        {
            DrugPolicy currentPolicy = pawn.drugs.CurrentPolicy;
            int num = currentPolicy.Count - 1;
            for (int i = 0; i <= num; i++)
            {
                if (pawn.drugs.AllowedToTakeScheduledNow(currentPolicy[i].drug))
                {
                    return 7.6f;
                }
            }
            return 0f;
        }

        protected override Job TryGiveJob(Pawn pawn)
        {
            DrugPolicy currentPolicy = pawn.drugs.CurrentPolicy;
            int num = currentPolicy.Count - 1;
            for (int i = 0; i <= num; i++)
            {
                ThingDef drugDef = currentPolicy[i].drug;
                if (pawn.drugs.AllowedToTakeScheduledNow(drugDef))
                {
                    Predicate<Thing> disp_validator = delegate (Thing x) { return isDispensaryValid(x,drugDef,pawn); };

                    //Try and find the closest dispensary with the requested drug, return if none found
                    Thing dispensary = GenClosest.ClosestThingReachable(
                        pawn.Position,
                        pawn.Map,
                        ThingRequest.ForDef(DispensaryDef),
                        PathEndMode.InteractionCell,
                        TraverseParms.For(pawn),
                        validator: disp_validator
                        );      
                    if (dispensary == null) return null;

                    //Try and find drug in the specified dispensary, return if none found
                    Thing drug = findDrugToDispense(dispensary, drugDef, pawn);
                    if(drug == null) return null;

                    if (((Building_Dispensary) dispensary).canDispenseDrugNow(pawn.Map, drug))
                    {
                        return new Job(GetDrug, dispensary, drug);
                    }
                }
            }
            return null;
        }

        private static bool isDispensaryValid(Thing thing, ThingDef drug, Pawn pawn)
        {
            Building_Dispensary dispensary = thing as Building_Dispensary;
            if (dispensary == null) return false;

            return pawn.CanReserve(new LocalTargetInfo(thing)) && dispensary.isDrugAvailable(pawn.Map, drug);
        }

        private static Thing findDrugToDispense(Thing disp, ThingDef drugDef, Pawn pawn)
        {
            if (disp.GetType() == typeof(Building_Dispensary))
            {
                Building_Dispensary dispensary = (Building_Dispensary)disp;
                Zone_Stockpile stockpile = dispensary.stockpile(pawn.Map);
                if (stockpile != null)
                {
                    foreach (Thing t in stockpile.AllContainedThings)
                    {
                        if ((t.def == drugDef) & pawn.CanReserve(new LocalTargetInfo(t))) return t;
                    }
                }
            }
            return null;
        }
    }

    public class JobDriver_GetDrugsFromDispensary : JobDriver
    {
        private TargetIndex dispIndex = TargetIndex.A;
        private TargetIndex drugIndex = TargetIndex.B;

        public override string GetReport()
        {
            return "Obtaining Policy Drugs From Dispensary";
        }

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            //Pawn actor = GetActor();

            //if (CanReserveAndReach(actor, dispIndex) && CanReserveAndReach(actor, drugIndex))
            //{
            //    LocalTargetInfo disptg = job.GetTarget(dispIndex);
            //    LocalTargetInfo drugtg = job.GetTarget(drugIndex);
            //    //if (((Building_Dispensary)disptg.Thing).canDispenseDrugNow(actor.Map, drugtg.Thing))
            //   // {
            //        return actor.Reserve(disptg, job) && actor.Reserve(drugtg, job);
            //    // }
            //}
            //return false;

            return true;

        }

        private bool CanReserveAndReach(Pawn a, TargetIndex t)
        {
            return a.CanReserveAndReach(this.job.GetTarget(t), PathEndMode.InteractionCell, Danger.Some);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            yield return Toils_DispenseDrug.ReserveDispensaryAndDrug(dispIndex, drugIndex);
            yield return Toils_Goto.GotoThing(dispIndex, PathEndMode.InteractionCell);
            yield return Toils_DispenseDrug.TakeDrugFromDispensary(dispIndex, drugIndex);
        }

    }

    public class Toils_DispenseDrug
    {
        public static Toil TakeDrugFromDispensary(TargetIndex DispTarget, TargetIndex DrugTarget)
        {
            Toil toil = new Toil();
            toil.initAction = delegate
            {
                Pawn actor = toil.GetActor();
                Job curJob = actor.jobs.curJob;
                Thing drug = curJob.GetTarget(DrugTarget).Thing;

                if (curJob.GetTarget(DispTarget).Thing.GetType() == typeof(Building_Dispensary))
                {
                    Building_Dispensary dispensary = (Building_Dispensary)curJob.GetTarget(DispTarget).Thing;

                    if (dispensary.canDispenseDrugNow(actor.Map, drug))
                    {
                        Thing drugOut = dispensary.tryDispenseDrug(actor.Map, drug);
                        if (drugOut == null) actor.jobs.EndCurrentJob(JobCondition.Incompletable);
                        actor.carryTracker.TryStartCarry(drugOut);
                        actor.jobs.StartJob(DrugAIUtility.IngestAndTakeToInventoryJob(drugOut, actor), JobCondition.Succeeded);
                    }
                    else actor.jobs.EndCurrentJob(JobCondition.Incompletable);
                }
                else
                {
                    Log.Error("Targeted dispensory is not of the type Building_Dispensory, Type recieved: " + curJob.GetTarget(DispTarget).GetType().ToString());
                    actor.jobs.EndCurrentJob(JobCondition.Errored);
                }
            };

            toil.AddFinishAction(delegate { toil.actor.ClearReservationsForJob(toil.GetActor().jobs.curJob); });

            toil.defaultCompleteMode = ToilCompleteMode.Delay;
            toil.defaultDuration = Building_Dispensary.DispenseDuration;


            return toil;
        }

        public static Toil ReserveDispensaryAndDrug(TargetIndex DispTarget, TargetIndex DrugTarget)
        {
            Toil toil = new Toil();
            toil.initAction = delegate
            {
                Pawn actor = toil.GetActor();
                Job curJob = actor.CurJob;
                if (actor.CanReserve(curJob.GetTarget(DispTarget))) { actor.Reserve(curJob.GetTarget(DispTarget), curJob); }
                else { actor.jobs.EndCurrentJob(JobCondition.Incompletable); }
                if (actor.CanReserve(curJob.GetTarget(DrugTarget))) { actor.Reserve(curJob.GetTarget(DrugTarget), curJob); }
                else { actor.jobs.EndCurrentJob(JobCondition.Incompletable); }
            };

            toil.defaultCompleteMode = ToilCompleteMode.Instant;
            return toil;
        }

    }
}

